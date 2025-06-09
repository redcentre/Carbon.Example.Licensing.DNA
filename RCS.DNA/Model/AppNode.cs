using System.Collections.ObjectModel;
using RCS.Licensing.Provider.Shared;

namespace RCS.DNA.Model;

public enum NodeType
{
	RealmRoot,
	Realm,
	RealmUserRoot,
	RealmCustomerRoot,
	CustomerRoot,
	Customer,
	Job,
	CustomerUserRoot,
	UserRoot,
	User,
	JobRoot,
	JobUserRoot,
	UserCustomerRoot,
	UserJobRoot
}

sealed class AppNode : NotifyBase
{
	public AppNode(NodeType type, object? id, string label, string? salt, bool? inactive, object? data)
	{
		string s = string.Format("{0}+{1}+{2}+{3}", type, id, label, salt);     // The salt disambiguates identical nodes in different parts of the tree.
		Uid = AppUtility.StableHash64(s);
		Type = type;
		Id = id?.ToString();
		Label = label;
		Inactive = inactive;
		if (data is NavCustomer cust) Customer = cust;
		else if (data is NavJob job) Job = job;
		else if (data is NavUser user) User = user;
		else if (data is NavRealm realm) Realm = realm;
	}

	/// <summary>
	/// The Uid of a node is composed from the 64-bit hash of the type, id, label, and salt.
	/// It's very likely that it will uniquely identify a node even across different runs of the application.
	/// </summary>
	public long Uid { get; }

	public string? Id { get; }

	public NodeType Type { get; }

	public AppNode? Parent { get; set; }

	public NavCustomer? Customer { get; }

	public NavJob? Job { get; }

	public NavUser? User { get; }

	public NavRealm? Realm { get; }

	public void AddChild(AppNode node)
	{
		Children ??= [];
		node.Parent = this;
		Children.Add(node);
	}

	ObservableCollection<AppNode>? _children;
	public ObservableCollection<AppNode>? Children
	{
		get => _children;
		set
		{
			if (_children != value)
			{
				_children = value;
				OnPropertyChanged(nameof(Children));
			}
		}
	}

	string? _label;
	public string? Label
	{
		get => _label;
		set
		{
			if (_label != value)
			{
				_label = value;
				OnPropertyChanged(nameof(Label));
			}
		}
	}

	bool? _inactive;
	public bool? Inactive
	{
		get => _inactive;
		set
		{
			if (_inactive != value)
			{
				_inactive = value;
				OnPropertyChanged(nameof(Inactive));
			}
		}
	}

	bool _isExpanded;
	public bool IsExpanded
	{
		get { return _isExpanded; }
		set
		{
			if (_isExpanded != value)
			{
				_isExpanded = value;
				OnPropertyChanged(nameof(IsExpanded));
			}
		}
	}

	bool _isSelected;
	public bool IsSelected
	{
		get { return _isSelected; }
		set
		{
			if (_isSelected != value)
			{
				_isSelected = value;
				OnPropertyChanged(nameof(IsSelected));
			}
		}
	}

	public override string ToString() => $"AppNode({Uid:X16},{Type},{Id},{Label},{Parent?.Uid:X16},{(IsSelected ? "\u261b" : "")},{(IsExpanded ? "\u25bc" : "")},{(Inactive == true ? "\u26d4" : "")},{Customer},{Job},{User},{Realm},{Children?.Count})";
}
