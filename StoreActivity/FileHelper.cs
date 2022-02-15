using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Msa.StoreActivity
{
    class FileHelper
    {
        public static void DownLoadFile(string pathDownload, string pathSaveFile)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(pathDownload, pathSaveFile);
        }

        public static void UploadFile(String originalFile, String folderUpload)
        {
            string input = Path.GetFileName(originalFile);
            string folder = Path.Combine(Path.Combine(folderUpload));
            Directory.CreateDirectory(folder);
            // Write to folder
            File.Copy(originalFile, Path.Combine(folder, input), true);
        }

        public static void UnzipFile(String pathUnzip, String pathFile)
        {
            FastZip fastZip = new FastZip();
            // = null => Will always overwrite if target filenames already exist
            fastZip.ExtractZip(pathFile, pathUnzip, null);
        }

        public static Dictionary<string, string> getCurrentVersionInFile(string fullPath)
        {
            JObject o1 = JObject.Parse(File.ReadAllText(fullPath));
            string newVersion = o1.GetValue("currentVersion").ToString();
            string status = o1.GetValue("status").ToString();
            Dictionary<string, string> dicResult = new Dictionary<string, string>();
            dicResult.Add("newVersion", newVersion);
            dicResult.Add("status", status);
            return dicResult;
        }
    }
}
