using System.Windows.Controls;
using RCS.DNA.Model;

namespace RCS.DNA;

partial class EditUserListControl : AppBaseControl
{
	public EditUserListControl()
	{
		InitializeComponent();
	}

	void UserSelection_Changed(object sender, SelectedCellsChangedEventArgs e)
	{
		Controller.SelectedListUsers = ((DataGrid)sender).SelectedCells.Cast<DataGridCellInfo>().Select(c => c.Item).OfType<BindUser>().Distinct().ToArray();
	}

	void User_DoubleClick(object sender, MouseButtonEventArgs e)
	{
		MainCommands.EditUser.Execute(null, this);
	}
}
