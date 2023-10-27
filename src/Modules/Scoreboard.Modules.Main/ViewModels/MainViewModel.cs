﻿using Microsoft.Win32;
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
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Scoreboard.Modules.Main.ViewModels;

class MainViewModel : ReactiveObject
{
    private readonly IMainService _mainService;
    public IMainModel Model { get; }
    public ICommand ChooseFileCommand { get; }
    public ICommand CameraCommand { get; }
    public ICommand FpsIncreaseCommand { get; }
    public ICommand FpsDecreaseCommand { get; }
    public ICommand ChooseRegionCommand { get; set; }
    public ICommand MouseDownCommand { get; }
    public ICommand MouseUpCommand { get; }
    public ICommand MouseMoveCommand { get; }
    public ICommand ResizeCommand { get; }
    public ICommand ClearLogCommand { get; }
    public ICommand DeleteRegionCommand { get; }
    public ICommand SaveLogCommand { get; }
    public ICommand SaveSettingsCommand { get; }
    public ICommand LoadSettingsCommand { get; }

    [Reactive] public CancellationTokenSource LastTokenSource { get; set; }
    [Reactive] public bool IsLeftMenuOpen { get; set; }
    [Reactive] public bool IsRightMenuOpen { get; set; } = true;
    private Task lastReadingTask;

    public int choosingID = -1;
    public int resizingID = -1;

    public bool isChoosing = false;

    public bool isResizing = false;

    public System.Windows.Point point;

    public MainViewModel()
    {
        _mainService = new MainService();
        Model = new MainModel();

        ChooseRegionCommand = new DelegateCommand<string?>(ChooseRegion);
        ResizeCommand = new DelegateCommand<string?>(Resize);
        DeleteRegionCommand = new DelegateCommand<string?>(DeleteRegion);

        MouseDownCommand = ReactiveCommand.Create
        (
            () =>
            {
                if (Mouse.OverrideCursor == Cursors.Cross)
                {
                    isChoosing = true;
                    Image img;
                    try
                    {

                        img = (Image)Mouse.DirectlyOver;
                    }
                    catch
                    {
                        return;
                    }
                    point = Mouse.GetPosition(img);
                    Mat mat = WriteableBitmapConverter.ToMat((WriteableBitmap)Model.Frame);
                    double x = point.X * (mat.Height / img.ActualHeight);
                    double y = point.Y * (mat.Width / img.ActualWidth);
                    Model.Points[choosingID] = new System.Windows.Point(x, y);
                    Model.IsChecked[choosingID / 2] = false;
                }

                else if (isResizing)
                {
                    Image img = (Image)Mouse.DirectlyOver;
                    point = Mouse.GetPosition(img);
                    Mat mat = WriteableBitmapConverter.ToMat((WriteableBitmap)Model.Frame);
                    double x = point.X * (mat.Height / img.ActualHeight);
                    double y = point.Y * (mat.Width / img.ActualWidth);

                    if (Math.Abs(Model.Points[choosingID].X - x) < 5 && Math.Abs(Model.Points[choosingID].Y - y) < 5)
                        resizingID = choosingID;
                    else if (Math.Abs(Model.Points[choosingID + 1].X - x) < 5 && Math.Abs(Model.Points[choosingID + 1].Y - y) < 5)
                        resizingID = choosingID + 1;
                }
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

                else if (isResizing)
                {
                    Image img = (Image)Mouse.DirectlyOver;
                    point = Mouse.GetPosition(img);
                    Mat mat = WriteableBitmapConverter.ToMat((WriteableBitmap)Model.CleanFrame);
                    double x = point.X * (mat.Height / img.ActualHeight);
                    double y = point.Y * (mat.Width / img.ActualWidth);

                    if (Math.Abs(Model.Points[choosingID].X - x) < 2 && Math.Abs(Model.Points[choosingID].Y - y) < 2)
                        Mouse.OverrideCursor = Cursors.SizeNWSE;
                    else if (Math.Abs(Model.Points[choosingID + 1].X - x) < 2 && Math.Abs(Model.Points[choosingID + 1].Y - y) < 2)
                    {
                        Mouse.OverrideCursor = Cursors.SizeNWSE;
                    }
                    else
                        Mouse.OverrideCursor = null;

                    if (resizingID >= 0)
                    {
                        Model.Points[resizingID] = new System.Windows.Point(x, y);
                        for (int i = 0; i < Model.Points.Length; i++)
                        {
                            if (Model.Points[i] != default)
                            {
                                if (Model.IsResizing[i / 2])
                                {
                                    mat.Rectangle(new OpenCvSharp.Point(Model.Points[i].X, Model.Points[i].Y), new OpenCvSharp.Point(Model.Points[i + 1].X, Model.Points[i + 1].Y), Scalar.Red, 1);
                                    mat.Rectangle(new OpenCvSharp.Point(Model.Points[i].X - 1, Model.Points[i].Y - 1), new OpenCvSharp.Point(Model.Points[i].X + 1, Model.Points[i].Y + 1), Scalar.Red, -1);
                                    mat.Rectangle(new OpenCvSharp.Point(Model.Points[i + 1].X - 1, Model.Points[i + 1].Y - 1), new OpenCvSharp.Point(Model.Points[i + 1].X + 1, Model.Points[i + 1].Y + 1), Scalar.Red, -1);
                                }
                                else
                                    mat.Rectangle(new OpenCvSharp.Point(Model.Points[i].X, Model.Points[i].Y), new OpenCvSharp.Point(Model.Points[i + 1].X, Model.Points[i + 1].Y), Scalar.White, 1);
                            }
                            i++;
                        }
                        Model.Frame = mat.ToWriteableBitmap();
                    }
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
                    {
                        Model.Points[choosingID] = default;
                        Model.Exists[choosingID / 2] = false;
                        Model.ButtonAction[choosingID / 2] = "Создать";
                        return;
                    }

                    Model.IsChecked[choosingID / 2] = true;
                    Model.Exists[choosingID / 2] = true;
                    Model.ButtonAction[choosingID / 2] = "Редактировать";
                    choosingID = -1;
                }
                else if (isResizing)
                {
                    Mouse.OverrideCursor = null;
                    resizingID = -1;

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
                    {
                        Model.Points[choosingID] = default;
                        Model.Exists[choosingID / 2] = false;
                        Model.IsChecked[choosingID / 2] = false;
                        Model.IsResizing[choosingID / 2] = false;
                        Model.TextColor[choosingID / 2] = Brushes.White;
                        Model.ButtonAction[choosingID / 2] = "Создать";
                    }
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

                if (!Model.CameraSettings.Any())
                {
                    MessageBox.Show("Устройства не обнаружены, проверьте подключение устройства и перезапустите приложение");
                    return;
                }

                lastReadingTask = ReadFile(LastTokenSource.Token, new VideoCapture(Model.CameraSetting));
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
                    if (i % 2 == 0 && Model.Points[i / 2].X != default)
                    {
                        Model.Exists[i / 2] = true;
                        Model.ButtonAction[i / 2] = "Редактировать";
                    }
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

        FpsIncreaseCommand = ReactiveCommand.Create
        (
            () =>
            {
                if (Model.Fps < 25)
                    Model.Fps++;
            }
        );

        FpsDecreaseCommand = ReactiveCommand.Create
        (
            () =>
            {
                if (Model.Fps > 1)
                    Model.Fps--;
            }
        );
    }

    private void DeleteRegion(string? parameter)
    {
        Mouse.OverrideCursor = null;
        Model.TextColor[choosingID / 2] = Brushes.White;
        Model.IsResizing[choosingID / 2] = false;
        Model.IsChoosing[choosingID / 2] = false;
        choosingID = Convert.ToInt32(parameter);
        Model.IsChecked[choosingID / 2] = false;
        Model.Points[choosingID] = default;
        isResizing = false;
        isChoosing = false;
        Model.Exists[choosingID / 2] = false;
        Model.ButtonAction[choosingID / 2] = "Создать";
    }

    private void ChooseRegion(string? parameter)
    {
        if (Model.ButtonAction[Convert.ToInt32(parameter) / 2] == "Создать")
        {
            Model.IsResizing[choosingID / 2] = false;
            isResizing = false;
            Model.TextColor[choosingID / 2] = Brushes.White;
            choosingID = Convert.ToInt32(parameter);
            Mouse.OverrideCursor = Cursors.Cross;
        }
        else
            Resize(parameter);
    }

    private void Resize(string? parameter)
    {
        if (Model.IsResizing[choosingID / 2])
        {
            Model.IsResizing[choosingID / 2] = false;
            isResizing = false;
            Model.TextColor[choosingID / 2] = Brushes.White;
        }
        if (choosingID == Convert.ToInt32(parameter) && Mouse.OverrideCursor != Cursors.Cross)
        {
            choosingID = -1;
            return;
        }
        Mouse.OverrideCursor = null;
        Model.IsChoosing[choosingID / 2] = false;
        choosingID = Convert.ToInt32(parameter);
        Model.TextColor[choosingID / 2] = Brushes.Red;
        isResizing = true;
        isChoosing = false;
        Model.IsResizing[choosingID / 2] = !Model.IsResizing[choosingID / 2];
    }

    private Task ReadFile(CancellationToken cancellationToken, VideoCapture videoCapture) => _mainService.ReadFile(Model, cancellationToken, videoCapture);
}
