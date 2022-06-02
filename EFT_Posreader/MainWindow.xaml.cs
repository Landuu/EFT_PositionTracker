using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing.Imaging;
using NHotkey;
using NHotkey.Wpf;
using IronOcr;
using static EFT_Posreader.MousePos;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using WatsonWebsocket;

namespace EFT_Posreader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static volatile Storage storage = new();
        private static volatile IronTesseract tesseract = new();
        private static volatile Logger logger;
        private SocketServer socketServer;
        private ConfigManager config = new();
        private Timer? timer;
        

        public MainWindow()
        {
            InitializeComponent();
            logger = new Logger(StatusBox);
            logger.LogStart();
            socketServer = new(logger);
            LoadAndViewConfig();
            HotkeyManager.Current.AddOrReplace("SelectRegion", Key.NumPad0, ModifierKeys.None, KpSelectRegion);
        }

        private void LoadAndViewConfig()
        {
            if (config.HasSavedData())
            {
                ScreenRegion sr = config.GetScreenRegion();
                storage.ScreenRegion = sr;
                ScreenRegionP1.Text = sr.Start.DisplayString;
                ScreenRegionP2.Text = sr.End.DisplayString;
                logger.Log($"Pomyślnie wczytano config! {sr.Start.DisplayString} {sr.End.DisplayString}");
            } else
            {
                logger.Log("Nie wczytano configu, ustal region ręcznie!");
            }
        }

        private void KpSelectRegion(object? sender, HotkeyEventArgs e)
        {
            if (!storage.SwitchRegionSelect) return;

            var point = GetMousePosition();

            if (storage.ScreenRegion.HasPopulated == 0)
            {
                storage.ScreenRegion.Start = point;
                logger.Log("Punkt 1 (Lewy-górny róg) wybrany!");
                ScreenRegionP1.Text = point.DisplayString;
            }
            else if (storage.ScreenRegion.HasPopulated == 1)
            {
                storage.ScreenRegion.End = point;
                logger.Log("Punkt 2 (Prawy-dolny róg) wybrany!");
                ScreenRegionP2.Text = point.DisplayString;
                storage.SwitchRegionSelect = false;
                ButtonRegionChoiceStop.IsEnabled = false;
                ButtonRegionChoiceLoad.IsEnabled = true;
                ButtonRegionChoiceSave.IsEnabled = true;
                logger.Log("Wybór regionu: Pomyślnie stworzono obszar!");
            }
        }

        private void ButtonRegionChoice_Click(object sender, RoutedEventArgs e)
        {
            storage.SwitchRegionSelect = true;
            ButtonRegionChoiceStop.IsEnabled = true;
            ButtonRegionChoiceLoad.IsEnabled = false;
            ButtonRegionChoiceSave.IsEnabled = false;
            storage.ScreenRegion.Reset();

            logger.Log("Wybór regionu: Kliknij Num0 aby wybrać punkt");
            ScreenRegionP1.Text = "null";
            ScreenRegionP2.Text = "null";
        }

        private void ButtonRegionChoiceStop_Click(object sender, RoutedEventArgs e)
        {
            storage.SwitchRegionSelect = false;
            ButtonRegionChoiceStop.IsEnabled = false;
            ButtonRegionChoiceLoad.IsEnabled = true;
            ButtonRegionChoiceSave.IsEnabled = true;
            storage.ScreenRegion.Reset();

            logger.Log("Wybór regionu: Zatrzymano!");
            ScreenRegionP1.Text = "null";
            ScreenRegionP2.Text = "null";
        }

        private void ButtonTestRegion_Click(object sender, RoutedEventArgs e)
        {
            if (storage.ScreenRegion.HasPopulated != 2)
            {
                logger.Log("Test: Błąd! Niepoprawny region ekranu!");
                return;
            }
            string fileName = TestFileName.Text;
            Bitmap b = new(storage.ScreenRegion.Width, storage.ScreenRegion.Height);
            Graphics g = Graphics.FromImage(b);
            g.CopyFromScreen(storage.ScreenRegion.Start.X, storage.ScreenRegion.Start.Y, 0, 0, b.Size, CopyPixelOperation.SourceCopy);
            b.Save(fileName, ImageFormat.Jpeg);
            b.Dispose();
            g.Dispose();
            logger.Log("Test: Zapisano obraz!");
        }

        private void ButtonRegionChoiceSave_Click(object sender, RoutedEventArgs e)
        {
            config.SaveScreenRegion(storage.ScreenRegion);
            logger.Log("Pomyślnie zapisano region do configu!");
        }

        private void ButtonRegionChoiceLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadAndViewConfig();
        }

        private void TimerFunction(object? state)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Bitmap b = new(storage.ScreenRegion.Width, storage.ScreenRegion.Height);
            Graphics g = Graphics.FromImage(b);
            g.CopyFromScreen(storage.ScreenRegion.Start.X, storage.ScreenRegion.Start.Y, 0, 0, b.Size, CopyPixelOperation.SourceCopy);
            var result = tesseract.Read(b);
            b.Dispose();
            g.Dispose();
            sw.Stop();
            string value = $"/{sw.ElapsedMilliseconds}ms/ [{result.Confidence}%] $PLD$ {result.Text}";
            if(storage.debugMode) logger.Log(value);
            socketServer.Send(value);
        }

        private void EnableAllButtons(bool state)
        {
            ButtonRegionChoice.IsEnabled = state;
            ButtonRegionChoiceSave.IsEnabled = state;
            ButtonRegionChoiceLoad.IsEnabled = state;
            ButtonTestRegion.IsEnabled = state;
        } 

        private void ButtonTaskSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (storage.isRunning)
            {
                // Stop
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
                storage.isRunning = false;
                ((Button)sender).Content = "Start";
                EnableAllButtons(true);
                CheckboxDebug.IsEnabled = true;
                logger.Log("Tracing: Stop");
            } else
            {
                // Start
                if(storage.ScreenRegion.HasPopulated != 2)
                {
                    logger.Log("Tracing: Błąd! Niepoprawny region ekranu!");
                    return;
                }

                timer = new Timer(new TimerCallback(TimerFunction), null, 2500, 2500);
                storage.isRunning = true;
                ((Button)sender).Content = "Stop";
                EnableAllButtons(false);
                CheckboxDebug.IsEnabled = false;
                logger.Log("Tracing: Start");
            }
        }

        private void CheckboxDebug_Checked(object sender, RoutedEventArgs e)
        {
            storage.debugMode = true;
            Trace.WriteLine(storage.debugMode);
        }

        private void CheckboxDebug_Unchecked(object sender, RoutedEventArgs e)
        {
            storage.debugMode = false;
            Trace.WriteLine(storage.debugMode);
        }
    }
}
