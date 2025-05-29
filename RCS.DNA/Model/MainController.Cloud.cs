using System.Collections.Generic;
using System.Windows.Threading;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Input;
using RCS.Licensing.Provider.Shared;

namespace RCS.DNA.Model;

sealed partial class MainController
{
	bool CanCloudCompare => BusyMessage == null && Provider != null && ObsCustomerPick?.Count > 1;

	[RelayCommand(CanExecute = nameof(CanCloudCompare))]
	async Task CloudCompare()
	{
		await WrapWork(Strings.ComparingBusy, async () =>
		{
			await InnerCloudCompare();
			TabIndex = 1;
		});
	}

	async Task InnerCloudCompare()
	{
		if (Provider == null) return;
		await Task.Delay(200);  // Lets the UI refresh
		XElement elem = await Provider!.CompareJobsAndContainers();
		var roots = new List<AppNode>();
		foreach (var celem in elem.Elements("Customer"))
		{
			var cid = (string?)celem.Attribute("Id");
			var cname = (string)celem.Attribute("Name")!;
			var dispname = (string?)celem.Attribute("DisplayName");
			var clabel = cname;
			var cerr = celem.Element("Error");
			var errtype = (string?)cerr?.Attribute("Type");
			var errmsg = cerr?.Value;
			if (errtype != null)
			{
				clabel += $" ({errtype})";
			}
			var cnode = new AppNode(NodeType.Customer, cid, clabel, null, errmsg != null, new NavCustomer() { Id = cid, Name = cname, DisplayName = dispname, Inactive = errmsg != null, CompareError = errmsg });
			roots.Add(cnode);
			foreach (var jelem in celem.Elements("Job"))
			{
				var jid = (string?)jelem.Attribute("Id");
				var jname = (string)jelem.Attribute("Name")!;
				var jlabel = jname;
				var state = (JobState)(int)jelem.Attribute("State")!;
				if (state == JobState.OrphanContainer)
				{
					jlabel += Strings.CloudNodeOrphanContainer;
				}
				else if (state == JobState.OrphanJobRecord)
				{
					jlabel += Strings.CloudNodeOrphanJob;
				}
				var jnode = new AppNode(NodeType.Job, jid, jlabel, null, state != 0, new NavJob() { Id = jid, Name = jname, Inactive = state != 0, CompareState = state });
				cnode.AddChild(jnode);
			}
			cnode.IsExpanded = true;
		}
		ObsCloudNodes = [.. roots.OrderBy(n => n.Label!.ToUpper())];
		Log($"Cloud compare -> {ObsCloudNodes!.Count}");
	}

	void AfterSelectedCloudNodeChanged()
	{
		Dispatcher.CurrentDispatcher.InvokeAsync(async () => await LoadBlobs());
	}

	/// <summary>
	/// A cloud comparison node has been selected.
	/// </summary>
	async Task LoadBlobs()
	{
		BlobDatas = null;
		if (SelectedCloudNode == null) return;
		if (SelectedCloudNode.Type == NodeType.Customer)
		{
			DisplayCustomer();
		}
		else if (SelectedCloudNode.Type == NodeType.Job)
		{
			await LoadBlobList();
		}
	}

	void DisplayCustomer()
	{
		var navcust = SelectedCloudNode!.Customer!;
		var map = new Dictionary<string, object?>
		{
			{ Strings.LabelAppId, navcust.Id },
			{ Strings.LabelAppName, navcust.Name },
			{ Strings.LabelAppDisplayName, navcust.DisplayName ?? "-" },
			{ Strings.LabelAppInactive, navcust.Inactive }
		};
		CloudCustomerMap = map;
	}

	async Task LoadBlobList()
	{
		// The login profile might contain the credentials for the Carbon engine.
		// If not, then all we can do is show a warning message and return.
		if (SelectedProfile!.CarbonLoginNameOrId == null || SelectedProfile.CarbonLoginPassword == null)
		{
			BlobListErrorMessage = "Carbon login credentials are not provided for a blob list.";
			return;
		}
		NavJob job = SelectedCloudNode!.Job!;
		if (job.CompareState == JobState.OrphanJobRecord)
		{
			BlobListErrorMessage = Strings.CloudJobNoContainer;
			return;
		}
		var cust = SelectedCloudNode.Parent!.Customer!;
		using var busy = ShowBusy(Strings.ListJobsBusy);
		try
		{
			using var wrap = await EngineWrap.CreateAsync(Provider!, SelectedProfile.CarbonLoginSequence, SelectedProfile.CarbonLoginNameOrId, SelectedProfile.CarbonLoginPassword, cust.Name, job.Name);
			BlobDatas = await wrap.Engine.ListJobBlobs(cust.Name, job.Name).Take(Metrics.MaxListBlobs).ToArrayAsync();
			Log($"Load blobs -> {BlobDatas!.Length}");
			BlobListErrorMessage = null;
		}
		catch (Exception cex)
		{
			BlobListErrorMessage = cex.Message;
		}
	}
}
