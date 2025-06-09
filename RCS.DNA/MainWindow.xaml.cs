using System.Windows.Threading;
using RCS.DNA.Model;

namespace RCS.DNA;

partial class MainWindow : Window
{
	DispatcherTimer? mainTimer;

	MainController Controller => (MainController)DataContext;

	public nint Handle => new System.Windows.Interop.WindowInteropHelper(this).Handle;

	public MainWindow()
	{
		InitializeComponent();
		Title = $"{Strings.AppTitle} {App.Version3}";
		if (!DesignerProperties.GetIsInDesignMode(this))
		{
			Loaded += MainWindow_Loaded;
			Closing += MainWindow_Closing;
			Closed += MainWindow_Closed;
			LoadWindowBounds();
		}
	}

	async void MainWindow_Loaded(object sender, RoutedEventArgs e)
	{
		mainTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
		mainTimer.Tick += (s, e2) =>
		{
			TimerTick();
		};
		mainTimer.Start();
		Controller.Startup();
		await Task.Delay(250);
		MainCommands.LaunchConnectPrompt.Execute(null, this);
		Controller.ForceSessionsCallback = (sessions) => ForceSessionsHandler(sessions);
		Controller.WarningCallback = (message) => Pop.Warning(this, message);
	}

	void MainWindow_Closing(object? sender, CancelEventArgs e)
	{
		mainTimer?.Stop();
		e.Cancel = false;
	}

	void MainWindow_Closed(object? sender, EventArgs e)
	{
		SaveWindowBounds();
		Controller.Shutdown();
	}

	void TimerTick()
	{
		Controller.TimerTick();
	}

	void LoadWindowBounds()
	{
		WindowStartupLocation = WindowStartupLocation.Manual;
		Rect r = SystemParameters.WorkArea;
		r.Inflate(-r.Width / 12, -r.Height / 12);
		r = Controller.Settings.Get(null, nameof(WindowStartupLocation), r);
		Top = r.Top;
		Left = r.Left;
		Width = r.Width;
		Height = r.Height;
	}

	void SaveWindowBounds()
	{
		if (WindowState == WindowState.Normal)
		{
			Controller.Settings.Put(null, nameof(WindowStartupLocation), new Rect(Left, Top, Width, Height));
		}
	}

	void UserListFilter_DoubleClick(object sender, MouseButtonEventArgs e)
	{
		Controller.FilterUserList = null;
	}
}
