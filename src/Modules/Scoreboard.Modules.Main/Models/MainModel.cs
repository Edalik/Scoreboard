using Prism.Mvvm;
using Scoreboard.Modules.Main.Models.Abstractions;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Scoreboard.Modules.Main.Models;

internal class MainModel : BindableBase, IMainModel
{
    private ImageSource _frame;
    public ImageSource Frame
    {
        get => _frame;
        set => SetProperty(ref _frame, value);
    }

    private Point[] _points = new Point[28];
    public Point[] Points
    {
        get => _points;
        set => SetProperty(ref _points, value);
    }

    private ObservableCollection<bool> _isChecked = new ObservableCollection<bool>() { false, false, false, false, false, false, false, false, false, false, false, false, false, false };
    public ObservableCollection<bool> IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }

    private string _log;
    public string Log {
        get => _log;
        set => SetProperty(ref _log, value);
    }

    private string[] _cameraSettings = { "Обычная", "Веб", "IP" };
    public string[] CameraSettings
    {
        get => _cameraSettings;
        set => SetProperty(ref _cameraSettings, value);
    }

    private string _cameraSetting = "Обычная";
    public string CameraSetting
    {
        get => _cameraSetting;
        set => SetProperty(ref _cameraSetting, value);
    }

    private string _ip;
    public string IP
    {
        get => _ip;
        set => SetProperty(ref _ip, value);
    }
}
