using System.Collections.Generic;
using System.Collections.Immutable;
using RCS.Azure.StorageAccount;
using RCS.DNA.Model.Extensions;
using RCS.Licensing.Provider.Shared.Entities;

namespace RCS.DNA.Model;

partial class MainController
{
	public async Task DeleteJob()
	{
		await WrapWork($"Deleting job {EditingJob!.Name}", async () =>
		{
			string jobId = EditingJob!.Id!;
			EditingJob = null;
			int count = await Provider!.DeleteJob(jobId);
			await InnerLoadNavigationTree(null);
			if (ObsCloudNodes != null) await InnerCloudCompare();
			ObsJobList = null;
		});
	}

	public async Task ValidateJob()
	{
		JobValidateErrors = null;
		await WrapWork(Strings.BusyValidateJob.Format(EditingJob!.Name), async () =>
		{
			JobValidateErrors = await Provider!.ValidateJob(EditingJob!.Id);
		});
	}

	public void PrepareForNewJob()
	{
		NewJobCustomerId = EditingJob?.Id ?? Strings.NoSelectId;
		NewJobName = null;
		AfterNewJobValueChanged();
	}

	public async Task CreateJob()
	{
		await WrapWork($"Creating new job {NewJobName}", async () =>
		{
			if (ObsJobPick.Count >= AuthResponse!.MaxJobs)
			{
				throw new Exception(Strings.NewJobLimit.Format(AuthResponse.Name, AuthResponse.MaxJobs));
			}
			var job = new Job()
			{
				Id = null,
				CustomerId = NewJobCustomerId,
				Name = NewJobName,
				Description = $"New job created by {Strings.AppTitle} {App.Version3}"
			};
			var result = await Provider!.UpsertJob(job);
			var newjob = result.Entity;
			if (NewJobMakeContainer)
			{
				var cust = await Provider.ReadCustomer(NewJobCustomerId);
				if (cust.StorageKey != null)
				{
					// Create the container if the parent customer has a connection
					// for a Storage Account and the container does not already exist.
					var conlist = await SubUtil.ListContainersAsync(cust.StorageKey).ToArrayAsync();
					if (!conlist.Any(c => c.Name == newjob.Name))
					{
						var sau = new StorageAccountUtility(cust.StorageKey);
						await sau.CreateContainer(newjob.Name);
					}
				}
			}
			await InnerLoadNavigationTree(newjob.Id);
		});
		if (ObsJobList != null)
		{
			await LoadJobList();
		}
		if (ObsCloudNodes != null)
		{
			await InnerCloudCompare();
		}
	}

	public void EditJob()
	{
		foreach (var n in SafeNavNodes!.Where(n => n.IsSelected)) n.IsSelected = false;
		var root = SafeNavNodes!.First(n => n.Type == NodeType.JobRoot);
		var node = root.Children!.First(n => n.Job!.Id == SelectedListJobs[0].Id);
		root.IsExpanded = true;
		node.IsSelected = true;
	}

	void AfterNewJobValueChanged()
	{
		var list = new List<string>();
		if (NewJobCustomerId == Strings.NoSelectId) list.Add(Strings.NewJobNoCustomer);
		if (NewJobName == null) list.Add(Strings.NewJobNoName);
		else if (!AppValidationRules.IsCloudJobNameValid(NewJobName)) list.Add(Strings.NewJobBadName);
		else if (ObsJobPick.Any(x => string.Compare(x.Name, NewJobName, StringComparison.OrdinalIgnoreCase) == 0)) list.Add(Strings.NewJobDuplicateName);
		NewJobErrors = list.Count > 0 ? list.ToImmutableArray() : null;
	}

	public async Task MaterialiseJob()
	{
		await WrapWork($"Materialising new job {SelectedCloudNode!.Job!.Name}", async () =>
		{
			string jobName = SelectedCloudNode!.Job!.Name;
			string customerId = SelectedCloudNode!.Parent!.Id!;
			var job = new Job()
			{
				Id = null,
				CustomerId = SelectedCloudNode!.Parent!.Id!,
				Name = SelectedCloudNode!.Job!.Name,
				Description = $"Materialised job created by {Strings.AppTitle} {App.Version3}"
			};
			var result = await Provider!.UpsertJob(job);
			await InnerLoadNavigationTree(result.Entity.Id);
			await InnerCloudCompare();
		});
		if (ObsJobList != null)
		{
			await LoadJobList();
		}
	}

	public async Task ConnectJobChildUsers()
	{
		await WrapWork(Strings.BusyConnectJobUser, async () =>
		{
			string[] userids = SelectedPicks.Select(p => p.Id).ToArray();
			var upjob = await Provider!.ConnectJobChildUsers(EditingJob!.Id, userids);
			await InnerLoadNavigationTree(upjob.Id);
		});
	}

	public async Task DisconnectJobChildUser()
	{
		await WrapWork(Strings.BusyDisconnectJobUser, async () =>
		{
			var upjob = await Provider!.DisconnectJobChildUser(EditingJob!.Id, SelectedJobChildUser!.Id);
			await InnerLoadNavigationTree(upjob.Id);
		});
	}

	PropertyChangedEventHandler jobListChangeHandler;

	public async Task LoadJobList()
	{
		await WrapWork(Strings.BusyLoadJobList, async () =>
		{
			jobListChangeHandler ??= new PropertyChangedEventHandler(TrapJobList_PropertyChanged);
			if (ObsJobList != null)
			{
				ObsJobList.ItemPropertyChanged -= jobListChangeHandler;
			}
			var jobs = await Provider!.ListJobs();
			ObsJobList = [.. jobs.Select(j => new BindJob(j)).OrderBy(j => j.Name.ToUpper())];
			ObsJobList.ItemPropertyChanged += jobListChangeHandler;
		});
	}

	void TrapJobList_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		var bc = (BindJob)sender!;
		EnqueueEntitySave(bc);
	}

	public async Task<string[]?> FetchVartreeNamesAsync()
	{
		return await Provider!.GetRealCloudVartreeNames(EditingJob!.Id);
	}
}
