using System.Windows.Controls;
using RCS.DNA.Model;

namespace RCS.DNA;

internal class AppBaseControl : UserControl
{
	protected MainController Controller => (MainController)DataContext;
}
