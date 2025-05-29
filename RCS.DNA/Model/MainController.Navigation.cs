using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using RCS.Licensing.Provider.Shared;

namespace RCS.DNA.Model;

partial class MainController
{
	AppNode realmsNode;
	AppNode customersNode;
	AppNode jobsNode;
	AppNode usersNode;

	// The Customer, Job and User root nodes of the navigation tree are always updated
	// to contain children that match all corresponding rows in the database. So they
	// can safely be used for lookups and similar scans.

	[RelayCommand(CanExecute = nameof(CanLoadNavigation))]
	async Task LoadNavigation()
	{
		await WrapWork(Strings.BusyLoadNavTree, async () =>
		{
			await InnerLoadNavigationTree(null);
			TabIndex = 0;
		});
	}

	bool CanLoadNavigation => BusyMessage == null && Provider != null;

	/// <summary>
	/// Loading the navigation creates collections of customers, jobs and users in memory collections
	/// in different forms that are unfortunately not unifiable. The navigation nodes that are the binding
	/// backing data for the tree have a NavXXX as their data baggage. Performing CRUD on the entities in
	/// the tree requires different branches to be updated, which is too difficult to do in-place, so the
	/// standard approach is to reload the navigation tree, and care is taken to preserve the previously
	/// expanded and selected state of the nodes if possible. The Uid on each node is very likely to be
	/// the same across different runs of the app.
	/// </summary>
	async Task InnerLoadNavigationTree(string? selectId)
	{
		// Reload the last known expended node uids and the node we want to re-select.
		// The last Uid might have been an Int32 in the past so it has to be converted safely
		// through a string to an Int64 and thereafter it will remain that way. One-time workaround.

		long[] lastExpUids = Settings.GetLongs(null, "ExpandedNavUids", []);
		string? lasts = Settings.Get(null, nameof(SelectedNavNode));
		long? lastSelUid = lasts == null ? null : long.Parse(lasts);
		int seed = 1000;

		// The nodes arrive as separate collections of types with referentially valid Ids.

		NavData navdata = await Provider!.GetNavigationData();

		// A special customer pick list with an 'empty' entry is needed in various places.

		ObsCustomerPick = new ObservableCollection<NavCustomer>(navdata!.Customers);
		ObsCustomerPick.Insert(0, new NavCustomer() { Id = Strings.NoSelectId, Name = Strings.NoSelectText });
		var rootlist = new List<AppNode>();

		#region Tree Building

		// ┌──────────────────────────────────────────────┐
		// │  Load Realms branch                          │
		// └──────────────────────────────────────────────┘

		if (Provider.SupportsRealms)
		{
			ObsRealmPick = new ObservableCollection<NavRealm>(navdata.Realms);
			realmsNode = new AppNode(NodeType.RealmRoot, null, Strings.NavNodeRealms, null, null, null);
			rootlist.Add(realmsNode);
			foreach (var realm in navdata.Realms.OrderBy(c => c.Name.ToUpperInvariant()))
			{
				var rnode = new AppNode(NodeType.Realm, realm.Id, realm.Name, null, realm.IsInactive, realm);
				realmsNode.AddChild(rnode);
				if (realm.UserIds.Length > 0)
				{
					var rurnode = new AppNode(NodeType.RealmUserRoot, ++seed, Strings.NavNodeUsers, null, null, null);
					rnode.AddChild(rurnode);
					var users = navdata.Users.Where(u => realm.UserIds.Contains(u.Id)).OrderBy(u => u.Name.ToUpper());
					foreach (var user in users)
					{
						var unode = new AppNode(NodeType.User, user.Id, user.Name, "U1", user.IsDisabled, user);
						rurnode.AddChild(unode);
					}
				}
				if (realm.CustomerIds.Length > 0)
				{
					var rcrnode = new AppNode(NodeType.RealmCustomerRoot, ++seed, Strings.NavNodeCustomers, null, null, null);
					rnode.AddChild(rcrnode);
					var custs = navdata.Customers.Where(c => realm.CustomerIds.Contains(c.Id)).OrderBy(c => c.Name.ToUpper());
					foreach (var cust in custs)
					{
						var cnode = new AppNode(NodeType.Customer, cust.Id, cust.Name, "C1", cust.Inactive, cust);
						rcrnode.AddChild(cnode);
					}
				}
			}
		}

		// ┌──────────────────────────────────────────────┐
		// │  Load Users branch                           │
		// └──────────────────────────────────────────────┘

		seed = 2000;
		ObsUserPick = new ObservableCollection<NavUser>(navdata.Users);
		usersNode = new AppNode(NodeType.UserRoot, null, Strings.NavNodeUsers, null, null, null);
		rootlist.Add(usersNode);
		foreach (var user in navdata.Users.OrderBy(u => u.Name.ToUpperInvariant()))
		{
			var unode = new AppNode(NodeType.User, user.Id, user.Name, "U2", user.IsDisabled, user);
			usersNode.AddChild(unode);
			var custs = navdata.Customers.Where(c => c.UserIds.Contains(user.Id)).OrderBy(c => c.Name.ToUpper());
			if (custs.Any())
			{
				var curoot = new AppNode(NodeType.UserCustomerRoot, ++seed, Strings.NavNodeCustomers, null, null, null);
				unode.AddChild(curoot);
				foreach (var cust in custs)
				{
					var ucnode = new AppNode(NodeType.Customer, cust.Id, cust.Name, "C2", cust.Inactive, cust);
					curoot.AddChild(ucnode);
				}
			}
			var jobs = navdata.Jobs.Where(j => j.UserIds.Contains(user.Id)).OrderBy(j => j.Name.ToUpper());
			if (jobs.Any())
			{
				var ujroot = new AppNode(NodeType.UserJobRoot, ++seed, Strings.NavNodeJobs, null, null, null);
				unode.AddChild(ujroot);
				foreach (var job in jobs)
				{
					var ujnode = new AppNode(NodeType.Job, job.Id, job.Name, "J1", job.Inactive, job);
					ujroot.AddChild(ujnode);
				}
			}
		}

		// ┌──────────────────────────────────────────────┐
		// │  Load Customers branch                       │
		// └──────────────────────────────────────────────┘

		seed = 3000;
		customersNode = new AppNode(NodeType.CustomerRoot, null, Strings.NavNodeCustomers, null, null, null);
		rootlist.Add(customersNode);
		foreach (var cust in navdata.Customers.OrderBy(c => c.Name.ToUpper()))
		{
			var cnode = new AppNode(NodeType.Customer, cust.Id, cust.Name, "C3", cust.Inactive, cust);
			customersNode.AddChild(cnode);
			var jobs = navdata.Jobs.Where(j => cust.JobIds.Contains(j.Id)).OrderBy(c => c.Name.ToUpper());
			foreach (var job in jobs)
			{
				var jnode = new AppNode(NodeType.Job, job.Id, job.Name, "J2", job.Inactive, job);
				cnode.AddChild(jnode);
				var jusers = navdata.Users.Where(u => u.JobIds.Contains(job.Id)).ToArray();
				foreach (var juser in jusers)
				{
					var junode = new AppNode(NodeType.User, juser.Id, juser.Name, "U3", juser.IsDisabled, juser);
					jnode.AddChild(junode);
				}
			}
			var users = navdata.Users.Where(u => u.CustomerIds.Contains(cust.Id)).OrderBy(c => c.Name.ToUpper());
			if (users.Any())
			{
				var curnode = new AppNode(NodeType.CustomerUserRoot, ++seed, Strings.NavNodeUsers, null, null, null);
				cnode.AddChild(curnode);
				foreach (var user in users)
				{
					var cunode = new AppNode(NodeType.User, user.Id, user.Name, "U4", user.IsDisabled, user);
					curnode.AddChild(cunode);
				}
			}
		}

		// ┌──────────────────────────────────────────────┐
		// │  Load Jobs branch                            │
		// └──────────────────────────────────────────────┘

		seed = 4000;
		ObsJobPick = new ObservableCollection<NavJob>(navdata.Jobs);
		jobsNode = new AppNode(NodeType.JobRoot, null, Strings.NavNodeJobs, null, null, null);
		rootlist.Add(jobsNode);
		foreach (var job in navdata.Jobs.OrderBy(j => j.Name.ToUpper()))
		{
			var jnode = new AppNode(NodeType.Job, job.Id, job.Name, "J3", job.Inactive, job);
			jobsNode.AddChild(jnode);
			var cust = navdata.Customers.FirstOrDefault(c => c.Id == job.CustomerId);
			if (cust != null)
			{
				var jcnode = new AppNode(NodeType.Customer, cust.Id, cust.Name, "C4", cust.Inactive, cust);
				jnode.AddChild(jcnode);
			}
			var users = navdata.Users.Where(c => c.JobIds.Contains(job.Id)).OrderBy(u => u.Name.ToUpper());
			if (users.Any())
			{
				var juroot = new AppNode(NodeType.JobUserRoot, ++seed, Strings.NavNodeUsers, null, null, null);
				jnode.AddChild(juroot);
				foreach (var user in users)
				{
					var junode = new AppNode(NodeType.User, user.Id, user.Name, "U5", user.IsDisabled, user);
					juroot.AddChild(junode);
				}
			}
		}

		// The collection of root nodes for the navigation tree.

		ObsNavNodes = new ObservableCollection<AppNode>(rootlist);

		#endregion

		// A flattened safe observable collection of all nodes in the navigation tree.
		// Handlers on all nodes detect change of expand or selection state which is
		// recorded in the settings for the next run.

		bool expandingGuard = false;
		SafeNavNodes?.Dispose();
		SafeNavNodes = [.. AppUtility.WalkNodes(ObsNavNodes!)];
		SafeNavNodes!.ItemPropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(AppNode.IsExpanded) && !expandingGuard)
			{
				// Trap a node expansion change and save up to 20 expanded node Uids.
				long[] ids = SafeNavNodes!.Where(n => n.IsExpanded).Select(n => n.Uid).Distinct().Take(20).ToArray();
				string join = string.Join(" ", ids);
				Settings.Put(null, "ExpandedNavUids", ids);
			}
			if (e.PropertyName == nameof(AppNode.IsSelected))
			{
				var node = (AppNode?)s;
				if (node?.IsSelected == true)
				{
					// The node selection may change rapidly, so it's throttled by a timer when the property changes.
					// After a pause that gives us the final node selection it cracks the node details to display
					// the required corresponding control on the right.
					LastSelectedNavNode = node;
				}
			}
		};

		// Re-expand the last known expanded nodes.

		expandingGuard = true;
		foreach (var node in SafeNavNodes.Where(n => !n.IsExpanded && n.Children?.Count > 0))
		{
			node.IsExpanded = lastExpUids.Contains(node.Uid);
		}
		expandingGuard = false;

		// Try to re-select the last selected node.

		var selnode = SafeNavNodes.FirstOrDefault(x => x.Uid == lastSelUid);
		if (selnode != null)
		{
			selnode.IsSelected = true;
		}

		//foreach (var node in SafeNavNodes!) Trace.WriteLine($"{node}");
		Log($"Load navigation {navdata.Customers.Length} {navdata.Jobs.Length} {navdata.Users.Length}");
	}

	#region Lazy Node Selection

	AppNode? _lastSelectedNavNode;
	/// <summary>
	/// Change of node selection restarts a timer that will lazily take the
	/// last one after a delay as being the actually selected one (see below).
	/// </summary>
	public AppNode? LastSelectedNavNode
	{
		get => _lastSelectedNavNode;
		set
		{
			if (_lastSelectedNavNode != value)
			{
				_lastSelectedNavNode = value;
				navtimer.Stop();
				navtimer.Start();
			}
		}
	}

	private void Navtimer_Tick(object? sender, EventArgs e)
	{
		navtimer.Stop();
		_ = AfterNodeSelected();
	}

	/// <summary>
	/// After a possible burst of nav node selections we now know the final one.
	/// The node type and details are cracked to set the correct properties that
	/// will cause the appropriate collections to load and display the matching
	/// control on the right of the mnavigation control.
	/// </summary>
	async Task AfterNodeSelected()
	{
		AppNode[] selnodes = SafeNavNodes!.Where(n => n.IsSelected).ToArray();
		// Remove previous editing change handlers.
		if (EditingCustomer != null)
		{
			EditingCustomer.PropertyChanged -= editingChangeHandler;
		}
		if (EditingJob != null)
		{
			EditingJob.PropertyChanged -= editingChangeHandler;
		}
		if (EditingUser != null)
		{
			EditingUser.PropertyChanged -= editingChangeHandler;
		}
		if (EditingRealm != null)
		{
			EditingRealm.PropertyChanged -= editingChangeHandler;
		}
		// The last selected node (in a possible rush) becomes the selection node.
		// Various node types may trigger the loading of an entity for editing
		// or the loading of a list of entities for selection.
		SelectedNavNode = _lastSelectedNavNode;
		Settings.Put(null, nameof(SelectedNavNode), SelectedNavNode?.Uid);
		Log($"Nav select {SelectedNavNode?.Uid:X16} {SelectedNavNode?.Type} {SelectedNavNode?.Id}");
		if (SelectedNavNode?.Type == NodeType.CustomerRoot)
		{
			if (ObsCustomerList == null)
			{
				await LoadCustomerList();
			}
		}
		if (SelectedNavNode?.Type == NodeType.JobRoot)
		{
			if (ObsJobList == null)
			{
				await LoadJobList();
			}
		}
		if (SelectedNavNode?.Type == NodeType.UserRoot)
		{
			if (ObsUserList == null)
			{
				await LoadUserList();
			}
		}
		if (SelectedNavNode?.Type == NodeType.RealmRoot)
		{
			if (ObsRealmList == null)
			{
				await LoadRealmList();
			}
		}
		else if (SelectedNavNode?.Type == NodeType.Customer)
		{
			await WrapWork($"Reading customer Id {SelectedNavNode.Id}", async () =>
			{
				var cust = await Provider!.ReadCustomer(SelectedNavNode.Id);
				EditingCustomer = new BindCustomer(cust, false);
				EditingCustomer!.PropertyChanged += editingChangeHandler;
			});
		}
		else if (SelectedNavNode?.Type == NodeType.Job)
		{
			await WrapWork($"Reading job Id {SelectedNavNode.Id}", async () =>
			{
				var job = await Provider!.ReadJob(SelectedNavNode.Id);
				EditingJob = new BindJob(job, false);
				EditingJob!.PropertyChanged += editingChangeHandler;
			});
		}
		else if (SelectedNavNode?.Type == NodeType.User)
		{
			await WrapWork($"Reading user Id {SelectedNavNode.Id}", async () =>
			{
				var user = await Provider!.ReadUser(SelectedNavNode.Id);
				EditingUser = new BindUser(user, false);
				EditingUser!.PropertyChanged += editingChangeHandler;
			});
		}
		else if (SelectedNavNode?.Type == NodeType.Realm)
		{
			await WrapWork($"Reading realm Id {SelectedNavNode.Id}", async () =>
			{
				var realm = await Provider!.ReadRealm(SelectedNavNode.Id);
				EditingRealm = new BindRealm(realm, false);
				EditingRealm!.PropertyChanged += editingChangeHandler;
			});
		}
	}

	#endregion
}
