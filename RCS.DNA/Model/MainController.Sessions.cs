using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using RCS.Carbon.Example.WebService.Common;
using RCS.Carbon.Example.WebService.Common.DTO;

namespace RCS.DNA.Model;

partial class MainController
{
	[RelayCommand(CanExecute = nameof(CanLoadSessions))]
	async Task LoadSessions()
	{
		if (string.IsNullOrEmpty(SelectedProfile.CarbonServiceApiKey))
		{
			WarningCallback($"A Carbon web service API Key must be set in the '{SelectedProfile.Name}' connection profile to allow session management.");
			return;
		}
		using var busy = ShowBusy("Listing sessions");
		using var client = new CarbonServiceClient(SelectedCarbonServiceUri);
		client.ApiKey = SelectedProfile.CarbonServiceApiKey;
		try
		{
			await Task.Delay(750);
			await InnerListSessions(client);
		}
		catch (Exception ex)
		{
			ShowAlert("List sessions failed", ex.Message);
		}
	}

	async Task InnerListSessions(CarbonServiceClient client)
	{
		var sessions = await client.ListSessions();
		string?[] hideIds = [SessionStatus.AnonymousSessionId];
		ObsSessions = new ObservableCollection<SessionStatus>(sessions.Where(s => !hideIds.Contains(s.SessionId)));
	}

	bool CanLoadSessions() => BusyMessage == null && SelectedCarbonServiceIndex > 0;

	public Func<SessionStatus[], bool>? ForceSessionsCallback { get; set; }

	[RelayCommand(CanExecute = nameof(CanForceSessions))]
	async Task ForceSessions()
	{
		if (!ForceSessionsCallback(SelectedSessions)) return;
		using var busy = ShowBusy("Loading sessions");
		using var client = new CarbonServiceClient(SelectedCarbonServiceUri);
		client.ApiKey = SelectedProfile.CarbonServiceApiKey;
		try
		{
			await Task.Delay(750);
			string idlist = string.Join(",", SelectedSessions.Select(s => s.SessionId));
			int count = await client.ForceSessions(idlist);
			if (count > 0)
			{
				await InnerListSessions(client);
			}
		}
		catch (Exception ex)
		{
			ShowAlert("Failed to delete selected sessions", ex.Message);
		}
	}

	bool CanForceSessions() => SelectedSessions.Length > 0 && BusyMessage == null;
}
