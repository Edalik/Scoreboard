using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Scoreboard.Modules.Main.Models.Abstractions;

internal interface IMainModel
{
    ImageSource Frame { get; set; }
    Point[] Points { get; set; }
    ObservableCollection<bool> IsChecked { get; set; }
    string Log { get; set; }
    string[] CameraSettings { get; set; }
    string CameraSetting { get; set; }
    string IP {  get; set; }
}
