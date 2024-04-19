using Newtonsoft.Json;
using System.Diagnostics;

namespace Industrialiot.Config
{
    public static class ConfigTools
    {
        public static Config ReadConfig()
        {
            Config config = new Config();
            string parentPath = AppContext.BaseDirectory;
            const string filePath = "config.json";

            Console.WriteLine(parentPath + filePath);

            if (File.Exists(filePath))
            {   
                Console.WriteLine(filePath);

                string jsonContent = File.ReadAllText(filePath);
                Console.WriteLine(jsonContent);
                config = JsonConvert.DeserializeObject<Config>(jsonContent);
            }

            return config;
        }
    }
}
