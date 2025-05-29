using System.IO;
using System.Reflection;

namespace RCS.DNA;

public partial class App : Application
{
	static App()
	{
		Assembly asm = typeof(App).Assembly;
		Company = asm.GetCustomAttribute<AssemblyCompanyAttribute>()!.Company;
		Product = asm.GetCustomAttribute<AssemblyProductAttribute>()!.Product;
		Title = asm.GetCustomAttribute<AssemblyTitleAttribute>()!.Title;
		Description = asm.GetCustomAttribute<AssemblyDescriptionAttribute>()!.Description;
		Copyright = asm.GetCustomAttribute<AssemblyCopyrightAttribute>()!.Copyright;
		Build = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
		AsmVersion = asm.GetName().Version!;
		FileVersion = asm.GetCustomAttribute<AssemblyFileVersionAttribute>()!.Version;
		HomeFolder = new DirectoryInfo(Path.GetDirectoryName(asm.Location)!);
	}

	public static string Company { get; set; }
	public static string Product { get; set; }
	public static string Title { get; set; }
	public static string Description { get; set; }
	public static string Copyright { get; set; }
	public static string Build { get; set; }
	public static Version AsmVersion { get; set; }
	public static string FileVersion { get; set; }
	public static string Version3 => $"{AsmVersion.Major}.{AsmVersion.Minor}.{AsmVersion.Build}";
	public static DirectoryInfo HomeFolder { get; set; }
}
