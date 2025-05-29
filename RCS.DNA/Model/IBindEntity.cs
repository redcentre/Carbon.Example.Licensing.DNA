namespace RCS.DNA.Model;

public enum EntityType
{
	Customer,
	Job,
	User,
	Realm
}

public interface IBindEntity
{
	EntityType Type { get; }
	string? Id { get; }
	string Name { get; set; }
	bool IsValid { get; }
}
