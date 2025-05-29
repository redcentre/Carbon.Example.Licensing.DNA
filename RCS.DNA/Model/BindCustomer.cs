using System.Collections.ObjectModel;
using RCS.Licensing.Provider.Shared;
using RCS.Licensing.Provider.Shared.Entities;

namespace RCS.DNA.Model;

public sealed class BindCustomer(Customer source, bool nullableChildren = true) : NotifyBase, IBindEntity
{
	readonly Customer _cust = source;

	public Customer GetEntity()
	{
		_cust.Jobs = Jobs?.Select(j => j.GetEntity()).ToArray();
		_cust.Users = Users?.Select(u => u.GetEntity()).ToArray();
		return _cust;
	}

	public void CopyPropsFrom(BindCustomer other)
	{
		Name = other.Name;
		DisplayName = other.DisplayName;
		Psw = other.Psw;
		StorageKey = other.StorageKey;
		CloudCustomerNames = (string[])other.CloudCustomerNames.Clone();
		DataLocation = other.DataLocation;
		Sequence = other.Sequence;
		Corporation = other.Corporation;
		Comment = other.Comment;
		Info = other.Info;
		Logo = other.Logo;
		SignInLogo = other.SignInLogo;
		SignInNote = other.SignInNote;
		Credits = other.Credits;
		Spent = other.Spent;
		MaxJobs = other.MaxJobs;
		Sunset = other.Sunset;
		Inactive = other.Inactive;
	}
	public override int GetHashCode() => $"{Type}:{Id}".GetHashCode();

	public override string ToString() => $"BindCustomer({_cust})";

	public bool AddCustomerName(string customerName)
	{
		if (CloudCustomerNames.Contains(customerName)) return false;
		CloudCustomerNames = [.. CloudCustomerNames.Concat([customerName]).OrderBy(n => n.ToUpperInvariant())];
		return true;
	}

	public bool RemoveCustomerName(string customerName)
	{
		if (!CloudCustomerNames.Contains(customerName) == true) return false;
		CloudCustomerNames = [.. CloudCustomerNames.Except([customerName]).OrderBy(n => n.ToUpperInvariant())];
		return true;
	}

	#region Customer Properties

	public EntityType Type => EntityType.Customer;

	public DateTime Created => _cust.Created;

	public string? Id
	{
		get => _cust.Id;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_cust.Id != newval)
			{
				_cust.Id = newval;
				OnPropertyChanged(nameof(_cust.Id));
			}
		}
	}

	public string Name
	{
		get => _cust.Name;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_cust.Name != newval)
			{
				_cust.Name = newval;
				OnPropertyChanged(nameof(_cust.Name));
				OnPropertyChanged(nameof(IsNameError));
			}
		}
	}

	public string? DisplayName
	{
		get => _cust.DisplayName;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_cust.DisplayName != newval)
			{
				_cust.DisplayName = newval;
				OnPropertyChanged(nameof(_cust.DisplayName));
			}
		}
	}

	public string? Psw
	{
		get => _cust.Psw;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_cust.Psw != newval)
			{
				_cust.Psw = newval;
				OnPropertyChanged(nameof(_cust.Psw));
			}
		}
	}

	public string? StorageKey
	{
		get => _cust.StorageKey;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_cust.StorageKey != newval)
			{
				_cust.StorageKey = newval;
				OnPropertyChanged(nameof(_cust.StorageKey));
				OnPropertyChanged(nameof(IsStorkeyError));
			}
		}
	}

	public string[] CloudCustomerNames
	{
		get => _cust.CloudCustomerNames;
		set
		{
			if (_cust.CloudCustomerNames != value)
			{
				_cust.CloudCustomerNames = value;
				OnPropertyChanged(nameof(_cust.CloudCustomerNames));
			}
		}
	}

	public DataLocationType? DataLocation
	{
		get => _cust.DataLocation;
		set
		{
			if (_cust.DataLocation != value)
			{
				_cust.DataLocation = value ?? DataLocationType.Cloud;
				OnPropertyChanged(nameof(_cust.DataLocation));
				OnPropertyChanged(nameof(IsNameError));
			}
		}
	}

	public int? Sequence
	{
		get => _cust.Sequence;
		set
		{
			if (_cust.Sequence != value)
			{
				_cust.Sequence = value;
				OnPropertyChanged(nameof(_cust.Sequence));
			}
		}
	}

	public string? Corporation
	{
		get => _cust.Corporation;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_cust.Corporation != newval)
			{
				_cust.Corporation = newval;
				OnPropertyChanged(nameof(_cust.Corporation));
			}
		}
	}

	public string? Comment
	{
		get => _cust.Comment;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_cust.Comment != newval)
			{
				_cust.Comment = newval;
				OnPropertyChanged(nameof(_cust.Comment));
			}
		}
	}

	public string? Info
	{
		get => _cust.Info;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_cust.Info != newval)
			{
				_cust.Info = newval;
				OnPropertyChanged(nameof(_cust.Info));
			}
		}
	}

	public string? Logo
	{
		get => _cust.Logo;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_cust.Logo != newval)
			{
				_cust.Logo = newval;
				OnPropertyChanged(nameof(_cust.Logo));
			}
		}
	}

	public string? SignInLogo
	{
		get => _cust.SignInLogo;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_cust.SignInLogo != newval)
			{
				_cust.SignInLogo = newval;
				OnPropertyChanged(nameof(_cust.SignInLogo));
			}
		}
	}

	public string? SignInNote
	{
		get => _cust.SignInNote;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_cust.SignInNote != newval)
			{
				_cust.SignInNote = newval;
				OnPropertyChanged(nameof(_cust.SignInNote));
			}
		}
	}

	public int? Credits
	{
		get => _cust.Credits;
		set
		{
			if (_cust.Credits != value)
			{
				_cust.Credits = value;
				OnPropertyChanged(nameof(_cust.Credits));
			}
		}
	}

	public int? Spent
	{
		get => _cust.Spent;
		set
		{
			if (_cust.Spent != value)
			{
				_cust.Spent = value;
				OnPropertyChanged(nameof(_cust.Spent));
			}
		}
	}

	public DateTime? Sunset
	{
		get => _cust.Sunset;
		set
		{
			if (_cust.Sunset != value)
			{
				_cust.Sunset = value;
				OnPropertyChanged(nameof(_cust.Sunset));
			}
		}
	}

	public int? MaxJobs
	{
		get => _cust.MaxJobs;
		set
		{
			if (_cust.MaxJobs != value)
			{
				_cust.MaxJobs = value;
				OnPropertyChanged(nameof(_cust.MaxJobs));
			}
		}
	}

	public bool Inactive
	{
		get => _cust.Inactive;
		set
		{
			if (_cust.Inactive != value)
			{
				_cust.Inactive = value;
				OnPropertyChanged(nameof(_cust.Inactive));
			}
		}
	}

	#endregion

	#region Join Properties

	public ObservableCollection<BindJob>? Jobs { get; } = source.Jobs == null ? nullableChildren ? null : [] : [.. source.Jobs.Select(j => new BindJob(j))];

	public ObservableCollection<BindUser>? Users { get; } = source.Users == null ? nullableChildren ? null : [] : [.. source.Users.Select(u => new BindUser(u))];

	#endregion

	#region Error Properties

	public bool IsValid => !IsNameError && !IsStorkeyError;

	public bool IsNameError => _cust.Name == null || (_cust.DataLocation == DataLocationType.Cloud && !AppValidationRules.IsCloudCustomerNameValid(_cust.Name));

	public bool IsStorkeyError => _cust.StorageKey != null && !AppValidationRules.IsStorageConnectValid(_cust.StorageKey);

	#endregion

}
