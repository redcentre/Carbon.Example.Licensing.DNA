using System.Reflection;
using System.Text.Json;
using Azure.Core;
using Orthogonal.Common.Basic;
using RCS.Azure.StorageAccount.Shared;
using RCS.DNA.Model.Extensions;
using RCS.Licensing.ClientLib;
using RCS.Licensing.Provider.Shared.Entities;

namespace RCS.DNA.Model;

public enum NewCustomerRealmType
{
	Existing,
	New
}

partial class MainController
{
	public async Task DeleteCustomer()
	{
		await WrapWork($"Deleting customer {EditingCustomer!.Name}", async () =>
		{
			string customerId = EditingCustomer!.Id!;
			EditingCustomer = null;
			int count = await Provider!.DeleteCustomer(customerId);
			await InnerLoadNavigationTree(null);
			if (ObsCloudNodes != null) await InnerCloudCompare();
			ObsCustomerList = null;
		});
	}

	public async Task ValidateCustomer()
	{
		CustomerValidateErrors = null;
		await WrapWork(Strings.ValidateCustomerBusy.Format(EditingCustomer!.Name), async () =>
		{
			CustomerValidateErrors = await Provider!.ValidateCustomer(EditingCustomer!.Id);
		});
	}

	/// <summary>
	/// Prepares all of the binding values required for the new customer dialog to work.
	/// Listing account is a slow process and theye rarely change so the results are cached for a moderate time.
	/// </summary>
	public async Task PrepareForNewCustomer()
	{
		NewCustomerBusyMessage = "Listing Azure accounts - Please wait...";
		NewCustomerErrorMessage = null;
		if (ObsRealmPick?.Any() == true)
		{
			SelectedNewCustomerExistingRealm = ObsRealmPick.First();
		}
		try
		{
			const string AccountCacheKey = "_azure-locations";
			SubscriptionAccount[] accounts;
			string json = SimpleFileCache.Get(AccountCacheKey, 30);
			if (json != null)
			{
				accounts = JsonSerializer.Deserialize<SubscriptionAccount[]>(json, JSerOpts1)!;
			}
			else
			{
				accounts = await SubUtil.ListAccounts().ToArrayAsync();
				json = JsonSerializer.Serialize(accounts, JSerOpts1);
				SimpleFileCache.Put(AccountCacheKey, json);
			}
			// Get (name,count) pairs of resource groups.
			// Unique names are put in the pick list.
			// The most common resource group is selected by default.
			var grptups = accounts.GroupBy(a => a.ResourceGroupName).Select(g => new { GrpName = g.Key, Count = g.Count() }).ToArray();
			var query1 = grptups.Select(t => t.GrpName!).Distinct().OrderBy(n => n.ToUpperInvariant()).Prepend(Strings.NoSelectText);
			NewCustomerResourceGroupNames = [.. query1];
			if (grptups.Length > 0)
			{
				NewCustomerSelectedResourceGroupName = grptups.OrderBy(t => t.Count).Last().GrpName;
			}
			else
			{
				NewCustomerSelectedResourceGroupName = query1.First();
			}
			// Extract location picks (Id,Name) from the AzureLocation class properties.
			var query2 = from p in typeof(AzureLocation).GetProperties(BindingFlags.Static | BindingFlags.Public).Where(p => p.PropertyType == typeof(AzureLocation))
						 let n = p.GetValue(null)
						 let nt = n.GetType()
						 let name = (string)nt.GetProperty(nameof(AzureLocation.Name))!.GetValue(n)!
						 let disp = (string)nt.GetProperty(nameof(AzureLocation.DisplayName))!.GetValue(n)!
						 orderby disp.ToUpperInvariant()
						 select new PickItem(name, disp);
			query2 = query2.Prepend(new PickItem(Strings.NoSelectId, Strings.NoSelectText));
			NewCustomerLocationPicks = [.. query2];
			NewCustomerSelectedLocationPick = NewCustomerLocationPicks[0];
		}
		catch (Exception ex)
		{
			NewCustomerErrorMessage = ex.Message;
		}
		finally
		{
			NewCustomerBusyMessage = null;
		}
	}

	public bool CanCreateNewCustomer()
	{
		bool goodName = LicensingUtility.IsValidAccountName(NewCustomerName);
		bool hasRG = NewCustomerSelectedResourceGroupName != Strings.NoSelectText;
		bool hasLoc = NewCustomerSelectedLocationPick?.Name != Strings.NoSelectId;
		// Mock following true if realms are not supported.
		bool hasExist = Provider?.SupportsRealms == true ? SelectedNewCustRealmType == NewCustomerRealmType.Existing && SelectedNewCustomerExistingRealm != null : true;
		bool hasNew = Provider?.SupportsRealms == true ? SelectedNewCustRealmType == NewCustomerRealmType.New && NewCustomerNewRealmName?.Length > 0 && !ObsRealmPick.Any(r => string.Compare(r.Name, NewCustomerNewRealmName, true) == 0) : true;
		return goodName && hasRG && hasLoc && (hasExist || hasNew);
	}

	/// <summary>
	/// Attempts to create a new customer. There is some validation before the creation attempt proceeds.
	/// </summary>
	/// <returns>
	/// A summary of the the Azure Storage Account that was created, or null if the creation failed, in which
	/// case the error message will visible in the dialog window.
	/// </returns>
	public async Task<SubscriptionAccount?> CreateCustomer()
	{
		SubscriptionAccount? account = null;
		NewCustomerErrorMessage = null;
		NewCustomerBusyMessage = "Creating a new customer and Azure Storage Account. This process may take a minute or more - Please wait...";
		try
		{
			bool? available = await SubUtil.IsStorageAccountNameAvailable(NewCustomerName!);
			if (available != true)
			{
				NewCustomerErrorMessage = "The customer name is not available in Azure.";
				return null;
			}
			account = await SubUtil.CreateStorageAccount(NewCustomerName!, NewCustomerSelectedResourceGroupName!, NewCustomerSelectedLocationPick!.Name, NewCustomerIsBlobsPublic);
			var cust = new Customer()
			{
				Id = null,  // This indicates a new unsaved customer
				Name = NewCustomerName,
				StorageKey = account.ConnectionString
			};
			// BUG Creating an RCS provider customer record fails because the parent AgencyId is required. This is not a problem in the Example provider.
			var result = await Provider!.UpsertCustomer(cust);
			var newcust = result.Entity;
			if (Provider.SupportsRealms == true)
			{
				// Realms are supported so the new customer must be connected to a realm.
				string realmId;
				if (SelectedNewCustRealmType == NewCustomerRealmType.New)
				{
					// A new realm is being created.
					var realm = new Realm()
					{
						Id = null,  // This indicates a new unsaved realm
						Name = NewCustomerNewRealmName!
					};
					var result2 = await Provider.UpsertRealm(realm);
					realmId = result2.Entity.Id;
				}
				else
				{
					// An existing realm was selected.
					realmId = SelectedNewCustomerExistingRealm!.Id;
				}
				// Join the customer to the new or existing realm.
				await Provider.ConnectRealmChildCustomers(realmId, [newcust.Id]);
			}
			await InnerLoadNavigationTree(newcust.Id);
			if (ObsCloudNodes != null) await InnerCloudCompare();
			ObsCustomerList = null;
			NewCustomerName = null;
			return account;
		}
		catch (Exception ex)
		{
			NewCustomerErrorMessage = ex.Message;
		}
		finally
		{
			NewCustomerBusyMessage = null;
		}
		return account;
	}

	public async Task ConnectCustomerChildUsers()
	{
		string[] userids = [.. SelectedPicks.Select(p => p.Id)];
		var upcust = await Provider!.ConnectCustomerChildUsers(EditingCustomer!.Id, userids);
		await InnerLoadNavigationTree(upcust.Id);
	}

	public async Task DisconnectCustomerChildUser()
	{
		var upcust = await Provider!.DisconnectCustomerChildUser(EditingCustomer!.Id, SelectedCustomerChildUser!.Id);
		await InnerLoadNavigationTree(upcust.Id);
	}

	PropertyChangedEventHandler custListChangeHandler;

	public async Task LoadCustomerList()
	{
		await WrapWork(Strings.LoadCustomerListBusy, async () =>
		{
			custListChangeHandler ??= new PropertyChangedEventHandler(TrapCustomerList_PropertyChanged);
			if (ObsCustomerList != null)
			{
				ObsCustomerList.ItemPropertyChanged -= custListChangeHandler;
			}
			var custs = await Provider!.ListCustomers();
			ObsCustomerList = [.. custs.Select(c => new BindCustomer(c)).OrderBy(c => c.Name.ToUpper())];
			ObsCustomerList.ItemPropertyChanged += custListChangeHandler;
		});
	}

	public void EditCustomer()
	{
		foreach (var n in SafeNavNodes!.Where(n => n.IsSelected)) n.IsSelected = false;
		var root = SafeNavNodes!.First(n => n.Type == NodeType.CustomerRoot);
		var node = root.Children!.First(n => n.Customer!.Id == SelectedListCustomers[0].Id);
		root.IsExpanded = true;
		node.IsSelected = true;
	}

	void TrapCustomerList_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		var bc = (BindCustomer)sender!;
		EnqueueEntitySave(bc);
	}
}
