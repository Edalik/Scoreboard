using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Scoreboard.Modules.Main.Models;
using Scoreboard.Modules.Main.Models.Abstractions;
using Scoreboard.Modules.Main.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Scoreboard.Modules.Main.ViewModels;

class MainViewModel
{
    private readonly IMainService _mainService;
    public IMainModel Model { get; }
    public ICommand ChooseFileCommand { get; }
    public DelegateCommand<string?> ChooseRegionCommand { get; set; }
    public ICommand MouseRightButtonDownCommand { get; }
    public ICommand MouseDownCommand { get; }
    public ICommand MouseUpCommand { get; }

    [Reactive] public CancellationTokenSource LastTokenSource { get; set; }
    private Task lastReadingTask;

    public int choosingID;

    public bool isChoosing = false;

    public System.Windows.Point point;

    public MainViewModel()
    {
        _mainService = new MainService();
        Model = new MainModel();

        ChooseRegionCommand = new DelegateCommand<string?>(ChooseRegion);

        MouseRightButtonDownCommand = ReactiveCommand.Create
        (
        () =>
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            isChoosing = false;
            Model.Points[choosingID] = default;
        }
        );

        MouseDownCommand = ReactiveCommand.Create
        (
            () =>
            {
                Image img = (Image)Mouse.DirectlyOver;
                point = Mouse.GetPosition(img);
            }
        );

        MouseUpCommand = ReactiveCommand.Create
        (
            () =>
            {
                if (isChoosing)
                {
                    Mat mat = WriteableBitmapConverter.ToMat((WriteableBitmap)Model.Frame);
                    Image img = (Image)Mouse.DirectlyOver;
                    Model.Points[choosingID + 1] = Mouse.GetPosition(img);
                    double x = point.X * (mat.Height / img.ActualHeight);
                    double y = point.Y * (mat.Width / img.ActualWidth);
                    Model.Points[choosingID] = new System.Windows.Point(x, y);
                    x = Model.Points[choosingID + 1].X * (mat.Height / img.ActualHeight);
                    y = Model.Points[choosingID + 1].Y * (mat.Width / img.ActualWidth);
                    Model.Points[choosingID + 1] = new System.Windows.Point(x, y);
                    Mouse.OverrideCursor = Cursors.Arrow;
                    isChoosing = false;
                }
            }
        );

        ChooseFileCommand = ReactiveCommand.Create
        (
            async () =>
            {
                LastTokenSource?.Cancel();
                LastTokenSource = new();

                if (lastReadingTask is not null)
                {
                    await lastReadingTask;
                }

                lastReadingTask = ReadFile(LastTokenSource.Token);
                await lastReadingTask;
            }
        );
    }

    private void ChooseRegion(string? parameter)
    {
        Mouse.OverrideCursor = Cursors.Cross;
        isChoosing = true;
        choosingID = Convert.ToInt32(parameter);
    }
    private Task ReadFile(CancellationToken cancellationToken) => _mainService.ReadFile(Model, cancellationToken);
}
