using System.IO;
using RCS.DNA.Model;
using RCS.DNA.Model.Extensions;
using WF = System.Windows.Forms;

namespace RCS.DNA;

partial class JobUploadWindow : AppBaseWindow
{
	public JobUploadWindow()
	{
		InitializeComponent();
		Loaded += JobUploadWindow_Loaded;
		Closing += JobUploadWindow_Closing;
	}

	void JobUploadWindow_Loaded(object sender, RoutedEventArgs e)
	{
		WindowUtility.HideMinimizeAndMaximizeButtons(this);
	}

	void JobUploadWindow_Closing(object? sender, CancelEventArgs e)
	{
		if (Controller.IsUploading)
		{
			string message = $"Uploading to job {Controller.EditingJob!.Name} is in progress. Are you sure you want to close the window?";
			if (Pop.Question(this, message)) return;
			e.Cancel = true;
		}
	}

	void PickSource_Click(object sender, RoutedEventArgs e)
	{
		var dialog = new WF.FolderBrowserDialog()
		{
			Description = "Select Upload Source Directory"
		};
		if (dialog.ShowDialog(this) == WF.DialogResult.OK)
		{
			var source = new DirectoryInfo(dialog.SelectedPath);
			if (!AppUtility.IsProbableJobDirectory(source))
			{
				string message = Strings.NotJobDirectoryPrompt.Format(source.FullName);
				if (!Pop.Question(this, message)) return;
			}
			Controller.UploadSourceDir = new DirectoryInfo(dialog.SelectedPath);
		}
	}

	void CloseUpload_Click(object sender, RoutedEventArgs e)
	{
		DialogResult = true;
	}

	private async void RunUpload_Click(object sender, RoutedEventArgs e)
	{
		// Issue #87 - Ask for confirmation if the source and destination job names mismatch.
		string srcname = Controller.UploadSourceDir!.Name;
		string destname = Controller.SelectedUpCustJobPick.JobName!;
		if (string.Compare(srcname, destname, true) != 0)
		{
			string message = $"Upload source folder job name '{srcname}' and destination job name '{destname}' do not match.\n\nDo you want to continue the upload with mismatched names?";
			if (!Pop.Question(this, message)) return;
		}
		await Controller.RunUploadAsync();
	}
}
