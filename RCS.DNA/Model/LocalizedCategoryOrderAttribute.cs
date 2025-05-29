using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace RCS.DNA.Model;

internal class LocalizedCategoryOrderAttribute(string key, int order) : CategoryOrderAttribute(Strings.ResourceManager.GetString(key)!, order);
