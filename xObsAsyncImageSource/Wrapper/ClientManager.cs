using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using xObsAsyncImageSource.SocketUtils.Helper;
using xObsAsyncImageSource.SocketUtils.Role;

namespace xObsAsyncImageSource.Wrapper
{
    public static class ClientManager
    {
        private static Random rand = new Random(new Random().Next());
        private static string[] catName = NameRepository.ClientNameList;
        private static ConcurrentDictionary<string, ChatClient> clientList = new();

        public static async Task Login(string feature, string? ip, int? port, Action<string>? callback = null)
        {
            if (clientList.ContainsKey(feature))
            {
                var client = clientList[feature];

                if (client.IsStarted)
                {
                    Proxy.MessageBox($"已登入 ({feature})");
                }
                else
                {
                    //重新登入
                    if (ClientManager.CheckIfValidNetworkEndpoint(ip, port))
                    {
                        if (client.IsStarted)
                        {
                            Proxy.MessageBox("已登入"); return;
                        }

                        var flag = await client.StartClient(ip!, port!.Value, response =>
                        {
                            if (IsPngFile(response.Message)) { callback?.Invoke(response.Message); }
                        });

                        Proxy.MessageBox(flag ? "登入成功" : "登入失败");
                    }
                }
            }
            else
            {
                //生成并首次登入
                var name = catName[rand.Next(0, catName.Length)];
                var idx = rand.Next(0, catName.Length);
                var client = new ChatClient(new CharacterInfo(name, feature, idx));

                if (ClientManager.CheckIfValidNetworkEndpoint(ip, port))
                {
                    if (client.IsStarted)
                    {
                        Proxy.MessageBox("已登入"); return;
                    }

                    var flag = await client.StartClient(ip!, port!.Value, response =>
                    {
                        if (IsPngFile(response.Message)) { callback?.Invoke(response.Message); }
                    });

                    if (flag)
                    {
                        clientList.TryAdd(feature, client);
                    }

                    Proxy.MessageBox(flag ? "登入成功" : "登入失败");
                }
                else
                {
                    Proxy.MessageBox("无效的 IP 地址或端口号");
                }
            }
        }
        public static async Task Logout(string feature)
        {
            if (clientList.ContainsKey(feature) && clientList[feature] is ChatClient client)
            {
                await client.StopClient();

                //Proxy.MessageBox($"{feature} 已登出");
            }
        }

        public static async Task SendMessage(string feature, string? message)
        {
            if (clientList.ContainsKey(feature))
            {
                var client = clientList[feature];

                if (client.IsStarted)
                {
                    var target = NameRepository.ServerName; // 直接给服务端发消息 = 群发
                    var msg = string.IsNullOrEmpty(message) ? "(empty message)" : message;
                    var payload = new AdditionalPayload(ColorRepository.ObsColor, client.CharacterInfo.AvatarIndex);

                    await client.SendMessageToClient(target, msg, payload);
                }
            }
            else
            {
                Proxy.MessageBoxTask("客户端不存在");
            }
        }

        private static bool CheckIfValidNetworkEndpoint(string? ipAddress, int? port)
        {
            // IsValidIPAddress
            if (!IPAddress.TryParse(ipAddress, out _)) { return false; }

            // IsValidPort
            if (!(port is not null && port > 0 && port <= 65535)) { return false; }

            return true;
        }
        private static bool IsPngFile(string filePath)
        {
            if (!File.Exists(filePath)) { return false; }

            byte[] pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

            // 读取文件的前8个字节
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] fileHeader = new byte[8];
                fs.Read(fileHeader, 0, fileHeader.Length);

                // 检查文件头是否与PNG签名匹配
                for (int i = 0; i < pngSignature.Length; i++)
                {
                    if (fileHeader[i] != pngSignature[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
