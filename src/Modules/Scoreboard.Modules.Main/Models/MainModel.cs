using Prism.Mvvm;
using Scoreboard.Modules.Main.Models.Abstractions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Media;

namespace Scoreboard.Modules.Main.Models;

internal class MainModel : BindableBase, IMainModel
{
    private ImageSource _cleanFrame;
    public ImageSource CleanFrame
    {
        get => _cleanFrame;
        set => SetProperty(ref _cleanFrame, value);
    }
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

    private ObservableCollection<bool> _isChoosing = new ObservableCollection<bool>() { false, false, false, false, false, false, false, false, false, false, false, false, false, false };
    public ObservableCollection<bool> IsChoosing
    {
        get => _isChoosing;
        set => SetProperty(ref _isChoosing, value);
    }

    private ObservableCollection<bool> _isResizing = new ObservableCollection<bool>() { false, false, false, false, false, false, false, false, false, false, false, false, false, false };
    public ObservableCollection<bool> IsResizing
    {
        get => _isResizing;
        set => SetProperty(ref _isResizing, value);
    }

    private ObservableCollection<bool> _exists = new ObservableCollection<bool>() { false, false, false, false, false, false, false, false, false, false, false, false, false, false };
    public ObservableCollection<bool> Exists
    {
        get => _exists;
        set => SetProperty(ref _exists, value);
    }

    private ObservableCollection<string> _buttonAction = new ObservableCollection<string>() { "Создать", "Создать", "Создать", "Создать", "Создать", "Создать", "Создать", "Создать", "Создать", "Создать", "Создать", "Создать", "Создать", "Создать" };
    public ObservableCollection<string> ButtonAction
    {
        get => _buttonAction;
        set => SetProperty(ref _buttonAction, value);
    }

    private ObservableCollection<Brush> _textColor = new ObservableCollection<Brush>() { Brushes.White, Brushes.White, Brushes.White, Brushes.White, Brushes.White, Brushes.White, Brushes.White, Brushes.White, Brushes.White, Brushes.White, Brushes.White, Brushes.White, Brushes.White, Brushes.White };
    public ObservableCollection<Brush> TextColor
    {
        get => _textColor;
        set => SetProperty(ref _textColor, value);
    }

    private List<string> _cameraSettings = GetAllConnectedCameras();
    public List<string> CameraSettings
    {
        get => _cameraSettings;
        set => SetProperty(ref _cameraSettings, value);
    }

    public static List<string> GetAllConnectedCameras()
    {
        var cameraNames = new List<string>();
        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')"))
        {
            foreach (var device in searcher.Get())
            {
                cameraNames.Add(device["Caption"].ToString());
            }
        }

        return cameraNames;
    }

    private int _cameraSetting = 0;
    public int CameraSetting
    {
        get => _cameraSetting;
        set => SetProperty(ref _cameraSetting, value);
    }

    private double _fps = 1;
    public double Fps
    {
        get => _fps;
        set => SetProperty(ref _fps, value);
    }

    private string _log = "";
    public string Log
    {
        get => _log;
        set => SetProperty(ref _log, value);
    }

    private string[] _saveSettings = { "Текстовый файл", "Протокол" };
    public string[] SaveSettings
    {
        get => _saveSettings;
        set => SetProperty(ref _saveSettings, value);
    }

    private string _saveSetting = "Текстовый файл";
    public string SaveSetting
    {
        get => _saveSetting;
        set => SetProperty(ref _saveSetting, value);
    }

    private bool _isAppend = true;
    public bool IsAppend
    {
        get => _isAppend;
        set => SetProperty(ref _isAppend, value);
    }

    private bool _fpsIncreaseEnabled = true;
    public bool FpsIncreaseEnabled
    {
        get => _fpsIncreaseEnabled;
        set => SetProperty(ref _fpsIncreaseEnabled, value);
    }

    private bool _fpsDecreaseEnabled = false;
    public bool FpsDecreaseEnabled
    {
        get => _fpsDecreaseEnabled;
        set => SetProperty(ref _fpsDecreaseEnabled, value);
    }

    private bool _isDetectionEnabled = false;
    public bool IsDetectionEnabled
    {
        get => _isDetectionEnabled;
        set => SetProperty(ref _isDetectionEnabled, value);
    }
}
