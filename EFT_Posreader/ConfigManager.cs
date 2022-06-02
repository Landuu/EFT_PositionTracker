using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;

namespace EFT_Posreader
{
    internal class ConfigManager
    {
        private readonly string fileName = "config.ini";
        private readonly string sectionName = "ScreenRegion";
        private readonly string[] keys = { "sx", "sy", "ex", "ey" };
        private FileIniDataParser parser = new();
        private IniData data;

        public ConfigManager()
        {
            if(!File.Exists(fileName))
            {
                File.Create(fileName).Dispose();
            }
            data = parser.ReadFile(fileName);
        }

        public bool HasSavedData()
        {
            bool has = true;
            foreach(string key in keys)
            {
                string v = Get(key);
                if (String.IsNullOrEmpty(v) || v == "-1")
                {
                    has = false;
                    break;
                }
            }
            return has;
        }

        public ScreenRegion GetScreenRegion()
        {
            return new(Get(keys[0]), Get(keys[1]), Get(keys[2]), Get(keys[3]));
        }

        public void SaveScreenRegion(ScreenRegion sr)
        {
            string[] values = { Convert.ToString(sr.Start.X), Convert.ToString(sr.Start.Y), Convert.ToString(sr.End.X), Convert.ToString(sr.End.Y) };
            for(int i = 0; i < keys.Length; i++)
            {
                Set(keys[i], values[i]);
            }
            Save();
        }

        public void Set(string key, string value)
        {
            data[sectionName][key] = value;
        }

        public string Get(string key)
        {
            return data[sectionName][key];
        }

        public void Save()
        {
            parser.WriteFile(fileName, data);
        }
    }
}
