using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Msa.StoreActivity
{
    public static class ConfigHelper
    {
        public static string nameTableConfig = ConfigurationManager.AppSettings.Get("NAME_TABLE_CONFIG");
        public static string valueTableConfig = ConfigurationManager.AppSettings.Get("VALUE_TABLE_CONFIG");

        public static string connectionString = ConfigurationManager.AppSettings.Get("ETPDB");
        public static string serverNameConn = ConfigurationManager.AppSettings.Get("SERVER_NAME");
        public static string userNameConn = ConfigurationManager.AppSettings.Get("USERNAME_ETPDB");
        public static string passwordConn = ConfigurationManager.AppSettings.Get("PASSWORD_ETPDB");
        public static string tableStoreConfig = ConfigurationManager.AppSettings.Get("TABLE_CONFIG");

        public static string taskCount = String.Empty;
        public static string hostName = String.Empty;
        public static string myIP = String.Empty;
        public static string nameEtpBaseUrl = ConfigurationManager.AppSettings.Get("ETP_BASE_URL");
        public static string nameEtpApiPing = ConfigurationManager.AppSettings.Get("ETP_API_PING");
        public static string nameEtpApiGetLastVersion = ConfigurationManager.AppSettings.Get("ETP_GET_LAST_VERSION");
        public static string nameStoreVersion = ConfigurationManager.AppSettings.Get("STORE_VERSION");
        public static string nameStoreId = ConfigurationManager.AppSettings.Get("STORE_ID");
        public static string nameStoreCode = ConfigurationManager.AppSettings.Get("STORE_CODE");
        public static string valueStoreVersion = String.Empty;
        public static string valueStoreId = String.Empty;
        public static string valueStoreCode = String.Empty;
        public static string valueEtpBaseUrl = String.Empty;
        public static string valueEtpApiPing = String.Empty;
        public static string valueEtpApiGetLastVersion = String.Empty;
        public static string cmd = String.Empty;
        public static string valueSourcePath = String.Empty;
        public static string valueTargetPath = String.Empty;
        public static string valuePathFileDownload = String.Empty;
        public static string valuePathSaveFile = String.Empty;
        public static string valueOriginalFileUpload = String.Empty;
        public static string valueFolderUpload = String.Empty;
    }
}
