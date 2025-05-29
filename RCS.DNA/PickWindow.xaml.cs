using System.Windows.Controls;
using RCS.DNA.Model;

namespace RCS.DNA;

partial class PickWindow : AppBaseWindow
{
	public PickWindow()
	{
		InitializeComponent();
		Loaded += PickWindow_Loaded;
	}

	private void PickWindow_Loaded(object sender, RoutedEventArgs e)
	{
		WindowUtility.HideMinimizeAndMaximizeButtons(this);
	}

	void PickGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
	{
		Controller.SelectedPicks = ((DataGrid)sender).SelectedCells.Cast<DataGridCellInfo>().Select(c => c.Item).OfType<EntityPickItem>().Distinct().ToArray();
	}

	void PickOK_Click(object sender, RoutedEventArgs e)
	{
		DialogResult = true;
	}
}
