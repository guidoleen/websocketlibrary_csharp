using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Threading.Tasks;

namespace WebSocketLibNetStandard.Model
{
    public class WebSocketCertificate
    {
        public static X509Certificate CreateServerCertificate(string fileName, string passWord)
        {
            X509Certificate2Collection collection = new X509Certificate2Collection();
            collection.Import(GetFilePathCertificate(fileName), passWord, X509KeyStorageFlags.PersistKeySet);

            return (X509Certificate) collection[0];
        }

        // Only Reads the bytes for this Task
        private static async Task<string> GetCertificateAsStringTask(string fileName)
        {
            string certificate = "";

            try
            {
                using (FileStream stream = File.Open(GetFilePathCertificate(fileName), FileMode.Open))
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);

                    certificate = System.Text.Encoding.UTF8.GetString(buffer);
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
                certificate = ee.ToString();
            }
            return certificate;
        }

        private static async Task<string> GetCertificateAsString(string fileName)
        {
            return await GetCertificateAsStringTask(fileName);
        }

        private static string GetFilePathCertificate(string fileName)
        {
            string currDir = System.IO.Directory.GetCurrentDirectory();
            string certificateFileName = $"{currDir}/{fileName}";

            return certificateFileName;
        }
    }
}
//// https://superuser.com/questions/580697/how-do-i-view-the-contents-of-a-pfx-file-on-windows
//// openssl.exe pkcs12 -info -in D:\SourceTesting\WebSocketLibNetStandard\utils\certfile.pfx | openssl.exe x509 -noout -text > D:\SourceTesting\WebSocketLibNetStandard\utils\certfile.pfx.details.txt