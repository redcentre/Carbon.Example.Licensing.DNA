namespace RCS.DNA;

partial class EditRealmControl : AppBaseControl
{
	public EditRealmControl()
	{
		InitializeComponent();
	}

	void UserGrid_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Delete)
		{
			MainCommands.DisconnectRealmChildUser.Execute(null, this);
		}
	}

	void CustomerGrid_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Delete)
		{
			MainCommands.DisconnectRealmChildCustomer.Execute(null, this);
		}
	}
}
