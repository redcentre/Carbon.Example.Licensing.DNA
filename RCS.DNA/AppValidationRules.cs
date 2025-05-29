using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace RCS.DNA;

partial class AppValidationRules : ValidationRule
{
	public bool Required { get; set; }

	public string? Type { get; set; }

	public override ValidationResult Validate(object value, CultureInfo cultureInfo)
	{
		string s = (string)value;
		if (Required && string.IsNullOrEmpty(s)) return new ValidationResult(false, "A non-blank vaue is required");
		if (Type == "CustomerName")
		{
			if (!IsCloudCustomerNameValid(s)) return new ValidationResult(false, "The cloud customer name syntax is unacceptable");
		}
		if (Type == "JobName")
		{
			if (!IsCloudJobNameValid(s)) return new ValidationResult(false, "The cloud job name syntax is unacceptable");
		}
		if (Type == "StorageKey")
		{
			if (!IsStorageConnectValid(s)) return new ValidationResult(false, "The storage connection string syntax is unacceptable");
		}
		return new ValidationResult(true, null);
	}

	public static bool IsCloudCustomerNameValid(string name) => RegCustomerName().IsMatch(name) && name.Length >= 3;

	public static bool IsCloudJobNameValid(string? name)
	{
		if (name == null) return false;
		if (name.Length < 3 || name.Length > 64) return false;
		if (name.Contains("--")) return false;
		if (name.EndsWith('-')) return false;
		if (name.Trim() != name) return false;
		return RegJobName().IsMatch(name);
	}

	public static bool IsUserNameValid(string name)
	{
		if (name == null) return false;
		if (name.Length < 3 || name.Length > 64) return false;
		if (name.Contains("  ")) return false;
		if (name.Trim() != name) return false;
		if (BadUserChars.Intersect([.. name]).Any()) return false;
		return true;
	}

	public static bool IsValidEmail(string email) => RegWholeEmail().IsMatch(email) && RegEmail().IsMatch(email);

	public static bool IsRealmNameValid(string name) => name.Length >= 1 && name.Length < 64 && !name.Contains('.');

	public static bool IsStorageConnectValid(string connect) => RegCustConnect().IsMatch(connect);

	public static Match CustomerConnectMatch(string connect) => RegCustConnect().Match(connect);

	static readonly char[] BadUserChars = Path.GetInvalidFileNameChars().Union([.. ",~`#$%^&()+=[]{}|\x7f"]).ToArray();

	[GeneratedRegex("^[a-z][a-z0-9]{0,63}$")]
	private static partial Regex RegCustomerName();

	[GeneratedRegex("^[a-z0-9][a-z0-9\\-]{0,63}$")]
	private static partial Regex RegJobName();

	[GeneratedRegex("^DefaultEndpointsProtocol=https;AccountName=(\\w+);AccountKey=([^;]+?)", RegexOptions.IgnoreCase, "en-AU")]
	private static partial Regex RegCustConnect();

	[GeneratedRegex(@"^[\w-]+(\.[\w-]+)*@([a-z0-9-]+(\.[a-z0-9-]+)*?\.[a-z]{2,6}|(\d{1,3}\.){3}\d{1,3})(:\d{4})?$")]
	private static partial Regex RegEmail();

	[GeneratedRegex(@"[a-zA-Z0-9@\-.]+")]
	private static partial Regex RegWholeEmail();
}

