using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Orthogonal.Common.Basic;
using RCS.Carbon.Example.WebService.Common.DTO;
using RCS.Carbon.Shared;
using RCS.Licensing.Provider.Shared;
using RCS.Licensing.Provider.Shared.Entities;

namespace RCS.DNA.Model;

partial class MainController
{
	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(LoadNavigationCommand))]
	[NotifyCanExecuteChangedFor(nameof(RunReportCommand))]
	ILicensingProvider? _provider;

	[ObservableProperty]
	string? _statusMessage = "Waiting for connect";

	[ObservableProperty]
	string? _statusTime = "00:00:00";

	[ObservableProperty]
	string? _statusTip;

	[ObservableProperty]
	int _appFontSize = 13;

	[ObservableProperty]
	string? _alertTitle;

	[ObservableProperty]
	string? _alertMessage;

	[ObservableProperty]
	AppMetrics _metrics;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(LoadNavigationCommand))]
	[NotifyCanExecuteChangedFor(nameof(CloudCompareCommand))]
	[NotifyCanExecuteChangedFor(nameof(RunReportCommand))]
	[NotifyCanExecuteChangedFor(nameof(LoadSessionsCommand))]
	[NotifyCanExecuteChangedFor(nameof(ForceSessionsCommand))]
	string? _busyMessage;

	[ObservableProperty]
	int _tabIndex;

	partial void OnTabIndexChanged(int value) => AfterTabIndexChanged();

	[ObservableProperty]
	string? _connectMessage;

	[ObservableProperty]
	bool _isConnectError;

	[ObservableProperty]
	bool _isConnecting;

	[ObservableProperty]
	SafeObservableCollection<BindRealm>? _obsRealmList;

	[ObservableProperty]
	BindRealm[] _selectedListRealms = [];

	[ObservableProperty]
	SafeObservableCollection<BindUser>? _obsUserList;

	[ObservableProperty]
	ListCollectionView? _viewUserList;

	[ObservableProperty]
	string? _filterUserList;

	[ObservableProperty]
	BindUser[] _selectedListUsers = [];

	[ObservableProperty]
	SafeObservableCollection<BindCustomer>? _obsCustomerList;

	[ObservableProperty]
	BindCustomer[] _selectedListCustomers = [];

	[ObservableProperty]
	SafeObservableCollection<BindJob>? _obsJobList;

	[ObservableProperty]
	BindJob[] _selectedListJobs = [];

	[ObservableProperty]
	ObservableCollection<AppNode>? _obsNavNodes;

	[ObservableProperty]
	ObservableCollection<AppNode>? _obsCloudNodes;

	[ObservableProperty]
	SafeObservableCollection<AppProfile>? _obsProfiles;

	[ObservableProperty]
	AppProfile? _selectedProfile;

	partial void OnSelectedProfileChanged(AppProfile? value) => AfterSelectedProfileChanged();

	[ObservableProperty]
	AppNode? _selectedNavNode;

	[ObservableProperty]
	AppNode? _selectedCloudNode;

	partial void OnSelectedCloudNodeChanged(AppNode? value) => AfterSelectedCloudNodeChanged();

	[ObservableProperty]
	AppNode? _addedNode;

	[ObservableProperty]
	SafeObservableCollection<AppNode>? _safeNavNodes;

	[ObservableProperty]
	BlobData[]? _blobDatas;

	[ObservableProperty]
	string? _blobListErrorMessage;

	[ObservableProperty]
	Dictionary<string, object?>? _cloudCustomerMap;

	[ObservableProperty]
	BitmapSource? _providerIconSource;

	[ObservableProperty]
	ObservableCollection<NavRealm> _obsRealmPick;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(CloudCompareCommand))]
	ObservableCollection<NavCustomer> _obsCustomerPick;

	[ObservableProperty]
	ObservableCollection<NavJob> _obsJobPick;

	[ObservableProperty]
	ObservableCollection<NavUser> _obsUserPick;

	[ObservableProperty]
	BindCustomer? _editingCustomer;

	[ObservableProperty]
	BindRealm? _editingRealm;

	[ObservableProperty]
	BindUser? _editingUser;

	[ObservableProperty]
	BindJob? _editingJob;

	[ObservableProperty]
	string[]? _customerValidateErrors;

	[ObservableProperty]
	string[]? _jobValidateErrors;

	[ObservableProperty]
	BindUser? _selectedCustomerChildUser;

	[ObservableProperty]
	BindUser? _selectedJobChildUser;

	[ObservableProperty]
	BindCustomer? _selectedUserChildCustomer;

	[ObservableProperty]
	BindJob? _selectedUserChildJob;

	[ObservableProperty]
	BindUser? _selectedRealmChildUser;

	[ObservableProperty]
	BindCustomer? _selectedRealmChildCustomer;

	[ObservableProperty]
	EntityPickItem[] _pickItems;

	partial void OnPickItemsChanged(EntityPickItem[] value) => AfterPickItemsChanged();

	[ObservableProperty]
	ListCollectionView _pickView;

	[ObservableProperty]
	EntityPickItem[] _selectedPicks;

	[ObservableProperty]
	string? _pickFilter;

	[ObservableProperty]
	User? _authResponse;

	[ObservableProperty]
	string? _newCustomerName;

	[ObservableProperty]
	string? _newCustomerBusyMessage;

	[ObservableProperty]
	string? _newCustomerErrorMessage;

	[ObservableProperty]
	string[]? _newCustomerResourceGroupNames;

	[ObservableProperty]
	string? _newCustomerSelectedResourceGroupName;

	[ObservableProperty]
	NavRealm? _selectedNewCustomerExistingRealm;

	[ObservableProperty]
	PickItem[]? _newCustomerLocationPicks;

	[ObservableProperty]
	PickItem? _newCustomerSelectedLocationPick;

	[ObservableProperty]
	bool _newCustomerIsBlobsPublic = true;

	[ObservableProperty]
	NewCustomerRealmType _selectedNewCustRealmType = NewCustomerRealmType.New;

	[ObservableProperty]
	string? _newCustomerNewRealmName;

	[ObservableProperty]
	string? _newRealmName;

	partial void OnNewRealmNameChanged(string? value) => AfterNewRealmValueChanged();

	[ObservableProperty]
	ImmutableArray<string>? _newRealmErrors;

	[ObservableProperty]
	string? _newJobCustomerId;

	partial void OnNewJobCustomerIdChanged(string? value) => AfterNewJobValueChanged();

	[ObservableProperty]
	string? _newJobName;

	partial void OnNewJobNameChanged(string? value) => AfterNewJobValueChanged();

	[ObservableProperty]
	bool _newJobMakeContainer = true;

	[ObservableProperty]
	ImmutableArray<string>? _newJobErrors;

	[ObservableProperty]
	string? _newUserCustomerId;

	partial void OnNewUserCustomerIdChanged(string? value) => AfterNewUserValueChanged();

	[ObservableProperty]
	string? _newUserName;

	partial void OnNewUserNameChanged(string? value) => AfterNewUserValueChanged();

	[ObservableProperty]
	string? _newUserPassword;

	partial void OnNewUserPasswordChanged(string? value) => AfterNewUserValueChanged();

	[ObservableProperty]
	ImmutableArray<string>? _newUserErrors;

	[ObservableProperty]
	SafeObservableCollection<UploadPick> _obsUploadPicks;

	[ObservableProperty]
	ObservableCollection<TransferProgress>? _obsUploadProgress;

	[ObservableProperty]
	DirectoryInfo? _uploadSourceDir;

	partial void OnUploadSourceDirChanged(DirectoryInfo? value) => AfterUploadSourceDirChanged();

	[ObservableProperty]
	bool _isUploading;

	[ObservableProperty]
	List<CustJobPick> _custJobPicks;

	[ObservableProperty]
	CustJobPick _selectedUpCustJobPick;

	[ObservableProperty]
	bool _uploadNewChangedOnly = true;

	[ObservableProperty]
	int _uploadParallelLimit;

	[ObservableProperty]
	int _uploadParallelMax;

	[ObservableProperty]
	int _uploadSelectedCount;

	[ObservableProperty]
	bool _isDownloading;

	[ObservableProperty]
	DirectoryInfo? _downloadDestinationDir;

	partial void OnDownloadDestinationDirChanged(DirectoryInfo? value) => AfterDownloadDestinationDirChanged();

	[ObservableProperty]
	CustJobPick _selectedDownloadCustJobPick;

	[ObservableProperty]
	ObservableCollection<TransferProgress>? _obsDownloadProgress;

	[ObservableProperty]
	string[]? _downIncludeFolders;

	[ObservableProperty]
	string[]? _downExcludeFolders;

	[ObservableProperty]
	string[]? _downIncludeFileGlobs;

	[ObservableProperty]
	string[]? _downExcludeFileGlobs;

	[ObservableProperty]
	bool _downloadNewChangedOnly = true;

	[ObservableProperty]
	ReportItem[] _reportItems;

	[ObservableProperty]
	ListCollectionView _reportView;

	[ObservableProperty]
	string? _newPassword;

	[ObservableProperty]
	string? _selectedCarbonServiceUri;

	partial void OnSelectedCarbonServiceUriChanged(string? value)
	{
		Trace.WriteLine($"#### _selectedCarbonServiceUri {value}");
		if (SelectedCarbonServiceIndex == 0)
		{
			ObsSessions = null;
			return;
		}
		loadSessionsCommand.Execute(null);
	}

	[ObservableProperty]
	ObservableCollection<SessionStatus>? _obsSessions;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(ForceSessionsCommand))]
	SessionStatus[] _selectedSessions = [];

	[ObservableProperty]
	string[] _carbonServiceUriPicks;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(LoadSessionsCommand))]
	int _selectedCarbonServiceIndex;

	partial void OnSelectedCarbonServiceIndexChanged(int value) => Trace.WriteLine($"#### _carbonServiceUriPickIndex {value}");
}
