using System.Collections.ObjectModel;
using System.Xml.Linq;
using Orthogonal.Common.Basic;
using RCS.Licensing.Provider.Shared.Entities;

namespace RCS.DNA.Model;

public sealed class BindRealm : NotifyBase, IBindEntity
{
	readonly Realm _realm;
	const string RootElemName = "Policy";
	const string ItemElemName = "item";
	const string NameAttrName = "name";

	public BindRealm(Realm source, bool nullableChildren = true)
	{
		_realm = source;
		ObsPolicy = [];
		if (source.Policy != null)
		{
			var element = XElement.Parse(source.Policy);
			foreach (var elem in element.Elements(ItemElemName))
			{
				string key = (string)elem.Attribute(NameAttrName)!;
				string value = (string)elem;
				ObsPolicy.Add(new RealmPolicyItem(key, value));
			}
		}
		Customers = source.Customers == null ? nullableChildren ? null : [] : [.. source.Customers.Select(c => new BindCustomer(c))];
		Users = source.Users == null ? nullableChildren ? null : [] : [.. source.Users.Select(u => new BindUser(u))];
	}

	public Realm GetEntity()
	{
		if (ObsPolicy.Count == 0)
		{
			_realm.Policy = null;
		}
		else
		{
			var element = new XElement(RootElemName,
				ObsPolicy.Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => new XElement(ItemElemName, new XAttribute(NameAttrName, x.Name!), x.Value))
			);
			_realm.Policy = element.ToString();
		}
		_realm.Customers = Customers?.Select(c => c.GetEntity()).ToArray();
		_realm.Users = Users?.Select(u => u.GetEntity()).ToArray();
		return _realm;
	}

	public void CopyPropsFrom(BindRealm other)
	{
		Name = other.Name;
		Inactive = other.Inactive;
		ConditionPolicyUpdate(other.ObsPolicy);
	}

	#region Policy Editing

	public SafeObservableCollection<RealmPolicyItem>? ObsEditingPolicy { get; set; }

	public void StartPolicyEdit()
	{
		ObsEditingPolicy = [.. ObsPolicy.Select(x => new RealmPolicyItem(x.Name, x.Value))];
	}

	public void SavePolicyEdit()
	{
		ConditionPolicyUpdate(ObsEditingPolicy!);
		ObsEditingPolicy = null;
	}

	public void CancelPolicyEdit()
	{
		ObsEditingPolicy = null;
	}

	void ConditionPolicyUpdate(SafeObservableCollection<RealmPolicyItem> source)
	{
		string sourceJoin = string.Join("+", source.Select(x => $"{x.Name}={x.Value}"));
		string targetJoin = string.Join("+", ObsPolicy.Select(x => $"{x.Name}={x.Value}"));
		if (string.Compare(sourceJoin, targetJoin, StringComparison.Ordinal) == 0) return;
		ObsPolicy.Clear();
		foreach (var item in source)
		{
			ObsPolicy.Add(item);
		}
		OnPropertyChanged(nameof(PolicyJoinedForDisplay));
	}

	#endregion

	public override int GetHashCode() => $"{Type}:{Id}".GetHashCode();

	public override string ToString() => $"BindRealm({_realm})";

	#region Realm Properties

	public EntityType Type => EntityType.Realm;

	public string? Id => _realm.Id;

	public DateTime Created => _realm.Created;

	public string Name
	{
		get => _realm.Name;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_realm.Name != newval)
			{
				_realm.Name = newval;
				OnPropertyChanged(nameof(_realm.Name));
				OnPropertyChanged(nameof(IsNameError));
			}
		}
	}

	public bool Inactive
	{
		get => _realm.Inactive;
		set
		{
			if (_realm.Inactive != value)
			{
				_realm.Inactive = value;
				OnPropertyChanged(nameof(_realm.Inactive));
			}
		}
	}

	#endregion

	#region Join Properties

	public string? PolicyJoinedForDisplay => ObsPolicy.Count == 0 ? null : string.Join(" \u25aa ", ObsPolicy.Select(x => $"{x.Name}={x.Value}"));

	public SafeObservableCollection<RealmPolicyItem> ObsPolicy { get; set; }

	public ObservableCollection<BindCustomer>? Customers { get; set; }

	public ObservableCollection<BindUser>? Users { get; set; }

	#endregion

	#region Error Properties

	public bool IsValid => !IsNameError;

	public bool IsNameError => _realm.Name == null || !AppValidationRules.IsRealmNameValid(_realm.Name);

	#endregion
}
