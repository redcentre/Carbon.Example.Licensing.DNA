namespace RCS.DNA;

partial class NewCustomerDialog : AppBaseWindow
{
	public NewCustomerDialog()
	{
		InitializeComponent();
		Loaded += CustDialog_Loaded;
	}

	async void CustDialog_Loaded(object sender, RoutedEventArgs e)
	{
		WindowUtility.HideMinimizeAndMaximizeButtons(this);
		BoxCustName.Focus();
		await Controller.PrepareForNewCustomer();
	}
}
