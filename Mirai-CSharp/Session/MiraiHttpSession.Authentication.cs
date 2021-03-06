﻿using Mirai_CSharp.Exceptions;
using Mirai_CSharp.Helpers;
using Mirai_CSharp.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mirai_CSharp
{
    public partial class MiraiHttpSession
    {
        /// <summary>
        /// 异步连接到mirai-api-http。
        /// </summary>
        /// <remarks>
        /// 不会连接到指令监控的ws服务。此方法不是线程安全的。
        /// </remarks>
        /// <exception cref="BotNotFoundException"/>
        /// <exception cref="InvalidAuthKeyException"/>
        /// <param name="options">连接信息</param>
        /// <param name="qqNumber">Session将要绑定的Bot的qq号</param>
        public Task ConnectAsync(MiraiHttpSessionOptions options, long qqNumber)
        {
            return ConnectAsync(options, qqNumber, false);
        }
        /// <summary>
        /// 异步连接到mirai-api-http。
        /// </summary>
        /// <remarks>
        /// 此方法线程安全。但是在连接过程中, 如果尝试多次调用, 除了第一次以后的所有调用都将立即返回。
        /// </remarks>
        /// <exception cref="BotNotFoundException"/>
        /// <exception cref="InvalidAuthKeyException"/>
        /// <param name="options">连接信息</param>
        /// <param name="qqNumber">Session将要绑定的Bot的qq号</param>
        /// <param name="listenCommand">是否监听指令相关的消息</param>
        public async Task ConnectAsync(MiraiHttpSessionOptions options, long qqNumber, bool listenCommand)
        {
            CheckDisposed();
            InternalSessionInfo session = new InternalSessionInfo();
            if (Interlocked.CompareExchange(ref SessionInfo, session, null!) == null)
            {
                try
                {
                    session.Client = new HttpClient();
                    session.SessionKey = await AuthorizeAsync(session.Client, options);
                    session.Options = options;
                    await VerifyAsync(session.Client, options, session.SessionKey, qqNumber);
                    session.QQNumber = qqNumber;
                    session.ApiVersion = await GetVersionAsync(session.Client, options);
                    CancellationTokenSource canceller = new CancellationTokenSource();
                    session.Canceller = canceller;
                    session.Token = canceller.Token;
                    session.Connected = true;
                    IMiraiSessionConfig config = await this.GetConfigAsync(session);
                    if (!config.EnableWebSocket.GetValueOrDefault())
                    {
                        await this.SetConfigAsync(session, new MiraiSessionConfig { CacheSize = config.CacheSize, EnableWebSocket = true });
                    }
                    CancellationToken token = session.Canceller.Token;
                    ReceiveMessageLoop(session, token);
                    if (listenCommand)
                    {
                        ReceiveCommandLoop(session, token);
                    }
                }
                catch
                {
                    SessionInfo = null!;
                    _ = InternalReleaseAsync(session);
                    throw;
                }
            }
        }

        private static async Task<string> AuthorizeAsync(HttpClient client, MiraiHttpSessionOptions options)
        {
            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(new { authKey = options.AuthKey });
            using JsonDocument j = await client.HttpPostAsync($"{options.BaseUrl}/auth", payload).GetJsonAsync();
            JsonElement root = j.RootElement;
            int code = root.GetProperty("code").GetInt32();
            return code switch
            {
                0 => root.GetProperty("session").GetString()!,
                _ => throw GetCommonException(code, in root)
            };
        }

        private static Task VerifyAsync(HttpClient client, MiraiHttpSessionOptions options, string sessionKey, long qqNumber)
        {
            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(new
            {
                sessionKey,
                qq = qqNumber
            });
            return InternalHttpPostAsync(client, $"{options.BaseUrl}/verify", payload);
        }
        /// <summary>
        /// 异步获取mirai-api-http的版本号
        /// </summary>
        /// <param name="client">要进行请求的 <see cref="HttpClient"/></param>
        /// <param name="options">连接信息</param>
        /// <returns>表示此异步操作的 <see cref="Task"/></returns>
        public static async Task<Version> GetVersionAsync(HttpClient client, MiraiHttpSessionOptions options)
        {
            using JsonDocument j = await client.HttpGetAsync($"{options.BaseUrl}/about").GetJsonAsync();
            JsonElement root = j.RootElement;
            int code = root.GetProperty("code").GetInt32();
            if (code == 0)
            {
#if NETSTANDARD2_0
                return Version.Parse(root.GetProperty("data").GetProperty("version").GetString()!.Substring(1)); // v1.0.0, skip 'v'
#else
                return Version.Parse(root.GetProperty("data").GetProperty("version").GetString()![1..]); // v1.0.0, skip 'v'
#endif
            }
            throw GetCommonException(code, in root);
        }

        /// <inheritdoc cref="GetVersionAsync(HttpClient, MiraiHttpSessionOptions)"/>
        public static Task<Version> GetVersionAsync(MiraiHttpSessionOptions options)
        {
            return GetVersionAsync(_Client, options);
        }

        /// <inheritdoc cref="GetVersionAsync(HttpClient, MiraiHttpSessionOptions)"/>
        public Task<Version> GetVersionAsync()
        {
            InternalSessionInfo session = SafeGetSession();
            return GetVersionAsync(session.Client, session.Options);
        }

        /// <summary>
        /// 异步释放Session
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task ReleaseAsync(CancellationToken token = default)
        {
            CheckDisposed();
            InternalSessionInfo? session = Interlocked.Exchange(ref SessionInfo, null!);
            if (session == null)
            {
                throw new InvalidOperationException("请先连接到一个Session。");
            }
            return InternalReleaseAsync(session, token);
        }

        private async Task InternalReleaseAsync(InternalSessionInfo session, CancellationToken token = default)
        {
            session.Connected = false;
            session.Canceller?.Cancel();
            session.Canceller?.Dispose();
            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(new
            {
                sessionKey = session.SessionKey,
                qq = session.QQNumber
            });
            try
            {
                await InternalHttpPostAsync(session.Client, $"{session.Options.BaseUrl}/release", payload, token: token);
            }
            finally
            {
                session.Client?.Dispose();
            }
        }
    }
}