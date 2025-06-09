using System.Text.RegularExpressions;
using System.Windows.Data;
using CommunityToolkit.Mvvm.Input;
using RCS.Licensing.Provider.Shared;

namespace RCS.DNA.Model;

partial class MainController
{
	bool CanRunReport => BusyMessage == null && Provider != null;

	[RelayCommand(CanExecute = nameof(CanRunReport))]
	async Task RunReport()
	{
		await WrapWork(Strings.BusyDatabaseReport, async () =>
		{
			await InnerRunReport();
			TabIndex = 2;
		});
	}

	async Task InnerRunReport()
	{
		if (Provider == null) return;
		await Task.Delay(200);  // Lets the UI refresh
		ReportItems = await Provider!.GetDatabaseReport();
		var view = new ListCollectionView(ReportItems.OrderByDescending(x => x.Level).ToList())
		{
			Filter = new Predicate<object>(ReportFilterProc)
		};
		ReportView = view;
		Log($"Run report {ReportItems.Length}");
	}

	bool ReportFilterProc(object value)
	{
		var ri = (ReportItem)value;
		bool passmsg = repRegMessage == null || repRegMessage.IsMatch(ri.Message);
		return passmsg;
	}

	Regex? repRegMessage;

	string? _reportMessageFilter;
	public string? ReportMessageFilter
	{
		get => _reportMessageFilter;
		set
		{
			string? rawval = string.IsNullOrEmpty(value) ? null : value;
			if (_reportMessageFilter != rawval)
			{
				_reportMessageFilter = rawval;
				repRegMessage = rawval == null ? null : new Regex(rawval, RegexOptions.IgnoreCase);
				OnPropertyChanged(nameof(ReportMessageFilter));
			}
		}
	}
}
