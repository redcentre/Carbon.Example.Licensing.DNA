using System.Globalization;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;

namespace RCS.DNA;

partial class AboutWindow : AppBaseWindow
{
	public AboutWindow()
	{
		InitializeComponent();
		Loaded += AboutWindow_Loaded;
	}

	void AboutWindow_Loaded(object sender, RoutedEventArgs e)
	{
		WindowUtility.HideMinimizeAndMaximizeButtons(this);
		LoadAsmInfo();
		FillReferences();
	}

	void LoadAsmInfo()
	{
		var asm = GetType().Assembly;
		var an = asm.GetName();
		var product = asm.GetCustomAttribute<AssemblyProductAttribute>()!.Product;
		var description = asm.GetCustomAttribute<AssemblyDescriptionAttribute>()!.Description;
		var copyright = asm.GetCustomAttribute<AssemblyCopyrightAttribute>()!.Copyright;
		var platform = asm.GetCustomAttribute<TargetPlatformAttribute>()!.PlatformName;
		var framework = asm.GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName;
		var config = asm.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;
		var build = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
		build = build.Split('.').First();
		var casm = typeof(RCS.Carbon.Shared.XDisplayProperties).Assembly;
		var can = casm.GetName();
		var cbuild = casm.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "BuildTime")?.Value;
		AboutItems = new RefItem[]
		{
			new("Version", an.Version),
			new("Build", build),
			new("Carbon Version", can.Version),
			new("Carbon Build", cbuild),
			new("Product", product),
			new("Copyright", copyright),
			new("Description", description),
			new("Provider", Controller.Provider?.ConfigSummary),
			new("Licensing", Controller.Metrics.LicensingUri),
			new("Configuration", config),
			new("Platform", platform),
			new("Framework", framework),
			new("CurrentCulture", Thread.CurrentThread.CurrentCulture),
			new("CurrentUICulture", Thread.CurrentThread.CurrentUICulture),
			new("InstalledUICulture", CultureInfo.InstalledUICulture),
		};
	}

	void FillReferences()
	{
		var refs = GetType().Assembly.GetReferencedAssemblies();
		RefItems = refs.Select(r => new RefItem(
				r.Name ?? "(no name)",
				r.Version?.ToString(),
				r.CultureInfo?.Name,
				NiceToken(r.GetPublicKeyToken())
			))
			.OrderBy(x => x.Name.ToUpper())
			.ToArray();
	}

	RefItem[]? _aboutItems;
	public RefItem[]? AboutItems
	{
		get => _aboutItems;
		set
		{
			if (_aboutItems != value)
			{
				_aboutItems = value;
				OnPropertyChanged(nameof(AboutItems));
			}
		}
	}

	RefItem[]? _refItems;
	public RefItem[]? RefItems
	{
		get => _refItems;
		set
		{
			if (_refItems != value)
			{
				_refItems = value;
				OnPropertyChanged(nameof(RefItems));
			}
		}
	}

	static string? NiceToken(byte[]? token) => token == null ? null : BitConverter.ToString(token).Replace("-", "");
}

public sealed record RefItem(string Name, object? Value, string? Culture = null, string? Token = null);
