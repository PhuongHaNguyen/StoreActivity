using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Msa.StoreActivity
{
    class ApiService
    {
        public static HttpResponseMessage GetLinkDownload(string valueEtpBaseUrl, string urlApiGetLastVersion)
        {
            string apiFull = valueEtpBaseUrl + urlApiGetLastVersion;
            HttpResponseMessage response = RunSync(() => Get(apiFull));
            return response;
        }

        public static async Task<HttpResponseMessage> Get(string url)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return response;
            }
        }

        public static HttpResponseMessage Ping(string myIP, string valueEtpBaseUrl, string valueEtpApiPing, string valueStoreId, string valueStoreCode, string valueStoreVersion)
        {
            try
            {
                Dictionary<string, string> para = new Dictionary<string, string>();
                string dateNow = DateTime.UtcNow.ToString("s") + "Z";
                var obj = new
                {
                    id = valueStoreId,
                    storeCode = valueStoreCode,
                    ip = myIP,
                    version = valueStoreVersion,
                    waiwaitingCount = 0,
                    liveAt = dateNow
                };
                var json = JsonConvert.SerializeObject(obj);
                HttpResponseMessage ret = Post(valueEtpBaseUrl, valueEtpApiPing, json, para);
                return ret;
            }
            catch
            {
                return null;
            }
        }

        public static HttpResponseMessage Post(string valueEtpBaseUrl, string valueEtpApiPing, string jsonRequest, Dictionary<string, string> param)
        {
            string url = valueEtpBaseUrl + valueEtpApiPing;
            return RunSync(() => PostAsync(url, jsonRequest));
        }

        public static async Task<HttpResponseMessage> PostAsync(string url, string json)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();

                HttpContent stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, stringContent);

                response.EnsureSuccessStatusCode();
                // return await response.Content.ReadAsStringAsync();
                return response;
            }
        }

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return Task.Run<Task<TResult>>(func).Unwrap().GetAwaiter().GetResult();
        }
    }
}
