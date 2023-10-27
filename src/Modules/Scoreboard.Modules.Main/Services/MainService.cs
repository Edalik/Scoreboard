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

    public Task ReadFile(IMainModel model, CancellationToken cancellationToken, VideoCapture videoCapture)
    {
        return Task.Run
        (
            async () =>
            {
                Mat frame = new Mat();
                //var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1 / model.Fps));
                videoCapture.Read(frame);
                int fps = (int)videoCapture.Fps;
                StreamWriter writer = new StreamWriter($"log.txt");
                while (!cancellationToken.IsCancellationRequested)
                //while (await periodicTimer.WaitForNextTickAsync() && !cancellationToken.IsCancellationRequested)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    Mat mat = frame.Clone();
                    Mat matg = frame.Clone();
                    Cv2.CvtColor(matg, matg, ColorConversionCodes.BGR2GRAY);
                    Cv2.AdaptiveThreshold(matg, matg, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 21, -21);
                    string log = "";
                    for (int i = 0; i < model.Points.Length; i++)
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
                            tmp = matg.Clone().SubMat(new OpenCvSharp.Rect((int)model.Points[i].X, (int)model.Points[i].Y, (int)model.Points[i + 1].X - (int)model.Points[i].X, (int)model.Points[i + 1].Y - (int)model.Points[i].Y));
                            //mat.Circle((int)model.Points[i].X, (int)model.Points[i].Y, 1, Scalar.Red, -1, LineTypes.AntiAlias);
                            //mat.Circle((int)model.Points[i + 1].X, (int)model.Points[i + 1].Y, 1, Scalar.Red, -1, LineTypes.AntiAlias);
                            //mat.Circle((int)model.Points[i + 1].X, (int)model.Points[i].Y, 1, Scalar.Red, -1, LineTypes.AntiAlias);
                            //mat.Circle((int)model.Points[i].X, (int)model.Points[i + 1].Y, 1, Scalar.Red, -1, LineTypes.AntiAlias);
                            string text = "";
                            try
                            {
                                if (i < 4)
                                {
                                    TesseractEngine tesseractEngine = new TesseractEngine(@"./tessdata", "rus", EngineMode.Default);
                                    tesseractEngine.DefaultPageSegMode = PageSegMode.SingleWord;
                                    text = tesseractEngine.Process(Pix.LoadFromMemory(tmp.Clone().ToBytes())).GetText().Trim('\n');
                                    text = Regex.Replace(text, "[^А-Яа-я]", "");
                                }
                                else
                                {
                                    TesseractEngine tesseractEngineDigits = new TesseractEngine(@"./tessdata", "digits", EngineMode.Default, "digits");
                                    tesseractEngineDigits.DefaultPageSegMode = PageSegMode.RawLine;
                                    text = tesseractEngineDigits.Process(Pix.LoadFromMemory(tmp.Clone().ToBytes())).GetText().Trim('\n');
                                    text = Regex.Replace(text, "[^0-9]", "");
                                }
                            }
                            catch
                            {

                            }
                            switch (i)
                            {
                                case 0:
                                    log += $"HOME={text}\n";
                                    break;
                                case 2:
                                    log += $"GUEST={text}\n";
                                    break;
                                case 4:
                                    log += $"PERIOD={text}\n";
                                    break;
                                case 6:
                                    log += $"TIME={text}\n";
                                    break;
                                case 8:
                                    log += $"HOME_SCORE={text}\n";
                                    break;
                                case 10:
                                    log += $"GUEST_SCORE={text}\n";
                                    break;
                                case 12:
                                    log += $"HOME_PENALTY1_NUM={text}\n";
                                    break;
                                case 14:
                                    log += $"HOME_PENALTY1_TIME={text}\n";
                                    break;
                                case 16:
                                    log += $"HOME_PENALTY2_NUM={text}\n";
                                    break;
                                case 18:
                                    log += $"HOME_PENALTY2_TIME={text}\n";
                                    break;
                                case 20:
                                    log += $"GUEST_PENALTY1_NUM={text}\n";
                                    break;
                                case 22:
                                    log += $"GUEST_PENALTY1_TIME={text}\n";
                                    break;
                                case 24:
                                    log += $"GUEST_PENALTY2_NUM={text}\n";
                                    break;
                                case 26:
                                    log += $"GUEST_PENALTY2_TIME={text}\n";
                                    break;
                            }
                        }
                        i++;
                    }
                    if (log != "")
                    {
                        log = $"Timestamp: {DateTime.Now}\n{log}";
                        if (model.IsAppend)
                            model.Log = log + "\n" + model.Log;
                        else
                            model.Log = log;
                        writer.WriteLine(log);
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
                    if ((int)(1000 / model.Fps) - (int)stopwatch.ElapsedMilliseconds > 0)
                        Thread.Sleep((int)(1000 / model.Fps) - (int)stopwatch.ElapsedMilliseconds);
                }
                writer.Close();
            }
        );
    }
}
