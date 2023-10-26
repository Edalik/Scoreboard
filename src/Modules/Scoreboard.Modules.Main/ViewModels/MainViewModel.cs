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
    public ICommand MouseDownCommand { get; }
    public ICommand MouseUpCommand { get; }
    public ICommand MouseMoveCommand { get; }
    public ICommand ResizeCommand { get; }
    public ICommand ClearLogCommand { get; }
    public ICommand SaveLogCommand { get; }
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

        MouseDownCommand = ReactiveCommand.Create
        (
            () =>
            {
                isChoosing = true;
                Image img = (Image)Mouse.DirectlyOver;
                point = Mouse.GetPosition(img);
                Mat mat = WriteableBitmapConverter.ToMat((WriteableBitmap)Model.Frame);
                double x = point.X * (mat.Height / img.ActualHeight);
                double y = point.Y * (mat.Width / img.ActualWidth);
                Model.Points[choosingID] = new System.Windows.Point(x, y);
                Model.IsChecked[choosingID / 2] = false;
            }
        );

        MouseMoveCommand = ReactiveCommand.Create
        (
            () =>
            {
                if (isChoosing)
                {
                    Image img = (Image)Mouse.DirectlyOver;
                    Mat mat = WriteableBitmapConverter.ToMat((WriteableBitmap)Model.CleanFrame);
                    Model.Points[choosingID + 1] = Mouse.GetPosition(img);
                    double x = Model.Points[choosingID + 1].X * (mat.Height / img.ActualHeight);
                    double y = Model.Points[choosingID + 1].Y * (mat.Width / img.ActualWidth);
                    Model.Points[choosingID + 1] = new System.Windows.Point(x, y);
                    for (int i = 0; i < Model.Points.Length; i++)
                    {
                        if (Model.Points[i] != default)
                        {
                            mat.Rectangle(new OpenCvSharp.Point(Model.Points[i].X, Model.Points[i].Y), new OpenCvSharp.Point(Model.Points[i + 1].X, Model.Points[i + 1].Y), Scalar.White, 1);
                        }
                        i++;
                    }
                    Model.Frame = mat.ToWriteableBitmap();
                }
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
                    Model.IsChoosing[choosingID / 2] = false;

                    if (Model.Points[choosingID + 1].X < Model.Points[choosingID].X)
                    {
                        double tmp = Model.Points[choosingID + 1].X;
                        Model.Points[choosingID + 1].X = Model.Points[choosingID].X;
                        Model.Points[choosingID].X = tmp;
                    }

                    if (Model.Points[choosingID + 1].Y < Model.Points[choosingID].Y)
                    {
                        double tmp = Model.Points[choosingID + 1].Y;
                        Model.Points[choosingID + 1].Y = Model.Points[choosingID].Y;
                        Model.Points[choosingID].Y = tmp;
                    }

                    if (Model.Points[choosingID + 1].X - Model.Points[choosingID].X < 10 || Model.Points[choosingID + 1].Y - Model.Points[choosingID].Y < 10)
                        Model.Points[choosingID] = default;

                    Model.IsChecked[choosingID / 2] = true;
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
                Model.FpsEnabled = false;

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

                Model.FpsEnabled = true;

                VideoCapture videoCapture = new VideoCapture(0);
                switch (Model.CameraSetting)
                {
                    case "Обычная":
                        break;
                    case "Веб":
                        break;
                    case "IP":
                        videoCapture.Open("https://192.168.100.3:8080/video");
                        break;
                }

                lastReadingTask = ReadFile(LastTokenSource.Token, videoCapture);
                await lastReadingTask;
            }
        );

        SaveLogCommand = ReactiveCommand.Create
        (
            () =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Текстовый файл (*.txt)|*.txt";
                if (saveFileDialog.ShowDialog() == false)
                {
                    return;
                }
                StreamWriter writer = new StreamWriter(saveFileDialog.FileName);
                writer.Write(Model.Log);
                writer.Close();
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
                saveFileDialog.Filter = "Файл настроек (*.settings)|*.settings";
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
                openFileDialog.Filter = "Файл настроек (*.settings)|*.settings";
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
        Mouse.OverrideCursor = Cursors.Cross;
    }
    private Task ReadFile(CancellationToken cancellationToken, VideoCapture videoCapture) => _mainService.ReadFile(Model, cancellationToken, videoCapture);
}
