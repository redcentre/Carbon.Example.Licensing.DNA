using System.Windows.Controls;
using RCS.DNA.Model;

namespace RCS.DNA;

partial class EditJobListControl : AppBaseControl
{
	public EditJobListControl()
	{
		InitializeComponent();
	}

	void JobList_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
	{
		Controller.SelectedListJobs = ((DataGrid)sender).SelectedCells.Cast<DataGridCellInfo>().Select(c => c.Item).OfType<BindJob>().Distinct().ToArray();
	}

	void JobList_DoubleClick(object sender, MouseButtonEventArgs e)
	{
		MainCommands.EditJob.Execute(null, this);
	}
}
