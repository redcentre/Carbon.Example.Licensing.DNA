using WF = System.Windows.Forms;

namespace RCS.DNA;

internal static class Pop
{
	public static void Info(Window owner, string message)
	{
		MessageBox.Show(owner, message, Strings.AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);
	}

	public static void Warning(Window owner, string message)
	{
		MessageBox.Show(owner, message, Strings.AppTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
	}

	public static void Error(Window owner, string message)
	{
		MessageBox.Show(owner, message, Strings.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
	}

	public static bool Question(Window owner, string message)
	{
		return MessageBox.Show(owner, message, Strings.AppTitle, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
	}

	public static void Error(Window owner, string heading, Exception ex) => Error(owner, heading, ex.Message, ex.ToString());

	public static void Error(Window owner, string heading, string message, string? details = null)
	{
		PrePop();
		var page = new WF.TaskDialogPage
		{
			Caption = App.Product,
			Heading = heading,
			Text = message,
			Icon = WF.TaskDialogIcon.ShieldErrorRedBar,
			AllowCancel = true,
			Expander = new WF.TaskDialogExpander
			{
				Text = details,
				CollapsedButtonText = "Error Details",
			},
			Buttons = { WF.TaskDialogButton.OK },
			DefaultButton = WF.TaskDialogButton.OK,
			SizeToContent = true
		};
		if (details != null)
		{
			page.Expander = new WF.TaskDialogExpander
			{
				Text = details,
				CollapsedButtonText = "Error Details",
			};
			var copyButton = new WF.TaskDialogButton("Copy");
			copyButton.Click += (s, e) =>
			{
				Clipboard.SetText(string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}", Environment.NewLine, Strings.AppTitle, DateTime.Now.ToString("f"), heading, message, details ?? "(not details)"));
			};
			page.Buttons.Insert(0, copyButton);
		}
		WF.TaskDialog.ShowDialog(((WF.IWin32Window)owner).Handle, page);
	}

	//public static bool Question(string title, string message)
	//{
	//	PrePop();
	//	var page = new WF.TaskDialogPage
	//	{
	//		Caption = App.Product,
	//		AllowCancel = true,
	//		AllowMinimize = false,
	//		Heading = title,
	//		Icon = WF.TaskDialogIcon.ShieldWarningYellowBar,
	//		Text = message,
	//		DefaultButton = WF.TaskDialogButton.No
	//	};
	//	page.Buttons.Add(WF.TaskDialogButton.Yes);
	//	page.Buttons.Add(WF.TaskDialogButton.No);
	//	return WF.TaskDialog.ShowDialog(AppHandle, page) == WF.TaskDialogButton.Yes;
	//}

	//public static void Warning(Window owner, string title, string message)
	//{
	//	PrePop();
	//	var page = new WF.TaskDialogPage
	//	{
	//		Caption = Strings.AppTitle,
	//		AllowCancel = true,
	//		AllowMinimize = false,
	//		Icon = WF.TaskDialogIcon.ShieldWarningYellowBar,
	//		Heading = title,
	//		Text = message
	//	};
	//	WF.TaskDialog.ShowDialog(AppHandle, page);
	//}

	//static IntPtr AppHandle => ((WF.IWin32Window)Application.Current.MainWindow).Handle;

	static bool predone = false;

	static void PrePop()
	{
		if (!predone)
		{
			WF.Application.EnableVisualStyles();
			predone = true;
		}
	}
}
