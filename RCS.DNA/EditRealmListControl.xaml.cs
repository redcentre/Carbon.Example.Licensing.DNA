using System.Windows.Controls;
using RCS.DNA.Model;

namespace RCS.DNA;

partial class EditRealmListControl : AppBaseControl
{
	public EditRealmListControl()
	{
		InitializeComponent();
	}

	void RealmList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
	{
		Controller.SelectedListRealms = ((DataGrid)sender).SelectedCells.Cast<DataGridCellInfo>().Select(c => c.Item).OfType<BindRealm>().Distinct().ToArray();
	}

	void RealmList_DoubleClick(object sender, MouseButtonEventArgs e)
	{
		MainCommands.EditRealm.Execute(null, this);
	}
}
