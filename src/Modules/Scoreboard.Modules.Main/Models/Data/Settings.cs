using CoreOSC;
using CoreOSC.IO;
using Scoreboard.Modules.Main.Models.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;

namespace Scoreboard.Modules.Main.Models.Data
{
    public class Settings : BindableBase
    {
        public string? LogPath { get; set; }
        public string? IpAddress { get; set; }
        public string? Port { get; set; }

        private ObservableCollection<bool> _isChecked = BoolCollection();
        public ObservableCollection<bool> IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }
        public static ObservableCollection<bool> BoolCollection()
        {
            var boolCollection = new ObservableCollection<bool>();
            for (int i = 0; i < 19; i++)
                boolCollection.Add(false);
            return boolCollection;
        }
        private ObservableCollection<string> _path = StringCollection();
        public ObservableCollection<string> Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }
        public static ObservableCollection<string> StringCollection()
        {
            var stringCollection = new ObservableCollection<string>();
            for (int i = 0; i < 19; i++)
                stringCollection.Add("");
            return stringCollection;
        }

        public static Settings GetSettings()
        {
            if (File.Exists("settings.json"))
            {
                string text = File.ReadAllText("settings.json");

                try
                {
                    Settings? settings = JsonSerializer.Deserialize<Settings>(text);

                    return settings == null ? new Settings() : settings;
                }
                catch
                {
                    return new Settings();
                }
            }
            else
            {
                return new Settings();
            }
        }

        public void SaveSettings()
        {
            string json = JsonSerializer.Serialize(this);
            File.WriteAllText("settings.json", json);
        }

        public async void SendData(IMainModel model)
        {
            if (string.IsNullOrEmpty(IpAddress) || string.IsNullOrEmpty(Port))
            {
                return;
            }
            try
            {
                using (var udpClient = new UdpClient(IpAddress, Convert.ToInt32(Port)))
                {
                    for (int i = 0; i < 19; i++)
                    {
                        if (IsChecked[i] && !string.IsNullOrEmpty(Path[i]))
                        {
                            var message = new OscMessage(new Address(Path[i]), new object[] { model.ScoreboardInfo.GetValue(i) });

                            await udpClient.SendMessageAsync(message);
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
