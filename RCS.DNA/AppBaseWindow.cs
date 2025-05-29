using RCS.DNA.Model;
using WF = System.Windows.Forms;

namespace RCS.DNA;

class AppBaseWindow : Window, INotifyPropertyChanged, WF.IWin32Window
{
	public event PropertyChangedEventHandler? PropertyChanged;

	protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	protected MainController Controller => (MainController)DataContext;

	public nint Handle => new System.Windows.Interop.WindowInteropHelper(this).Handle;
}
