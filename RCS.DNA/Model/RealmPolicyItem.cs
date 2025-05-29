namespace RCS.DNA.Model;

public sealed class RealmPolicyItem : NotifyBase
{
	public RealmPolicyItem()
	{
	}

	public RealmPolicyItem(string? name, string? value)
	{
		Name = name;
		Value = value;
	}

	public override string ToString() => $"({Name}={Value})";

	string? _name;
	public string? Name
	{
		get => _name;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_name != newval)
			{
				_name = newval;
				OnPropertyChanged(nameof(Name));
			}
		}
	}

	string? _value;
	public string? Value
	{
		get => _value;
		set
		{
			string? newval = string.IsNullOrEmpty(value) ? null : value;
			if (_value != newval)
			{
				_value = newval;
				OnPropertyChanged(nameof(Value));
			}
		}
	}
}
