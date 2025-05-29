using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace RCS.DNA.Model;

static class AppUtility
{
	public static T? FindVisualParent<T>(object source) where T : DependencyObject
	{
		var dep = (DependencyObject)source;
		while (dep != null && dep is not T)
		{
			dep = VisualTreeHelper.GetParent(dep);
		}
		return dep == null ? default : (T)dep;
	}

	public static bool IsProbableJobDirectory(DirectoryInfo directory)
	{
		var rootfiles = directory.EnumerateFiles().Take(100).ToArray();
		var rootdirs = directory.EnumerateDirectories().Take(100).ToArray();
		var hasini = rootfiles.Any(f => string.Compare(f.Name, AppMetrics.JobIniFilename, StringComparison.OrdinalIgnoreCase) == 0);
		var hascasedata = rootdirs.Any(d => string.Compare(d.Name, AppMetrics.CaseDataDirname, StringComparison.OrdinalIgnoreCase) == 0);
		return hasini || hascasedata;
	}

	public static string? ToWrapString(string? value, int maxlen = 80)
	{
		if (value == null) return null;
		MatchCollection mc = Regex.Matches(value, @"(.{1," + (maxlen - 1) + @"})(?:\s|$)");
		var lines = mc.Cast<Match>().Select(m => m.Value);
		return string.Join(Environment.NewLine, lines);
	}

	public static IEnumerable<AppNode> WalkNodes(IEnumerable<AppNode> nodes)
	{
		if (nodes != null)
		{
			foreach (var node in nodes)
			{
				yield return node;
				if (node.Children != null)
				{
					foreach (var child in WalkNodes(node.Children))
					{
						yield return child;
					}
				}
			}
		}
	}

	/// <summary>
	/// This removes the single need for the IO Hashing package.
	/// See: https://gfkeogh.blogspot.com/2016/07/windows-universal-gethashcode.html
	/// </summary>
	public static long StableHash64(string value)
	{
		ulong hash = 14695981039346656037UL;
		for (int i = 0; i < value.Length; i++)
		{
			hash ^= value[i];
			hash = hash * 1099511628211UL;
			hash = (2770643476691UL * hash) + 4354685564936844689UL;
		}
		return (long)hash;
	}
}
