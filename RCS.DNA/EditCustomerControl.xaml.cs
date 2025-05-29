namespace RCS.DNA;

partial class EditCustomerControl : AppBaseControl
{
	public EditCustomerControl()
	{
		InitializeComponent();
	}

	void UserGrid_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Delete)
		{
			MainCommands.DisconnectCustomerChildUser.Execute(null, this);
		}
	}
}
