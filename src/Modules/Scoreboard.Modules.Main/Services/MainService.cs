﻿using OpenCvSharp;
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
                if (!videoCapture.Read(frame))
                {
                    return;
                }
                int count = 0;
                int fps = (int)videoCapture.Fps;
                string date = DateTime.Now.ToString("d.M.yyyy_H.mm.ss");
                Directory.CreateDirectory("Logs");
                File.Create($"Logs\\Log_{date}.txt");
                Directory.CreateDirectory($"DataSet\\{date}");
                while (!cancellationToken.IsCancellationRequested)
                //while (await periodicTimer.WaitForNextTickAsync() && !cancellationToken.IsCancellationRequested)
                {
                    if (model.IsSavingDataSet)
                    {
                        frame.SaveImage($"DataSet\\{date}\\{count}.png");
                        count++;
                    }
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    Mat mat = frame.Clone();
                    Mat matg = frame.Clone();
                    Cv2.CvtColor(matg, matg, ColorConversionCodes.BGR2GRAY);
                    Cv2.AdaptiveThreshold(matg, matg, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 21, -21);
                    string log = "";
                    string[] texts = new string[19];
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
                                    texts[index] = text;
                                }
                                else
                                {
                                    TesseractEngine tesseractEngineDigits = new TesseractEngine(@"./tessdata", "ssd", EngineMode.Default, "digits");
                                    tesseractEngineDigits.DefaultPageSegMode = PageSegMode.RawLine;
                                    text = tesseractEngineDigits.Process(Pix.LoadFromMemory(tmp.Clone().ToBytes())).GetText().Trim('\n');
                                    text = Regex.Replace(text, "[^0-9]", "");
                                    texts[index] = text;
                                }
                            }
                            catch
                            {

                            }
                        }
                    });
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
                            string text = texts[i / 2];
                            StreamWriter logWriter;
                            switch (i)
                            {
                                case 0:
                                    logWriter = new StreamWriter($"Logs\\HOME.txt");
                                    log += $"HOME={text}\n";
                                    logWriter.Write($"HOME={text}\n");
                                    logWriter.Close();
                                    break;
                                case 2:
                                    logWriter = new StreamWriter($"Logs\\GUEST.txt");
                                    log += $"GUEST={text}\n";
                                    logWriter.Write($"GUEST={text}\n");
                                    logWriter.Close();
                                    break;
                                case 4:
                                    logWriter = new StreamWriter($"Logs\\PERIOD.txt");
                                    log += $"PERIOD={text}\n";
                                    logWriter.Write($"PERIOD={text}\n");
                                    logWriter.Close();
                                    break;
                                case 6:
                                    logWriter = new StreamWriter($"Logs\\TIME_MINUTES.txt");
                                    log += $"TIME_MINUTES={text}\n";
                                    logWriter.Write($"TIME_MINUTES={text}\n");
                                    logWriter.Close();
                                    break;
                                case 8:
                                    logWriter = new StreamWriter($"Logs\\TIME_SECONDS.txt");
                                    log += $"TIME_SECONDS={text}\n";
                                    logWriter.Write($"TIME_SECONDS={text}\n");
                                    logWriter.Close();
                                    break;
                                case 10:
                                    logWriter = new StreamWriter($"Logs\\HOME_SCORE.txt");
                                    log += $"HOME_SCORE={text}\n";
                                    logWriter.Write($"HOME_SCORE={text}\n");
                                    logWriter.Close();
                                    break;
                                case 12:
                                    logWriter = new StreamWriter($"Logs\\GUEST_SCORE.txt");
                                    log += $"GUEST_SCORE={text}\n";
                                    logWriter.Write($"GUEST_SCORE={text}\n");
                                    logWriter.Close();
                                    break;
                                case 14:
                                    logWriter = new StreamWriter($"Logs\\HOME_PENALTY1_NUM.txt");
                                    log += $"HOME_PENALTY1_NUM={text}\n";
                                    logWriter.Write($"HOME_PENALTY1_NUM={text}\n");
                                    logWriter.Close();
                                    break;
                                case 16:
                                    logWriter = new StreamWriter($"Logs\\HOME_PENALTY1_TIME_MINUTES.txt");
                                    log += $"HOME_PENALTY1_TIME_MINUTES={text}\n";
                                    logWriter.Write($"HOME_PENALTY1_TIME_MINUTES={text}\n");
                                    logWriter.Close();
                                    break;
                                case 18:
                                    logWriter = new StreamWriter($"Logs\\HOME_PENALTY1_TIME_SECONDS.txt");
                                    log += $"HOME_PENALTY1_TIME_SECONDS={text}\n";
                                    logWriter.Write($"HOME_PENALTY1_TIME_SECONDS={text}\n");
                                    logWriter.Close();
                                    break;
                                case 20:
                                    logWriter = new StreamWriter($"Logs\\HOME_PENALTY2_NUM.txt");
                                    log += $"HOME_PENALTY2_NUM={text}\n";
                                    logWriter.Write($"HOME_PENALTY2_NUM={text}\n");
                                    logWriter.Close();
                                    break;
                                case 22:
                                    logWriter = new StreamWriter($"Logs\\HOME_PENALTY2_TIME_MINUTES.txt");
                                    log += $"HOME_PENALTY2_TIME_MINUTES={text}\n";
                                    logWriter.Write($"HOME_PENALTY2_TIME_MINUTES={text}\n");
                                    logWriter.Close();
                                    break;
                                case 24:
                                    logWriter = new StreamWriter($"Logs\\HOME_PENALTY2_TIME_SECONDS.txt");
                                    log += $"HOME_PENALTY2_TIME_SECONDS={text}\n";
                                    logWriter.Write($"HOME_PENALTY2_TIME_SECONDS={text}\n");
                                    logWriter.Close();
                                    break;
                                case 26:
                                    logWriter = new StreamWriter($"Logs\\GUEST_PENALTY1_NUM.txt");
                                    log += $"GUEST_PENALTY1_NUM={text}\n";
                                    logWriter.Write($"GUEST_PENALTY1_NUM={text}\n");
                                    logWriter.Close();
                                    break;
                                case 28:
                                    logWriter = new StreamWriter($"Logs\\GUEST_PENALTY1_TIME_MINUTES.txt");
                                    log += $"GUEST_PENALTY1_TIME_MINUTES={text}\n";
                                    logWriter.Write($"GUEST_PENALTY1_TIME_MINUTES={text}\n");
                                    logWriter.Close();
                                    break;
                                case 30:
                                    logWriter = new StreamWriter($"Logs\\GUEST_PENALTY1_TIME_SECONDS.txt");
                                    log += $"GUEST_PENALTY1_TIME_SECONDS={text}\n";
                                    logWriter.Write($"GUEST_PENALTY1_TIME_SECONDS={text}\n");
                                    logWriter.Close();
                                    break;
                                case 32:
                                    logWriter = new StreamWriter($"Logs\\GUEST_PENALTY2_NUM.txt");
                                    log += $"GUEST_PENALTY2_NUM={text}\n";
                                    logWriter.Write($"GUEST_PENALTY2_NUM={text}\n");
                                    logWriter.Close();
                                    break;
                                case 34:
                                    logWriter = new StreamWriter($"Logs\\GUEST_PENALTY2_TIME_MINUTES.txt");
                                    log += $"GUEST_PENALTY2_TIME_MINUTES={text}\n";
                                    logWriter.Write($"GUEST_PENALTY2_TIME_MINUTES={text}\n");
                                    logWriter.Close();
                                    break;
                                case 36:
                                    logWriter = new StreamWriter($"Logs\\GUEST_PENALTY2_TIME_SECONDS.txt");
                                    log += $"GUEST_PENALTY2_TIME_SECONDS={text}\n";
                                    logWriter.Write($"GUEST_PENALTY2_TIME_SECONDS={text}\n");
                                    logWriter.Close();
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
                        using (StreamWriter writer = new StreamWriter("Logs\\Log.txt", true))
                        {
                            writer.WriteLine(log);
                        }
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
            }
        );
    }
}