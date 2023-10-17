using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Scoreboard.Modules.Main.Models;
using Scoreboard.Modules.Main.Models.Abstractions;
using Scoreboard.Modules.Main.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Scoreboard.Modules.Main.ViewModels;

class MainViewModel
{
    private readonly IMainService _mainService;
    public IMainModel Model { get; }
    public ICommand ChooseFileCommand { get; }
    public ICommand CameraCommand { get; }
    public DelegateCommand<string?> ChooseRegionCommand { get; set; }
    public ICommand MouseRightButtonDownCommand { get; }
    public ICommand MouseDownCommand { get; }
    public ICommand MouseUpCommand { get; }
    public ICommand ClearLogCommand { get; }
    public ICommand SaveSettingsCommand { get; }
    public ICommand LoadSettingsCommand { get; }

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
            Mouse.OverrideCursor = null;
            isChoosing = false;
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
                    Mouse.OverrideCursor = null;
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

                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == false)
                {
                    return;
                }
                VideoCapture videoCapture = new VideoCapture(openFileDialog.FileName);

                lastReadingTask = ReadFile(LastTokenSource.Token, videoCapture);
                await lastReadingTask;
            }
        );

        CameraCommand = ReactiveCommand.Create
        (
            async () =>
            {
                LastTokenSource?.Cancel();
                LastTokenSource = new();

                if (lastReadingTask is not null)
                {
                    await lastReadingTask;
                }


                VideoCapture videoCapture = new VideoCapture(0);
                switch (Model.CameraSetting)
                {
                    case "Обычная":
                        break;
                    case "Веб":
                        break;
                    case "IP":
                        videoCapture.Open(Model.IP);
                        break;
                }

                lastReadingTask = ReadFile(LastTokenSource.Token, videoCapture);
                await lastReadingTask;
            }
        );

        ClearLogCommand = ReactiveCommand.Create
        (
            () =>
            {
                Model.Log = "";
            }
        );

        SaveSettingsCommand = ReactiveCommand.Create
        (
            () =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Файл настроек (*.txt)|*.txt";
                if (saveFileDialog.ShowDialog() == false)
                {
                    return;
                }
                StreamWriter writer = new StreamWriter(saveFileDialog.FileName);
                for (int i = 0; i < 28; i++)
                {
                    writer.WriteLine(Model.Points[i].X);
                    writer.WriteLine(Model.Points[i].Y);
                }
                for (int i = 0; i < 14; i++)
                {
                    writer.WriteLine(Model.IsChecked[i]);
                }
                writer.Close();
            }
        );

        LoadSettingsCommand = ReactiveCommand.Create
        (
            () =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Файл настроек (*.txt)|*.txt";
                if (openFileDialog.ShowDialog() == false)
                {
                    return;
                }
                StreamReader reader = new StreamReader(openFileDialog.FileName);
                for (int i = 0; i < 28; i++)
                {
                    Model.Points[i].X = Convert.ToDouble(reader.ReadLine());
                    Model.Points[i].Y = Convert.ToDouble(reader.ReadLine());
                }
                for (int i = 0; i < 14; i++)
                {
                    if (reader.ReadLine() == "True")
                        Model.IsChecked[i] = true;
                    else
                        Model.IsChecked[i] = false;
                }
                reader.Close();
            }
        );
    }

    private void ChooseRegion(string? parameter)
    {
        choosingID = Convert.ToInt32(parameter);
        if (Model.IsChecked[choosingID / 2])
        {
            Mouse.OverrideCursor = Cursors.Cross;
            isChoosing = true;
        }
    }
    private Task ReadFile(CancellationToken cancellationToken, VideoCapture videoCapture) => _mainService.ReadFile(Model, cancellationToken, videoCapture);
}
