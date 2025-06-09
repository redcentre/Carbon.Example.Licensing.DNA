using System.Windows.Controls;
using RCS.Carbon.Example.WebService.Common.DTO;

namespace RCS.DNA;

partial class ViewSessionsControl : AppBaseControl
{
	public ViewSessionsControl()
	{
		InitializeComponent();
	}

	private void Sessions_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
	{
		Controller.SelectedSessions = [.. ((DataGrid)sender).SelectedCells.Select(c => c.Item).Cast<SessionStatus>().Distinct()];
	}
}
