using System.IO;
using WF = System.Windows.Forms;

namespace RCS.DNA;

partial class JobDownloadWindow : AppBaseWindow
{
	public JobDownloadWindow()
	{
		InitializeComponent();
		Loaded += JobDownloadWindow_Loaded;
		Closing += JobDownloadWindow_Closing;
	}

	void JobDownloadWindow_Loaded(object sender, RoutedEventArgs e)
	{
		WindowUtility.HideMinimizeAndMaximizeButtons(this);
	}

	void JobDownloadWindow_Closing(object? sender, CancelEventArgs e)
	{
		if (Controller.IsDownloading)
		{
			string message = $"Downloading job {Controller.EditingJob!.Name} is in progress. Are you sure you want to close the window?";
			if (Pop.Question(this, message)) return;
			e.Cancel = true;
		}
	}

	void PickDestination_Click(object sender, RoutedEventArgs e)
	{
		var dialog = new WF.FolderBrowserDialog()
		{
			Description = "Select Download Destination Directory"
		};
		if (dialog.ShowDialog(this) == WF.DialogResult.OK)
		{
			Controller.DownloadDestinationDir = new DirectoryInfo(dialog.SelectedPath);
		}
	}

	void CloseDownload_Click(object sender, RoutedEventArgs e)
	{
		DialogResult = true;
	}

	private async void RunDownload_Click(object sender, RoutedEventArgs e)
	{
		string srcjob = Controller.SelectedDownloadCustJobPick.JobName!;
		string destdir = Controller.DownloadDestinationDir!.Name;
		await Controller.RunDownloadAsync();
	}
}
