using System.Windows;
using System.Windows.Media;

namespace Scoreboard.Modules.Main.Models.Abstractions;

internal interface IMainModel
{
    ImageSource Frame { get; set; }
    Point[] Points { get; set; }
}
