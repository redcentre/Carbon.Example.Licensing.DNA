using System.IO;
using System.Threading;
using RCS.Carbon.Shared;

namespace RCS.DNA.Model;

partial class MainController
{
	public void PrepareDownload()
	{
		string downdir = Settings.Get(null, nameof(DownloadDestinationDir), null);
		if (downdir != null)
		{
			var dir = new DirectoryInfo(downdir);
			if (dir.Exists)
			{
				DownloadDestinationDir = dir;
			}
		}
		// "Customer - Job" pick selections flattened.
		var query = from c in ObsCustomerPick.Where(c => c.Id != Strings.NoSelectId)
					from jid in c.JobIds
					select new CustJobPick(c.Id, c.Name, jid, ObsJobPick.First(x => x.Id == jid).Name);
		CustJobPicks = query.ToList();
		CustJobPicks.Insert(0, new CustJobPick(null, null, null, null));
		SelectedDownloadCustJobPick = CustJobPicks.FirstOrDefault(x => x.JobId == EditingJob?.Id) ?? query.First();
		ObsDownloadProgress ??= [];
		ObsDownloadProgress!.Clear();
	}

	void AfterDownloadDestinationDirChanged()
	{
		Settings.Put(null, nameof(DownloadDestinationDir), DownloadDestinationDir!.FullName);
	}

	CancellationTokenSource? ctsDownload = null;

	public async Task RunDownloadAsync()
	{
		ObsDownloadProgress!.Clear();
		var progress = new Progress<TransferProgress>(p =>
		{
			if (p.Action == "Skip"/* || p.Action == "Start"*/) return;
			AddDownloadLog(p);
		});
		IsDownloading = true;
		ctsDownload = new CancellationTokenSource();
		Settings.Put(null, nameof(DownloadDestinationDir), DownloadDestinationDir!.FullName);
		var parameters = new JobDownloadParameters(SelectedDownloadCustJobPick.JobId, DownloadDestinationDir, 4, DownloadNewChangedOnly, DownIncludeFolders, DownExcludeFolders, DownIncludeFileGlobs, DownExcludeFileGlobs, progress, ctsDownload.Token);
		await WrapWork(Strings.BusyJobDownload, async () =>
		{
			using var wrap = await EngineWrap.CreateAsync(Provider!, SelectedProfile!.CarbonLoginSequence, SelectedProfile.CarbonLoginNameOrId!, SelectedProfile.CarbonLoginPassword!, SelectedDownloadCustJobPick.CustomerName!, SelectedDownloadCustJobPick.JobName!);
			JobDownloadResults results = await wrap.Engine.DownloadAsync(parameters);
		}, (ex) =>
		{
			//AddDownloadLog(null, true, ex.Message);
		},
		() =>
		{
			ctsDownload.Dispose();
			ctsDownload = null;
			IsDownloading = false;
		});
	}

	void AddDownloadLog(TransferProgress item)
	{
		ObsDownloadProgress!.Insert(0, item);
		if (ObsDownloadProgress.Count > Metrics.MaxLogItems)
		{
			ObsDownloadProgress.RemoveAt(Metrics.MaxLogItems);
		}
	}

	public bool CancelDownloadJob()
	{
		if (ctsDownload == null) return false;
		ctsDownload.Cancel();
		return true;
	}
}
