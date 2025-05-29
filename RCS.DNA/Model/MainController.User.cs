using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using ExcelDataReader;
using RCS.Licensing.Provider.Shared.Entities;

namespace RCS.DNA.Model;

partial class MainController
{
	#region Create User

	public void PrepareForNewUser()
	{
		NewUserCustomerId = Strings.NoSelectId;
		NewUserName = null;
		NewUserPassword = null;
		AfterNewUserValueChanged();
	}

	public async Task CreateUser()
	{
		await WrapWork($"Creating new user {NewUserName}", async () =>
		{
			var user = new User()
			{
				Id = null,
				Name = NewUserName,
				EntityId = NewUserCustomerId!,
				Psw = NewUserPassword,
				Comment = $"New user created by {Strings.AppTitle} {App.Version3}"
			};
			var result = await Provider!.UpsertUser(user);
			var doneuser = await Provider!.ConnectUserChildCustomers(result.Entity.Id, [NewUserCustomerId!]);
			await InnerLoadNavigationTree(doneuser.Id);
		});
		if (ObsUserList != null)
		{
			await LoadUserList();
		}
	}

	void AfterNewUserValueChanged()
	{
		var list = new List<string>();
		if (NewUserCustomerId == Strings.NoSelectId) list.Add("Select customer");
		if (NewUserPassword == null) list.Add("Password required");
		if (NewUserName == null) list.Add("Enter user name");
		else if (!AppValidationRules.IsUserNameValid(NewUserName)) list.Add("Bad user name");
		else if (ObsUserPick.Any(x => string.Compare(x.Name, NewUserName, StringComparison.OrdinalIgnoreCase) == 0)) list.Add("Duplicate user name");
		NewUserErrors = list.Count > 0 ? list.ToImmutableArray() : null;
	}

	#endregion

	public void RandomPassword()
	{
		const string Chars = "23456789abcdefghjkmnpqrstuvwxyz";
		int len = Random.Shared.Next(6, 9);
		static int Rnd() => Guid.NewGuid().GetHashCode() & 0x7fffffff;
		int[] ixs = Enumerable.Range(0, len).Select(i => Rnd() % Chars.Length).ToArray();
		NewPassword = new(ixs.Select(i => Chars[i]).ToArray());
	}

	public void ChangePassword()
	{
		EditingUser!.Psw = NewPassword;
	}

	public async Task DeleteUser()
	{
		string userId = EditingUser!.Id!;
		await WrapWork($"Deleting user {EditingUser!.Name}", async () =>
		{
			// The editing user must be nulled otherwise the EntityId property of the User
			// will be set to null during the nav reload, which triggers a User update for
			// the Id that has just been deleted and it crashes. After delete the nav tree
			// doesn't know what to select do it goes back to the default (first node?).
			// The same logic is required in all places where there's an entity delete.
			EditingUser = null;
			int count = await Provider!.DeleteUser(userId);
			await InnerLoadNavigationTree(null);
			var listuser = ObsUserList?.FirstOrDefault(u => u.Id == userId);
			if (listuser != null) ObsUserList!.Remove(listuser);
		});
	}

	public async Task ConnectUserChildCustomers()
	{
		await WrapWork(Strings.BusyConnectUserCust, async () =>
		{
			string[] custids = SelectedPicks.Select(p => p.Id).ToArray();
			var upuser = await Provider!.ConnectUserChildCustomers(EditingUser!.Id, custids);
			await InnerLoadNavigationTree(upuser.Id);
		});
	}

	public async void DisconnectUserChildCustomer()
	{
		await WrapWork(Strings.BusyDisconnectUserCust, async () =>
		{
			var upuser = await Provider!.DisconnectUserChildCustomer(EditingUser!.Id, SelectedUserChildCustomer!.Id);
			await InnerLoadNavigationTree(upuser.Id);
		});
	}

	public async Task ConnectUserChildJobs()
	{
		await WrapWork(Strings.BusyConnectUserJob, async () =>
		{
			string[] jobids = SelectedPicks.Select(p => p.Id).ToArray();
			var upuser = await Provider!.ConnectUserChildJobs(EditingUser!.Id, jobids);
			await InnerLoadNavigationTree(upuser.Id);
		});
	}

	public async void DisconnectUserChildJob()
	{
		await WrapWork(Strings.BusyDisconnectUserJob, async () =>
		{
			var upuser = await Provider!.DisconnectUserChildJob(EditingUser!.Id, SelectedUserChildJob!.Id);
			await InnerLoadNavigationTree(upuser.Id);
		});
	}

	PropertyChangedEventHandler userListChangeHandler;

	public async Task LoadUserList()
	{
		await WrapWork(Strings.BusyUserList, async () =>
		{
			userListChangeHandler ??= new PropertyChangedEventHandler(TrapUserList_PropertyChanged);
			if (ObsUserList != null)
			{
				ObsUserList.ItemPropertyChanged -= userListChangeHandler;
			}
			var users = await Provider!.ListUsers();
			ObsUserList = [.. users.Select(u => new BindUser(u)).OrderBy(u => u.Name.ToUpper())];
			ObsUserList.ItemPropertyChanged += userListChangeHandler;
			ViewUserList = new ListCollectionView(ObsUserList);
			ViewUserList!.Filter = new Predicate<object>(UserListFilterProc);
		});
	}

	public void EditUser()
	{
		foreach (var n in SafeNavNodes!.Where(n => n.IsSelected)) n.IsSelected = false;
		var root = SafeNavNodes!.First(n => n.Type == NodeType.UserRoot);
		var node = root.Children!.First(n => n.User!.Id == SelectedListUsers[0].Id);
		root.IsExpanded = true;
		node.IsSelected = true;
	}

	bool UserListFilterProc(object item)
	{
		var user = (BindUser)item;
		bool namePass = FilterUserList == null || Regex.IsMatch(user.Name, Regex.Escape(FilterUserList), RegexOptions.IgnoreCase);
		bool emailPass = FilterUserList == null || Regex.IsMatch(user.Email ?? "", Regex.Escape(FilterUserList), RegexOptions.IgnoreCase);
		return namePass || emailPass;
	}

	void TrapUserList_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		var bc = (BindUser)sender!;
		EnqueueEntitySave(bc);
	}

	const string ImportUserSheetName = "Import Users";

	/// <summary>
	/// Import is quite complicated with lots of validation. Read the inline comments.
	/// </summary>
	public async Task ImportUsers(string filename, Func<int, IList<User>, bool> callback)
	{
		// Use the public utility package to magically read the excel sheets into an
		// ADO DataSet with tables, columns and rows. We expect a specific sheet name
		// to exist containing the import data. For more information see:
		// https://github.com/ExcelDataReader/ExcelDataReader

		using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using var reader = ExcelReaderFactory.CreateReader(stream);
		var config = new ExcelDataSetConfiguration()
		{
			UseColumnDataType = false,
			ConfigureDataTable = (reader) => new ExcelDataTableConfiguration()
			{
				UseHeaderRow = true
			}
		};
		var ds = reader.AsDataSet(config);
		var dt = ds.Tables.Cast<DataTable>().FirstOrDefault(t => t.TableName == ImportUserSheetName);
		if (dt == null) throw new Exception($"Import expects to find a worksheet named '{ImportUserSheetName}'");

		// Verify that every import column name has a corresponding User property.

		var uprops = typeof(User).GetProperties();
		var coldefs = dt.Columns.Cast<DataColumn>().Select(c => new { Col = c, Prop = uprops.FirstOrDefault(x => x.Name == c.ColumnName) }).ToArray();
		string[] badColNames = coldefs.Where(x => x.Prop == null).Select(x => x.Col.ColumnName).ToArray();
		if (badColNames.Length > 0) throw new Exception($"Columns found with names that do not correspond to a User property: " + string.Join(",", badColNames));

		// Verify that no forbidden properties are attempting to be imported.

		string[] ForbidNames = new string[]
		{
			nameof(User.Id), nameof(User.PassHash), nameof(User.Customers), nameof(User.Jobs)
		};
		badColNames = coldefs.Where(x => ForbidNames.Contains(x.Col.ColumnName)).Select(x => x.Col.ColumnName).ToArray();
		if (badColNames.Length > 0) throw new Exception($"Columns found which are not importable: " + string.Join(",", badColNames));

		// The import outer loop goes down the rows to create one user each.
		// The inner loop goes across the columns to validate and set the User properties.
		// If all the values are valid then a collection of users results which are candidates for import.

		var rand = new Random();
		var users = new List<User>();
		foreach (DataRow row in dt.Rows)
		{
			var user = new User();
			foreach (var coldef in coldefs)
			{
				string name = coldef.Prop!.Name;
				int ord = coldef.Col.Ordinal;
				string? rawval = row.IsNull(ord) ? null : row[ord].ToString();
				// ┌───────────────────────────────────────────────────────────────┐
				// │  Properties that are optional string arrays.                  │
				// └───────────────────────────────────────────────────────────────┘
				if (coldef.Prop.PropertyType == typeof(string[]))
				{
					if (rawval != null)
					{
						string[] parts = rawval.Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						coldef.Prop.SetValue(user, parts);
					}
				}
				// ┌───────────────────────────────────────────────────────────────┐
				// │  Properties that are dates.                                   │
				// └───────────────────────────────────────────────────────────────┘
				else if (coldef.Prop.PropertyType == typeof(DateTime) || coldef.Prop.PropertyType == typeof(DateTime?))
				{
					if (rawval != null)
					{
						if (!DateTime.TryParseExact(rawval, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var gooddate)) throw new Exception($"{name} value '{rawval}' must be in the culture neutral format YYYY-MM-DD");
						coldef.Prop.SetValue(user, gooddate);
					}
				}
				// ┌───────────────────────────────────────────────────────────────┐
				// │  Properties that are Int32.                                   │
				// └───────────────────────────────────────────────────────────────┘
				else if (coldef.Prop.PropertyType == typeof(int) || coldef.Prop.PropertyType == typeof(int?))
				{
					if (rawval != null)
					{
						if (!int.TryParse(rawval, NumberFormatInfo.InvariantInfo, out var goodnum)) throw new Exception($"{name} value '{rawval}' is not in integer");
						coldef.Prop.SetValue(user, goodnum);
					}
				}
				// ┌───────────────────────────────────────────────────────────────┐
				// │  Properties that are Guid.                                    │
				// └───────────────────────────────────────────────────────────────┘
				else if (coldef.Prop.PropertyType == typeof(Guid) || coldef.Prop.PropertyType == typeof(Guid?))
				{
					if (rawval != null)
					{
						if (!Guid.TryParse(rawval, out var gooduid)) throw new Exception($"{name} value '{rawval}' is not a Guid");
						coldef.Prop.SetValue(user, gooduid);
					}
				}
				// ┌───────────────────────────────────────────────────────────────┐
				// │  Properties that are Bool.                                    │
				// └───────────────────────────────────────────────────────────────┘
				else if (coldef.Prop.PropertyType == typeof(bool) || coldef.Prop.PropertyType == typeof(bool?))
				{
					if (rawval != null)
					{
						string[] trues = new string[] { "TRUE", "1", "Y" };
						string[] falses = new string[] { "FALSE", "0", "N" };
						string upval = rawval.ToUpperInvariant();
						if (trues.Contains(upval)) coldef.Prop.SetValue(user, true);
						else if (falses.Contains(upval)) coldef.Prop.SetValue(user, false);
						else throw new Exception($"{name} value '{rawval}' is not a boolean value");
					}
				}
				// ┌───────────────────────────────────────────────────────────────┐
				// │  Fall through to assume it's a string. If other property      │
				// │  types are added to the User class then falling through to    │
				// │  here may crash, so special case code will be neded above.    │
				// └───────────────────────────────────────────────────────────────┘
				else
				{
					if (rawval != null)
					{
						coldef.Prop.SetValue(user, rawval);
					}
				}
			}
			users.Add(user);
		}

		// Validate there is something to import.

		if (users.Count == 0) throw new Exception(Strings.UserImportNoUsers);
		if (users.Any(u => string.IsNullOrEmpty(u.Name) || string.IsNullOrEmpty(u.Psw))) throw new Exception($"All import users must provide values for the {nameof(User.Name)} and {nameof(User.Psw)} properties");

		// Validate if any import user names already exist. Prevent this now because
		// it will probably produce a runtime error because user names are unique.

		var oldrows = (await Provider!.ListUsers()).Select(u => new { u.Id, u.Name }).ToArray();
		User[] newusers = users.Where(u => !oldrows.Any(o => string.Compare(u.Name, o.Name, StringComparison.OrdinalIgnoreCase) == 0)).ToArray();
		if (newusers.Length == 0)
		{
			//string join = string.Join(" ", dupusers.Select(u => u.Name));
			string message = Strings.UserImportAllDuplicates;
			throw new Exception(message);
		}

		// Import can happen now. Many changes may result, so the
		// navigation and optional user list controls are reloaded.

		if (callback(users.Count, newusers))
		{
			await WrapWork("Importing Users", async () =>
			{
				foreach (var newuser in newusers)
				{
					var upuser = await Provider!.UpsertUser(newuser);
				}
			});
		}
		await LoadNavigation();
		if (SelectedNavNode?.Type == NodeType.UserRoot)
		{
			await LoadUserList();
		}
	}
}
