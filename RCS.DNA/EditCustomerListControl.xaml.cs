using System.Windows.Controls;
using RCS.DNA.Model;

namespace RCS.DNA;

partial class EditCustomerListControl : AppBaseControl
{
	public EditCustomerListControl()
	{
		InitializeComponent();
	}

	void CustList_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
	{
		Controller.SelectedListCustomers = ((DataGrid)sender).SelectedCells.Cast<DataGridCellInfo>().Select(c => c.Item).OfType<BindCustomer>().Distinct().ToArray();
	}

	void CustList_DoubleClick(object sender, MouseButtonEventArgs e)
	{
		MainCommands.EditCustomer.Execute(null, this);
	}
}
