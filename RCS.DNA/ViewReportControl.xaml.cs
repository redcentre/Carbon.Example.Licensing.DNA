namespace RCS.DNA;

partial class ViewReportControl : AppBaseControl
{
	public ViewReportControl()
	{
		InitializeComponent();
	}

	void MessageFilter_DoubleClick(object sender, MouseButtonEventArgs e)
	{
		Controller.ReportMessageFilter = null;
	}
}
