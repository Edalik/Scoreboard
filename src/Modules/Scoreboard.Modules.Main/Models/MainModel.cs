using Prism.Mvvm;
using Scoreboard.Modules.Main.Models.Abstractions;
using System;
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
}
