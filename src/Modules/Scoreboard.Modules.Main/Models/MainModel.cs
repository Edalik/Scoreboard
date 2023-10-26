using Prism.Mvvm;
using Scoreboard.Modules.Main.Models.Abstractions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    //{ "Обычная", "Веб", "IP" }

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

    private string _cameraSetting = "Обычная";
    public string CameraSetting
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

    private bool _fpsEnabled = false;
    public bool FpsEnabled
    {
        get => _fpsEnabled;
        set => SetProperty(ref _fpsEnabled, value);
    }
}
