using RCS.DNA.Model;

namespace RCS.DNA;

partial class ViewNavigationControl : AppBaseControl
{
	public ViewNavigationControl()
	{
		InitializeComponent();
	}

	void NavigationTree_SelItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		Controller.LastSelectedNavNode = (AppNode)e.NewValue;
	}

	void NavigationTree_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Delete && Controller.SelectedNavNode != null)
		{
			if (Controller.SelectedNavNode.Type == NodeType.Customer)
			{
				MainCommands.DeleteCustomer.Execute(null, this);
			}
			else if (Controller.SelectedNavNode.Type == NodeType.Job)
			{
				MainCommands.DeleteJob.Execute(null, this);
			}
			else if (Controller.SelectedNavNode.Type == NodeType.User)
			{
				MainCommands.DeleteUser.Execute(null, this);
			}
		}
	}
}
