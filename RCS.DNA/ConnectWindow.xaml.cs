namespace RCS.DNA;

partial class ConnectWindow : AppBaseWindow
{
	public ConnectWindow()
	{
		InitializeComponent();
		Loaded += ConnectWindow_Loaded;
		Closing += ConnectWindow_Closing;
	}

	async void ConnectWindow_Loaded(object sender, RoutedEventArgs e)
	{
		WindowUtility.HideMinimizeAndMaximizeButtons(this);
		// Auto connect will only happen if the option is on, the profile has enough data
		// to connect, this is the first load of the window, and no shift keys are pressed.
		if (Controller.Metrics.AutoConnect && Controller.SelectedProfile!.CanConnect && ++Controller.ConnectPromptCount == 1 && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
		{
			await Task.Delay(1000);
			MainCommands.Connect.Execute(this, Owner);
		}
	}

	void ConnectWindow_Closing(object? sender, CancelEventArgs e)
	{
		if (Controller.IsConnecting)
		{
			Pop.Warning(this, "Cannot close the window while connecting.");
			e.Cancel = true;
		}
	}

	void ProfileList_DoubleClick(object sender, MouseButtonEventArgs e)
	{
		MainCommands.Connect.Execute(this, Owner);  // Parameter and Target are required from this dialog.
	}
}
