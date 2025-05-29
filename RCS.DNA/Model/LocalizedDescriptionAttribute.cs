namespace RCS.DNA.Model;

internal class LocalizedDescriptionAttribute(string key) : DescriptionAttribute(Strings.ResourceManager.GetString(key)!)
{
}
