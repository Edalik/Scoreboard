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

    private Point[] _points = new Point[38];
    public Point[] Points
    {
        get => _points;
        set => SetProperty(ref _points, value);
    }

    private ObservableCollection<bool> _isChecked = BoolCollection();
    public ObservableCollection<bool> IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }

    private ObservableCollection<bool> _isChoosing = BoolCollection();
    public ObservableCollection<bool> IsChoosing
    {
        get => _isChoosing;
        set => SetProperty(ref _isChoosing, value);
    }

    private ObservableCollection<bool> _isResizing = BoolCollection();
    public ObservableCollection<bool> IsResizing
    {
        get => _isResizing;
        set => SetProperty(ref _isResizing, value);
    }

    private ObservableCollection<bool> _exists = BoolCollection();
    public ObservableCollection<bool> Exists
    {
        get => _exists;
        set => SetProperty(ref _exists, value);
    }

    public static ObservableCollection<bool> BoolCollection()
    {
        var boolCollection = new ObservableCollection<bool>();
        for (int i = 0; i < 19; i++)
            boolCollection.Add(false);
        return boolCollection;
    }

    private ObservableCollection<string> _buttonAction = ButtonActionCollection();
    public ObservableCollection<string> ButtonAction
    {
        get => _buttonAction;
        set => SetProperty(ref _buttonAction, value);
    }

    public static ObservableCollection<string> ButtonActionCollection()
    {
        var boolCollection = new ObservableCollection<string>();
        for (int i = 0; i < 19; i++)
            boolCollection.Add("Создать");
        return boolCollection;
    }

    private ObservableCollection<Brush> _textColor = TextColorCollection();
    public ObservableCollection<Brush> TextColor
    {
        get => _textColor;
        set => SetProperty(ref _textColor, value);
    }

    public static ObservableCollection<Brush> TextColorCollection()
    {
        var boolCollection = new ObservableCollection<Brush>();
        for (int i = 0; i < 19; i++)
            boolCollection.Add(Brushes.White);
        return boolCollection;
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

    private double _fps = 2;
    public double Fps
    {
        get => _fps;
        set => SetProperty(ref _fps, value);
    }

    private int _id = 0;
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private string _log = "";
    public string Log
    {
        get => _log;
        set => SetProperty(ref _log, value);
    }

    private string _detectionButtonText = "Начать распознавание";
    public string DetectionButtonText

    {
        get => _detectionButtonText;
        set => SetProperty(ref _detectionButtonText, value);
    }
    public static BrushConverter bc = new BrushConverter();
    private Brush _detectionButtonColor = (Brush)bc.ConvertFrom("#03a9f4");
    public Brush DetectionButtonColor

    {
        get => _detectionButtonColor;
        set => SetProperty(ref _detectionButtonColor, value);
    }

    private string[] _saveSettings = { "Текстовый файл", "Протокол" };
    public string[] SaveSettings
    {
        get => _saveSettings;
        set => SetProperty(ref _saveSettings, value);
    }

    private int _saveSetting = 0;
    public int SaveSetting
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

    private bool _fpsDecreaseEnabled;
    public bool FpsDecreaseEnabled
    {
        get => _fpsDecreaseEnabled;
        set => SetProperty(ref _fpsDecreaseEnabled, value);
    }

    private bool _isDetectionEnabled;
    public bool IsDetectionEnabled
    {
        get => _isDetectionEnabled;
        set => SetProperty(ref _isDetectionEnabled, value);
    }

    private bool _isAdvancedMode;
    public bool IsAdvancedMode
    {
        get => _isAdvancedMode;
        set => SetProperty(ref _isAdvancedMode, value);
    }

    private bool _isSavingDataSet;
    public bool IsSavingDataSet
    {
        get => _isSavingDataSet;
        set => SetProperty(ref _isSavingDataSet, value);
    }
}
