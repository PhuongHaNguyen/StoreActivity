using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Msa.StoreActivity
{
    public class Program
    {

        private static SqlConnection conn = null;
        // private static readonly log4net.Config.XmlConfigurator.Configure();
        // private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
        private static Log log = new Log();

        public static void Main(string[] args)
        {
        }

        public static void StopServices()
        {
            log.logInfo("Stop services");
        }

        public static async Task processAutoUpdate(string servicesName, string folderRootService, DataTable dtbStoreConfig)
        {
            
            try
            {
                string taskCount = string.Empty;
                string cmd = String.Empty;
                string valueSourcePath = String.Empty;
                string valueTargetPath = String.Empty;
                string valuePathFileDownload = String.Empty;
                string valuePathSaveFile = String.Empty;
                string valueOriginalFileUpload = String.Empty;
                string valueFolderUpload = String.Empty;

                log.logInfo("servicesName:" + servicesName);
                log.logInfo("folderRootService:" + folderRootService);

                if (dtbStoreConfig != null && dtbStoreConfig.Rows.Count > 0)
                {
                    setValueConfig(dtbStoreConfig);

                    log.logInfo("EtpBaseUrl:" + ConfigHelper.valueEtpBaseUrl);
                    log.logInfo("EtpApiPing:" + ConfigHelper.valueEtpApiPing);

                    ConfigHelper.connectionString = string.Format(ConfigHelper.connectionString, ConfigHelper.serverNameConn, ConfigHelper.userNameConn, ConfigHelper.passwordConn);
                    log.logInfo("SqlConnecttion:" + ConfigHelper.connectionString);
                    
                    conn = Msa.StoreActivity.DbHelper.OpenConnectDB(ConfigHelper.connectionString);

                    HttpResponseMessage responsePingStore = ApiService.Ping(ConfigHelper.myIP, ConfigHelper.valueEtpBaseUrl, ConfigHelper.valueEtpApiPing, ConfigHelper.valueStoreId, ConfigHelper.valueStoreCode, ConfigHelper.valueStoreVersion);

                    if (responsePingStore.StatusCode.Equals(System.Net.HttpStatusCode.OK))
                    {
                        log.logInfo("Call API " + ConfigHelper.valueEtpBaseUrl + ConfigHelper.valueEtpApiPing + " Success");

                        // check version
                        var jsonString = await responsePingStore.Content.ReadAsStringAsync();
                        JObject jsonObj = JObject.Parse(jsonString);
                        string versionServer = (string)jsonObj["serverVersion"];
                        taskCount = (string)jsonObj["taskCount"];

                        log.logInfo("Start check update DB");
                        bool isUpdateDb = CheckUpdateVersionDb(folderRootService, ConfigHelper.valueStoreVersion, versionServer, ConfigHelper.nameStoreVersion,conn);

                        if (!ConfigHelper.valueStoreVersion.Equals(versionServer) && !isUpdateDb)
                        {
                            log.logInfo("Start Upgrade version");
                            if (!String.IsNullOrEmpty(ConfigHelper.valueEtpApiGetLastVersion))
                            {
                                HttpResponseMessage responseGetLastVersion = ApiService.GetLinkDownload(ConfigHelper.valueEtpBaseUrl, ConfigHelper.valueEtpApiGetLastVersion);
                                log.logInfo("EtpApiGetLastVersion:" + ConfigHelper.valueEtpApiGetLastVersion);
                                var jsonStringGetLastVersion = await responseGetLastVersion.Content.ReadAsStringAsync();
                                JObject jsonObjGetLastVersion = JObject.Parse(jsonStringGetLastVersion);
                                valuePathFileDownload = (string)jsonObjGetLastVersion["sourceCode"];

                                log.logInfo("Path file new version:"+ valuePathFileDownload);
                                string[] arr = valuePathFileDownload.Split('/');
                                string nameFileDownload = arr[arr.Length - 1];
                                string dateNow = DateTime.Now.ToString("yyyyMMddHHmmss");
                                valuePathSaveFile = Path.Combine(Path.GetTempPath(), dateNow);
                                
                                log.logInfo("Folder save file download:" + valuePathSaveFile);
                                
                                bool existsFolder = Directory.Exists(valuePathSaveFile);
                                if (!existsFolder)
                                {
                                    Directory.CreateDirectory(valuePathSaveFile);
                                }
                                string fullPathSaveFile = valuePathSaveFile + "\\" + nameFileDownload;
                                log.logInfo("Full path save file download:" + fullPathSaveFile);
                                FileHelper.DownLoadFile(valuePathFileDownload, fullPathSaveFile);

                                // Start run app Store update
                                log.logInfo("Start Run StoreActivityUpdate");
                                RunStoreActivityUpdate(servicesName, valuePathSaveFile, folderRootService, versionServer);
                                log.logInfo("Run StoreActivityUpdate success");
                            }

                            //        log.Info("Path file download:" + valuePathFileDownload);
                            //        string nameFileDownload = "fileDownload.zip";
                            //        string fullPathSaveFile = valuePathSaveFile + "\\" + nameFileDownload;
                            //        DownLoadFile(valuePathFileDownload, fullPathSaveFile);
                            //        log.Info("Download file success");

                            //        // Start run app Store update
                            //        valueSourcePath = valuePathSaveFile;
                            //        log.Info("Start Run StoreActivityUpdate");
                            //        RunStoreActivityUpdate(valueServicesName,valueSourcePath,valueTargetPath);
                            //        log.Info("Run StoreActivityUpdate success");

                            //        // unzip file
                            //        string pathFileZip = valueTargetPath + "\\" + nameFileDownload;
                            //        UnzipFile(valueTargetPath, pathFileZip);
                        }

                        //    // Task count
                        if (!String.IsNullOrEmpty(taskCount))
                        {
                            log.logInfo("Execute task count");
                            //Array listCmd = cmd.Split('|');
                            //for (int i = 0; i < listCmd.Length; i++)
                            //{
                            //    log.Info("Start Execute Command line " + listCmd.GetValue(i).ToString());
                            //    ExecuteCommandLine(listCmd.GetValue(i).ToString());
                            //    log.Info("Execute Command line " + listCmd.GetValue(i).ToString() + " success");
                            //}
                        }
                        //    // Upload file
                        //    log.Info("Start upload file:"+ valueOriginalFileUpload);
                        //    UploadFile(valueOriginalFileUpload, valueFolderUpload);
                        //    log.Info("Upload file success");
                    }
                }
                else
                {
                    log.logInfo("Table config there is no data");
                }
            }
            catch (Exception ex)
            {
                log.logError("Exception message:" + ex.Message);
                log.logError("Exception full strack:" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        public static void RunStoreActivityUpdate(string valueServicesName, string valueSourcePath, string valueTargetPath, string versionServer)
        {
            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "ToolUpdate\\StoreActivityUpdate.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = valueServicesName + " " + valueSourcePath + " " + valueTargetPath + " " + versionServer;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                    exeProcess.Close();
                }
            }
            catch (Exception ex)
            {
                log.logError("Exception message:" + ex.Message);
                log.logError("Exception full strack:" + ex.ToString());
            }
        }

        public static void ExecuteCommandLine(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = @"C:\Windows\System32\"
            };

            StringBuilder sb = new StringBuilder();
            Process p = Process.Start(processInfo);
            p.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
            p.BeginOutputReadLine();
            p.WaitForExit();
        }

        public static bool CheckUpdateVersionDb(string folderRootService, string valueStoreVersion, string versionServer, string nameStoreVersion, SqlConnection conn)
        {
            int result = 0;
            // read file upgrade version
            string NameFile = "UpgradeVersion.json";
            string fullPath = folderRootService + "\\" + NameFile;
            log.logInfo("Full path file upgrade:" + fullPath);

            if (!File.Exists(fullPath))
            {
                return false;
            }
            Dictionary<string, string> dicResult = FileHelper.getCurrentVersionInFile(fullPath);

            string currentVer = string.Empty;
            string statusUpgradeVer = string.Empty;

            dicResult.TryGetValue("newVersion", out currentVer);
            dicResult.TryGetValue("status", out statusUpgradeVer);

            log.logInfo("Store Version:" + valueStoreVersion);
            log.logInfo("Version in file:" + currentVer);
            log.logInfo("Version get Api:" + versionServer);
            log.logInfo("Status upgrade version:" + statusUpgradeVer);

            log.logInfo("Current read file:" + currentVer);
            log.logInfo("Status read file:" + statusUpgradeVer);
            if (!currentVer.Equals(valueStoreVersion) && currentVer.Equals(versionServer) && "ok".Equals(statusUpgradeVer.ToLower()))
            {
                log.logInfo("Start update version DB");
                string tableConfig = ConfigurationManager.AppSettings.Get("TABLE_CONFIG");
                log.logInfo("tableConfig:" + tableConfig);
                log.logInfo("currentVer:" + currentVer);
                log.logInfo("nameStoreVersion:" + nameStoreVersion);
                result = DbHelper.UpdateVersionDb(tableConfig, currentVer, nameStoreVersion, conn);
                if (result != 0)
                {
                    return true;
                }
                return false;
            }
            else {
                return false;
            }
        }

        public static void setValueConfig(DataTable dtbStoreConfig)
        {
            ConfigHelper.nameTableConfig = ConfigurationManager.AppSettings.Get("NAME_TABLE_CONFIG");
            ConfigHelper.valueTableConfig = ConfigurationManager.AppSettings.Get("VALUE_TABLE_CONFIG");
            string taskCount = string.Empty;

            ConfigHelper.hostName = Dns.GetHostName();
            ConfigHelper.myIP = Dns.GetHostByName(ConfigHelper.hostName).AddressList[0].ToString();
            ConfigHelper.nameEtpBaseUrl = ConfigurationManager.AppSettings.Get("ETP_BASE_URL");
            ConfigHelper.nameEtpApiPing = ConfigurationManager.AppSettings.Get("ETP_API_PING");
            ConfigHelper.nameEtpApiGetLastVersion = ConfigurationManager.AppSettings.Get("ETP_GET_LAST_VERSION");

            string nameStoreVersion = ConfigurationManager.AppSettings.Get("STORE_VERSION");
            string nameStoreId = ConfigurationManager.AppSettings.Get("STORE_ID");
            string nameStoreCode = ConfigurationManager.AppSettings.Get("STORE_CODE");

            for (int i = 0; i < dtbStoreConfig.Rows.Count; i++)
            {
                if (dtbStoreConfig.Rows[i][ConfigHelper.nameTableConfig].Equals(ConfigHelper.nameEtpBaseUrl))
                {
                    ConfigHelper.valueEtpBaseUrl = dtbStoreConfig.Rows[i][ConfigHelper.valueTableConfig].ToString().Trim();
                }
                if (dtbStoreConfig.Rows[i][ConfigHelper.nameTableConfig].Equals(ConfigHelper.nameEtpApiPing))
                {
                    ConfigHelper.valueEtpApiPing = dtbStoreConfig.Rows[i][ConfigHelper.valueTableConfig].ToString().Trim();
                }
                if (dtbStoreConfig.Rows[i][ConfigHelper.nameTableConfig].Equals(nameStoreCode))
                {
                    ConfigHelper.valueStoreCode = dtbStoreConfig.Rows[i][ConfigHelper.valueTableConfig].ToString().Trim();
                }
                if (dtbStoreConfig.Rows[i][ConfigHelper.nameTableConfig].Equals(nameStoreId))
                {
                    ConfigHelper.valueStoreId = dtbStoreConfig.Rows[i][ConfigHelper.valueTableConfig].ToString().Trim();
                }
                if (dtbStoreConfig.Rows[i][ConfigHelper.nameTableConfig].Equals(nameStoreVersion))
                {
                    ConfigHelper.valueStoreVersion = dtbStoreConfig.Rows[i][ConfigHelper.valueTableConfig].ToString().Trim();
                }
                if (dtbStoreConfig.Rows[i][ConfigHelper.nameTableConfig].Equals(ConfigHelper.nameEtpApiGetLastVersion))
                {
                    ConfigHelper.valueEtpApiGetLastVersion = dtbStoreConfig.Rows[i][ConfigHelper.valueTableConfig].ToString().Trim();
                }
            }
        }
    }
}