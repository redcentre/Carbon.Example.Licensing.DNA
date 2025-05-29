using System.IO;

namespace RCS.DNA.Model;

internal class UploadPick(FileSystemInfo info) : NotifyBase
{
	bool _isSelected;
	public bool IsSelected
	{
		get => _isSelected;
		set
		{
			if (_isSelected != value)
			{
				_isSelected = value;
				OnPropertyChanged(nameof(IsSelected));
			}
		}
	}

	public FileSystemInfo Info { get; } = info;

	public bool IsDirectory => Info.Attributes.HasFlag(FileAttributes.Directory);
}
