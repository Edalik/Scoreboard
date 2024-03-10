using Scoreboard.Modules.Main.Models.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Scoreboard.Modules.Main.Models.Abstractions;

internal interface IMainModel
{
    ImageSource CleanFrame { get; set; }
    ImageSource Frame { get; set; }
    Point[] Points { get; set; }
    ObservableCollection<bool> IsChecked { get; set; }
    ObservableCollection<bool> IsChoosing { get; set; }
    ObservableCollection<bool> IsResizing { get; set; }
    ObservableCollection<bool> Exists { get; set; }
    ObservableCollection<string> ButtonAction { get; set; }
    ObservableCollection<Brush> TextColor { get; set; }
    List<string> CameraSettings { get; set; }
    int CameraSetting { get; set; }
    double Fps { get; set; }
    string LogPath { get; set; }
    string Log { get; set; }
    string DetectionButtonText { get; set; }
    Brush DetectionButtonColor { get; set; }
    string[] SaveSettings { get; set; }
    int SaveSetting { get; set; }
    bool IsAppend {  get; set; }
    bool FpsIncreaseEnabled { get; set; }
    bool FpsDecreaseEnabled { get; set; }
    bool IsDetectionEnabled { get; set; }
    bool IsAdvancedMode { get; set; }
    bool IsSavingDataSet { get; set; }
    ScoreboardInfo ScoreboardInfo { get; set; }
}
