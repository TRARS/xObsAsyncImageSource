using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using xObsAsyncImageSource.SocketUtils.Helper;
using xObsAsyncImageSource.SocketUtils.Role;

namespace xObsAsyncImageSource.ImageSelector.Wrapper
{
    public static class ServerManager
    {
        public static event EventHandler<List<string>> OnOnlineListUpdated;

        private static ChatClient anonymous = new(new CharacterInfo("Anonymous", string.Empty, new Random().Next(0, NameRepository.ClientNameList.Length)));
        private static ChatServer server = new(NameRepository.ServerName);
        private static bool isTaskRunning;

        private static List<string> onlineList = new();

        public static async Task StartServer(string ip, int port, Action<int>? callback0, Action callback1)
        {
            if (server.IsStarted) { return; }

            if (isTaskRunning is false)
            {
                isTaskRunning = true;

                await Task.Run(async () =>
                {
                    if (!CheckIfValidNetworkEndpoint(ip, port)) { return; }

                    var flag = await server.StartListening(ip, port, realPort =>
                    {
                        callback0?.Invoke(realPort);
                        callback1?.Invoke();

                        server.OnOnlineListUpdated += (s, e) =>
                        {
                            onlineList = e;

                            ServerManager.OnOnlineListUpdated?.Invoke(s, onlineList);
                        };

                        Task.Run(async () =>
                        {
                            if (await anonymous.StartClient(ip, realPort) is false)
                            {
                                throw new NotImplementedException();
                            }
                        });
                    });

                    if (flag is false)
                    {
                        var info = string.Join("\n", LocalAddressRepository.LocalIPs.ToList());
                        await Task.Run(() => MessageBox.Show($"创建服务端失败，请填入正确本机IP。\n本机局域网IP地址列表:\n{info}"));
                    }
                }).ContinueWith(_ =>
                {
                    isTaskRunning = false;
                });
            }
        }

        public static async Task PushPng2Obs(string targetClientName, string message, bool showMsgBox = true)
        {
            if (!server.IsStarted)
            {
                if (showMsgBox) { MessageBox.Show("服务端未启动"); }
                return;
            }
            if (!onlineList.Contains(targetClientName) && targetClientName != server.Name)
            {
                if (showMsgBox) { MessageBox.Show($"目标客户端({targetClientName})不存在"); }
                return;
            }
            if (!anonymous.IsStarted)
            {
                if (showMsgBox) { MessageBox.Show("匿名客户端未启动"); }
                return;
            }

            await anonymous.SendMessageToClient(targetClientName, message ?? string.Empty, new(ColorRepository.AnonymousColor, anonymous.CharacterInfo.AvatarIndex));
        }

        private static bool CheckIfValidNetworkEndpoint(string? ipAddress, int? port)
        {
            // IsValidIPAddress
            if (!IPAddress.TryParse(ipAddress, out _)) { return false; }

            // IsValidPort
            if (!(port is not null && port >= 0 && port <= 65535)) { return false; }

            return true;
        }
    }
}
