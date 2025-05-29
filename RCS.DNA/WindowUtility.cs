using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace RCS.DNA;

internal static partial class WindowUtility
{
	const int GWL_STYLE = -16;
	const int WS_MAXIMIZEBOX = 0x10000;
	const int WS_MINIMIZEBOX = 0x20000;

	[LibraryImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowLongPtrW")]
	internal static partial int GetWindowLong(IntPtr hwnd, int index);

	[LibraryImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongPtrW")]
	internal static partial int SetWindowLong(IntPtr hwnd, int index, int value);

	public static void HideMinimizeAndMaximizeButtons(Window window)
	{
		IntPtr hwnd = new WindowInteropHelper(window).Handle;
		var currentStyle = GetWindowLong(hwnd, GWL_STYLE);
		SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
	}
}
