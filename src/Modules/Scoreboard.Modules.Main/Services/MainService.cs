using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Scoreboard.Modules.Main.Models.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Tesseract;

namespace Scoreboard.Modules.Main.Services;

internal class MainService : IMainService
{

    public Task CaptureVideo(IMainModel model, CancellationToken cancellationToken, VideoCapture videoCapture)
    {
        return Task.Run
        (
            async () =>
            {
                Mat frame = new Mat();
                //var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1 / model.Fps));
                if (!videoCapture.Read(frame))
                {
                    return;
                }
                int count = 0;
                int saveDataSetTime = 0;
                int fps = (int)videoCapture.Fps;
                string date = DateTime.Now.ToString("d.M.yyyy_H.mm.ss");
                while (!cancellationToken.IsCancellationRequested)
                //while (await periodicTimer.WaitForNextTickAsync() && !cancellationToken.IsCancellationRequested)
                {
                    if (model.IsSavingDataSet && saveDataSetTime > 1000)
                    {
                        if (!Directory.Exists($"DataSet\\{date}"))
                            Directory.CreateDirectory($"DataSet\\{date}");
                        frame.SaveImage($"DataSet\\{date}\\{count}.png");
                        count++;
                    }
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    Mat mat = frame.Clone();
                    Mat matg = frame.Clone();
                    Cv2.CvtColor(matg, matg, ColorConversionCodes.BGR2GRAY);
                    Cv2.AdaptiveThreshold(matg, matg, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 21, -21);
                    string log = "";
                    Parallel.For(0, model.Points.Length / 2, index =>
                    {
                        int i = index * 2;
                        if (model.IsDetectionEnabled && model.IsChecked[index] && model.Points[i] != default)
                        {
                            Mat tmp = matg.Clone().SubMat(new OpenCvSharp.Rect((int)model.Points[i].X, (int)model.Points[i].Y, (int)model.Points[i + 1].X - (int)model.Points[i].X, (int)model.Points[i + 1].Y - (int)model.Points[i].Y));
                            string text = "";
                            try
                            {
                                if (i < 4)
                                {
                                    TesseractEngine tesseractEngine = new TesseractEngine(@"./tessdata", "rus", EngineMode.Default);
                                    tesseractEngine.DefaultPageSegMode = PageSegMode.SingleWord;
                                    text = tesseractEngine.Process(Pix.LoadFromMemory(tmp.Clone().ToBytes())).GetText().Trim('\n');
                                    text = Regex.Replace(text, "[^А-Яа-я]", "");
                                    model.ScoreboardData.SetStatValue(index, text);
                                }
                                else
                                {
                                    TesseractEngine tesseractEngineDigits = new TesseractEngine(@"./tessdata", "ssd", EngineMode.Default, "digits");
                                    tesseractEngineDigits.DefaultPageSegMode = PageSegMode.RawLine;
                                    text = tesseractEngineDigits.Process(Pix.LoadFromMemory(tmp.Clone().ToBytes())).GetText().Trim('\n');
                                    text = Regex.Replace(text, "[^0-9]", "");
                                    model.ScoreboardData.SetStatValue(index, text);
                                }
                            }
                            catch
                            {

                            }
                        }
                    });
                    for (int i = 0; i < model.Points.Length; i += 2)
                    {
                        Mat tmp = new Mat();
                        if (model.Points[i] != default)
                        {
                            if (model.IsResizing[i / 2])
                            {
                                mat.Rectangle(new OpenCvSharp.Point(model.Points[i].X, model.Points[i].Y), new OpenCvSharp.Point(model.Points[i + 1].X, model.Points[i + 1].Y), Scalar.Red, 1);
                                mat.Rectangle(new OpenCvSharp.Point(model.Points[i].X - 1, model.Points[i].Y - 1), new OpenCvSharp.Point(model.Points[i].X + 1, model.Points[i].Y + 1), Scalar.Red, -1);
                                mat.Rectangle(new OpenCvSharp.Point(model.Points[i + 1].X - 1, model.Points[i + 1].Y - 1), new OpenCvSharp.Point(model.Points[i + 1].X + 1, model.Points[i + 1].Y + 1), Scalar.Red, -1);
                            }
                            else
                                mat.Rectangle(new OpenCvSharp.Point(model.Points[i].X, model.Points[i].Y), new OpenCvSharp.Point(model.Points[i + 1].X, model.Points[i + 1].Y), Scalar.White, 1);
                        }
                        if (model.IsDetectionEnabled && model.IsChecked[i / 2] && model.Points[i] != default)
                        {
                            string text = model.ScoreboardData.GetStatValue(i / 2);
                            string statName = model.ScoreboardData.GetStatName(i / 2);
                            if (text != null)
                                using (StreamWriter logWriter = new StreamWriter($"{model.LogPath}\\{statName}.txt"))
                                {
                                    log += $"{statName}={text}\n";
                                    logWriter.Write(text);
                                    logWriter.Close();
                                }
                        }
                    }
                    if (log != "")
                    {
                        log = $"Timestamp: {DateTime.Now}\n{log}";
                        if (model.IsAppend)
                            model.Log = log + "\n" + model.Log;
                        else
                            model.Log = log;
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        model.Frame = mat.ToBitmapSource();
                        model.CleanFrame = frame.ToBitmapSource();
                    });

                    if (videoCapture.CaptureType == CaptureType.File)
                        for (int i = 0; i < (fps / model.Fps) && videoCapture.PosFrames < videoCapture.FrameCount - 1; i++)
                            videoCapture.Read(frame);
                    else
                        videoCapture.Read(frame);

                    stopwatch.Stop();
                    saveDataSetTime += (int)stopwatch.ElapsedMilliseconds;
                    if ((int)(1000 / model.Fps) - (int)stopwatch.ElapsedMilliseconds > 0)
                        Thread.Sleep((int)(1000 / model.Fps) - (int)stopwatch.ElapsedMilliseconds);
                }
            }
        );
    }
}