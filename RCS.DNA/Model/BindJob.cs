using System.Collections.ObjectModel;
using RCS.Licensing.Provider.Shared;
using RCS.Licensing.Provider.Shared.Entities;

namespace RCS.DNA.Model;

public sealed class BindJob : NotifyBase, IBindEntity
{
	readonly Job _job;

	public BindJob(Job source, bool nullableChildren = true)
	{
		_job = source;
		Users = source.Users == null ? nullableChildren ? null : [] : [.. source.Users.Select(u => new BindUser(u))];
		Customer = source.Customer == null ? null : new BindCustomer(source.Customer);
	}

	public Job GetEntity()
	{
		_job.Users = Users?.Select(u => u.GetEntity()).ToArray();
		return _job;
	}

	public void CopyPropsFrom(BindJob other)
	{
		DisplayName = other.DisplayName;
		CustomerId = other.CustomerId;
		VartreeNames = (string[])other.VartreeNames.Clone();
		DataLocation = other.DataLocation;
		Sequence = other.Sequence;
		Cases = other.Cases;
		LastUpdate = other.LastUpdate;
		Description = other.Description;
		Info = other.Info;
		Logo = other.Logo;
		Url = other.Url;
		IsMobile = other.IsMobile;
		DashboardsFirst = other.DashboardsFirst;
		Inactive = other.Inactive;
	}

	public override int GetHashCode() => $"{Type}:{Id}".GetHashCode();

	public override string ToString() => $"BindJob({_job})";

	#region Job Properties

	public EntityType Type => EntityType.Job;

	public string? Id => _job.Id;

	public DateTime Created => _job.Created;

	public DataLocationType? DataLocation
	{
		get => _job.DataLocation;
		set
		{
			if (_job.DataLocation != value)
			{
				_job.DataLocation = value;
				OnPropertyChanged(nameof(_job.DataLocation));
			}
		}
	}

	public string Name
	{
		get => _job.Name;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_job.Name != newval)
			{
				_job.Name = newval;
				OnPropertyChanged(nameof(_job.Name));
				OnPropertyChanged(nameof(IsNameError));
			}
		}
	}

	public string? DisplayName
	{
		get => _job.DisplayName;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_job.DisplayName != newval)
			{
				_job.DisplayName = newval;
				OnPropertyChanged(nameof(_job.DisplayName));
			}
		}
	}

	public string? CustomerId
	{
		get => _job.CustomerId;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_job.CustomerId != newval)
			{
				_job.CustomerId = newval;
				OnPropertyChanged(nameof(_job.CustomerId));
			}
		}
	}

	//public bool CustomerIDChanged => _original?.CustomerId != CustomerId;

	public string[] VartreeNames
	{
		get => _job.VartreeNames;
		set
		{
			if (_job.VartreeNames != value)
			{
				_job.VartreeNames = value;
				OnPropertyChanged(nameof(_job.VartreeNames));
			}
		}
	}

	public string? Description
	{
		get => _job.Description;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_job.Description != newval)
			{
				_job.Description = newval;
				OnPropertyChanged(nameof(_job.Description));
			}
		}
	}

	public int? Sequence
	{
		get => _job.Sequence;
		set
		{
			if (_job.Sequence != value)
			{
				_job.Sequence = value;
				OnPropertyChanged(nameof(_job.Sequence));
			}
		}
	}

	public int? Cases
	{
		get => _job.Cases;
		set
		{
			if (_job.Cases != value)
			{
				_job.Cases = value;
				OnPropertyChanged(nameof(_job.Cases));
			}
		}
	}

	public DateTime? LastUpdate
	{
		get => _job.LastUpdate;
		set
		{
			if (_job.LastUpdate != value)
			{
				_job.LastUpdate = value;
				OnPropertyChanged(nameof(_job.LastUpdate));
			}
		}
	}

	public string? Info
	{
		get => _job.Info;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_job.Info != newval)
			{
				_job.Info = newval;
				OnPropertyChanged(nameof(_job.Info));
			}
		}
	}

	public string? Logo
	{
		get => _job.Logo;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_job.Logo != newval)
			{
				_job.Logo = newval;
				OnPropertyChanged(nameof(_job.Logo));
			}
		}
	}

	public string? Url
	{
		get => _job.Url;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_job.Url != newval)
			{
				_job.Url = newval;
				OnPropertyChanged(nameof(_job.Url));
			}
		}
	}

	public bool IsMobile
	{
		get => _job.IsMobile;
		set
		{
			if (_job.IsMobile != value)
			{
				_job.IsMobile = value;
				OnPropertyChanged(nameof(_job.IsMobile));
			}
		}
	}

	public bool DashboardsFirst
	{
		get => _job.DashboardsFirst;
		set
		{
			if (_job.DashboardsFirst != value)
			{
				_job.DashboardsFirst = value;
				OnPropertyChanged(nameof(_job.DashboardsFirst));
			}
		}
	}

	public bool Inactive
	{
		get => _job.Inactive;
		set
		{
			if (_job.Inactive != value)
			{
				_job.Inactive = value;
				OnPropertyChanged(nameof(_job.Inactive));
			}
		}
	}

	#endregion

	#region Join Properties

	public ObservableCollection<BindUser>? Users { get; }

	public BindCustomer? Customer { get; set; }

	#endregion

	#region Error Properties

	public bool IsValid => !IsNameError;

	public bool IsNameError => _job.Name == null || (Customer?.DataLocation == DataLocationType.Cloud && !AppValidationRules.IsCloudJobNameValid(_job.Name));

	#endregion
}
