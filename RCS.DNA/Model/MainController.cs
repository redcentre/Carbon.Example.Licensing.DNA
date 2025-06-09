using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Orthogonal.NSettings;
using RCS.Azure.StorageAccount;
using RCS.Licensing.Example.Provider;
using RCS.Licensing.Provider;
using RCS.Licensing.Provider.Shared;
using RCS.Licensing.Provider.Shared.Entities;

namespace RCS.DNA.Model;

#pragma warning disable CS8618      // Non-nullable field must contain a non-null value when exiting constructor.                                                                                   

sealed partial class MainController : ObservableObject
{
	public ISettingsProcessor Settings { get; private set; }

	DispatcherTimer navtimer;
	readonly DirectoryInfo appdir = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), App.Company));
	public Action<string>? WarningCallback { get; set; }

	#region Lifetime

	public MainController()
	{
		Settings = new RegistrySettings();
		Metrics = new AppMetrics();
		Log("Controller ctor");
	}

	public void Startup()
	{
		editingChangeHandler = new PropertyChangedEventHandler(EditingEntity_PropertyChanged);
		saveQueue = [];
		ProviderIconSource = Images.Key16;
		ObsCustomerPick = [];
		ObsJobPick = [];
		ObsUserPick = [];
		UploadParallelMax = Environment.ProcessorCount;
		UploadParallelLimit = Math.Min(4, Environment.ProcessorCount);
		if (!appdir.Exists)
		{
			appdir.Create();
		}
		navtimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
		navtimer.Tick += Navtimer_Tick;
		MigrateSettings();
		Metrics.Load(Settings);
		LoadProfiles();
	}

	/// <summary>
	/// Running this app V2+ will cause a manual migration of the V1+ settings if necessary.
	/// </summary>
	void MigrateSettings()
	{
		bool? b = Settings.GetBool(null, nameof(Metrics.AutoConnect));
		if (b != null) return;
		using (var key = Registry.CurrentUser.OpenSubKey($"Software\\{App.Company}\\{App.Product} 1.2", false))
		{
			if (key == null) return;
		}
		// A migration is required.
		var oldset = new RegistrySettings(App.Company, App.Product, "1.2");
		// Migrate the non-profile settings.
		b = oldset.GetBool(null, nameof(Metrics.AutoConnect));
		Settings.Put(null, nameof(Metrics.AutoConnect), b);
		string? s = oldset.Get(null, nameof(DownloadDestinationDir));
		Settings.Put(null, nameof(DownloadDestinationDir), s);
		s = oldset.Get(null, "ExpandedNavUids");
		Settings.Put(null, "ExpandedNavUids", s);
		int? i = oldset.GetInt(null, nameof(Metrics.MaxListBlobs));
		Settings.Put(null, nameof(Metrics.MaxListBlobs), i);
		i = oldset.GetInt(null, nameof(Metrics.MaxLogItems));
		Settings.Put(null, nameof(Metrics.MaxLogItems), i);
		double? d = oldset.GetDouble(null, nameof(Metrics.SaveDelaySeconds));
		Settings.Put(null, nameof(Metrics.SaveDelaySeconds), d);
		s = oldset.Get(null, nameof(Metrics.UploadDirNames));
		Settings.Put(null, nameof(Metrics.UploadDirNames), s);
		s = oldset.Get(null, nameof(UploadSourceDir));
		Settings.Put(null, nameof(UploadSourceDir), s);
		s = oldset.Get(null, nameof(WindowStartupLocation));
		Settings.Put(null, nameof(WindowStartupLocation), s);
		// Migrate the old provider settings into profiles.
		const string KeySql = "Profile_0";
		const string KeyRCS = "Profile_1";
		string? ado = oldset.Get(nameof(ExampleLicensingProvider), "adoConnectionString");
		string? prodkey = oldset.Get(nameof(ExampleLicensingProvider), "productKey");
		if (ado != null)
		{
			Settings.Put(KeySql, nameof(AppProfile.Name), "SQL Server Database");
			Settings.Put(KeySql, nameof(AppProfile.CreatedUtc), DateTime.UtcNow);
			Settings.Put(KeySql, nameof(AppProfile.SqlAdoConnect), ado);
			Settings.Put(KeySql, nameof(AppProfile.SqlProductKey), prodkey);
		}
		string? apikey = oldset.Get(nameof(RedCentreLicensingProvider), "apikey");
		string? licaddr = oldset.Get(nameof(RedCentreLicensingProvider), "licensingBaseAddress");
		string? oldtimeout = oldset.Get(nameof(RedCentreLicensingProvider), "timeout");
		int timeout = oldtimeout == null ? 30 : int.TryParse(oldtimeout, out int t) ? t : 30;
		if (apikey != null)
		{
			Settings.Put(KeyRCS, nameof(AppProfile.Name), "RCS Web Service");
			Settings.Put(KeyRCS, nameof(AppProfile.CreatedUtc), DateTime.UtcNow);
			Settings.Put(KeyRCS, nameof(AppProfile.RcsApiKey), apikey);
			Settings.Put(KeyRCS, nameof(AppProfile.RcsServiceBaseAddress), licaddr);
			Settings.Put(KeyRCS, nameof(AppProfile.RcsServiceTimeout), timeout);
		}
		// Migrate the provider specific settings last used.
		var lastprov = oldset.Get(null, "LastProvider");
		string? provkey = lastprov == nameof(ExampleLicensingProvider) ? KeySql : lastprov == nameof(RedCentreLicensingProvider) ? KeyRCS : null;
		if (provkey != null)
		{
			Settings.Put(provkey, provkey == KeySql ? nameof(AppProfile.SqlProviderActive) : nameof(AppProfile.RCSProviderActive), true);
			s = oldset.Get(null, "UserId");
			Settings.Put(provkey, nameof(AppProfile.LoginId), s);
			byte[] buff1 = oldset.GetBuffer(null, "Password", null);
			if (buff1 != null)
			{
				try
				{
					byte[] buff2 = ProtectedData.Unprotect(buff1, null, DataProtectionScope.CurrentUser);
					s = Encoding.UTF8.GetString(buff2);
					Settings.Put(provkey, nameof(AppProfile.Password), s);
				}
				catch (Exception ex)
				{
					Log(ex.Message);
				}
			}
			s = oldset.Get(null, "CarbonLoginNameOrId");
			Settings.Put(provkey, nameof(AppProfile.CarbonLoginNameOrId), s);
			s = oldset.Get(null, "CarbonLoginPassword");
			Settings.Put(provkey, nameof(AppProfile.CarbonLoginPassword), s);
			var seq = oldset.Get(null, "LoginSequence", AuthenticateSequence.IdThenName);
			Settings.Put(provkey, nameof(AppProfile.CarbonLoginSequence), seq);
		}
	}

	/// <summary>
	/// Profiles are stored in settings under group names Profile_n (n incremements for each new profile).
	/// </summary>
	void LoadProfiles()
	{
		ObsProfiles = [];
		string[] profileKeys = [.. Settings.ListGroups().Where(g => RegGroupSeq().IsMatch(g ?? ""))];
		foreach (string profileKey in profileKeys)
		{
			var prof = new AppProfile(profileKey)
			{
				Name = Settings.Get(profileKey, nameof(AppProfile.Name)),
				UserNameIsEmail = Settings.GetBool(profileKey, nameof(AppProfile.UserNameIsEmail)) ?? false,
				CreatedUtc = Settings.GetDateTime(profileKey, nameof(AppProfile.CreatedUtc), DateTime.UtcNow),
				LastUpdateUtc = Settings.GetDateTime(profileKey, nameof(AppProfile.LastUpdateUtc)),
				LastConnectUtc = Settings.GetDateTime(profileKey, nameof(AppProfile.LastConnectUtc)),
				ConnectCount = Settings.GetInt(profileKey, nameof(AppProfile.ConnectCount), 0),
				LoginId = Settings.Get(profileKey, nameof(AppProfile.LoginId)),
				Password = Settings.Get(profileKey, nameof(AppProfile.Password)),
				CarbonLoginNameOrId = Settings.Get(profileKey, nameof(AppProfile.CarbonLoginNameOrId)),
				CarbonLoginPassword = Settings.Get(profileKey, nameof(AppProfile.CarbonLoginPassword)),
				CarbonLoginSequence = Settings.Get(profileKey, nameof(AppProfile.CarbonLoginSequence), AuthenticateSequence.NameThenId),
				CarbonServiceApiKey = Settings.Get(profileKey, nameof(AppProfile.CarbonServiceApiKey)),
				SqlProviderActive = Settings.GetBool(profileKey, nameof(AppProfile.SqlProviderActive)) ?? false,
				SqlAdoConnect = Settings.Get(profileKey, nameof(AppProfile.SqlAdoConnect)),
				SqlProductKey = Settings.Get(profileKey, nameof(AppProfile.SqlProductKey)),
				RCSProviderActive = Settings.GetBool(profileKey, nameof(AppProfile.RCSProviderActive)) ?? false,
				RcsApiKey = Settings.Get(profileKey, nameof(AppProfile.RcsApiKey)),
				RcsServiceTimeout = Settings.GetInt(profileKey, nameof(AppProfile.RcsServiceTimeout)) ?? 30,
				RcsServiceBaseAddress = Settings.Get(profileKey, nameof(AppProfile.RcsServiceBaseAddress)),
				SubscriptionId = Settings.Get(profileKey, nameof(AppProfile.SubscriptionId)),
				TenantId = Settings.Get(profileKey, nameof(AppProfile.TenantId)),
				ApplicationId = Settings.Get(profileKey, nameof(AppProfile.ApplicationId)),
				ClientSecret = Settings.Get(profileKey, nameof(AppProfile.ClientSecret))
			};
			// Special case for the Uri collection.
			string[] uris = Settings.GetStrings(profileKey, nameof(AppProfile.CarbonServiceBaseUris), []);
			foreach (string uri in uris)
			{
				prof.CarbonServiceBaseUris.Add(uri);
			}
			ObsProfiles!.Add(prof);
		}
		ObsProfiles!.ItemPropertyChanged += (s, e) =>
		{
			// Trap property changes to the selected profile and save the changed
			// property value in the settings group for the profile.
			string[] SkipProps =
			[
				nameof(AppProfile.LastUpdateUtc), nameof(AppProfile.LastConnectUtc),
				nameof(AppProfile.ConnectCount), nameof(AppProfile.CanConnect),
				nameof(AppProfile.HasAzure)
			];
			if (SkipProps.Contains(e.PropertyName)) return;
			var prof = (AppProfile)s!;
			object? val = prof.GetType().GetProperty(e.PropertyName!)!.GetValue(prof);
			if (val is ICollection<string> ss)
			{
				// Special case - string collection.
				// QUESTION When the profile Uri collection changes, a set of N change events happen, one for each item in the new collection. Inefficient, but not a serious problem.
				Settings.Put(prof.ProfileKey, e.PropertyName, ss.ToArray());
			}
			else
			{
				// Default case - hopefully a simple supported settings type.
				Settings.Put(prof.ProfileKey, e.PropertyName, val);
			}
			// Needs manual update and save.
			prof.LastUpdateUtc = DateTime.UtcNow;
			Settings.Put(prof.ProfileKey, nameof(AppProfile.LastUpdateUtc), prof.LastUpdateUtc);
			// Any previous login error message is erased.
			IsConnectError = false;
			ConnectMessage = null;
		};
		string lastgroup = Settings.Get(null, nameof(AppProfile.ProfileKey), null);
		SelectedProfile = ObsProfiles!.FirstOrDefault(p => p.ProfileKey == lastgroup) ?? ObsProfiles.FirstOrDefault();
	}

	public void Shutdown()
	{
		navtimer.Stop();
	}

	public void ShowAlert(string title, string message)
	{
		AlertTitle = title;
		AlertMessage = message;
	}

	[RelayCommand]
	void CloseAlert() => AlertTitle = AlertMessage = null;

	#endregion

	#region Metrics

	public void BeginMetricsEdit() => Metrics.BeginEdit();

	public void CancelMetricsEdit() => Metrics.CancelEdit();

	public void EndMetricsEdit()
	{
		Metrics.EndEdit();
		Metrics.LastSaveTime = DateTime.Now;
		Metrics.Save(Settings);
	}

	#endregion

	#region Time and Saving

	// NOTE -- When performing any of the 1+1+2 pairs of connect and disconnect, the editing entity's
	// collection is updated, but we are not interested in a change event for the collections.
	// By fabulous luck, any collection change has a matching change to an array of connected names
	// for the collection in an entity property, so an edit and save will be queued. The following flag
	// can be set true before any save queue add to force a navigation refresh after the entity save.

	bool isRefreshPending = false;

	/// <summary>
	/// Any change to an editable property of an editable entity will be trapped here.
	/// The entity is added to a save pending collection if it's not already present.
	/// Changes in the editing entity must be pushed into the corresponding row in a
	/// list grid if one is loaded.
	/// </summary>
	void EditingEntity_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		var entity = (IBindEntity)sender!;
		if (!entity.IsValid)
		{
			Log($"Skip entity save -> {entity}");
			return;
		}
		EnqueueEntitySave(entity);
	}

	sealed class SaveItem(DateTime saveDueTime, IBindEntity entity)
	{
		public DateTime SaveDueTime { get; set; } = saveDueTime;
		public IBindEntity Entity { get; } = entity;
		public override string ToString() => $"SaveItem({SaveDueTime:T},{Entity})";
	}

	Dictionary<int, SaveItem> saveQueue;
	PropertyChangedEventHandler editingChangeHandler;

	bool _isSaving;
	public bool IsSaving
	{
		get => _isSaving;
		set
		{
			if (_isSaving != value)
			{
				_isSaving = value;
				OnPropertyChanged(nameof(IsSaving));
			}
		}
	}

	public void TimerTick()
	{
		StatusTime = DateTime.Now.ToString("HH:mm:ss");
		string join = string.Join(" + ", saveQueue.Select(q => $"({q.Key:X8},{q.Value.Entity.Id},{q.Value.Entity.Name})"));
		if (join.Length > 0) Log($"Save tick {DateTime.Now:T} {join}");
		_ = PollSaveQueue();
	}

	void EnqueueEntitySave(IBindEntity entity)
	{
		int key = entity.GetHashCode();
		var duetime = DateTime.UtcNow.AddSeconds(Metrics.SaveDelaySeconds);
		if (saveQueue.TryGetValue(key, out var saveItem))
		{
			saveItem.SaveDueTime = duetime;
		}
		else
		{
			saveItem = new SaveItem(duetime, entity);
			saveQueue.Add(key, saveItem);
		}
	}

	async Task PollSaveQueue()
	{
		if (IsSaving) return;
		async Task RefreshCheck()
		{
			if (isRefreshPending)
			{
				await InnerLoadNavigationTree(null);
				if (ObsCloudNodes != null)
				{
					await InnerCloudCompare();
				}
				if (ReportItems != null)
				{
					await RunReport();
				}
				isRefreshPending = false;
			}
		}
		foreach (var kvp in saveQueue)
		{
			if (kvp.Value.SaveDueTime < DateTime.UtcNow)
			{
				// ┌───────────────────────────────────────────────────────────────┐
				// │ A bindable entity is due for save. It's a bit ugly, but each  │
				// │ entity type has to be cracked and treated separately because  │
				// │ there is different post-save processing for each type.        │
				// │ The job post-save processing is particularly tricky, so take  │
				// │ care when modifying that block of code.                       │
				// └───────────────────────────────────────────────────────────────┘
				IsSaving = true;
				if (kvp.Value.Entity.Type == EntityType.Customer)
				{
					await WrapWork(Strings.UpdateCustomerBusy, async () =>
					{
						BindCustomer bindcust = (BindCustomer)kvp.Value.Entity;
						Customer cust = bindcust.GetEntity();
						Log($"Save {kvp.Key:X8} {cust}");
						var resultc = await Provider!.UpsertCustomer(cust);
						var savecust = resultc.Entity;
						// Update matching navigation cust nodes.
						foreach (var node in SafeNavNodes!.Where(n => n.Type == NodeType.Customer && n.Id == savecust.Id).ToArray())
						{
							var ncust = node.Customer!;
							node.Label = ncust.Name = savecust.Name;
							node.Inactive = ncust.Inactive = savecust.Inactive;
						}
						// Update any matching row in a loaded customer list
						var row = ObsCustomerList?.FirstOrDefault(c => c.Id == savecust.Id);
						row?.CopyPropsFrom(new BindCustomer(savecust));
						bindcust.DataLocation = savecust.DataLocation;  // SPECIAL CASE: Cannot save null.
						await RefreshCheck();
					});
				}
				else if (kvp.Value.Entity.Type == EntityType.Job)
				{
					await WrapWork(Strings.UpdateJobBusy, async () =>
					{
						BindJob bindjob = (BindJob)kvp.Value.Entity;
						Job job = bindjob.GetEntity();
						Log($"Save {kvp.Key:X8} {job}");
						var resultj = await Provider!.UpsertJob(job);
						var savejob = resultj.Entity;
						// Update matching navigation job nodes and their data.
						var jnodetups = SafeNavNodes!.Where(n => n.Type == NodeType.Job && n.Id == savejob.Id).Select(n => new { JNode = n, NJob = n.Job! }).ToArray();
						foreach (var jtup in jnodetups)
						{
							jtup.JNode.Label = jtup.NJob.Name = savejob.Name!;
							jtup.JNode.Inactive = jtup.NJob.Inactive = savejob.Inactive;
							jtup.NJob.CustomerId = savejob.CustomerId;
						}
						// Update any matching row in a loaded job list
						var row = ObsJobList?.FirstOrDefault(j => j.Id == savejob.Id);
						row?.CopyPropsFrom(new BindJob(savejob));
						bindjob.DataLocation = savejob.DataLocation;  // SPECIAL CASE: Cannot save null.
						if (EditingJob != null)
						{
							EditingJob.LastUpdate = savejob.LastUpdate;
						}
						await RefreshCheck();
					});
				}
				else if (kvp.Value.Entity.Type == EntityType.User)
				{
					await WrapWork(Strings.UpdateUserBusy, async () =>
					{
						var user = ((BindUser)kvp.Value.Entity).GetEntity();
						var resultu = await Provider!.UpsertUser(user);
						var saveuser = resultu.Entity;
						// Update matching navigation user nodes.
						foreach (var node in SafeNavNodes!.Where(n => n.Type == NodeType.User && n.Id == saveuser.Id))
						{
							var nuser = node.User!;
							node.Label = nuser.Name = saveuser.Name!;
							node.Inactive = nuser.IsDisabled = saveuser.IsDisabled;
						}
						// Update any matching row in a loaded user list
						var row = ObsUserList?.FirstOrDefault(u => u.Id == saveuser.Id);
						row?.CopyPropsFrom(new BindUser(saveuser));
						if (EditingUser?.Id == saveuser.Id)
						{
							EditingUser.PassHash = saveuser.PassHash;
						}
						await RefreshCheck();
					});
				}
				else if (kvp.Value.Entity.Type == EntityType.Realm)
				{
					await WrapWork(Strings.UpdateRealmBusy, async () =>
					{
						var realm = ((BindRealm)kvp.Value.Entity).GetEntity();
						Log($"Save {kvp.Key:X8} {realm}");
						var resultr = await Provider!.UpsertRealm(realm);
						var saverealm = resultr.Entity;
						// Update matching navigation realm nodes.
						foreach (var node in SafeNavNodes!.Where(n => n.Type == NodeType.Realm && n.Id == saverealm.Id))
						{
							var nrealm = node.Realm!;
							node.Label = nrealm.Name = saverealm.Name!;
							node.Inactive = nrealm.IsInactive = saverealm.Inactive;
						}
						// Update any matching row in a loaded realm list
						var row = ObsRealmList?.FirstOrDefault(r => r.Id == saverealm.Id);
						row?.CopyPropsFrom(new BindRealm(saverealm));
						await RefreshCheck();
					});
				}
				saveQueue.Remove(kvp.Key);
				IsSaving = false;
			}
		}
	}

	#endregion

	#region Private

	static void Log(string message) => Trace.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{Environment.CurrentManagedThreadId}] {message}");

	void AfterTabIndexChanged()
	{
		Log("");
		if (TabIndex == 1 && ObsCloudNodes == null)
		{
			_ = CloudCompare();
		}
		if (TabIndex == 2 && ReportItems == null)
		{
			_ = RunReport();
			Dispatcher.CurrentDispatcher.Invoke(new Action(async () => await RunReport()));
		}
	}

	void AfterSelectedProfileChanged()
	{
		Settings.Put(null, nameof(AppProfile.ProfileKey), SelectedProfile?.ProfileKey);
	}

	void AfterPickItemsChanged()
	{
		PickFilter = null;
		PickView = new ListCollectionView(PickItems)
		{
			Filter = new Predicate<object>(PickFilterProc)
		};
	}

	bool PickFilterProc(object value)
	{
		var item = (EntityPickItem)value;
		return PickFilter == null || Regex.IsMatch(item.Name, Regex.Escape(PickFilter), RegexOptions.IgnoreCase);
	}

	public string?[] DataLocationPicks =>
	[
		string.Empty,
		DataLocationType.Server.ToString(),
		DataLocationType.Cloud.ToString(),
		DataLocationType.Desktop.ToString()
	];

	async Task WrapWork(string title, Func<Task> worker, Action<Exception>? errorCallback = null, Action? finallyCallback = null)
	{
		using var busy = ShowBusy(title);
		try
		{
			await worker();
		}
		catch (Exception ex)
		{
			ShowAlert(title, ex.Message);
			errorCallback?.Invoke(ex);
		}
		finally
		{
			finallyCallback?.Invoke();
		}
	}

	public void ShellRun(string filename)
	{
		try
		{
			Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
		}
		catch (Exception ex)
		{
			ShowAlert("Failed to launch", ex.Message);
		}
	}

	BusyHandle ShowBusy(string message) => new(message, this);

	sealed class BusyHandle : IDisposable
	{
		readonly MainController _controller;
		public BusyHandle(string message, MainController controller)
		{
			controller.BusyMessage = message + "\u2026";
			_controller = controller;
		}
		public void Dispose()
		{
			_controller.BusyMessage = null;
		}
	}

	SubscriptionUtility _subutil;
	SubscriptionUtility SubUtil => _subutil ??= new SubscriptionUtility(SelectedProfile!.SubscriptionId!, SelectedProfile.TenantId!, SelectedProfile.ApplicationId!, SelectedProfile.ClientSecret!);

	[GeneratedRegex(@"^Profile_(\d+)$")]
	private static partial Regex RegGroupSeq();

	#endregion
}
