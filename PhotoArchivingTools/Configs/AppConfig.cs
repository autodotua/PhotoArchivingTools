using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace PhotoArchivingTools.Configs
{
    public class AppConfig
    {
        private const string configFile = "configs.json";

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true,
        };
        private static AppConfig instance;
        public static AppConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    try
                    {
                        if (File.Exists(configFile))
                        {
                            var json = File.ReadAllText(configFile);
                            instance = JsonSerializer.Deserialize<AppConfig>(json, jsonOptions);
                        }
                        else
                        {
                            instance = new AppConfig();
                        }
                    }
                    catch (Exception ex)
                    {
                        instance = new AppConfig();
                    }
                }
                return instance;
            }
        }
        public List<PhotoSlimmingConfig> PhotoSlimmingConfigs { get; set; } = new List<PhotoSlimmingConfig>() { new PhotoSlimmingConfig() };
        public RepairModifiedTimeConfig RepairModifiedTimeConfig { get; set; } = new RepairModifiedTimeConfig();
        public TimeClassifyConfig TimeClassifyConfig { get; set; } = new TimeClassifyConfig();
        public UselessJpgCleanerConfig UselessJpgCleanerConfig { get; set; } = new UselessJpgCleanerConfig();
        public EncryptorConfig EncryptorConfig { get; set; } = new EncryptorConfig();

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(this, jsonOptions);
                File.WriteAllText(configFile, json);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
