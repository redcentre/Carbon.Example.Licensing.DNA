using System.Collections.ObjectModel;
using RCS.Licensing.Provider.Shared;
using RCS.Licensing.Provider.Shared.Entities;

namespace RCS.DNA.Model;

public sealed class BindUser : NotifyBase, IBindEntity
{
	readonly User _user;

	public BindUser(User source, bool nullableChildren = true)
	{
		_user = source;
		Customers = source.Customers == null ? nullableChildren ? null : [] : [.. source.Customers.Select(c => new BindCustomer(c))];
		Jobs = source.Jobs == null ? nullableChildren ? null : [] : [.. source.Jobs.Select(j => new BindJob(j))];
	}

	public User GetEntity()
	{
		_user.Customers = Customers?.Select(c => c.GetEntity()).ToArray();
		_user.Jobs = Jobs?.Select(j => j.GetEntity()).ToArray();
		return _user;
	}

	public void CopyPropsFrom(BindUser other)
	{
		Name = other.Name;
		ProviderId = other.ProviderId;
		Psw = other.Psw;
		PassHash = (byte[]?)other.PassHash?.Clone();
		Email = other.Email;
		EntityId = other.EntityId;
		CloudCustomerNames = (string[])other.CloudCustomerNames.Clone();
		JobNames = (string[])other.JobNames.Clone();
		VartreeNames = (string[])other.VartreeNames.Clone();
		DashboardNames = (string[])other.DashboardNames.Clone();
		DataLocation = other.DataLocation;
		Sequence = other.Sequence;
		Comment = other.Comment;
		Roles = (string[]?)other.Roles?.Clone();
		Filter = other.Filter;
		LoginMacs = other.LoginMacs;
		LoginCount = other.LoginCount;
		LoginMax = other.LoginMax;
		LastLogin = other.LastLogin;
		Sunset = other.Sunset;
		MaxJobs = other.MaxJobs;
		Version = other.Version;
		MinVersion = other.MinVersion;
		IsDisabled = other.IsDisabled;
	}

	public override int GetHashCode() => $"{Type}:{Id}".GetHashCode();

	public override string ToString() => $"BindUser({_user})";

	#region User Properties

	public EntityType Type => EntityType.User;

	public string? Id => _user.Id;

	public DateTime Created => _user.Created;

	public string Name
	{
		get => _user.Name;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_user.Name != newval)
			{
				_user.Name = newval;
				OnPropertyChanged(nameof(_user.Name));
				OnPropertyChanged(nameof(IsNameError));
			}
		}
	}

	public string? ProviderId
	{
		get => _user.ProviderId;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_user.ProviderId != newval)
			{
				_user.ProviderId = newval;
				OnPropertyChanged(nameof(_user.ProviderId));
			}
		}
	}

	public string? Psw
	{
		get => _user.Psw;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_user.Psw != newval)
			{
				_user.Psw = newval;
				OnPropertyChanged(nameof(_user.Psw));
			}
		}
	}

	public byte[]? PassHash
	{
		get => _user.PassHash;
		set
		{
			if (_user.PassHash != value)
			{
				_user.PassHash = value;
				OnPropertyChanged(nameof(_user.PassHash));
			}
		}
	}

	public string? Email
	{
		get => _user.Email;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_user.Email != newval)
			{
				_user.Email = newval;
				OnPropertyChanged(nameof(_user.Email));
				OnPropertyChanged(nameof(IsEmailError));
			}
		}
	}

	public string? EntityId
	{
		get => _user.EntityId;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_user.EntityId != newval)
			{
				_user.EntityId = newval;
				OnPropertyChanged(nameof(_user.EntityId));
			}
		}
	}

	public string[] CloudCustomerNames
	{
		get => _user.CloudCustomerNames;
		set
		{
			if (_user.CloudCustomerNames != value)
			{
				_user.CloudCustomerNames = value;
				OnPropertyChanged(nameof(_user.CloudCustomerNames));
			}
		}
	}

	public string[] JobNames
	{
		get => _user.JobNames;
		set
		{
			if (_user.JobNames != value)
			{
				_user.JobNames = value;
				OnPropertyChanged(nameof(_user.JobNames));
			}
		}
	}

	public string[] VartreeNames
	{
		get => _user.VartreeNames;
		set
		{
			if (_user.VartreeNames != value)
			{
				_user.VartreeNames = value;
				OnPropertyChanged(nameof(_user.VartreeNames));
			}
		}
	}

	public string[] DashboardNames
	{
		get => _user.DashboardNames;
		set
		{
			if (_user.DashboardNames != value)
			{
				_user.DashboardNames = value;
				OnPropertyChanged(nameof(_user.DashboardNames));
			}
		}
	}

	public DataLocationType? DataLocation
	{
		get => _user.DataLocation;
		set
		{
			if (_user.DataLocation != value)
			{
				_user.DataLocation = value;
				OnPropertyChanged(nameof(_user.DataLocation));
			}
		}
	}

	public int? Sequence
	{
		get => _user.Sequence;
		set
		{
			if (_user.Sequence != value)
			{
				_user.Sequence = value;
				OnPropertyChanged(nameof(_user.Sequence));
			}
		}
	}

	public Guid Uid
	{
		get => _user.Uid;
		set
		{
			if (_user.Uid != value)
			{
				_user.Uid = value;
				OnPropertyChanged(nameof(_user.Uid));
			}
		}
	}

	public string? Comment
	{
		get => _user.Comment;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_user.Comment != newval)
			{
				_user.Comment = newval;
				OnPropertyChanged(nameof(_user.Comment));
			}
		}
	}

	public string[]? Roles
	{
		get => _user.RoleSet;
		set
		{
			if (_user.RoleSet != value)
			{
				_user.RoleSet = value;
				OnPropertyChanged(nameof(_user.Roles));
			}
		}
	}

	public string? Filter
	{
		get => _user.Filter;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_user.Filter != newval)
			{
				_user.Filter = newval;
				OnPropertyChanged(nameof(_user.Filter));
			}
		}
	}

	public string? LoginMacs
	{
		get => _user.LoginMacs;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_user.LoginMacs != newval)
			{
				_user.LoginMacs = newval;
				OnPropertyChanged(nameof(_user.LoginMacs));
			}
		}
	}

	public int? LoginCount
	{
		get => _user.LoginCount;
		set
		{
			if (_user.LoginCount != value)
			{
				_user.LoginCount = value;
				OnPropertyChanged(nameof(_user.LoginCount));
			}
		}
	}

	public int? LoginMax
	{
		get => _user.LoginMax;
		set
		{
			if (_user.LoginMax != value)
			{
				_user.LoginMax = value;
				OnPropertyChanged(nameof(_user.LoginMax));
			}
		}
	}

	public DateTime? LastLogin
	{
		get => _user.LastLogin;
		set
		{
			if (_user.LastLogin != value)
			{
				_user.LastLogin = value;
				OnPropertyChanged(nameof(_user.LastLogin));
			}
		}
	}

	public DateTime? Sunset
	{
		get => _user.Sunset;
		set
		{
			if (_user.Sunset != value)
			{
				_user.Sunset = value;
				OnPropertyChanged(nameof(_user.Sunset));
			}
		}
	}

	public int? MaxJobs
	{
		get => _user.MaxJobs;
		set
		{
			if (_user.MaxJobs != value)
			{
				_user.MaxJobs = value;
				OnPropertyChanged(nameof(_user.MaxJobs));
			}
		}
	}

	public string? Version
	{
		get => _user.Version;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_user.Version != newval)
			{
				_user.Version = newval;
				OnPropertyChanged(nameof(_user.Version));
			}
		}
	}

	public string? MinVersion
	{
		get => _user.MinVersion;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_user.MinVersion != newval)
			{
				_user.MinVersion = newval;
				OnPropertyChanged(nameof(_user.MinVersion));
			}
		}
	}

	public bool IsDisabled
	{
		get => _user.IsDisabled;
		set
		{
			if (_user.IsDisabled != value)
			{
				_user.IsDisabled = value;
				OnPropertyChanged(nameof(_user.IsDisabled));
			}
		}
	}

	#endregion

	#region Join Properties

	public ObservableCollection<BindCustomer>? Customers { get; set; }

	public ObservableCollection<BindJob>? Jobs { get; set; }

	#endregion

	#region Error Properties

	public bool IsValid => !IsNameError && !IsEmailError;

	public bool IsNameError => _user.Name == null || !AppValidationRules.IsUserNameValid(_user.Name);

	public bool IsEmailError => _user.Email != null && !AppValidationRules.IsValidEmail(_user.Email);

	#endregion
}
