namespace RCS.DNA.Model;

internal class LocalizedCategoryAttribute(string key) : CategoryAttribute(key)
{
	protected override string? GetLocalizedString(string value) => Strings.ResourceManager.GetString(value)!;
}
