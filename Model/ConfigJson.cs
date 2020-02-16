using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebSocketLibNetStandard.Model
{
    public static class ConfigJson<TResult>
    {
        public static async Task<TResult> GetJsonFromConfigFile()
        {
            return await JsonFromConfigFile();
        }

        private static async Task<TResult> JsonFromConfigFile()
        {
            string file = $"{GetJsonConfigFile()}websocketconfig.json";
            TResult webSocketConfigForJson = default(TResult);
            try
            {
                using (FileStream configFile = File.OpenRead(file)) // D:\SourceTesting\WebSocketLibNetStandard\configuration.json
                {
                    byte[] buffer = new byte[configFile.Length];
                    await configFile.ReadAsync(buffer, 0, (int)configFile.Length);

                    string jsonResult = Encoding.UTF8.GetString(buffer);
                    webSocketConfigForJson = JsonConvert.DeserializeObject<TResult>(jsonResult);
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            
            return webSocketConfigForJson;
        }

        private static string GetJsonConfigFile()
        {
            string dirinfo = new DirectoryInfo(
                System.IO.Directory.GetCurrentDirectory()).FullName;

            dirinfo = dirinfo.Replace("\\bin", "")
                                    .Replace("\\Debug", "")
                                    .Replace("\\obj", "");

            return dirinfo[dirinfo.Length-1] == '\\' ? dirinfo : dirinfo + "\\";
        }
    }
}
