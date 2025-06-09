using Microsoft.Win32;
using RCS.Azure.StorageAccount.Shared;
using RCS.Carbon.Example.WebService.Common.DTO;
using RCS.DNA.Model;
using RCS.DNA.Model.Extensions;
using RCS.Licensing.Provider.Shared;

namespace RCS.DNA;

internal static partial class MainCommands
{
	// The following command list must be manually synchronised with the MainWindow XAML file.

	public static RoutedUICommand HelpAbout = new("Help About", "HelpAbout", typeof(Window));
	public static RoutedUICommand EditMetrics = new("EditMetrics", "EditMetrics", typeof(Window));

	public static RoutedUICommand LaunchConnectPrompt = new("Launch Connect Prompt", "LaunchConnectPrompt", typeof(Window));
	public static RoutedUICommand Connect = new("Connect", "Connect", typeof(Window));
	public static RoutedUICommand Disconnect = new("Disconnect", "Disconnect", typeof(Window));
	public static RoutedUICommand DeleteProfile = new("Delete Profile", "DeleteProfile", typeof(Window));

	public static RoutedUICommand NewCustomer = new("New Customer", "NewCustomer", typeof(Window));
	public static RoutedUICommand CreateCustomer = new("Create Customer", "CreateCustomer", typeof(Window));
	public static RoutedUICommand DeleteCustomer = new("Delete Customer", "DeleteCustomer", typeof(Window));
	public static RoutedUICommand ValidateCustomer = new("Validate Customer", "ValidateCustomer", typeof(Window));
	public static RoutedUICommand LoadCustomerList = new("Load Customer List", "LoadCustomerList", typeof(Window));
	public static RoutedUICommand EditCustomer = new("Edit Customer", "EditCustomer", typeof(Window));

	public static RoutedUICommand NewJob = new("New Job", "NewJob", typeof(Window));
	public static RoutedUICommand CreateJob = new("Create Job", "CreateJob", typeof(Window));
	public static RoutedUICommand MaterialiseJob = new("Materialise Job", "MaterialiseJob", typeof(Window));
	public static RoutedUICommand DeleteJob = new("Delete Job", "DeleteJob", typeof(Window));
	public static RoutedUICommand ValidateJob = new("Validate Job", "ValidateJob", typeof(Window));
	public static RoutedUICommand LoadJobList = new("Load Job List", "LoadJobList", typeof(Window));
	public static RoutedUICommand UploadJob = new("Upload Job", "UploadJob", typeof(Window));
	public static RoutedUICommand CancelUploadJob = new("CancelUpload Job", "CancelUploadJob", typeof(Window));
	public static RoutedUICommand DownloadJob = new("Download Job", "DownloadJob", typeof(Window));
	public static RoutedUICommand CancelDownloadJob = new("CancelDownload Job", "CancelDownloadJob", typeof(Window));
	public static RoutedUICommand EditJob = new("Edit Job", "EditJob", typeof(Window));

	public static RoutedUICommand NewUser = new("New User", "NewUser", typeof(Window));
	public static RoutedUICommand CreateUser = new("Create User", "CreateUser", typeof(Window));
	public static RoutedUICommand DeleteUser = new("Delete User", "DeleteUser", typeof(Window));
	public static RoutedUICommand LoadUserList = new("Load User List", "LoadUserList", typeof(Window));
	public static RoutedUICommand ImportUsers = new("Import Users", "ImportUsers", typeof(Window));
	public static RoutedUICommand EditUser = new("Edit User", "EditUser", typeof(Window));

	public static RoutedUICommand DisconnectCustomerChildUser = new("Disconnect Customer Child User", "DisconnectCustomerChildUser", typeof(Window));
	public static RoutedUICommand ConnectCustomerChildUsers = new("Connect Customer Child User", "ConnectCustomerChildUser", typeof(Window));
	public static RoutedUICommand DisconnectJobChildUser = new("Disconnect Job Child User", "DisconnectJobChildUser", typeof(Window));
	public static RoutedUICommand ConnectJobChildUsers = new("Connect Job Child Users", "ConnectJobChildUsers", typeof(Window));
	public static RoutedUICommand DisconnectUserChildCustomer = new("Disconnect User Child Customer", "DisconnectUserChildCustomer", typeof(Window));
	public static RoutedUICommand ConnectUserChildCustomers = new("Connect User Child Customers", "ConnectUserChildCustomers", typeof(Window));
	public static RoutedUICommand DisconnectUserChildJob = new("Disconnect User Child Job", "DisconnectUserChildJob", typeof(Window));
	public static RoutedUICommand ConnectUserChildJobs = new("Connect User Child Jobs", "ConnectUserChildJobs", typeof(Window));

	public static RoutedUICommand NewRealm = new("New Realm", "NewRealm", typeof(Window));
	public static RoutedUICommand CreateRealm = new("Create Realm", "CreateRealm", typeof(Window));
	public static RoutedUICommand DeleteRealm = new("Delete Realm", "DeleteRealm", typeof(Window));
	public static RoutedUICommand LoadRealmList = new("Load Realm List", "LoadRealmList", typeof(Window));
	public static RoutedUICommand RealmPolicyEdit = new("Realm Policy Edit", "RealmPolicyEdit", typeof(Window));
	public static RoutedUICommand DisconnectRealmChildUser = new("Disconnect Realm Child User", "DisconnectRealmChildUser", typeof(Window));
	public static RoutedUICommand ConnectRealmChildUsers = new("Connect Realm Child Users", "ConnectRealmChildUsers", typeof(Window));
	public static RoutedUICommand DisconnectRealmChildCustomer = new("Disconnect Realm Child Customer", "DisconnectRealmChildCustomer", typeof(Window));
	public static RoutedUICommand ConnectRealmChildCustomers = new("Connect Realm Child Customers", "ConnectRealmChildCustomers", typeof(Window));
	public static RoutedUICommand EditRealm = new("Edit Realm", "EditRealm", typeof(Window));

	public static RoutedUICommand LaunchPasswordChange = new("Launch Password Change", "LaunchPasswordChange", typeof(Window));
	public static RoutedUICommand RandomPassword = new("Random Password", "RandomPassword", typeof(Window));
	public static RoutedUICommand ChangePassword = new("Change Password", "ChangePassword", typeof(Window));
}

partial class MainWindow
{
	void HelpAboutCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
	void HelpAboutExecute(object sender, ExecutedRoutedEventArgs e) => PromptHelpAbout();

	void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
	void CloseExecute(object sender, ExecutedRoutedEventArgs e) => Close();

	void EditMetricsCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
	void EditMetricsExecute(object sender, ExecutedRoutedEventArgs e) => PromptEditMetrics();

	void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
	void HelpExecute(object sender, ExecutedRoutedEventArgs e) => Controller.ShellRun("https://github.com/redcentre/Carbon.Example.Licensing.DNA");

	void LaunchConnectPromptCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.AuthResponse == null;
	void LaunchConnectPromptExecute(object sender, ExecutedRoutedEventArgs e) => PromptConnect();

	void ConnectCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.SelectedProfile?.CanConnect == true && !Controller.IsConnecting;
	async void ConnectExecute(object sender, ExecutedRoutedEventArgs e) => await TrapConnect((Window)e.Parameter);

	void DisconnectCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.AuthResponse != null;
	async void DisconnectExecute(object sender, ExecutedRoutedEventArgs e)
	{
		await Controller.Disconnect();
		PromptConnect();
	}

	void DeleteProfileCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.SelectedProfile != null;
	void DeleteProfileExecute(object sender, ExecutedRoutedEventArgs e) => PromptDeleteProfile();

	void NewCustomerCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null;
	void NewCustomerExecute(object sender, ExecutedRoutedEventArgs e) => PromptNewCustomer();

	void CreateCustomerCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.CanCreateNewCustomer();
	async void CreateCustomerExecute(object sender, ExecutedRoutedEventArgs e) => await CreateCustomerWrap();

	void DeleteCustomerCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.SelectedNavNode?.Type == NodeType.Customer && Controller.EditingCustomer != null;
	async void DeleteCustomerExecute(object sender, ExecutedRoutedEventArgs e) => await PromptDeleteCustomer();

	void ValidateCustomerCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.SelectedNavNode?.Type == NodeType.Customer && Controller.EditingCustomer != null && !Controller.EditingCustomer.IsNameError && !Controller.EditingCustomer.IsStorkeyError;
	async void ValidateCustomerExecute(object sender, ExecutedRoutedEventArgs e) => await PromptValidateCustomer();

	void LoadCustomerListCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null && Controller.SelectedNavNode?.Type == NodeType.CustomerRoot;
	async void LoadCustomerListExecute(object sender, ExecutedRoutedEventArgs e) => await Controller.LoadCustomerList();

	void EditCustomerCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.SelectedListCustomers.Length == 1;
	void EditCustomerExecute(object sender, ExecutedRoutedEventArgs e) => Controller.EditCustomer();

	void NewJobCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null;
	async void NewJobExecute(object sender, ExecutedRoutedEventArgs e) => await PromptNewJob();

	void CreateJobCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null && Controller.NewJobErrors == null;
	async void CreateJobExecute(object sender, ExecutedRoutedEventArgs e) => await Controller.CreateJob();

	void MaterialiseJobCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null && Controller.SelectedCloudNode?.Type == NodeType.Job && Controller.SelectedCloudNode.Job?.CompareState == JobState.OrphanContainer;
	async void MaterialiseJobExecute(object sender, ExecutedRoutedEventArgs e) => await PromptMaterialiseJob();

	void DeleteJobCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.SelectedNavNode?.Type == NodeType.Job && Controller.EditingJob != null;
	async void DeleteJobExecute(object sender, ExecutedRoutedEventArgs e) => await PromptDeleteJob();

	void ValidateJobCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.SelectedNavNode?.Type == NodeType.Job && Controller.EditingJob != null && !Controller.EditingJob.IsNameError && Controller.EditingJob.CustomerId != null;
	async void ValidateJobExecute(object sender, ExecutedRoutedEventArgs e) => await PromptValidateJob();

	void LoadJobListCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null && Controller.SelectedNavNode?.Type == NodeType.JobRoot;
	async void LoadJobListExecute(object sender, ExecutedRoutedEventArgs e) => await Controller.LoadJobList();

	void UploadJobCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null && Controller.ObsJobPick.Count > 0;
	void UploadJobExecute(object sender, ExecutedRoutedEventArgs e) => PromptUploadJob();

	void CancelUploadJobCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.IsUploading;
	void CancelUploadJobExecute(object sender, ExecutedRoutedEventArgs e) => Controller.CancelUploadJob();

	void DownloadJobCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null && Controller.ObsJobPick.Count > 0;
	void DownloadJobExecute(object sender, ExecutedRoutedEventArgs e) => PromptDownloadJob();

	void CancelDownloadJobCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.IsDownloading;
	void CancelDownloadJobExecute(object sender, ExecutedRoutedEventArgs e) => Controller.CancelDownloadJob();

	void EditJobCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.SelectedListJobs.Length == 1;
	void EditJobExecute(object sender, ExecutedRoutedEventArgs e) => Controller.EditJob();

	void NewUserCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null;
	async void NewUserExecute(object sender, ExecutedRoutedEventArgs e) => await PromptNewUser();

	void CreateUserCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null && Controller.NewUserErrors == null;
	async void CreateUserExecute(object sender, ExecutedRoutedEventArgs e) => await Controller.CreateUser();

	void DeleteUserCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.SelectedNavNode?.Type == NodeType.User && Controller.EditingUser != null;
	async void DeleteUserExecute(object sender, ExecutedRoutedEventArgs e) => await PromptDeleteUser();

	void LoadUserListCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null && Controller.SelectedNavNode?.Type == NodeType.UserRoot;
	async void LoadUserListExecute(object sender, ExecutedRoutedEventArgs e) => await Controller.LoadUserList();

	void ImportUsersCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null;
	async void ImportUsersExecute(object sender, ExecutedRoutedEventArgs e) => await PromptImportUsers();

	void EditUserCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.SelectedListUsers.Length == 1;
	void EditUserExecute(object sender, ExecutedRoutedEventArgs e) => Controller.EditUser();

	void ConnectCustomerChildUsersCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.EditingCustomer != null;
	async void ConnectCustomerChildUsersExecute(object sender, ExecutedRoutedEventArgs e) => await PromptConnectCustomerChildUsers();

	void DisconnectCustomerChildUserCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.EditingCustomer != null && Controller.SelectedCustomerChildUser != null;
	async void DisconnectCustomerChildUserExecute(object sender, ExecutedRoutedEventArgs e) => await Controller.DisconnectCustomerChildUser();

	void ConnectJobChildUsersCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.EditingJob != null;
	async void ConnectJobChildUsersExecute(object sender, ExecutedRoutedEventArgs e) => await PromptConnectJobChildUsers();

	void DisconnectJobChildUserCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.EditingJob != null && Controller.SelectedJobChildUser != null;
	async void DisconnectJobChildUserExecute(object sender, ExecutedRoutedEventArgs e) => await PromptDisconnectJobChildUser();

	void ConnectUserChildCustomersCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.EditingUser != null;
	async void ConnectUserChildCustomersExecute(object sender, ExecutedRoutedEventArgs e) => await PromptConnectUserChildCustomers();

	void DisconnectUserChildCustomerCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.EditingUser != null && Controller.SelectedUserChildCustomer != null;
	void DisconnectUserChildCustomerExecute(object sender, ExecutedRoutedEventArgs e) => Controller.DisconnectUserChildCustomer();

	void ConnectUserChildJobsCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.EditingUser != null;
	async void ConnectUserChildJobsExecute(object sender, ExecutedRoutedEventArgs e) => await PromptConnectUserChildJobs();

	void DisconnectUserChildJobCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.EditingUser != null && Controller.SelectedUserChildJob != null;
	void DisconnectUserChildJobExecute(object sender, ExecutedRoutedEventArgs e) => Controller.DisconnectUserChildJob();

	void NewRealmCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null;
	async void NewRealmExecute(object sender, ExecutedRoutedEventArgs e) => await PromptNewRealm();

	void CreateRealmCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null && Controller.NewRealmErrors == null;
	async void CreateRealmExecute(object sender, ExecutedRoutedEventArgs e) => await Controller.CreateRealm();

	void DeleteRealmCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.SelectedNavNode?.Type == NodeType.Realm && Controller.EditingRealm != null;
	async void DeleteRealmExecute(object sender, ExecutedRoutedEventArgs e) => await PromptDeleteRealm();

	void LoadRealmListCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null && Controller.SelectedNavNode?.Type == NodeType.RealmRoot;
	async void LoadRealmListExecute(object sender, ExecutedRoutedEventArgs e) => await Controller.LoadRealmList();

	void RealmPolicyEditCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.Provider != null && Controller.SelectedNavNode?.Type == NodeType.Realm;
	void RealmPolicyEditExecute(object sender, ExecutedRoutedEventArgs e) => PromptRealmPolicyEdit();

	void ConnectRealmChildUsersCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.EditingRealm != null;
	async void ConnectRealmChildUsersExecute(object sender, ExecutedRoutedEventArgs e) => await PromptConnectRealmChildUsers();

	void DisconnectRealmChildUserCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.EditingRealm != null && Controller.SelectedRealmChildUser != null;
	void DisconnectRealmChildUserExecute(object sender, ExecutedRoutedEventArgs e) => Controller.DisconnectRealmChildUser();

	void ConnectRealmChildCustomersCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.EditingRealm != null;
	async void ConnectRealmChildCustomersExecute(object sender, ExecutedRoutedEventArgs e) => await PromptConnectRealmChildCustomers();

	void EditRealmCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.SelectedListRealms.Length == 1;
	void EditRealmExecute(object sender, ExecutedRoutedEventArgs e) => Controller.EditRealm();

	void DisconnectRealmChildCustomerCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.EditingRealm != null && Controller.SelectedRealmChildCustomer != null;
	void DisconnectRealmChildCustomerExecute(object sender, ExecutedRoutedEventArgs e) => Controller.DisconnectRealmChildCustomer();

	void LaunchPasswordChangeCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.BusyMessage == null && Controller.EditingUser != null;
	void LaunchPasswordChangeExecute(object sender, ExecutedRoutedEventArgs e) => PromptPasswordChange();

	void RandomPasswordCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
	void RandomPasswordExecute(object sender, ExecutedRoutedEventArgs e) => Controller.RandomPassword();

	void ChangePasswordCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.NewPassword?.Length > 0;
	void ChangePasswordExecute(object sender, ExecutedRoutedEventArgs e) => Controller.ChangePassword();

	void PromptConnect()
	{
		Controller.ConnectMessage = null;
		Controller.IsConnectError = false;
		var dialog = new ConnectWindow()
		{
			Owner = this,
			DataContext = DataContext
		};
		dialog.ShowDialog();
	}

	async Task TrapConnect(Window dialog)
	{
		bool success = await Controller.Connect();
		if (success)
		{
			dialog.Close();
		}
	}

	void PromptDeleteProfile()
	{
		string message = $"Do you want to delete profile '{Controller.SelectedProfile!.Name}'?";
		if (Pop.Question(this, message))
		{
			Controller.DeleteProfile();
		}
	}

	void PromptHelpAbout()
	{
		var window = new AboutWindow()
		{
			Owner = this,
			DataContext = DataContext
		};
		window.ShowDialog();
	}

	void PromptEditMetrics()
	{
		var dialog = new MetricsEditDialog()
		{
			Owner = this,
			DataContext = DataContext
		};
		Controller.BeginMetricsEdit();
		if (dialog.ShowDialog() == true)
		{
			Controller.EndMetricsEdit();
		}
		else
		{
			Controller.CancelMetricsEdit();
		}
	}

	async Task PromptDeleteCustomer()
	{
		string message = Strings.DeleteCustomerPrompt.Format(Controller.EditingCustomer!.Id!, Controller.EditingCustomer!.Name);
		if (Pop.Question(this, message))
		{
			await Controller.DeleteCustomer();
		}
	}

	void PromptNewCustomer()
	{
		if (!Controller.SelectedProfile!.HasAzure)
		{
			Pop.Warning(this, "Creating a new customer requires that the four Azure values be entered in the connect profile.");
			return;
		}
		var dialog = new NewCustomerDialog()
		{
			Owner = this,
			DataContext = DataContext
		};
		dialog.ShowDialog();
	}

	async Task CreateCustomerWrap()
	{
		SubscriptionAccount? newcust = await Controller.CreateCustomer();
		if (newcust != null)
		{
			Pop.Info(this, $"Customer '{newcust.Name}' has been added to the licensing database and a corresponding Azure Storage Account has been created.");
		}
	}

	async Task PromptValidateCustomer()
	{
		try
		{
			await Controller.ValidateCustomer();
		}
		catch (Exception ex)
		{
			Pop.Error(this, "Customer Validation", ex.Message, ex.ToString());
			return;
		}
		if (Controller.CustomerValidateErrors != null)
		{
			if (Controller.CustomerValidateErrors.Length == 0)
			{
				string message = Strings.ValidateCustomerPass.Format(Controller.EditingCustomer!.Id!, Controller.EditingCustomer!.Name);
				Pop.Info(this, message);
			}
			else
			{
				string join = string.Join("\n", Controller.CustomerValidateErrors.Select(e => $"\u25aa {e}"));
				string message = Strings.ValidateCustomerFail.Format(Controller.EditingCustomer!.Id!, Controller.EditingCustomer!.Name, join);
				Pop.Info(this, message);
			}
		}
	}

	async Task PromptDeleteJob()
	{
		string message = $"Do you want to delete job Id {Controller.EditingJob!.Id} Name '{Controller.EditingJob!.Name}'.\n\nNOTE -- Deleting a job removes it from the licensing database, but it does NOT delete any existing Azure Container that corresponds to the job.";
		if (Pop.Question(this, message))
		{
			await Controller.DeleteJob();
		}
	}

	async Task PromptDeleteUser()
	{
		string message = $"Do you want to delete user Id {Controller.EditingUser!.Id} Name '{Controller.EditingUser!.Name}'.\n\nWARNING -- Deleting a user can have side effects through the licensing system, and it may be an unrecoverable change.";
		if (Pop.Question(this, message))
		{
			await Controller.DeleteUser();
		}
	}

	async Task PromptNewJob()
	{
		Controller.PrepareForNewJob();
		var dialog = new NewJobDialog()
		{
			Owner = this,
			DataContext = DataContext
		};
		if (dialog.ShowDialog() == true)
		{
			await Task.Delay(250);
			MainCommands.CreateJob.Execute(null, this);
		}
	}

	async Task PromptMaterialiseJob()
	{
		string conname = Controller.SelectedCloudNode!.Job!.Name;
		string? message = null;
		if (Controller.BlobDatas == null)
		{
			message = Strings.InaccessibleJobContainerPrompt.Format(conname);
		}
		else
		{
			bool hasini = Controller.BlobDatas.Any(b => string.Compare(b.Name, AppMetrics.JobIniFilename, StringComparison.OrdinalIgnoreCase) == 0);
			bool hascd = Controller.BlobDatas.Any(b => string.Compare(b.Name, AppMetrics.CaseDataDirname, StringComparison.OrdinalIgnoreCase) == 0);
			if (!hasini || !hascd)
			{
				message = Strings.NotJobContainerPrompt.Format(conname);
			}
		}
		if (message != null)
		{
			if (Pop.Question(this, message))
			{
				await Controller.MaterialiseJob();
			}
		}
	}

	async Task PromptNewUser()
	{
		Controller.PrepareForNewUser();
		var dialog = new NewUserDialog()
		{
			Owner = this,
			DataContext = DataContext
		};
		if (dialog.ShowDialog() == true)
		{
			await Task.Delay(250);
			MainCommands.CreateUser.Execute(null, this);
		}
	}

	async Task PromptValidateJob()
	{
		try
		{
			await Controller.ValidateJob();
		}
		catch (Exception ex)
		{
			Pop.Error(this, "Job Validation", ex.Message, ex.ToString());
			return;
		}
		if (Controller.JobValidateErrors != null)
		{
			if (Controller.JobValidateErrors.Length == 0)
			{
				string message = $"Validation Passed\n\nJob Id {Controller.EditingJob!.Id} Name {Controller.EditingJob!.Name} has a corresponding Azure Container and it is accessible.";
				Pop.Info(this, message);
			}
			else
			{
				string message = $"Validation Failed\n\nJob Id {Controller.EditingJob!.Id} Name {Controller.EditingJob!.Name} returned the following errors attempting to access the corresponding Azure Container:\n\n" + string.Join("\n", Controller.JobValidateErrors.Select(e => $"\u25aa {e}"));
				Pop.Info(this, message);
			}
		}
	}

	async Task PromptImportUsers()
	{
		var dialog = new OpenFileDialog()
		{
			Title = "Select User Import File",
			Filter = "Excel Files|*.xlsx|All Files|*.*"
		};
		if (dialog.ShowDialog(this) != true) return;
		try
		{
			await Controller.ImportUsers(dialog.FileName, (count, users) =>
			{
				string message = count == users.Count ? $"User import count = {users.Count}" :
				$"";//\n\nDo you want to continue and run the import?";
				return Pop.Question(this, message);
			});
		}
		catch (Exception ex)
		{
			//MessageBox.Show(this, ex.Message, Strings.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
			string message = $"{ex.Message}. See the DNA help web page section titled Bulk User Import.";
			Pop.Error(this, "Import users failed", message, ex.ToString());
		}
	}

	async Task PromptConnectCustomerChildUsers()
	{
		Controller.PickItems = Controller.ObsUserPick
			.Where(u => !Controller.EditingCustomer!.Users!.Any(e => e.Id == u.Id))
			.Select(u => new EntityPickItem(PickEntityType.User, u.Id, u.Name, u.Email, u.IsDisabled))
			.OrderBy(u => u.Name.ToUpper())
			.ToArray();
		if (Controller.PickItems.Length > 0)
		{
			var picker = new PickWindow
			{
				Owner = this,
				Title = "Select Users",
				DataContext = DataContext
			};
			if (picker.ShowDialog() == true)
			{
				await Controller.ConnectCustomerChildUsers();
			}
		}
	}

	async Task PromptConnectJobChildUsers()
	{
		Controller.PickItems = Controller.ObsUserPick
			.Where(u => !Controller.EditingJob!.Users!.Any(e => e.Id == u.Id))
			.Select(u => new EntityPickItem(PickEntityType.User, u.Id, u.Name, u.Email, u.IsDisabled))
			.OrderBy(u => u.Name.ToUpper())
			.ToArray();
		if (Controller.PickItems.Length > 0)
		{
			var picker = new PickWindow
			{
				Owner = this,
				Title = "Select Users",
				DataContext = DataContext
			};
			if (picker.ShowDialog() == true)
			{
				try
				{
					await Controller.ConnectJobChildUsers();
				}
				catch (Exception ex)
				{
					Pop.Error(this, "Connect Job Child Users", ex.Message, ex.ToString());
				}
			}
		}
	}

	async Task PromptDisconnectJobChildUser()
	{
		try
		{
			await Controller.DisconnectJobChildUser();
		}
		catch (Exception ex)
		{
			Pop.Error(this, "Disconnect Job Child User", ex.Message, ex.ToString());
		}
	}

	async Task PromptConnectUserChildCustomers()
	{
		Controller.PickItems = Controller.ObsCustomerPick
			.Where(c => !Controller.EditingUser!.Customers!.Any(e => e.Id == c.Id) && c.Id != Strings.NoSelectId)
			.Select(c => new EntityPickItem(PickEntityType.Customer, c.Id, c.Name, c.DisplayName, c.Inactive))
			.OrderBy(c => c.Name.ToUpper())
			.ToArray();
		if (Controller.PickItems.Length > 0)
		{
			var picker = new PickWindow
			{
				Owner = this,
				Title = "Select Customers",
				DataContext = DataContext
			};
			if (picker.ShowDialog() == true)
			{
				await Controller.ConnectUserChildCustomers();
			}
		}
	}

	async Task PromptConnectUserChildJobs()
	{
		Controller.PickItems = Controller.ObsJobPick
			.Where(j => !Controller.EditingUser!.Jobs!.Any(e => e.Id == j.Id))
			.Select(j => new EntityPickItem(PickEntityType.Job, j.Id, j.Name, j.DisplayName, j.Inactive))
			.OrderBy(j => j.Name.ToUpper())
			.ToArray();
		if (Controller.PickItems.Length > 0)
		{
			var picker = new PickWindow
			{
				Owner = this,
				Title = "Select Jobs",
				DataContext = DataContext
			};
			if (picker.ShowDialog() == true)
			{
				await Controller.ConnectUserChildJobs();
			}
		}
	}

	async Task PromptNewRealm()
	{
		Controller.PrepareForNewRealm();
		var dialog = new NewRealmDialog()
		{
			Owner = this,
			DataContext = DataContext
		};
		if (dialog.ShowDialog() == true)
		{
			await Task.Delay(250);
			MainCommands.CreateRealm.Execute(null, this);
		}
	}

	async Task PromptDeleteRealm()
	{
		string message = $"Do you want to delete realm Id {Controller.EditingRealm!.Id} Name '{Controller.EditingRealm!.Name}'.\n\nWARNING -- Deleting a realm can have side effects through the licensing system, and it may be an unrecoverable change.";

		if (Pop.Question(this, message))
		{
			await Controller.DeleteRealm();
		}
	}

	void PromptRealmPolicyEdit()
	{
		Controller.PrepareRealmPolicyEdit();
		var dialog = new RealmPolicyEditDialog()
		{
			Owner = this,
			DataContext = DataContext
		};
		if (dialog.ShowDialog() == true)
		{
			Controller.SaveRealmPolicyEdit();
		}
		else
		{
			Controller.CancelRealmPolicyEdit();
		}
	}

	async Task PromptConnectRealmChildUsers()
	{
		Controller.PickItems = Controller.ObsUserPick
			.Where(u => !Controller.EditingRealm!.Users!.Any(e => e.Id == u.Id) && u.Id != Strings.NoSelectId)
			.Select(u => new EntityPickItem(PickEntityType.User, u.Id, u.Name, null, u.IsDisabled))
			.OrderBy(u => u.Name.ToUpper())
			.ToArray();
		if (Controller.PickItems.Length > 0)
		{
			var picker = new PickWindow
			{
				Owner = this,
				Title = "Select Users",
				DataContext = DataContext
			};
			if (picker.ShowDialog() == true)
			{
				await Controller.ConnectRealmChildUsers();
			}
		}
	}

	async Task PromptConnectRealmChildCustomers()
	{
		Controller.PickItems = Controller.ObsCustomerPick
			.Where(c => !Controller.EditingRealm!.Customers!.Any(e => e.Id == c.Id))
			.Select(c => new EntityPickItem(PickEntityType.Customer, c.Id, c.Name, c.DisplayName, c.Inactive))
			.OrderBy(c => c.Name.ToUpper())
			.ToArray();
		if (Controller.PickItems.Length > 0)
		{
			var picker = new PickWindow
			{
				Owner = this,
				Title = "Select Customers",
				DataContext = DataContext
			};
			if (picker.ShowDialog() == true)
			{
				await Controller.ConnectRealmChildCustomers();
			}
		}
	}

	void PromptUploadJob()
	{
		Controller.PrepareUpload();
		var window = new JobUploadWindow()
		{
			Owner = this,
			DataContext = DataContext
		};
		window.ShowDialog();
	}

	void PromptDownloadJob()
	{
		Controller.PrepareDownload();
		var window = new JobDownloadWindow()
		{
			Owner = this,
			DataContext = DataContext
		};
		window.ShowDialog();
	}

	void PromptPasswordChange()
	{
		Controller.NewPassword = null;
		var window = new ChangePasswordWindow()
		{
			Owner = this,
			DataContext = DataContext
		};
		window.ShowDialog();
	}

	bool ForceSessionsHandler(SessionStatus[] sessions)
	{
		string message = sessions.Length == 1 ?
			"Do you want to forcible end the selected session?" :
			$"Do you want to forcibly end the {sessions.Length} selected sessions?";
		return Pop.Question(this, message);
	}
}
