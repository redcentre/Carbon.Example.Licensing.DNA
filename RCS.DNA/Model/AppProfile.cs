using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace RCS.DNA.Model;

public enum AuthenticateSequence
{
	IdOnly,
	NameOnly,
	IdThenName,
	NameThenId
}

[CategoryOrder("Profile", 0)]
[CategoryOrder("DNA Licence", 1)]
[CategoryOrder("SQL Database Provider", 2)]
[CategoryOrder("RCS Service Provider", 3)]
[CategoryOrder("Carbon Licence", 4)]
[CategoryOrder("Carbon Service", 5)]
[CategoryOrder("Azure", 6)]
sealed class AppProfile : NotifyBase
{
	public AppProfile(string profilKey)
	{
		ProfileKey = profilKey;
		//CarbonServiceBaseUris.ListChanged += CarbonServiceBaseUris_ListChanged;
		CarbonServiceBaseUris.CollectionChanged += CarbonServiceBaseUris_CollectionChanged;
	}

	private void CarbonServiceBaseUris_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(CarbonServiceBaseUris));
	}

	//void CarbonServiceBaseUris_ListChanged(object? sender, EventArgs e)
	//{
	//	OnPropertyChanged(nameof(CarbonServiceBaseUris));
	//}

	/// <summary>
	/// The Settings group key for the profile.
	/// </summary>
	[Browsable(false)]
	public string ProfileKey { get; }

	#region Non-Edit Properties

	[Browsable(false)]
	public bool CanConnect
	{
		get
		{
			bool canLogin = !string.IsNullOrEmpty(_loginId) && !string.IsNullOrEmpty(_password);
			bool sql = _sqlProviderActive && !string.IsNullOrEmpty(_sqlAdoConnect) && !string.IsNullOrEmpty(_sqlProductKey);
			bool rcs = _rcsProviderActive && !string.IsNullOrEmpty(_rcsApiKey) && !string.IsNullOrEmpty(_rcsServiceBaseAddress);
			return canLogin && (sql || rcs);
		}
	}

	DateTime _createdUtc;
	[Browsable(false)]
	public DateTime CreatedUtc
	{
		get => _createdUtc;
		set
		{
			if (_createdUtc != value)
			{
				_createdUtc = value;
				OnPropertyChanged(nameof(CreatedUtc));
			}
		}
	}

	DateTime? _lastUpdateUtc;
	[Browsable(false)]
	public DateTime? LastUpdateUtc
	{
		get => _lastUpdateUtc;
		set
		{
			if (_lastUpdateUtc != value)
			{
				_lastUpdateUtc = value;
				OnPropertyChanged(nameof(LastUpdateUtc));
			}
		}
	}

	DateTime? _lastConnectUtc;
	[Browsable(false)]
	public DateTime? LastConnectUtc
	{
		get => _lastConnectUtc;
		set
		{
			if (_lastConnectUtc != value)
			{
				_lastConnectUtc = value;
				OnPropertyChanged(nameof(LastConnectUtc));
			}
		}
	}

	int _connectCount;
	[Browsable(false)]
	public int ConnectCount
	{
		get => _connectCount;
		set
		{
			if (_connectCount != value)
			{
				_connectCount = value;
				OnPropertyChanged(nameof(ConnectCount));
			}
		}
	}

	#endregion

	#region Profile

	string? _name;
	[Category("Profile")]
	[DisplayName("Name")]
	[Description("The display name of the login profile.")]
	public string? Name
	{
		get => _name;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? ProfileKey : value;
			if (_name != newval)
			{
				_name = newval;
				OnPropertyChanged(nameof(Name));
			}
		}
	}

	bool _userNameIsEmail = true;
	[Category("Profile")]
	[DisplayName("User Name is Email")]
	[Description("The User Name property (aka the Account Name) is expected to be an email address.")]
	public bool UserNameIsEmail
	{
		get => _userNameIsEmail;
		set
		{
			if (_userNameIsEmail != value)
			{
				_userNameIsEmail = value;
				OnPropertyChanged(nameof(UserNameIsEmail));
			}
		}
	}

	#endregion

	#region DNA Login via RCS Service

	string? _loginId;
	[Category("DNA Licence")]
	[DisplayName("Account Id")]
	[Description("The application licence account Id provided by Red Centre Software.")]
	public string? LoginId
	{
		get => _loginId;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_loginId != newval)
			{
				_loginId = newval;
				OnPropertyChanged(nameof(LoginId));
				OnPropertyChanged(nameof(CanConnect));
			}
		}
	}

	string? _password;
	[Category("DNA Licence")]
	[DisplayName("Password")]
	[Description("The application licence account password provided by Red Centre Software.")]
	public string? Password
	{
		get => _password;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_password != newval)
			{
				_password = newval;
				OnPropertyChanged(nameof(Password));
				OnPropertyChanged(nameof(CanConnect));
			}
		}
	}

	#endregion

	#region SQL Database Provider

	bool _sqlProviderActive;
	[Category("SQL Database Provider")]
	[DisplayName("Active")]
	[Description("Check to make the SQL Database provider the active one.")]
	public bool SqlProviderActive
	{
		get => _sqlProviderActive;
		set
		{
			if (_sqlProviderActive != value)
			{
				_sqlProviderActive = value;
				OnPropertyChanged(nameof(SqlProviderActive));
				if (_sqlProviderActive && _rcsProviderActive)
				{
					RCSProviderActive = false;
				}
				OnPropertyChanged(nameof(CanConnect));
			}
		}
	}

	string? _sqlAdoConnect;
	[Category("SQL Database Provider")]
	[DisplayName("ADO.NET Connect")]
	[Description("The ADO.NET connection string to the licensing SQL Server database.")]
	public string? SqlAdoConnect
	{
		get => _sqlAdoConnect;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_sqlAdoConnect != newval)
			{
				_sqlAdoConnect = newval;
				OnPropertyChanged(nameof(SqlAdoConnect));
				OnPropertyChanged(nameof(CanConnect));
			}
		}
	}

	string? _sqlProductKey;
	[Category("SQL Database Provider")]
	[DisplayName("Product Key")]
	[Description("A product key is provided by Red Centre Software (support@redcentresoftware.com).")]
	public string? SqlProductKey
	{
		get => _sqlProductKey;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_sqlProductKey != newval)
			{
				_sqlProductKey = newval;
				OnPropertyChanged(nameof(SqlProductKey));
				OnPropertyChanged(nameof(CanConnect));
			}
		}
	}

	#endregion

	#region RCS Service Provider

	bool _rcsProviderActive;
	[Category("RCS Service Provider")]
	[DisplayName("Active")]
	[Description("Check to make the RCS service provider the active one.")]
	public bool RCSProviderActive
	{
		get => _rcsProviderActive;
		set
		{
			if (_rcsProviderActive != value)
			{
				_rcsProviderActive = value;
				OnPropertyChanged(nameof(RCSProviderActive));
				if (_rcsProviderActive && _sqlProviderActive)
				{
					SqlProviderActive = false;
				}
				OnPropertyChanged(nameof(CanConnect));
			}
		}
	}

	string? _rcsApiKey;
	[Category("RCS Service Provider")]
	[DisplayName("API Key")]
	[Description("An API key is provided by Red Centre Software (support@redcentresoftware.com).")]
	public string? RcsApiKey
	{
		get => _rcsApiKey;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_rcsApiKey != newval)
			{
				_rcsApiKey = newval;
				OnPropertyChanged(nameof(RcsApiKey));
				OnPropertyChanged(nameof(CanConnect));
			}
		}
	}

	string? _rcsServiceBaseAddress;
	[Category("RCS Service Provider")]
	[DisplayName("Service Base Address")]
	[Description("Base address (uri) of the Red Centre Software compliant licensing web service.")]
	public string? RcsServiceBaseAddress
	{
		get => _rcsServiceBaseAddress;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_rcsServiceBaseAddress != newval)
			{
				_rcsServiceBaseAddress = newval;
				OnPropertyChanged(nameof(RcsServiceBaseAddress));
				OnPropertyChanged(nameof(CanConnect));
			}
		}
	}

	int _rcsServiceTimeout = 30;
	[Category("RCS Service Provider")]
	[DisplayName("Service Timeout")]
	[Description("Timeout in seconds for the Red Centre Software compliant licensing web service.")]
	[DefaultValue(30)]
	public int RcsServiceTimeout
	{
		get => _rcsServiceTimeout;
		set
		{
			if (_rcsServiceTimeout != value)
			{
				_rcsServiceTimeout = value;
				OnPropertyChanged(nameof(RcsServiceTimeout));
			}
		}
	}

	#endregion

	#region Carbon Licence

	// Note the property names contain 'Login' which is misleading and legacy,
	// they should be 'Licence', but changing them would break existing profiles.

	string? _carbonLoginNameOrId;
	[Category("Carbon Licence")]
	[DisplayName("Name or Id")]
	[Description("The licence Name or Id credential for the Carbon engine.")]
	public string? CarbonLoginNameOrId
	{
		get => _carbonLoginNameOrId;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_carbonLoginNameOrId != newval)
			{
				_carbonLoginNameOrId = newval;
				OnPropertyChanged(nameof(CarbonLoginNameOrId));
			}
		}
	}

	string? _carbonLoginPassword;
	[Category("Carbon Licence")]
	[DisplayName("Password")]
	[Description("The licence password credential for the Carbon engine.")]
	public string? CarbonLoginPassword
	{
		get => _carbonLoginPassword;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_carbonLoginPassword != newval)
			{
				_carbonLoginPassword = newval;
				OnPropertyChanged(nameof(CarbonLoginPassword));
			}
		}
	}

	AuthenticateSequence _carbonLoginSequence = AuthenticateSequence.IdThenName;
	[Category("Carbon Licence")]
	[DisplayName("Sequence")]
	[Description("Specifies the sequence in which Carbon engine licence authentication attempts will be made.")]
	[DefaultValue(typeof(AuthenticateSequence), "IdThenName")]
	public AuthenticateSequence CarbonLoginSequence
	{
		get => _carbonLoginSequence;
		set
		{
			if (_carbonLoginSequence != value)
			{
				_carbonLoginSequence = value;
				OnPropertyChanged(nameof(CarbonLoginSequence));
			}
		}
	}

	#endregion

	#region Carbon Service

	[Category("Carbon Service")]
	[DisplayName("Carbon Service Uris")]
	[Description("A list of Carbon web service base Uris.")]
	public ObservableCollection<string> CarbonServiceBaseUris { get; set; } = [];   // QUESTION Why does this have be both get and set for the property grid control?

	string? _carbonServiceApiKey;
	[Category("Carbon Service")]
	[DisplayName("API Key")]
	[Description("The API Key for access to the Carbon web service.")]
	public string? CarbonServiceApiKey
	{
		get => _carbonServiceApiKey;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_carbonServiceApiKey != newval)
			{
				_carbonServiceApiKey = newval;
				OnPropertyChanged(nameof(CarbonServiceApiKey));
			}
		}
	}

	#endregion

	#region Azure

	[Browsable(false)]
	public bool HasAzure => _subscriptionId != null && _tenantId != null && _applicationId != null && _clientSecret != null;

	string? _subscriptionId;
	[Category("Azure")]
	[DisplayName("Subscription Id")]
	[Description("The Azure Subscription Id displayed in the Azure portal Subscriptions > Account blade.")]
	public string? SubscriptionId
	{
		get => _subscriptionId;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_subscriptionId != newval)
			{
				_subscriptionId = newval;
				OnPropertyChanged(nameof(SubscriptionId));
				OnPropertyChanged(nameof(HasAzure));
			}
		}
	}

	string? _tenantId;
	[Category("Azure")]
	[DisplayName("Tenant Id")]
	[Description("The Azure Tenant Id displayed in the Azure portal Entra ID blade.")]
	public string? TenantId
	{
		get => _tenantId;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_tenantId != newval)
			{
				_tenantId = newval;
				OnPropertyChanged(nameof(TenantId));
				OnPropertyChanged(nameof(HasAzure));
			}
		}
	}

	string? _applicationId;
	[Category("Azure")]
	[DisplayName("Application Id")]
	[Description("The Azure Application Id displayed in the Azure portal Entra ID > App Registrations blade.")]
	public string? ApplicationId
	{
		get => _applicationId;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_applicationId != newval)
			{
				_applicationId = newval;
				OnPropertyChanged(nameof(ApplicationId));
				OnPropertyChanged(nameof(HasAzure));
			}
		}
	}

	string? _clientSecret;
	[Category("Azure")]
	[DisplayName("Client Secret")]
	[Description("The Azure Application client secret displayed in the Azure portal Entra ID > App Registrations > Certificates & Secrets blade.")]
	public string? ClientSecret
	{
		get => _clientSecret;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_clientSecret != newval)
			{
				_clientSecret = newval;
				OnPropertyChanged(nameof(ClientSecret));
				OnPropertyChanged(nameof(HasAzure));
			}
		}
	}

	#endregion
}
