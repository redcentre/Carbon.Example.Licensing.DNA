using System.Collections.Immutable;
using System.IO;
using System.Threading;
using RCS.Carbon.Shared;

namespace RCS.DNA.Model;

partial class MainController
{
	public void PrepareUpload()
	{
		string updir = Settings.Get(null, nameof(UploadSourceDir), null);
		if (updir != null)
		{
			var dir = new DirectoryInfo(updir);
			if (dir.Exists)
			{
				UploadSourceDir = dir;
			}
		}
		// "Customer - Job" pick selections flattened.
		var query = from c in ObsCustomerPick.Where(c => c.Id != Strings.NoSelectId)
					from jid in c.JobIds
					select new CustJobPick(c.Id, c.Name, jid, ObsJobPick.First(x => x.Id == jid).Name);
		CustJobPicks = query.ToList();
		CustJobPicks.Insert(0, new CustJobPick(null, null, null, null));
		SelectedUpCustJobPick = CustJobPicks.FirstOrDefault(x => x.JobId == EditingJob?.Id) ?? query.First();
		ObsUploadProgress ??= [];
		ObsUploadProgress!.Clear();
	}

	PropertyChangedEventHandler uploadPickChangeHandler;

	void AfterUploadSourceDirChanged()
	{
		Settings.Put(null, nameof(UploadSourceDir), UploadSourceDir!.FullName);
		// Upload source files and folders with some pre-selected and counted.
		var picks = UploadSourceDir!
			.EnumerateFileSystemInfos()
			.Take(100)
			.Select(x => new UploadPick(x))
			.OrderBy(x => x.IsDirectory ? 0 : 1)
			.ThenBy(x => x.Info.FullName.ToUpperInvariant())
			.ToArray();
		string[] preselExts = Metrics.UploadExtensions == null ? [] : Metrics.UploadExtensions.Split(" ,".ToCharArray());
		string[] preselNames = Metrics.UploadDirNames == null ? [] : Metrics.UploadDirNames.Split(" ,".ToCharArray());
		foreach (var pick in picks)
		{
			if (pick.IsDirectory)
			{
				pick.IsSelected = preselNames.Contains(pick.Info.Name, StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				pick.IsSelected = preselExts.Contains(pick.Info.Extension, StringComparer.OrdinalIgnoreCase);
			}
		}
		UploadSelectedCount = picks.Count(p => p.IsSelected);
		uploadPickChangeHandler ??= new PropertyChangedEventHandler(TrapUploadPickChanged);
		if (ObsUploadPicks != null)
		{
			ObsUploadPicks.Dispose();
			ObsUploadPicks.ItemPropertyChanged -= uploadPickChangeHandler;
		}
		ObsUploadPicks = [.. picks];
		ObsUploadPicks!.ItemPropertyChanged += uploadPickChangeHandler;
	}

	void TrapUploadPickChanged(object? sender, PropertyChangedEventArgs e)
	{
		UploadSelectedCount = ObsUploadPicks!.Count(p => p.IsSelected);
	}

	CancellationTokenSource? ctsUpload = null;

	public async Task RunUploadAsync()
	{
		ObsUploadProgress!.Clear();
		var progress = new Progress<TransferProgress>(p =>
		{
			if (p.Action == "Skip"/* || p.Action == "Start"*/) return;
			AddUploadLog(p);
		});
		IsUploading = true;
		ctsUpload = new CancellationTokenSource();
		Settings.Put(null, nameof(UploadSourceDir), UploadSourceDir!.FullName);
		var sources = ObsUploadPicks.Where(p => p.IsSelected).Select(p => p.Info.Name).ToArray();
		var parameters = new JobUploadParameters(UploadSourceDir, sources, 4, UploadNewChangedOnly, progress, ctsUpload.Token);
		//var engine = new VEngine(Provider!);
		await WrapWork(Strings.BusyJobUpload, async () =>
		{
			using var wrap = await EngineWrap.CreateAsync(Provider!, SelectedProfile!.CarbonLoginSequence, SelectedProfile.CarbonLoginNameOrId!, SelectedProfile.CarbonLoginPassword!, SelectedUpCustJobPick.CustomerName!, SelectedUpCustJobPick.JobName!);
			JobUploadResults results = await wrap.Engine.UploadAsync(parameters);
		}, (ex) =>
		{
			//AddUploadLog(null, true, ex.Message);
		},
		() =>
		{
			ctsUpload.Dispose();
			ctsUpload = null;
			IsUploading = false;
		});
	}

	void AddUploadLog(TransferProgress item)
	{
		ObsUploadProgress!.Insert(0, item);
		if (ObsUploadProgress.Count > Metrics.MaxLogItems)
		{
			ObsUploadProgress.RemoveAt(Metrics.MaxLogItems);
		}
	}

	public bool CancelUploadJob()
	{
		if (ctsUpload == null) return false;
		ctsUpload.Cancel();
		return true;
	}
}
