namespace RCS.DNA.Model;

public enum PickEntityType
{
	Undefined,
	User,
	Customer,
	Job
}

sealed record EntityPickItem(PickEntityType Type, string Id, string Name, string? DisplayName, bool IsInactive);
