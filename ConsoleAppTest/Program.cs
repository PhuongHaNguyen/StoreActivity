using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ConsoleAppTest
{
    class Program
    {
        static void Main(string[] args)
        {
            /*string servicesName = "AxInstSV";
            string folderRoot = @"D:\HaNguyen\Target2";
            DataTable dtbStoreConfig = new DataTable();
            Msa.StoreActivity.Program.processAutoUpdate(servicesName,folderRoot, dtbStoreConfig);*/
            try
            {
                Console.WriteLine(DateTime.UtcNow.ToString("s") + "Z");
                Dictionary<string, string> dicResult = new Dictionary<string, string>();
                string a = dicResult["123456"];
            }
            catch (Exception ex)
            {
            
            }
            finally 
            {
                Console.WriteLine("finally");
            }
        }
    }
}