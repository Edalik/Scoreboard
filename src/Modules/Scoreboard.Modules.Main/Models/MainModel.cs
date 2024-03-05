using Prism.Mvvm;
using Scoreboard.Modules.Main.Models.Abstractions;
using Scoreboard.Modules.Main.Models.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        for (int i = 1; i <= 5; i++)
            cameraNames.Add($"Камера {i}");

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

    private string _logPath = GetLogPath();
    public string LogPath
    {
        get => _logPath;
        set => SetProperty(ref _logPath, value);
    }

    public static string GetLogPath()
    {
        if (File.Exists("zxc.zxc"))
        {
            using (StreamReader reader = new StreamReader("zxc.zxc"))
            {
                return reader.ReadLine();
            }
        }
        else
            return "";
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

    private bool _isAppend;
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

    public ScoreboardData _scoreboardData = new ScoreboardData();
    public ScoreboardData ScoreboardData
    {
        get => _scoreboardData;
        set => SetProperty(ref _scoreboardData, value);
    }
}
