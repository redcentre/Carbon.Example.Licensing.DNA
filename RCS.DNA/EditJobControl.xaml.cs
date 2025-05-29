namespace RCS.DNA;

partial class EditJobControl : AppBaseControl
{
	public EditJobControl()
	{
		InitializeComponent();
	}

	void UserGrid_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Delete)
		{
			MainCommands.DisconnectJobChildUser.Execute(null, this);
		}
	}

	async void FetchVartreeNames_Click(object sender, RoutedEventArgs e)
	{
		var cust = Controller.EditingJob!.Customer!;
		if (cust.StorageKey == null)
		{
			string message = $"The job's parent customer '{cust.Name}' does not have a storage key assigned. The job's variable tree names are unknown.";
			Pop.Warning(Window.GetWindow(this), message);
			return;
		}
		try
		{
			string[]? vtrnames = await Controller.FetchVartreeNamesAsync();
			if (vtrnames == null)
			{
				string message = $"The container for job {Controller.EditingJob!.Name} is inaccessible. The variable tree names are unknown.";
				Pop.Error(Window.GetWindow(this), message);
				return;
			}
			if (vtrnames.Length == 0)
			{
				string message = $"The container for job {Controller.EditingJob!.Name} does not define any variable trees.";
				Pop.Warning(Window.GetWindow(this), message);
				return;
			}
			string newJoin = string.Join(" ", vtrnames.Order());
			string oldJoin = string.Join(" ", Controller.EditingJob!.VartreeNames.Order());
			if (newJoin == oldJoin)
			{
				string message = "The job's variable tree names match the real ones. No change is needed.";
				Pop.Info(Window.GetWindow(this), message);
				return;
			}
			string message2 = $"Do you want to fill the variable tree name field from the following name(s) found in the container for job {Controller.EditingJob!.Name}?\n\n{newJoin}";
			if (Pop.Question(Window.GetWindow(this), message2))
			{
				Controller.EditingJob.VartreeNames = vtrnames;
			}
		}
		catch (Exception ex)
		{
			string message = $"Failed to read the list of vartree names for job {Controller.EditingJob!.Name}\n\n{ex.Message}";
			Pop.Error(Window.GetWindow(this), "Vartree Error", message, ex.ToString());
		}
	}
}
