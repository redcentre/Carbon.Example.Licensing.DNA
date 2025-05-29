namespace RCS.DNA;

partial class RealmPolicyEditDialog : AppBaseWindow
{
	public RealmPolicyEditDialog()
	{
		InitializeComponent();
		Loaded += RealmDialog_Loaded;
	}

	void RealmDialog_Loaded(object sender, RoutedEventArgs e)
	{
		WindowUtility.HideMinimizeAndMaximizeButtons(this);
	}

	void OKPolicyEdit_Click(object sender, RoutedEventArgs e)
	{
		DialogResult = true;
	}
}
