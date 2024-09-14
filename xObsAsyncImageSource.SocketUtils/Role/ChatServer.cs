using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using xObsAsyncImageSource.Helper;

namespace xObsAsyncImageSource.SocketUtils.Role
{
    //服务端
    public partial class ChatServer : ChatBase
    {
        //SemaphoreSlimのインスタンスを生成
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(0, 1);

        //
        private Socket serverSocket;

        //字典，新客户端连接时，往 _socketMap 添加该客户端
        private Dictionary<string, Socket> _socketMap = new();     // address -> Socket  3.
        private Dictionary<string, string> _userMap = new();       // name -> address    1.
        private Dictionary<string, string> _userMapReverse = new();// address -> name    2.

        /// <summary>
        /// 构造
        /// </summary>
        public ChatServer(string name, int age = 18)
        {
            base.CharacterName = $"{name}";
            base.CharacterAge = $"{age}";
        }

        // TCP/IPの接続開始処理
        public async Task<bool> StartListening(string address, int port, Action<int> port_act)
        {
            if (StartState is StartState.Started || StartState is StartState.Starting) { return true; }

            // 始まるんだ
            StartState = StartState.Starting;

            // IPアドレスとポート番号を指定して、ローカルエンドポイントを設定
            if (IPAddress.TryParse(address, out var validaddress))
            {
                IPEndPoint localEndPoint = new IPEndPoint(validaddress, port);

                // TCP/IPのソケットを作成
                Socket TcpServer = serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //
                int retryCount = 0;

                while (StartState is not StartState.Stopped)
                {
                    if (retryCount++ > 10)
                    {
                        Debug.WriteLine("★server -> サーバー初期化できませんでした");
                        break;
                    }

                    try
                    {
                        TcpServer.Bind(localEndPoint);  // TCP/IPのソケットをローカルエンドポイントにバインド
                        TcpServer.Listen(32);           // 待ち受け開始

                        port_act.Invoke(((IPEndPoint)TcpServer.LocalEndPoint!).Port); // ポート番号を外に送り返す

                        StartState = StartState.Started;

                        await Task.Run(() =>
                        {
                            while (StartState is StartState.Started)
                            {
                                // 非同期ソケットを開始して、接続をリッスンする
                                Debug.WriteLine("★server -> 接続待機中...");
                                TcpServer.BeginAccept(new AsyncCallback(InitAcceptCallback), TcpServer);

                                // シグナル状態になるまで待機
                                semaphoreSlim.Wait();
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"StartListening Error—{ex.Message}");
                    }
                }
            }

            if (StartState is StartState.Stopped)
            {
                return true;
            }

            StartState = StartState.None; return false;
        }

        // サーバーを停止する
        public async Task StopListening()
        {
            if (StartState is StartState.Started)
            {
                StartState = StartState.Stopped;

                foreach (var client in _socketMap.Values)
                {
                    client.Close();
                }

                serverSocket.Close();

                _socketMap.Clear();
                _userMap.Clear();
                _userMapReverse.Clear();

                semaphoreSlim.Release();
            }

            await Task.CompletedTask;
        }

        private void InitAcceptCallback(IAsyncResult ar)
        {
            try
            {
                if (StartState is StartState.Stopped) { return; }

                // シグナル状態にし、メインスレッドの処理を続行する
                semaphoreSlim.Release();

                // クライアント要求を処理するソケットを取得
                Socket TcpServer = (Socket)ar.AsyncState;
                Socket TcpClient = TcpServer.EndAccept(ar);

                Debug.WriteLine($"★server -> Client({TcpClient.RemoteEndPoint})が接続した");

                // 端末からデータ受信を待ち受ける
                StateObject state = new StateObject();
                state.workSocket = TcpClient;
                TcpClient.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(InitReceiveCallback), state);
            }
            catch { }
        }
        private void InitReceiveCallback(IAsyncResult ar)
        {
            var content = string.Empty;

            try
            {
                // 非同期オブジェクトからソケット情報を取得
                StateObject state = (StateObject)ar.AsyncState;
                Socket TcpClient = state.workSocket;

                // クライアントソケットからデータを読み取り
                int bytesRead = TcpClient.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // 受信したデータを蓄積
                    state.sb.Append(Encoding.Unicode.GetString(state.buffer, 0, bytesRead));

                    // 蓄積データの終端タグを確認
                    content = state.sb.ToString();

                    if (content.IndexOf($"{EOF}") > -1)
                    {
                        // 終了タグ<EOF>があれば、読み取り完了
                        Debug.WriteLine($"★server -> Client({TcpClient.RemoteEndPoint})曰く「{content}」");

                        // ログイン認証
                        var jsonObject = this.JsonDeserialize<ClientInitialAuthentication>(content.TrimEnd($"{EOF}".ToCharArray()));
                        // ログインメッセージ
                        var loginName = $"{jsonObject.Name}";
                        var loginAddress = $"{TcpClient.RemoteEndPoint}";
                        var loginMessage = $"{loginName}({loginAddress})がサーバーにログインした";
                        var loginReplyExPayload = new AdditionalPayload("#FFFF0000", 0)
                        {
                            ExMessageType = ExMessageType.SystemReply,
                            ExMessage = $"{this.CharacterName}が貴方に注目✨"
                        };
                        var loginReply = JsonSerialize(new ClientMessage(base.CharacterName, loginName, loginMessage, loginReplyExPayload));
                        // 
                        _userMap.TryAdd(loginName, loginAddress);
                        _userMapReverse.TryAdd(loginAddress, loginName);

                        // ASCIIコードをバイトデータに変換
                        byte[] byteData = Encoding.Unicode.GetBytes($"{loginReply}" + $"{EOF}");

                        // クライアントへデータの送信を開始
                        TcpClient.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(InitSendCallback), TcpClient);
                    }
                    else
                    {
                        // 取得していないデータがあるので、受信再開
                        TcpClient.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(InitReceiveCallback), state);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitReceiveCallback Error—{ex.Message}");
            }
        }
        private void InitSendCallback(IAsyncResult ar)
        {
            try
            {
                // 非同期オブジェクトからソケット情報を取得
                Socket TcpClient = (Socket)ar.AsyncState;

                // クライアントへデータ送信完了
                int bytesSent = TcpClient.EndSend(ar);

                // 将客户端信息写入两字典 TcpClient.RemoteEndPoint
                OnNewClientConnected(TcpClient);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitSendCallback Error—{ex.Message}");
            }
        }
    }

    // 服务端收到消息时的回调函数
    public partial class ChatServer
    {
        // 新客户端加入，开始接收[该客户端]的消息
        private void OnNewClientConnected(Socket client)
        {
            string key = client.RemoteEndPoint!.ToString()!;
            _socketMap.TryAdd(key, client);

            OnOnlineListUpdated?.Invoke(this, _userMapReverse.Values.ToList());

            var joiner = _userMapReverse[key];
            {
                this.BroadcastMessageToAllClients($"<{joiner}>上线了！", ExMessageType.SystemAlert);
                this.BroadcastMessageToAllClients(string.Empty, ExMessageType.OnlineUsersCount, _socketMap.Count);
            }

            // 端末からデータ受信を待ち受ける
            StateObject state = new StateObject();
            state.workSocket = client;
            //state.key = key;
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(OnNewClientReceiveCallback_Forwarded), state);

        }
        // 收到[该客户端]消息后，执行该回调函数（具体行为：解析JSON中的目标客户端地址，将消息转发给目标客户端）
        private void OnNewClientReceiveCallback_Forwarded(IAsyncResult ar)
        {
            try
            {
                // 非同期オブジェクトからソケット情報を取得
                StateObject state = (StateObject)ar.AsyncState;
                Socket TcpClient = state.workSocket;
                //string key = state.key;

                // クライアントソケットからデータを読み取り
                int bytesRead = TcpClient.EndReceive(ar);

                if (bytesRead > 0 || state.packetSticking)
                {
                    // 受信したデータを蓄積
                    state.sb.Append(Encoding.Unicode.GetString(state.buffer, 0, bytesRead));

                fixSticking:
                    // 蓄積データの終端タグを確認
                    var content = state.sb.ToString();

                    // 終了タグ<EOF>があれば、読み取り完了
                    if (content.IndexOf($"{EOF}") > -1)
                    {
                        state.sb.Clear();// 已拿到完整消息，缓存可以清空了

                        // 紧急粘包处理
                        var sidx = content.IndexOf($"{EOF}");
                        if (sidx + EOF.Length < content.Length)
                        {
                            var previous_content = content.Substring(0, sidx + EOF.Length); //首个完整的包
                            var current_content = content.Substring(sidx + EOF.Length);     //后面几个粘包
                            content = previous_content;
                            state.sb.Append(current_content);
                            state.packetSticking = true;

                            Debug.WriteLine($"◆server({base.CharacterName}) -> 粘包处理中...");
                        }
                        else
                        {
                            state.packetSticking = false;
                        }

                        // 解析Json，获取 sender、receiver、message
                        var jsonObject = this.JsonDeserialize<ClientMessage>(content.TrimEnd($"{EOF}".ToCharArray()));
                        if (jsonObject != null)
                        {
                            _userMap.TryGetValue(jsonObject.ReceiverName, out string? _receiverAddress);// 获得收件人地址
                            _userMap.TryGetValue(jsonObject.SenderName, out string? _senderAddress);    // 获得发件人地址
                            if (_receiverAddress is not null && _senderAddress is not null)
                            {
                                var senderName = jsonObject.SenderName;
                                var senderAddress = _senderAddress.ToString();
                                var senderMessage = jsonObject.Message;
                                var additionalPayload = jsonObject.AdditionalPayload;

                                var receiverName = jsonObject.ReceiverName;
                                var receiverAddress = _receiverAddress.ToString();
                                var receiverClient = _socketMap[_receiverAddress]; // 获得收件人socket

                                var jsonString = JsonSerialize(new ClientMessage(senderName, receiverName, senderMessage, additionalPayload));
                                byte[] byteData = Encoding.Unicode.GetBytes($"{jsonString}" + $"{EOF}");
                                receiverClient.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(OnSpecificClientSendCallback), receiverClient);
                            }
                            else
                            {
                                if (jsonObject.ReceiverName == this.CharacterName)
                                {
                                    //Debug.WriteLine($"もう！「{this.CharacterName}」にメッセージを送っちゃダメだってば...😖");

                                    // 收件人为服务端的消息直接转为群发
                                    this.BroadcastMessageToAllClients(jsonObject);
                                }
                                else
                                {
                                    //System.Windows.MessageBox.Show($"送信先のユーザー「{jsonObject.ReceiverName}」がサーバーにログインしていない");
                                    Win32.MessageBox(IntPtr.Zero, $"送信先のユーザー「{jsonObject.ReceiverName}」がサーバーにログインしていない", "Error", 0x00040000);
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("Server: json解析失败");
                        }

                        if (state.packetSticking) { goto fixSticking; } // 我特么直接goto
                    }

                    //else
                    {
                        // 取得していないデータがあるので、受信再開
                        TcpClient.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(OnNewClientReceiveCallback_Forwarded), state);
                    }
                }
                else
                {
                    // ここにまで辿り着けないよう祈る
                    throw new InvalidOperationException("Server: The code should not reach this point.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Server: OnNewClientReceiveCallback_Forwarded Error—{ex.Message}");

                OnClientDisconnect(ar);
            }
        }

        // 服务端主动将消息发送至指定客户端后，执行该回调函数
        private void OnSpecificClientSendCallback(IAsyncResult ar)
        {
            try
            {
                // 非同期オブジェクトからソケット情報を取得
                Socket TcpClient = (Socket)ar.AsyncState;

                // クライアントへデータ送信完了
                int bytesSent = TcpClient.EndSend(ar);

                //Debug.WriteLine($"server -> 「{bytesSent} byte」をClientへ送信");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Server: OnSpecificClientSendCallback Error—{ex.Message}");
            }
        }

        // 断连善后
        private void OnClientDisconnect(IAsyncResult ar)
        {
            try
            {
                var oldstate = (StateObject)ar.AsyncState!;
                var address = $"{oldstate.workSocket.RemoteEndPoint}";
                var client = _socketMap[address!];

                if (client.Connected)
                {
                    // client.Disconnect(true);

                    // 重新收消息
                    StateObject state = new StateObject();
                    state.workSocket = client;
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(OnNewClientReceiveCallback_Forwarded), state);
                }
                else
                {
                    var exitor = _userMapReverse[address];
                    {
                        _socketMap.Remove(address);
                        _userMap.Remove(_userMapReverse[address]);
                        _userMapReverse.Remove(address);
                        Debug.WriteLine($"★server -> Client({address})が切断した");

                        this.BroadcastMessageToAllClients($"<{exitor}>下线了...", ExMessageType.SystemAlert);
                        this.BroadcastMessageToAllClients(string.Empty, ExMessageType.OnlineUsersCount, _socketMap.Count);

                        OnOnlineListUpdated?.Invoke(this, _userMapReverse.Values.ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"★server: OnClientDisconnect Error—{ex.Message}");
            }
        }
    }

    // 服务端主动向客户端发消息
    public partial class ChatServer
    {
        // 服务端 主动群发
        private void BroadcastMessageToAllClients(string message)
        {
            foreach (var server_to_client in _socketMap.Values)
            {
                var jsonString = JsonSerialize(new ClientMessage(base.CharacterName, "親愛なるあなた", message, new("#FFFF0000", 0)));
                byte[] byteData = Encoding.Unicode.GetBytes($"{jsonString}" + $"{EOF}");
                server_to_client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(OnSpecificClientSendCallback), server_to_client);
            }
        }

        // 服务端 主动群发 系统消息
        private void BroadcastMessageToAllClients(string message, ExMessageType exType, object? exObj = null)
        {
            if (exType is not ExMessageType.SystemAlert && exType is not ExMessageType.OnlineUsersCount)
            {
                return;
            }

            var senderName = this.CharacterName;
            var senderMessage = string.Empty;
            var additionalPayload = new AdditionalPayload("#FFFF0000", 0)
            {
                ExMessageType = exType,
                ExMessage = message,
                ExObject = $"{exObj}"
            };

            var jsonString = JsonSerialize(new ClientMessage(senderName, "親愛なるそなた", senderMessage, additionalPayload));
            byte[] byteData = Encoding.Unicode.GetBytes($"{jsonString}" + $"{EOF}");

            foreach (var server_to_client in _socketMap.Values)
            {
                server_to_client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(OnSpecificClientSendCallback), server_to_client);
            }
        }

        // 服务端 代理群发
        private void BroadcastMessageToAllClients(ClientMessage jsonObject)
        {
            _userMap.TryGetValue(jsonObject.SenderName, out string? _senderAddress);    // 获得发件人地址
            if (_senderAddress is not null)
            {
                var senderName = jsonObject.SenderName;
                var senderAddress = _senderAddress.ToString();
                var senderMessage = jsonObject.Message;
                var additionalPayload = jsonObject.AdditionalPayload;

                var jsonString = JsonSerialize(new ClientMessage(senderName, "親愛なるそなた", senderMessage, additionalPayload));
                byte[] byteData = Encoding.Unicode.GetBytes($"{jsonString}" + $"{EOF}");

                foreach (var server_to_client in _socketMap.Values)
                {
                    server_to_client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(OnSpecificClientSendCallback), server_to_client);
                }
            }
        }

        // 服务端 代理单发
        private void SendMessageToClient(string targetName, string message)
        {
            _userMap.TryGetValue(targetName, out string? _receiverAddress);//获得收件人地址
            _socketMap.TryGetValue(_receiverAddress ?? "", out var server_to_client);
            if (_receiverAddress is not null && server_to_client is not null)
            {
                var jsonString = JsonSerialize(new ClientMessage(base.CharacterName, targetName, message, new("#FFFF0000", 0)));
                byte[] byteData = Encoding.Unicode.GetBytes($"{jsonString}" + $"{EOF}");
                server_to_client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(OnSpecificClientSendCallback), server_to_client);
            }
        }


        // 启动标记
        public bool IsStarted => (StartState == StartState.Started);

        // 刷新在线列表
        public event EventHandler<List<string>> OnOnlineListUpdated;
        // 服务端名字
        public string Name => base.CharacterName;
    }
}
