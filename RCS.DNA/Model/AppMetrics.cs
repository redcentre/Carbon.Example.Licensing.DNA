using System.Collections.Generic;
using System.Reflection;
using Orthogonal.NSettings;

namespace RCS.DNA.Model;

[LocalizedCategoryOrder(nameof(Strings.MetricsApplicationCategory), 0)]
[LocalizedCategoryOrder(nameof(Strings.MetricsUploadCategory), 1)]
[LocalizedCategoryOrder(nameof(Strings.MetricsUserVisibilityCategory), 2)]
[LocalizedCategoryOrder(nameof(Strings.MetricsCustVisibilityCategory), 3)]
public sealed partial class AppMetrics : NotifyBase, IEditableObject
{
	public const string JobIniFilename = "job.ini";
	public const string CaseDataDirname = "CaseData";

	[Browsable(false)]
	public DateTime? LastSaveTime { get; set; }

	#region Persist

	public void Save(ISettingsProcessor processor)
	{
		LastSaveTime = DateTime.Now;
		foreach (var prop in GetType().GetProperties())
		{
			processor.Put(null, prop.Name, prop.GetValue(this));
		}
	}

	public void Load(ISettingsProcessor processor)
	{
		foreach (var prop in GetType().GetProperties())
		{
			var defattr = prop.GetCustomAttribute<DefaultValueAttribute>();
			prop.SetValue(this, processor.GetObject(null, prop.Name, prop.PropertyType) ?? defattr?.Value);
		}
	}

	#endregion

	#region IEditable

	Dictionary<string, object?>? _map;

	public void BeginEdit()
	{
		foreach (var prop in GetType().GetProperties())
		{
			_map ??= [];
			_map[prop.Name] = prop.GetValue(this);
		}
	}

	public void CancelEdit()
	{
		foreach (var prop in GetType().GetProperties())
		{
			prop.SetValue(this, _map![prop.Name]);
		}
		_map = null;
	}

	public void EndEdit()
	{
		_map = null;
	}

	#endregion
}
