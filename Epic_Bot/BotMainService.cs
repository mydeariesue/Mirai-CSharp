using Epic_Bot.Helper;
using Epic_Bot.Model;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using Mirai_CSharp.Plugin.Interfaces;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Epic_Bot
{
    public class BotMainService : IGroupMessage
    {
        Dictionary<string, string> urls = new Dictionary<string, string>()
        {
            [".card"] = "api/services/app/V1_Card/SimulateCard",
            [".camp"] = "api/services/app/V1_Camp/GetCampingStr"
        };
        public async Task<bool> GroupMessage(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            var apiAddress = AppSettingHelper.GetSettingValue("ApiAddress");
            var message = e.Chain[1].ToString();

            var urldto = GetUrlAndMessage(message);
            if (urldto != null)
            {
                var url = apiAddress + urldto.Url;
                var values = new Dictionary<string, string>
                {
                   { "message", urldto.Message },
                   { "groupid", e.Sender.Group.Id.ToString() },
                   { "name", e.Sender.Name},
                   { "qq", e.Sender.Id.ToString() },
                };
                var result = await HttpHelper.SendPostHttpRequest<SimpleResult>(url, values);
                IMessageBase[] chain = new IMessageBase[]
                {
                    new PlainMessage(result.data.message)
                };
                await session.SendGroupMessageAsync(e.Sender.Group.Id, chain);
            }
            return false;
        }

        /// <summary>
        /// 获取url和消息体
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private UrlDto GetUrlAndMessage(string message)
        {
            var order = "";
            if (message.StartsWith(".card") || message.StartsWith(".抽卡"))
                order = ".card";
            var mes = GetSendMessage(message, order);
            if (!string.IsNullOrEmpty(mes))
            {
                urls.TryGetValue(order, out var url);
                var result = new UrlDto() { Url = url, Message = mes };
                return result;
            }
            return null;
        }


        /// <summary>
        /// 获取指令内容
        /// </summary>
        /// <param name="message"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private string GetSendMessage(string message, string order)
        {
            Regex r = new Regex(order);
            Match m = r.Match(message);
            if (m.Success)
                return message.Replace(order, "").Trim();
            return null;
        }

    }



}
