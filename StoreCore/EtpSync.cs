using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Msa.StoreActivity;

namespace StoreCore
{
    public partial class EtpSync : ServiceBase
    {
        private Timer timerPing = null;
        private Timer timerEvent = null;
        private string servicesName = string.Empty;
        private string folderRootService = string.Empty;
        private SqlConnection conn = null;
        private DataTable dtbStoreConfig = null;
        Msa.StoreActivity.Log log = new Log();

        public EtpSync()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            try
            {
                // timer Ping
                // get config check Db
                GetConfig();

                Msa.StoreActivity.ConfigHelper.connectionString = string.Format(Msa.StoreActivity.ConfigHelper.connectionString, Msa.StoreActivity.ConfigHelper.serverNameConn, Msa.StoreActivity.ConfigHelper.userNameConn, Msa.StoreActivity.ConfigHelper.passwordConn);
                log.logInfo("SqlConnecttion:" + Msa.StoreActivity.ConfigHelper.connectionString);
                conn = Msa.StoreActivity.DbHelper.OpenConnectDB(Msa.StoreActivity.ConfigHelper.connectionString);
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    log.logInfo("Connect sql success");
                    // check db & table
                    string sql = string.Empty;
                    if (String.IsNullOrEmpty(Msa.StoreActivity.ConfigHelper.tableStoreConfig))
                    {
                        return;
                    }
                    sql = "select * from information_schema.tables where table_name = '" + Msa.StoreActivity.ConfigHelper.tableStoreConfig + "'";
                    DataTable dtbHasTableStoreConfig = Msa.StoreActivity.DbHelper.SelectData(conn, sql);
                    if (dtbHasTableStoreConfig == null || dtbHasTableStoreConfig.Rows.Count == 0)
                    {
                        // create table & insert data  
                        string sqlCreateTable = ConfigurationManager.AppSettings.Get("SQL_CREATE_TABLE");
                        sqlCreateTable = string.Format(sqlCreateTable, Msa.StoreActivity.ConfigHelper.tableStoreConfig);
                        log.logInfo("Sql create table:" + sqlCreateTable);
                        int resultCreateTable = Msa.StoreActivity.DbHelper.ExecuteNonQuery(conn, sqlCreateTable);
                        if (resultCreateTable != 0)
                        {
                            log.logInfo("Create table " + Msa.StoreActivity.ConfigHelper.tableStoreConfig + " success");
                            dtbStoreConfig = CheckInsertData();
                        }
                    }
                    else
                    {
                        dtbStoreConfig = CheckInsertData();
                    }
                }
                else 
                {
                    log.logInfo("Connect sql fail");
                    return;
                }
                // get services name
                ServiceController sc = new ServiceController(this.ServiceName);
                this.servicesName = sc.ServiceName.Trim();
                this.folderRootService = AppDomain.CurrentDomain.BaseDirectory;

                // create timer from libary System.Timers
                timerPing = new Timer();
                // Execute every 20s
                timerPing.Interval = 20000;
                // timer event tick
                timerPing.Elapsed += timer_Tick;
                // Enable timer
                timerPing.Enabled = true;

                // timer Event Todo
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

        protected override void OnStop()
        {
            Msa.StoreActivity.Program.StopServices();
        }

        private async void timer_Tick(object sender, ElapsedEventArgs args)
        {
            await Msa.StoreActivity.Program.processAutoUpdate(this.servicesName, this.folderRootService, this.dtbStoreConfig);
        }

        private void GetConfig()
        {
            Msa.StoreActivity.ConfigHelper.connectionString = ConfigurationManager.AppSettings.Get("ETPDB");
            Msa.StoreActivity.ConfigHelper.serverNameConn = ConfigurationManager.AppSettings.Get("SERVER_NAME");
            Msa.StoreActivity.ConfigHelper.userNameConn = ConfigurationManager.AppSettings.Get("USERNAME_ETPDB");
            Msa.StoreActivity.ConfigHelper.passwordConn = ConfigurationManager.AppSettings.Get("PASSWORD_ETPDB");
            Msa.StoreActivity.ConfigHelper.tableStoreConfig = ConfigurationManager.AppSettings.Get("TABLE_CONFIG");

            Msa.StoreActivity.ConfigHelper.nameEtpBaseUrl = ConfigurationManager.AppSettings.Get("ETP_BASE_URL");
            Msa.StoreActivity.ConfigHelper.valueEtpBaseUrl = ConfigurationManager.AppSettings.Get("VALUE_ETP_BASE_URL");

            Msa.StoreActivity.ConfigHelper.nameEtpApiPing = ConfigurationManager.AppSettings.Get("ETP_API_PING");
            Msa.StoreActivity.ConfigHelper.valueEtpApiPing = ConfigurationManager.AppSettings.Get("VALUE_ETP_API_PING_URL");

            Msa.StoreActivity.ConfigHelper.nameEtpApiGetLastVersion = ConfigurationManager.AppSettings.Get("ETP_GET_LAST_VERSION");
            Msa.StoreActivity.ConfigHelper.valueEtpApiGetLastVersion = ConfigurationManager.AppSettings.Get("VALUE_ETP_GET_LAST_VERSION");

            Msa.StoreActivity.ConfigHelper.nameStoreId = ConfigurationManager.AppSettings.Get("STORE_ID");
            Msa.StoreActivity.ConfigHelper.valueStoreId = ConfigurationManager.AppSettings.Get("VALUE_STORE_ID");

            Msa.StoreActivity.ConfigHelper.nameStoreCode = ConfigurationManager.AppSettings.Get("STORE_CODE");
            Msa.StoreActivity.ConfigHelper.valueStoreCode = ConfigurationManager.AppSettings.Get("VALUE_STORE_CODE");

            Msa.StoreActivity.ConfigHelper.nameStoreVersion = ConfigurationManager.AppSettings.Get("STORE_VERSION");
            Msa.StoreActivity.ConfigHelper.valueStoreVersion = ConfigurationManager.AppSettings.Get("VALUE_STORE_VERSION");
        }

        private string createSqlInsertDataDefault()
        {
            log.logInfo("createSqlInsertDataDefault START");
            string sql = "INSERT INTO StoreConfig (Name, Value) VALUES";
            string subSql = "";
            if (!String.IsNullOrEmpty(Msa.StoreActivity.ConfigHelper.nameEtpBaseUrl) && !String.IsNullOrEmpty(Msa.StoreActivity.ConfigHelper.valueEtpBaseUrl))
            {
                subSql += " ('" + Msa.StoreActivity.ConfigHelper.nameEtpBaseUrl + "','" + Msa.StoreActivity.ConfigHelper.valueEtpBaseUrl + "')";
            }
            if (!String.IsNullOrEmpty(Msa.StoreActivity.ConfigHelper.nameEtpApiPing) && !String.IsNullOrEmpty(Msa.StoreActivity.ConfigHelper.valueEtpApiPing))
            {
                subSql += ", ('" + Msa.StoreActivity.ConfigHelper.nameEtpApiPing + "','" + Msa.StoreActivity.ConfigHelper.valueEtpApiPing + "')";
            }
            if (!String.IsNullOrEmpty(Msa.StoreActivity.ConfigHelper.nameEtpApiGetLastVersion) && !String.IsNullOrEmpty(Msa.StoreActivity.ConfigHelper.valueEtpApiGetLastVersion))
            {
                subSql += " ,('" + Msa.StoreActivity.ConfigHelper.nameEtpApiGetLastVersion + "','" + Msa.StoreActivity.ConfigHelper.valueEtpApiGetLastVersion + "')";
            }
            if (!String.IsNullOrEmpty(Msa.StoreActivity.ConfigHelper.nameStoreId) && !String.IsNullOrEmpty(Msa.StoreActivity.ConfigHelper.valueStoreId))
            {
                subSql += ", ('" + Msa.StoreActivity.ConfigHelper.nameStoreId + "','" + Msa.StoreActivity.ConfigHelper.valueStoreId + "')";
            }
            if (!String.IsNullOrEmpty(Msa.StoreActivity.ConfigHelper.nameStoreCode) && !String.IsNullOrEmpty(Msa.StoreActivity.ConfigHelper.valueStoreCode))
            {
                subSql += ", ('" + Msa.StoreActivity.ConfigHelper.nameStoreCode + "','" + Msa.StoreActivity.ConfigHelper.valueStoreCode + "')";
            }
            if (!String.IsNullOrEmpty(Msa.StoreActivity.ConfigHelper.nameStoreVersion) && !String.IsNullOrEmpty(Msa.StoreActivity.ConfigHelper.valueStoreVersion))
            {
                subSql += ", ('" + Msa.StoreActivity.ConfigHelper.nameStoreVersion + "','" + Msa.StoreActivity.ConfigHelper.valueStoreVersion + "')";
            }

            if (!String.IsNullOrEmpty(subSql))
            {
                sql += subSql;
            }
            else 
            {
                sql = string.Empty;
            }
            log.logInfo("createSqlInsertDataDefault END");
            log.logInfo("Sql insert:"+sql);
            return sql;
        }

        private DataTable CheckInsertData()
        {
            string sqlSelectTableStoreConfig = "select * from " + Msa.StoreActivity.ConfigHelper.tableStoreConfig;
            DataTable dtbResult = Msa.StoreActivity.DbHelper.SelectData(conn, sqlSelectTableStoreConfig);
            log.logInfo("Data table " + Msa.StoreActivity.ConfigHelper.tableStoreConfig + ":" + dtbResult.Rows.Count);
            if (dtbResult.Rows.Count == 0 || dtbResult == null)
            {
                // insert data
                string sqlInsertData = createSqlInsertDataDefault();
                log.logInfo("sqlInsertData " + sqlInsertData);
                if (!String.IsNullOrEmpty(sqlInsertData))
                {
                    int resultInsertData = Msa.StoreActivity.DbHelper.ExecuteNonQuery(conn, sqlInsertData);
                    log.logInfo("resultInsertData: " + resultInsertData);
                    dtbResult = Msa.StoreActivity.DbHelper.SelectData(conn, sqlSelectTableStoreConfig);
                    log.logInfo("Count rows table: " + dtbResult);
                }
            }
            return dtbResult;
        }

    }
}
