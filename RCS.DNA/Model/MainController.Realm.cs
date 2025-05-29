using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using RCS.Licensing.Provider.Shared.Entities;

namespace RCS.DNA.Model;

partial class MainController
{
	public async Task DeleteRealm()
	{
		string realmId = EditingRealm!.Id!;
		await WrapWork($"Deleting realm {EditingRealm!.Name}", async () =>
		{
			EditingRealm = null;
			int count = await Provider!.DeleteRealm(realmId);
			await InnerLoadNavigationTree(null);
			ObsRealmList = null;
		});
	}

	public void PrepareForNewRealm()
	{
		NewRealmName = null;
		AfterNewRealmValueChanged();
	}

	public async Task CreateRealm()
	{
		await WrapWork($"Creating new realm {NewRealmName}", async () =>
		{
			var realm = new Realm()
			{
				Id = null,
				Name = NewRealmName,
			};
			var result = await Provider!.UpsertRealm(realm);
			await InnerLoadNavigationTree(result.Entity.Id);
		});
		if (ObsUserList != null)
		{
			await LoadRealmList();
		}
	}

	public async Task ConnectRealmChildUsers()
	{
		string[] userids = SelectedPicks.Select(p => p.Id).ToArray();
		var uprealm = await Provider!.ConnectRealmChildUsers(EditingRealm!.Id, userids);
		await InnerLoadNavigationTree(uprealm.Id);
	}

	public async void DisconnectRealmChildUser()
	{
		var uprealm = await Provider!.DisconnectRealmChildUser(EditingRealm!.Id, SelectedRealmChildUser!.Id);
		await InnerLoadNavigationTree(uprealm.Id);
	}

	public async Task ConnectRealmChildCustomers()
	{
		string[] jobids = SelectedPicks.Select(p => p.Id).ToArray();
		var uprealm = await Provider!.ConnectRealmChildCustomers(EditingRealm!.Id, jobids);
		await InnerLoadNavigationTree(uprealm.Id);
	}

	public async void DisconnectRealmChildCustomer()
	{
		var uprealm = await Provider!.DisconnectRealmChildCustomer(EditingRealm!.Id, SelectedRealmChildCustomer!.Id);
		await InnerLoadNavigationTree(uprealm.Id);
	}

	public void EditRealm()
	{
		foreach (var n in SafeNavNodes!.Where(n => n.IsSelected)) n.IsSelected = false;
		var root = SafeNavNodes!.First(n => n.Type == NodeType.RealmRoot);
		var node = root.Children!.First(n => n.Realm!.Id == SelectedListRealms[0].Id);
		root.IsExpanded = true;
		node.IsSelected = true;
	}

	public void PrepareRealmPolicyEdit()
	{
		EditingRealm!.StartPolicyEdit();
	}

	public void SaveRealmPolicyEdit()
	{
		EditingRealm!.SavePolicyEdit();
		EnqueueEntitySave(EditingRealm);
	}

	public void CancelRealmPolicyEdit()
	{
		EditingRealm!.CancelPolicyEdit();
	}

	void AfterNewRealmValueChanged()
	{
		// Validates values to create a new Realm. Currently just the Name is validated.
		var list = new List<string>();
		if (NewRealmName == null) list.Add(Strings.NewRealmNoname);
		else if (!AppValidationRules.IsRealmNameValid(NewRealmName)) list.Add(Strings.NewRealmBadName);
		else if (ObsRealmPick.Any(x => string.Compare(x.Name, NewRealmName, StringComparison.OrdinalIgnoreCase) == 0)) list.Add(Strings.NewRealmDuplicateName);
		NewRealmErrors = list.Count > 0 ? list.ToImmutableArray() : null;
	}

	PropertyChangedEventHandler realmListChangeHandler;

	public async Task LoadRealmList()
	{
		await WrapWork(Strings.BusyLoadRealmList, async () =>
		{
			realmListChangeHandler ??= new PropertyChangedEventHandler(TrapRealmList_PropertyChanged);
			if (ObsRealmList != null)
			{
				ObsRealmList.ItemPropertyChanged -= realmListChangeHandler;
			}
			var realms = await Provider!.ListRealms();
			ObsRealmList = [.. realms.Select(r => new BindRealm(r)).OrderBy(r => r.Name.ToUpper())];
			ObsRealmList.ItemPropertyChanged += realmListChangeHandler;
		});
	}

	void TrapRealmList_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		var bc = (BindRealm)sender!;
		EnqueueEntitySave(bc);
	}
}
