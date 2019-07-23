using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Game
{
    public enum KeyBindName
    {
        Use = 1,
        Inventory = 2,
        Up = 3,
        Down = 4,
        Right = 5,
        Left = 6,
        Attack = 7,
        Action1 = 8,
        Action2 = 9,
        Action3 = 10,
        PickTarget = 11,
        Quest = 12
    }

    public class SettingsManager : MonoBehaviour
    {
        protected string SettingsFolder;

        protected string SettingsFile;

        [Serializable]
        [XmlRoot("settings")]
        public class SettingsSerialized
        {
            public bool Fullscreen;
            public int ResWidth;
            public int ResHeight;
            public int ResRate;
            public List<(KeyBindName Key, KeyCode Value)> KeyBindsList;
        }

        public static SettingsManager Instance { get; private set; }

        public Resolution[] Resolutions;

        public bool Fullscreen { get; private set; }
        public Resolution CurrentResolution { get; private set; }
        public int CurrentResolutionIndex =>
            Array.IndexOf(this.Resolutions, this.CurrentResolution);

        private Dictionary<KeyBindName, KeyCode> KeyBinds;

        public void Awake()
        {
            Instance = this;
            this.SettingsFolder = Path.Combine(Application.persistentDataPath, "Settings");
            this.SettingsFile = Path.Combine(this.SettingsFolder, "settings.config");
            this.LoadSettings();
        }

        public void SetFullScreen(bool value)
        {
            this.Fullscreen = value;
            Screen.fullScreen = value;
        }

        public void SetResolution(int index)
        {
            if (index == -1)
                index = this.Resolutions.Length - 1;

            var res = this.Resolutions[index];
            this.CurrentResolution = res;
            Screen.SetResolution(res.width, res.height, this.Fullscreen, res.refreshRate);
        }

        public KeyCode GetKeyBindValue(KeyBindName bindName)
        {
            if (this.KeyBinds.TryGetValue(bindName, out var keyCode))
                return keyCode;

            return KeyCode.None;
        }

        public void SetKeyBind(KeyBindName bindName, KeyCode keyCode)
        {
            if (this.KeyBinds.ContainsValue(keyCode))
                this.KeyBinds[this.KeyBinds.FirstOrDefault(b => b.Value == keyCode).Key] = KeyCode.None;

            this.KeyBinds[bindName] = keyCode;
        }

        public void LoadSettings()
        {
            SettingsSerialized settings = null;
            try
            {
                if (File.Exists(this.SettingsFile))
                    using (var fileStream = new FileStream(this.SettingsFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var xs = new XmlSerializer(typeof(SettingsSerialized));
                        settings = (SettingsSerialized)xs.Deserialize(fileStream);
                    }
            }
            catch (Exception) { }

            var resolutions = Screen.resolutions
                .Distinct()
                .Where(x => x.width >= 800 && x.refreshRate >= 60)
                .OrderByDescending(x => x.width)
                .ThenByDescending(x => x.height)
                .ThenByDescending(x => x.refreshRate)
                .ToArray();

            if (settings != null)
            {
                this.SetFullScreen(settings.Fullscreen);
                this.Resolutions = resolutions;
                this.CurrentResolution = this.Resolutions
                    .FirstOrDefault(r => r.width == settings.ResWidth
                                      && r.height == settings.ResHeight
                                      && r.refreshRate == settings.ResRate);

                this.SetResolution(this.CurrentResolutionIndex);
                this.KeyBinds = settings.KeyBindsList.ToDictionary(b => b.Key, b => b.Value);
            }
            else
            {
                this.Resolutions = resolutions;
                this.KeyBinds = new Dictionary<KeyBindName, KeyCode>
                {
                    { KeyBindName.Use, KeyCode.E },
                    { KeyBindName.Up, KeyCode.W },
                    { KeyBindName.Down, KeyCode.S },
                    { KeyBindName.Right, KeyCode.D },
                    { KeyBindName.Left, KeyCode.A },
                    { KeyBindName.Attack, KeyCode.Space },
                    { KeyBindName.Action1, KeyCode.Alpha1 },
                    { KeyBindName.Action2, KeyCode.Alpha2 },
                    { KeyBindName.Action3, KeyCode.Alpha3 },
                    { KeyBindName.Inventory, KeyCode.I },
                    { KeyBindName.PickTarget, KeyCode.Tab },
                    { KeyBindName.Quest, KeyCode.Q }
                };
                this.Fullscreen = Screen.fullScreen;
                this.CurrentResolution = Screen.currentResolution;

                this.SaveSettings();
            }
        }

        public void SaveSettings()
        {
            var settings = new SettingsSerialized
            {
                Fullscreen = this.Fullscreen,
                ResWidth = this.CurrentResolution.width,
                ResHeight = this.CurrentResolution.height,
                ResRate = this.CurrentResolution.refreshRate,
                KeyBindsList = this.KeyBinds.Select(b => (b.Key, b.Value)).ToList()
            };

            var saveBckpPath = this.SettingsFile + Guid.NewGuid() + ".bckp";
            try
            {
                Directory.CreateDirectory(this.SettingsFolder);

                if (File.Exists(this.SettingsFile))
                    File.Move(this.SettingsFile, saveBckpPath);

                using (var fileStream = new FileStream(this.SettingsFile, FileMode.Create, FileAccess.Write, FileShare.Read))
                    new XmlSerializer(typeof(SettingsSerialized)).Serialize(fileStream, settings);

                File.Delete(saveBckpPath);
            }
            catch (Exception)
            {
                if (File.Exists(saveBckpPath))
                    File.Move(saveBckpPath, this.SettingsFile);
            }
        }
    }
}
