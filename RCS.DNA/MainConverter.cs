using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Media;
using RCS.DNA.Model;
using RCS.Licensing.Provider.Shared;

namespace RCS.DNA;

internal partial class MainConverter : IValueConverter
{
	public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		string convarg = (string)parameter;
		if (convarg == "None")
		{
			return value == null;
		}
		if (convarg == "Some")
		{
			return value != null;
		}
		if (convarg == "ArraySome")
		{
			var arr = (Array?)value;
			return arr?.Length > 0;
		}
		if (convarg == "Not")
		{
			return !(bool)value;
		}
		if (convarg == "TrueVisible")
		{
			return (bool)value ? Visibility.Visible : Visibility.Collapsed;
		}
		if (convarg == "TrueVisibleHide")
		{
			return (bool)value ? Visibility.Visible : Visibility.Hidden;
		}
		if (convarg == "FalseCollapseVisible")
		{
			return !(bool)value ? Visibility.Collapsed : Visibility.Visible;
		}
		if (convarg == "SomeVisible")
		{
			return value != null ? Visibility.Visible : Visibility.Collapsed;
		}
		if (convarg == "NoneVisible")
		{
			return value == null ? Visibility.Visible : Visibility.Collapsed;
		}
		if (convarg == "StringArray")
		{
			var arr = (string[])value;
			return arr == null ? null : string.Join(",", arr.Where(a => !string.IsNullOrWhiteSpace(a)));
		}
		var regjoin = RegErrorJoin().Match(convarg);
		if (regjoin.Success)
		{
			var errors = (ImmutableArray<string>?)value;
			if (errors == null) return $"Ready to create {regjoin.Groups[1].Value}";
			return string.Join(" \u25aa ", errors);
		}
		if (convarg == "NewRecordErrorFore")
		{
			var errors = (ImmutableArray<string>?)value;
			if (errors != null) return Brushes.DarkRed;
			return Brushes.DarkSlateGray;
		}
		if (convarg == "ShowUserJobAlert")
		{
			var user = (BindUser?)value;
			if (user?.Jobs == null) return Visibility.Collapsed;
			bool anydups = user.Jobs.GroupBy(j => j.Name).Select(g => new { Name = g.Key, Count = g.Count() }).Any(x => x.Count > 1);
			return anydups ? Visibility.Visible : Visibility.Collapsed;
		}
		if (convarg == "StringFilterArray")
		{
			var ss = (string[]?)value;
			if (ss?.Length > 0) return string.Join(" ", ss);
			return null;
		}
		if (convarg == "DataLocation")
		{
			var loc = (DataLocationType?)value;
			return loc == null ? string.Empty : loc.ToString();
		}
		if (convarg == "LocalTime6")
		{
			var time = (DateTime?)value;
			return time?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
		}
		if (convarg == "LocalTime4")
		{
			var time = (DateTime?)value;
			return time?.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
		}
		if (convarg == "LogTime")
		{
			return ((DateTime)value).ToLocalTime().ToString("HH:mm:ss.fff");
		}
		if (convarg == "LogId")
		{
			var id = (int?)value;
			return id?.ToString("X8")[..4];
		}
		if (convarg == "ConnectMessageFore")
		{
			return (bool?)value == true ? Brushes.Red : SystemColors.WindowTextBrush;
		}
		if (convarg == "NewCustomerFeedbackFore")
		{
			var msg = (string?)value;
			return msg?.Length > 0 ? Brushes.Red : SystemColors.WindowTextBrush;
		}
		if (convarg == "LogSecs")
		{
			var secs = (double?)value;
			return secs?.ToString("F2");
		}
		if (convarg == "ZeroVisible")
		{
			return (int?)value > 0 ? Visibility.Hidden : Visibility.Visible;
		}
		if (convarg == "DefParamRequired")
		{
			return (bool)value ? "Required" : "Optional";
		}
		if (convarg == "GridJobIcon")
		{
			return (bool?)value == true ? Images.JobX16 : Images.Job16;
		}
		if (convarg == "UploadPickIcon")
		{
			return (bool?)value == true ? Images.FolderClosed16 : Images.File16;
		}
		if (convarg == "GridUserIcon")
		{
			return (bool?)value == true ? Images.UserX16 : Images.User16;
		}
		if (convarg == "GridRealmIcon")
		{
			return (bool?)value == true ? Images.RealmX16 : Images.Realm16;
		}
		if (convarg == "GridCustomerIcon")
		{
			return (bool?)value == true ? Images.CustomerX16 : Images.Customer16;
		}
		if (convarg == "SortBindUsers")
		{
			return ((IEnumerable<BindUser>)value).OrderBy(u => u.Name.ToUpper());
		}
		if (convarg == "SortBindCustomers")
		{
			return ((IEnumerable<BindCustomer>)value).OrderBy(c => c.Name.ToUpper());
		}
		if (convarg == "SortBindJobs")
		{
			return ((IEnumerable<BindJob>)value).OrderBy(j => j.Name.ToUpper());
		}
		if (convarg == "PickIcon")
		{
			var item = (EntityPickItem)value;
			if (item.Type == PickEntityType.User) return item.IsInactive ? Images.UserX16 : Images.User16;
			else if (item.Type == PickEntityType.Customer) return item.IsInactive ? Images.CustomerX16 : Images.Customer16;
			else if (item.Type == PickEntityType.Job) return item.IsInactive ? Images.JobX16 : Images.Job16;
			return Images.Unknown16;
		}
		if (convarg == "ReportIcon")
		{
			var level = (int)value;
			if (level == 0) return Images.ReportLevel0;
			else if (level == 1) return Images.ReportLevel1;
			else if (level == 2) return Images.ReportLevel2;
			else return Images.Unknown16;
		}
		if (convarg == "GridJobIcon")
		{
			return (bool?)value == true ? Images.JobX16 : Images.Job16;
		}
		if (convarg == "GridJobTooltip")
		{
			var job = (BindJob?)value;
			return job == null ? null : $"Job Id {job.Id} Customer Id {job.CustomerId}";
		}
		if (convarg == "ShowDate")
		{
			var date = (DateTime?)value;
			return date?.ToShortDateString();
		}
		if (convarg == "DialogMessageFore")
		{
			return (bool)value ? Brushes.DarkRed : Brushes.DarkSlateGray;
		}
		if (convarg == "NavNodeTip")
		{
			var node = (AppNode)value;
			string tip = $"Type: {node.Type} \u25aa Uid: {node.Uid:X16}";
			if (node.Customer != null) tip += $" \u25aa Customer {node.Customer.Id} '{node.Customer.Name}'";
			else if (node.Job != null) tip += $" \u25aa Job {node.Job.Id} '{node.Job.Name}'";
			else if (node.User != null) tip += $" \u25aa User {node.User.Id} '{node.User.Name}'";
			else if (node.Realm != null) tip += $" \u25aa User {node.Realm.Id} '{node.Realm.Name}'";
			return tip;
		}
		if (convarg == "CompareText")
		{
			var node = (AppNode?)value;
			if (node == null) return "No selection";
			if (node.Type == NodeType.Customer)
			{
				if (node.Customer!.CompareError != null) return node.Customer.CompareError;
				else return Strings.StorageAndCustomerOK;
			}
			else if (node.Type == NodeType.Job)
			{
				var njob = node.Job!;
				if (njob.CompareState == JobState.OK) return Strings.ContainerAndJobOK;
				if (njob.CompareState == JobState.OrphanContainer) return Strings.ContainerOrphan;
				else if (njob.CompareState == JobState.OrphanJobRecord) return Strings.JobOrphan;
			}
			return null;
		}
		if (convarg == "CompareTextFore")
		{
			var node = (AppNode?)value;
			if (node != null)
			{
				if (node.Type == NodeType.Customer)
				{
					if (node?.Customer!.CompareError != null) return Brushes.Red;
				}
				else if (node?.Type == NodeType.Job)
				{
					var njob = node.Job!;
					if (njob.CompareState != JobState.OK) return Brushes.Red;
				}
			}
			return SystemColors.WindowTextBrush;
		}
		if (convarg == "IsGoodJob")
		{
			var node = (AppNode?)value;
			if (node?.Type == NodeType.Job)
			{
				var njob = node.Job!;
				return njob.CompareState == JobState.OK;
			}
			return false;
		}
		if (convarg == "PassHash")
		{
			var bytes = (byte[]?)value;
			if (bytes == null) return null;
			string hex = BitConverter.ToString(bytes).Replace("-", "");
			if (hex.Length > 32)
			{
				hex = string.Concat(hex.AsSpan(0, 32), "\u2026");
			}
			return hex;
		}
		if (convarg == "ShowRoles")
		{
			return value is not string[] roles ? null : string.Join(" ", roles);
		}
		var m1 = RegDateLocal().Match(convarg);
		if (m1.Success)
		{
			var date = (DateTime?)value;
			return date?.ToLocalTime().ToString(m1.Groups[1].Value);
		}
		var m2 = RegNodeTypeVisible().Match(convarg);
		if (m2.Success)
		{
			var node = (AppNode)value;
			return node?.Type.ToString() == m2.Groups[1].Value ? Visibility.Visible : Visibility.Collapsed;
		}
		string[] tokens = convarg.Split("|".ToCharArray());
		if (tokens[0] == "EnumBool")
		{
			return tokens[1] == value.ToString();
		}
		if (tokens[0] == "NumVisible")
		{
			return (int)value == int.Parse(tokens[1]) ? Visibility.Visible : Visibility.Collapsed;
		}
		if (tokens[0] == "CloudPanelVisible")
		{
			var node = value as AppNode;
			if (value == null) return Visibility.Hidden;
			return node?.Type.ToString() == tokens[1] ? Visibility.Visible : Visibility.Hidden;
		}
		if (convarg == "Int32z")
		{
			var n = (int?)value;
			return n > 0 ? n : null;
		}
		if (convarg == "CustomerPickItem")
		{
			string id = (string)value;
			return id ?? Strings.NoSelectId;
		}
		var m3 = RegLabelDate().Match(convarg);
		if (m3.Success)
		{
			string label = m3.Groups[1].Value;
			string show = value == null ? "—" : ((DateTime)value).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
			return $"{label} {show}";
		}
		throw new ArgumentException($"MainConverter.Convert {parameter}");
	}

	public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		string convarg = (string)parameter;
		if (convarg == "StringArray")
		{
			var joined = (string)value;
			if (string.IsNullOrEmpty(joined)) return Array.Empty<string>();
			return joined.Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		}
		if (convarg == "StringFilterArray")
		{
			var s = (string?)value;
			if (string.IsNullOrEmpty(s)) return null;
			return s.Split(" ,;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		}
		if (convarg == "DataLocation")
		{
			string locs = (string)value;
			return locs.Length == 0 ? (DataLocationType?)null : Enum.Parse<DataLocationType>(locs);
		}
		string[] tokens = convarg.Split("|".ToCharArray());
		if (tokens[0] == "EnumBool")
		{
			var enumval = Enum.Parse(targetType, tokens[1]);
			return (bool)value ? enumval : Binding.DoNothing;
		}
		if (convarg == "Int32z")
		{
			var n = (int?)value;
			return n > 0 ? n : null;
		}
		if (convarg == "CustomerPickItem")
		{
			string id = (string)value;
			return id == Strings.NoSelectId ? null : id;
		}
		throw new ArgumentException($"MainConverter.ConvertBack {parameter}");
	}

	[GeneratedRegex(@"DateLocal\|(\w+)")]
	private static partial Regex RegDateLocal();

	[GeneratedRegex(@"NodeTypeVisible\|(\w+)")]
	private static partial Regex RegNodeTypeVisible();

	[GeneratedRegex(@"^NewRecordErrorJoin\|(\w+)$")]
	private static partial Regex RegErrorJoin();

	[GeneratedRegex(@"^LabelDate\|(.+)$")]
	private static partial Regex RegLabelDate();
}
