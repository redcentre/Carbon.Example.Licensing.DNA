namespace RCS.DNA;

partial class NewRealmDialog : AppBaseWindow
{
	public NewRealmDialog()
	{
		InitializeComponent();
		Loaded += RealmDialog_Loaded;
	}

	void RealmDialog_Loaded(object sender, RoutedEventArgs e)
	{
		WindowUtility.HideMinimizeAndMaximizeButtons(this);
		TextRealmName.Focus();
	}

	private void CreateRealm_Click(object sender, RoutedEventArgs e)
	{
		DialogResult = true;
	}
}
