namespace RCS.DNA.Model;

public class NotifyBase : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	protected void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
