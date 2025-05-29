using RCS.DNA.Model;

namespace RCS.DNA;

partial class ViewCloudControl : AppBaseControl
{
	public ViewCloudControl()
	{
		InitializeComponent();
	}

	void CloudTree_SelItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		Controller.SelectedCloudNode = (AppNode)e.NewValue;
	}
}
