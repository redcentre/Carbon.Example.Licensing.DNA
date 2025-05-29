namespace RCS.DNA;

partial class EditUserControl : AppBaseControl
{
	public EditUserControl()
	{
		InitializeComponent();
	}

	void JobGrid_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Delete)
		{
			MainCommands.DisconnectUserChildJob.Execute(null, this);
		}
	}

	void CustomerGrid_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Delete)
		{
			MainCommands.DisconnectUserChildCustomer.Execute(null, this);
		}
	}
}
