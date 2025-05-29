namespace RCS.DNA.Model.Extensions;

public static class StringExtensions
{
	public static string Format(this string message, params object[] args)
	{
		return string.Format(message, args);
	}
}
