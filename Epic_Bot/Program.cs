using Mirai_CSharp;
using Mirai_CSharp.Models;
using System;
using System.Threading.Tasks;

namespace Epic_Bot
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            MiraiHttpSessionOptions options = new MiraiHttpSessionOptions("127.0.0.1", 30000, "1234567890");
            await using MiraiHttpSession session = new MiraiHttpSession();
            BotMainService service = new BotMainService();
            session.AddPlugin(service);
            // 使用上边提供的信息异步连接到 mirai-api-http
            await session.ConnectAsync(options, 2795354785); // 自己填机器人QQ号
            while (true)
            {
                if (await Console.In.ReadLineAsync() == "exit")
                {
                    return;
                }
            }
        }
    }
}
