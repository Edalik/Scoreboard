using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Scoreboard.Modules.Main.Models;
using Scoreboard.Modules.Main.Models.Abstractions;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Tesseract;

namespace Scoreboard.Modules.Main.Services;

internal class MainService : IMainService
{

    public Task ReadFile(IMainModel model, CancellationToken cancellationToken)
    {
        return Task.Run
        (
            async () =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == false)
                {
                    return;
                }
                VideoCapture videoCapture = new VideoCapture(openFileDialog.FileName);
                Mat frame = new Mat();
                var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                videoCapture.Read(frame);
                int fps = (int)videoCapture.Fps;
                while (await periodicTimer.WaitForNextTickAsync() && !cancellationToken.IsCancellationRequested)
                {
                    Mat mat = frame.Clone();
                    Mat matg = frame.Clone();
                    Cv2.CvtColor(matg, matg, ColorConversionCodes.BGR2GRAY);
                    Cv2.AdaptiveThreshold(matg, matg, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 21, -21);
                    StreamWriter writer = new StreamWriter("result.txt");
                    for (int i = 0; i < model.Points.Length; i++)
                    {
                        if (model.Points[i] != default)
                        {
                            Mat tmp = matg.Clone().SubMat(new OpenCvSharp.Rect((int)model.Points[i].X, (int)model.Points[i].Y, (int)model.Points[i + 1].X - (int)model.Points[i].X, (int)model.Points[i + 1].Y - (int)model.Points[i].Y));
                            mat.Rectangle(new OpenCvSharp.Point(model.Points[i].X, model.Points[i].Y), new OpenCvSharp.Point(model.Points[i + 1].X, model.Points[i + 1].Y), Scalar.White, 1);
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
                                    writer.Write($"HOME={text}\n");
                                    break;
                                case 2:
                                    writer.Write($"GUEST={text}\n");
                                    break;
                                case 4:
                                    writer.Write($"PERIOD={text}\n");
                                    break;
                                case 6:
                                    writer.Write($"TIME={text}\n");
                                    break;
                                case 8:
                                    writer.Write($"HOME_SCORE={text}\n");
                                    break;
                                case 10:
                                    writer.Write($"GUEST_SCORE={text}\n");
                                    break;
                                case 12:
                                    writer.Write($"HOME_PENALTY1_NUM={text}\n");
                                    break;
                                case 14:
                                    writer.Write($"HOME_PENALTY1_TIME={text}\n");
                                    break;
                                case 16:
                                    writer.Write($"HOME_PENALTY2_NUM={text}\n");
                                    break;
                                case 18:
                                    writer.Write($"HOME_PENALTY2_TIME={text}\n");
                                    break;
                                case 20:
                                    writer.Write($"GUEST_PENALTY1_NUM={text}\n");
                                    break;
                                case 22:
                                    writer.Write($"GUEST_PENALTY1_TIME={text}\n");
                                    break;
                                case 24:
                                    writer.Write($"GUEST_PENALTY2_NUM={text}\n");
                                    break;
                                case 26:
                                    writer.Write($"GUEST_PENALTY2_TIME={text}\n");
                                    break;
                            }
                        }
                        i++;
                    }
                    writer.Close();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        model.Frame = mat.ToBitmapSource();
                    });
                    for (int i = 0; i < fps && videoCapture.PosFrames < videoCapture.FrameCount - 1; i++)
                        videoCapture.Read(frame);
                }
            }
        );
    }
}
