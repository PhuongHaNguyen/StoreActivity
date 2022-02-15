using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msa.StoreActivity
{
    public class DbHelper
    {
        public static SqlConnection OpenConnectDB(String connection)
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connection;
            conn.Open();
            return conn;
        }

        public static DataTable SelectData(SqlConnection conn, String select)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter sda = new SqlDataAdapter(select, conn);
            sda.Fill(dt);
            return dt;
        }

        public static int ExecuteNonQuery(SqlConnection conn, string sql)
        {
            int result = 0;
            using (SqlCommand command = new SqlCommand(sql, conn))
            result = command.ExecuteNonQuery();
            return result;
        }

        public int ExecuteInsertQuery(SqlConnection conn, string query, SqlParameter[] parameter)
        {
            int affectedRow = 0;
            SqlDataAdapter adapter = new SqlDataAdapter();
            SqlCommand com = new SqlCommand();
            try
            {
                com.Connection = conn;
                com.CommandText = query;
                com.Parameters.AddRange(parameter);
                adapter.InsertCommand = com;
                affectedRow = com.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return 0;
            }
            return affectedRow;
        }

        public static DataTable getTableConfig(SqlConnection conn)
        {
            DataTable dtbConfig = new DataTable();
            string SqlString = "SELECT * FROM {0}";
            string tableConfig = ConfigurationManager.AppSettings.Get("TABLE_CONFIG");
            SqlString = string.Format(SqlString, tableConfig);
            dtbConfig = DbHelper.SelectData(conn, SqlString);
            return dtbConfig;
        }

        public static int  UpdateVersionDb(string table, string valueUpdate, string column, SqlConnection conn)
        {
            int rowsAffected = 0;
            string sqlUpdate = "UPDATE " + table;
            SqlCommand cmd = new SqlCommand(sqlUpdate + " SET Value = @Value WHERE Name = @Name", conn);
            cmd.Parameters.AddWithValue("@Value", valueUpdate);
            cmd.Parameters.AddWithValue("@Name", column);
            /*if (conn == null || conn.State == ConnectionState.Closed || conn.State == ConnectionState.Broken)
            { 
                string sqlConnection = ConfigurationManager.AppSettings.Get("TABLE_CONFIG");
            }*/
            rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected;
        }

        public static void CloseConnectDB(SqlConnection conn)
        {
            conn.Close();
        }
    }
}