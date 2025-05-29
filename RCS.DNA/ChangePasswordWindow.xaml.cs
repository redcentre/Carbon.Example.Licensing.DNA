using System.Diagnostics;
using System.IO;
using System.Text;

namespace RCS.DNA;

partial class ChangePasswordWindow : AppBaseWindow
{
	public ChangePasswordWindow()
	{
		InitializeComponent();
		Loaded += ChangePasswordWindow_Loaded;
	}

	private void ChangePasswordWindow_Loaded(object sender, RoutedEventArgs e)
	{
		TextPassword.Focus();
	}

	void GeneratePass_Clicked(object sender, RoutedEventArgs e)
	{
		if (ChkShowResults.IsChecked == true)
		{
			string html = """
			<html>
			<head>
			  <style>
				body { font-family: Calbiri,Arial,sans-serif; background-color: ghostwhite; }
				h2 { color: darkslategray; }
				.t1 { border-collapse: collapse; background-color: white; }
				  .t1 td { border: 1px solid DarkGrey; padding: 5px; white-space: nowrap; }
					.t1 td:nth-child(1) { text-align: right; }
				.s1 { font-size: 200%; font-family: Consolas,monospace; border: 1px solid grey; padding: 0.3rem 0.5rem; background-color: white; }
				.p1 { font-size: 80%; font-family: Verdana,sans-serif; margin-top: 2rem; color: darkslategray; }
			  </style>
			</head>
			<body>
			  <h2>PASSWORD CHANGED</h2>
			  <table class="t1">
				<tr><td>User Id</td><td>$ID</td></tr>
				<tr><td>User Name</td><td>$NAME</td></tr>
				<tr><td>Email</td><td>$EMAIL</td></tr>
			  </table>
			  <p>New temporary password</p>
			  <p><span class="s1">$PASS</span></p>
			  <p class="p1">$TIME</p>
			</body>
			</html>
			""";
			var sb = new StringBuilder(html);
			sb.Replace("$ID", Controller.EditingUser!.Id);
			sb.Replace("$NAME", Controller.EditingUser.Name);
			sb.Replace("$EMAIL", Controller.EditingUser.Email);
			sb.Replace("$PASS", Controller.NewPassword);
			sb.Replace("$TIME", DateTime.Now.ToString("r"));
			string filename = Path.Combine(Path.GetTempPath(), "_change_password.htm");
			File.WriteAllText(filename, sb.ToString());
			Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
		}
		DialogResult = true;
	}
}
