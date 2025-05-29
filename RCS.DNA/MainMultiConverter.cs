using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using RCS.DNA.Model;
using RCS.Licensing.ClientLib;
using RCS.Licensing.Provider.Shared;

namespace RCS.DNA;

internal class MainMultiConverter : IMultiValueConverter
{
	readonly List<string> newCustErrors = [];

	public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		string convarg = (string)parameter;
		if (convarg == "AnyTrue")
		{
			return values.Any(v => (bool?)v == true);
		}
		if (convarg == "TreeIcon")
		{
			var type = values[0] as NodeType?;
			var expand = values[1] as bool?;
			var inactive = values[2] as bool?;
			if (type == NodeType.CustomerRoot) return expand == true ? Images.FolderOpen16 : Images.FolderClosed16;
			else if (type == NodeType.Customer) return inactive == true ? Images.CustomerX16 : Images.Customer16;
			else if (type == NodeType.Job) return inactive == true ? Images.JobX16 : Images.Job16;
			else if (type == NodeType.JobUserRoot) return expand == true ? Images.FolderOpen16 : Images.FolderClosed16;
			else if (type == NodeType.CustomerUserRoot) return expand == true ? Images.FolderOpen16 : Images.FolderClosed16;
			else if (type == NodeType.UserCustomerRoot) return expand == true ? Images.FolderOpen16 : Images.FolderClosed16;
			else if (type == NodeType.UserJobRoot) return expand == true ? Images.FolderOpen16 : Images.FolderClosed16;
			else if (type == NodeType.JobRoot) return expand == true ? Images.FolderOpen16 : Images.FolderClosed16;
			else if (type == NodeType.UserRoot) return expand == true ? Images.FolderOpen16 : Images.FolderClosed16;
			else if (type == NodeType.User) return inactive == true ? Images.UserX16 : Images.User16;
			else if (type == NodeType.RealmRoot) return expand == true ? Images.FolderOpen16 : Images.FolderClosed16;
			else if (type == NodeType.Realm) return inactive == true ? Images.RealmX16 : Images.Realm16;
			else if (type == NodeType.RealmUserRoot) return expand == true ? Images.FolderOpen16 : Images.FolderClosed16;
			else if (type == NodeType.RealmCustomerRoot) return expand == true ? Images.FolderOpen16 : Images.FolderClosed16;
			else return Images.Unknown16;
		}
		if (convarg == "UserListSource")
		{
			var view = values[0] as ListCollectionView;
			var filt = values[1] as string;
			view?.Refresh();
			return view;
		}
		if (convarg == "UserNotEmailIconVisibility")
		{
			var name = values[0] as string;
			var isEmail = values[1] as bool?;
			return isEmail == true && name != null && !AppValidationRules.IsValidEmail(name) ? Visibility.Visible : Visibility.Collapsed;
		}
		if (convarg == "EmailRedundantIconVisibility")
		{
			var name = values[0] as string;
			var email = values[1] as string;
			var isEmail = values[2] as bool?;
			return isEmail == true && email != null && string.Compare(name, email) == 0 ? Visibility.Visible : Visibility.Collapsed;
		}
		if (convarg == "ConnectMessage")
		{
			var isConnecting = values[0] as bool?;
			var isError = values[1] as bool?;
			var canConnect = values[2] as bool?;
			var msg = values[3] as string;
			if (msg != null) return msg;
			return canConnect == true ? "Ready to connect" : Strings.ConnectLackValues;
		}
		if (convarg == "NewCustomerFeedback")
		{
			var busymsg = values[0] as string;
			var errormsg = values[1] as string;
			var custname = values[2] as string;
			var resname = values[3] as string;
			var locpick = values[4] as PickItem;
			var realmType = values[5] as NewCustomerRealmType?;
			var existRealm = values[6] as NavRealm;
			var newRealm = values[7] as string;
			var realms = values[8] as ObservableCollection<NavRealm>;
			var provider = values[9] as ILicensingProvider;
			if (busymsg != null) return busymsg;
			newCustErrors.Clear();
			if (errormsg != null) newCustErrors.Add(errormsg);
			if (!LicensingUtility.IsValidAccountName(custname)) newCustErrors.Add("Invalid customer name");
			if (resname == Strings.NoSelectText) newCustErrors.Add("No realm selected");
			if (locpick?.Name == Strings.NoSelectId) newCustErrors.Add("No location selected");
			if (provider?.SupportsRealms == true)
			{
				if (realmType == NewCustomerRealmType.Existing && existRealm == null) newCustErrors.Add("No existing realm selected");
				if (realmType == NewCustomerRealmType.New && newRealm == null) newCustErrors.Add("No new realm name entered");
				if (realmType == NewCustomerRealmType.New && realms?.Any(r => string.Compare(r.Name, newRealm, true) == 0) == true) newCustErrors.Add("New realm name already exists");
			}
			if (newCustErrors.Any()) return "\u26d4 " + string.Join(" \u25aa ", newCustErrors);
			return "Ready";
		}
		if (convarg == "CanSaveJob")
		{
			var dirty = values[0] as bool?;
			var namebad = values[1] as bool?;
			return dirty == true && namebad != true;
		}
		if (convarg == "CanStartUpload")
		{
			var dir = values[0] as DirectoryInfo;
			var running = values[1] as bool?;
			var selcount = values[2] as int?;
			var nameid = values[3] as string;
			var pass = values[4] as string;
			var pick = values[5] as CustJobPick;
			return pick?.CustomerId != null && dir?.Exists == true && running == false && selcount > 0 && nameid?.Length >= 3 && pass?.Length >= 6;
		}
		if (convarg == "CanStartDownload")
		{
			var dir = values[0] as DirectoryInfo;
			var running = values[1] as bool?;
			var pick = values[2] as CustJobPick;
			var cred = values[3] as string;
			var pass = values[4] as string;
			return running == false && pick?.CustomerId != null && dir?.Exists == true && cred?.Length >= 3 && pass?.Length >= 6;
		}
		if (convarg == "CustomerIdToName")
		{
			if (values[0] is not string id || values[1] is not ObservableCollection<NavCustomer> custnavs) return null;
			return custnavs.FirstOrDefault(c => c.Id == id)?.Name;
		}
		if (convarg == "BlobCountBack")
		{
			var max = values[0] as int?;
			var count = values[1] as int?;
			return count >= max ? Brushes.Red : Brushes.DimGray;
		}
		if (convarg == "BlobCountTip")
		{
			var max = values[0] as int?;
			var count = values[1] as int?;
			var blobs = values[2] as BlobData[];
			return count >= max ? $"The blob list limit of {max} was reached." : $"Listed blob count {count}. The maximum is {max}.";
		}
		if (convarg == "ReportSource")
		{
			var view = values[0] as ListCollectionView;
			var filter = values[1] as string;
			view?.Refresh();
			return view;
		}
		if (convarg == "PickSource")
		{
			var view = values[0] as ListCollectionView;
			var filter = values[1] as string;
			view?.Refresh();
			return view;
		}
		if (convarg == "UserRealms")
		{
			if (values[0] is not IList<NavRealm> realms || values[1] is not string userid) return null;
			var rnames = realms.Where(r => r.UserIds.Contains(userid)).Select(r => r.Name);
			return rnames.Any() ? string.Join(",", rnames) : null;
		}
		if (convarg == "CustomerRealms")
		{
			if (values[0] is not IList<NavRealm> realms || values[1] is not string custid) return null;
			var rnames = realms.Where(r => r.CustomerIds.Contains(custid)).Select(r => r.Name);
			return rnames.Any() ? string.Join(",", rnames) : null;
		}
		throw new NotImplementedException($"MainMultiConverter.Convert {parameter}");
	}

	public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException($"MainMultiConverter.ConvertBack {parameter}");
	}
}
