using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Orthogonal.Common.Basic;
using RCS.Licensing.ClientLib;
using RCS.Licensing.Example.Provider;
using RCS.Licensing.Provider;
using RCS.Licensing.Provider.Shared.Entities;

namespace RCS.DNA.Model;

partial class MainController
{
	readonly string licenceServiceAgent = $"{Strings.AppTitle.ToLowerInvariant()}/{App.AsmVersion.Major}.{App.AsmVersion.Minor}";
	readonly JsonSerializerOptions JSerOpts1 = new() { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
	public int ConnectPromptCount { get; set; }

	/// <summary>
	/// A 'Connect' is an internal name for the combined operation of authenticating against the Red Centre Software licensing
	/// service to verify permission to use this quite powerful tool by some customer, then loading the selected licensing provider
	/// and calling to load the navigation tree.
	/// </summary>
	public async Task<bool> Connect()
	{
		IsConnecting = true;
		// BUG The Connect button does not disable as soon as IsConnecting is set true. The delay doesn't work.
		ShowConnectMessage(false, "Attempting RCS licensing service authenticate.");
		await Task.Delay(500);
		try
		{
			// ┌───────────────────────────────────────────────────────────────┐
			// │  Attempt to authenticate against the RCS licensing service.   │
			// │  The account will rarely change, so it's cached for hours.    │
			// └───────────────────────────────────────────────────────────────┘
			const string LicenceCacheKey = "dna-licence-cache-v1";
			const int LicenceCacheMinutes = 120;
			string? json = SimpleFileCache.Get(LicenceCacheKey, LicenceCacheMinutes);
			if (json == null)
			{
				await Task.Delay(500);
				using var client = new LicensingClient(licenceServiceAgent, null, Metrics.LicensingUri);
				User? tempresp = await client.MiniIdAuthenticate(SelectedProfile!.LoginId!, SelectedProfile!.Password!);
				if (tempresp == null)
				{
					ShowConnectMessage(true, Strings.DNABadCredentials);
					return false;
				}
				if (!tempresp.RoleSet.Contains("DNA", StringComparer.OrdinalIgnoreCase))
				{
					ShowConnectMessage(true, Strings.DNANoAuth);
					return false;
				}
				AuthResponse = tempresp;
				json = JsonSerializer.Serialize(tempresp, JSerOpts1);
				SimpleFileCache.Put(LicenceCacheKey, json);
			}
			else
			{
				AuthResponse = JsonSerializer.Deserialize<User>(json, JSerOpts1);
			}
			// ┌───────────────────────────────────────────────────────────────┐
			// │  Prepare the required licensing provider in the profile.      │
			// │  Note that there are currently only two recognised providers  │
			// │  and hard-coding is needed to create instances. If more       │
			// │  providers are implemented then the following code needs to   │
			// │  be expanded to match.                                        │
			// └───────────────────────────────────────────────────────────────┘
			if (SelectedProfile!.RCSProviderActive)
			{
				Provider = new RedCentreLicensingProvider(SelectedProfile.RcsServiceBaseAddress!, SelectedProfile.RcsApiKey!, SelectedProfile.RcsServiceTimeout);
			}
			else
			{
				Provider = new ExampleLicensingProvider(SelectedProfile.SqlAdoConnect!, SelectedProfile.SqlProductKey);
			}
			Provider.ProviderLog += (s, e) =>
			{
				Log($"PROV {e}");
			};
			StatusMessage = Provider!.Name;
			StatusTip = Provider!.Description;
			var provtype = Provider.GetType();
			string? icoresname = provtype.Assembly.GetManifestResourceNames().FirstOrDefault(n => Path.GetExtension(n) == ".ico");
			if (icoresname != null)
			{
				using var stream = provtype.Assembly.GetManifestResourceStream(icoresname);
				ProviderIconSource = GetIco(stream);
			}
			if (ProviderIconSource == null)
			{
				using var stream = Application.GetResourceStream(new Uri("Resources/ProviderDefault.ico", UriKind.Relative)).Stream;
				ProviderIconSource = GetIco(stream);
			}
			// ┌───────────────────────────────────────────────────────────────┐
			// │  Authenticated and provider is loaded. The first provider     │
			// │  call is to load the navigation tree so the app can begin.    │
			// └───────────────────────────────────────────────────────────────┘
			ShowConnectMessage(false, Strings.ConnectNavLoading);
			await InnerLoadNavigationTree(null);
			// These profile properties need manual update and saving to avoid recursion.
			SelectedProfile!.LastConnectUtc = DateTime.UtcNow;
			Settings.Put(SelectedProfile.ProfileKey, nameof(AppProfile.LastConnectUtc), SelectedProfile.LastConnectUtc);
			SelectedProfile!.ConnectCount++;
			Settings.Put(SelectedProfile.ProfileKey, nameof(AppProfile.ConnectCount), SelectedProfile.ConnectCount);
			CarbonServiceUriPicks = [.. SelectedProfile.CarbonServiceBaseUris.Prepend("---- SELECT ----")];
			SelectedCarbonServiceUri = CarbonServiceUriPicks[0];
			await Task.Delay(200);
			ShowConnectMessage(false, Strings.ConnectSuccess);
			await Task.Delay(1000);
		}
		catch (Exception ex)
		{
			ShowConnectMessage(true, ex.Message);
			return false;
		}
		finally
		{
			IsConnecting = false;
		}
		return true;
	}

	/// <summary>
	/// Disconnect just changes the UI, as there is no server-side action like a 'logout' to perform.
	/// </summary>
	public async Task Disconnect()
	{
		IsConnectError = false;
		IsConnecting = false;
		ConnectMessage = null;
		AuthResponse = null;
		Provider = null;
		ObsNavNodes = null;
		ObsCloudNodes = null;
		ObsUserList = null;
		ViewUserList = null;
		ObsCustomerList = null;
		ObsJobList = null;
		ObsRealmList = null;
		TabIndex = 0;
		ProviderIconSource = Images.Key16;
		StatusMessage = Strings.ConnectWaiting;
		CloseAlert();
		await Task.CompletedTask;
	}

	[RelayCommand]
	void AddProfile()
	{
		int max = ObsProfiles!.Select(p => RegGroupNum().Match(p.ProfileKey))
			.Select(m => int.Parse(m.Groups[1].Value))
			.DefaultIfEmpty()
			.Max();
		int seq = max + 1;
		var prof = new AppProfile($"Profile_{seq}")
		{
			CreatedUtc = DateTime.UtcNow,
			Name = $"Profile No.{seq}"
		};
		ObsProfiles!.Add(prof);
		Settings.Put(prof.ProfileKey, nameof(AppProfile.CreatedUtc), prof.CreatedUtc);
		Settings.Put(prof.ProfileKey, nameof(AppProfile.Name), prof.Name);
		SelectedProfile = prof;
	}

	public void DeleteProfile()
	{
		Settings.DeleteGroup(SelectedProfile!.ProfileKey);
		ObsProfiles!.Remove(SelectedProfile!);
		SelectedProfile = null;
	}

	void ShowConnectMessage(bool error, string message)
	{
		IsConnectError = error;
		ConnectMessage = message;
	}

	static BitmapFrame? GetIco(Stream? stream)
	{
		if (stream == null) return null;
		var decoder = new IconBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
		return decoder.Frames.FirstOrDefault(f => f.Width == 16 && f.Height == 16);
	}

	[GeneratedRegex(@"^Profile_(\d+)$")]
	private static partial Regex RegGroupNum();
}
