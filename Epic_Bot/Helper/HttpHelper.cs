using Epic_Bot.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Epic_Bot.Helper
{
    public static class HttpHelper
    {
        /// <summary>
        /// post发送请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Url"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static async Task<T> SendPostHttpRequest<T>(string Url, Dictionary<string, string> Data)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            var bs = TransferHttpData(Data);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            req.ContentLength = bs.Length;
            req.KeepAlive = false;
            req.ServicePoint.Expect100Continue = false;
            req.ServicePoint.UseNagleAlgorithm = false;
            req.ServicePoint.ConnectionLimit = 65500;
            req.AllowWriteStreamBuffering = false;
            req.Proxy = null;
            req.ServicePoint.ConnectionLimit = int.MaxValue;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
                reqStream.Close();
            }
            HttpWebResponse response = (HttpWebResponse)await req.GetResponseAsync();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                var read = (await reader.ReadToEndAsync()).ToString();
                var result = JsonConvert.DeserializeObject<T>(read);
                response.Close();
                req.Abort();
                return result;
            };        
        }


        private static byte[] TransferHttpData(Dictionary<string, string> input)
        {
            StringBuilder buffer = new StringBuilder();//这是要提交的数据
            int i = 0;

            //通过泛型集合转成要提交的参数和数据
            foreach (string key in input.Keys)
            {
                if (i > 0)
                    buffer.AppendFormat("&{0}={1}", key, input[key]);
                else
                    buffer.AppendFormat("{0}={1}", key, input[key]);
                i++;
            }
            byte[] bs = Encoding.UTF8.GetBytes(buffer.ToString());//UTF-8
            return bs;
        }
    }
}
