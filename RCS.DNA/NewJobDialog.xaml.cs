namespace RCS.DNA;

partial class NewJobDialog : AppBaseWindow
{
	public NewJobDialog()
	{
		InitializeComponent();
		Loaded += JobDialog_Loaded;
		PreviewKeyUp += NewJobDialog_PreviewKeyUp;
	}

	void NewJobDialog_PreviewKeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			DialogResult = false;
		}
	}

	void JobDialog_Loaded(object sender, RoutedEventArgs e)
	{
		WindowUtility.HideMinimizeAndMaximizeButtons(this);
		DropCustomer.Focus();
	}

	private void CreateJob_Click(object sender, RoutedEventArgs e)
	{
		DialogResult = true;
	}
}
