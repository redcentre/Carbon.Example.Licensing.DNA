namespace RCS.DNA.Model;

internal class LocalizedDisplayNameAttribute(string key) : DisplayNameAttribute(Strings.ResourceManager.GetString(key)!)
{
}
