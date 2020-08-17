using Epic_Bot.Helper;
using Epic_Bot.Model;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using Mirai_CSharp.Plugin.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Epic_Bot
{
    public class BotMainService : IGroupMessage, IGroupApply, INewFriendApply, IBotInvitedJoinGroup, IFriendMessage
    {
        List<ApiUrl> urls = new List<ApiUrl>()
        {
            new ApiUrl(){Order = ".card", Type = "POST", Url = "api/services/app/V1_Card/SimulateCard"},
            new ApiUrl(){Order = ".camp", Type = "GET", Url = "api/services/app/V1_Camp/GetCampingStr"},
            new ApiUrl(){Order = ".help", Type = "GET", Url = "api/services/app/V1_Common/GetHelpInfos"},
            new ApiUrl(){Order = ".gvghelp", Type = "GET", Url = "api/services/app/V1_Common/GetGvgHelp"},
            new ApiUrl(){Order = ".camphelp", Type = "GET", Url = "api/services/app/V1_Common/GetCampHelp"},
            new ApiUrl(){Order = ".hero", Type = "POST", Url = "api/services/app/V1_Common/QueryHeroList"},
            new ApiUrl(){Order = ".rename", Type = "PUT", Url = "api/services/app/V1_Common/UpdateHeroAliasName"},
            new ApiUrl(){Order = ".heroname", Type = "POST", Url = "api/services/app/V1_Common/QueryHeroAliasName"},
            new ApiUrl(){Order = ".gvginfo", Type = "GET", Url = "api/services/app/V1_Gvg/GetGvgInfos"},
            new ApiUrl(){Order = ".sgvg", Type = "PUT", Url = "api/services/app/V1_Gvg/UpdateGvgInfo"},
            new ApiUrl(){Order = ".exsgvg", Type = "GET", Url = "api/services/app/V1_Gvg/GetGvgExampleOrder"},
            new ApiUrl(){Order = ".guild", Type = "PUT", Url = "api/services/app/V1_Gvg/UpdateGuildName"},
            new ApiUrl(){Order = ".battle", Type = "PUT", Url = "api/services/app/V1_Gvg/UpdateBattleName"},
            new ApiUrl(){Order = ".permission", Type = "GET", Url = "api/services/app/V1_Common/GetPermission"},
        };


        public async Task<bool> GroupMessage(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            var apiAddress = AppSettingHelper.GetSettingValue("ApiAddress");
            var message = e.Chain[1].ToString().TrimEnd().TrimStart();

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
                var result = new SimpleResult();
                if (urldto.Type == "POST")
                    result = await HttpHelper.SendPostHttpRequest<SimpleResult>(url, values);
                else if (urldto.Type == "GET")
                    result = await HttpHelper.SendGetHttpRequest<SimpleResult>(url, values);
                else if (urldto.Type == "PUT")
                    result = await HttpHelper.SendPutHttpRequest<SimpleResult>(url, values);

                IMessageBase[] chain = new IMessageBase[]
                {
                    new PlainMessage(result.data.message)
                };
                await session.SendGroupMessageAsync(e.Sender.Group.Id, chain);
            }
            return false;
        }

        public async Task<bool> FriendMessage(MiraiHttpSession session, IFriendMessageEventArgs e)
        {
            IMessageBase[] chain = new IMessageBase[]
            {
               e.Chain[1]              
            };
            if (e.Sender.Id == 330036841)
            {
                var groups = await session.GetGroupListAsync();
                foreach(var a in groups)
                {
                    await session.SendGroupMessageAsync(a.Id, chain);
                }
            }
            return false; // 不阻断消息传递。如需阻断请返回true
        }

        /// <summary>
        /// 邀请群，暂时不做权限控制
        /// </summary>
        /// <param name="session"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task<bool> GroupApply(MiraiHttpSession session, IGroupApplyEventArgs e)
        {
            await session.HandleGroupApplyAsync(e, GroupApplyActions.Allow, "略略略");
            return false;
        }

        public async Task<bool> BotInvitedJoinGroup(MiraiHttpSession session, IBotInvitedJoinGroupEventArgs e)
        {
            await session.HandleGroupApplyAsync(e, GroupApplyActions.Allow, "略略略");
            return false;
        }

        public async Task<bool> NewFriendApply(MiraiHttpSession session, INewFriendApplyEventArgs e)
        {
            var apiAddress = AppSettingHelper.GetSettingValue("ApiAddress");
            var url = urls.FirstOrDefault(x => x.Order == ".permission").Url;
            url += apiAddress;
            var values = new Dictionary<string, string>
                {
                   { "Type", "0" },
                   { "qq", e.FromQQ.ToString() },
                };
            var result = await HttpHelper.SendGetHttpRequest<SimpleResult>(url, values);
            if (result.data.code == 0)
                await session.HandleNewFriendApplyAsync(e, FriendApplyAction.Deny, "没有权限,请联系管理员330036841或加群456480628");
            else
                await session.HandleNewFriendApplyAsync(e, FriendApplyAction.Allow, "略略略");
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
            if (message == ".help")
                order = ".help";
            if (message != ".help" && message == ".gvghelp")
                order = ".gvghelp";
            if (message != ".help" && message == ".camphelp")
                order = ".camphelp";
            if (message.StartsWith(".card") || message.StartsWith(".抽卡"))
                order = ".card";
            if ((message.StartsWith(".露营") || message.StartsWith(".camp")) && message != ".camphelp")
                order = ".camp";
            if (message.StartsWith(".hero") && !message.StartsWith(".heroname"))
                order = ".hero";
            if (message.StartsWith(".rename"))
                order = ".rename";
            if (message.StartsWith(".heroname"))
                order = ".heroname";
            if (message.StartsWith(".gvginfo"))
                order = ".gvginfo";
            if (message.StartsWith(".sgvg"))
                order = ".sgvg";
            if (message.StartsWith(".exsgvg"))
                order = ".exsgvg";
            if (message.StartsWith(".guild"))
                order = ".guild";
            if (message.StartsWith(".battle"))
                order = ".battle";
            var mes = GetSendMessage(message, order);
            var url = urls.FirstOrDefault(x => x.Order == order);
            var result = new UrlDto() { Url = url.Url, Message = mes, Type = url.Type };
            return result;
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
