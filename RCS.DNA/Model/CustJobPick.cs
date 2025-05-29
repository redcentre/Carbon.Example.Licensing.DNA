namespace RCS.DNA.Model;

record CustJobPick(string? CustomerId, string? CustomerName, string? JobId, string? JobName)
{
	public string PickText => CustomerId == null ? Strings.NoSelectText : $"{CustomerName} - {JobName}";
}
