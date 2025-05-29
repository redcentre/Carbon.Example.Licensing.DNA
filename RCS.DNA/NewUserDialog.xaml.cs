namespace RCS.DNA;

partial class NewUserDialog : AppBaseWindow
{
	public NewUserDialog()
	{
		InitializeComponent();
		Loaded += UserDialog_Loaded;
	}

	void UserDialog_Loaded(object sender, RoutedEventArgs e)
	{
		WindowUtility.HideMinimizeAndMaximizeButtons(this);
		TextName.Focus();
	}

	void CreateUser_Click(object sender, RoutedEventArgs e)
	{
		DialogResult = true;
	}
}
