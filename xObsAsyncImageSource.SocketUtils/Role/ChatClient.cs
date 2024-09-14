using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using xObsAsyncImageSource.SocketUtils.Helper;

namespace xObsAsyncImageSource.SocketUtils.Role
{
    //客户端
    public partial class ChatClient : ChatBase
    {
        /// <summary>
        /// 构造
        /// </summary>
        //public ChatClient(string name, int age = 18)
        //{
        //    base.CharacterName = $"{name}";
        //    base.CharacterAge = $"{age}";
        //}

        /// <summary>
        /// 构造2
        /// </summary>
        public ChatClient(CharacterInfo characterInfo)
        {
            var feature = string.IsNullOrEmpty(characterInfo.AvatarFeature) ? "" : $"_{characterInfo.AvatarFeature}";

            base.CharacterInfo = characterInfo;
            base.CharacterName = $"{characterInfo.AvatarName}{feature}";
            base.CharacterAge = $"{characterInfo.AvatarAge}";
        }
    }
    public partial class ChatClient
    {
        // SemaphoreSlimのインスタンスを生成
        private SemaphoreSlim semaphoreSlim;// = new SemaphoreSlim(0, 1);

        // 收到消息时，执行上层提供的回调函数（将 senderName、senderMessage 回传到上层）
        private Action<ClientMessage>? _callback;

        // 储存服务端地址
        private Socket? client_to_server;


        // 启动客户端
        public async Task<bool> StartClient(string ipaddress, int port, Action<ClientMessage>? callback = null)
        {
            await Task.CompletedTask;

            if (StartState == StartState.Started || StartState == StartState.Starting)
            {
                return true;
            }
            else
            {
                // 再接続ができるようになろう
                StartState = StartState.Starting;

                semaphoreSlim = new SemaphoreSlim(0, 1);
                _callback = callback;
            }

            // サーバーへ接続
            try
            {
                // IPアドレスとポート番号を取得
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ipaddress), port);

                // TCP/IPのソケットを作成
                Socket? client = new Socket(IPAddress.Parse(ipaddress).AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // サーバーをキャッチ
                client_to_server = client;

                // エンドポイント（IPアドレスとポート）へ接続
                client.BeginConnect(endpoint, new AsyncCallback(InitConnectCallback), client);
                semaphoreSlim.Wait();  // 接続シグナルになるまで待機

                if (StartState == StartState.StartFalse) { return false; }

                // 初回接続認証（仮）
                string data = JsonSerialize(new ClientInitialAuthentication(base.CharacterName, base.CharacterAge));

                // ASCIIエンコーディングで送信データをバイトの配列に変換
                byte[] byteData = Encoding.Unicode.GetBytes(data + $"{EOF}");

                // サーバーへデータを送信
                client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(InitSendCallback), client);
                semaphoreSlim.Wait();  // 送信シグナルになるまで待機

                // ソケット情報を保持する為のオブジェクトを生成
                StateObject state = new StateObject();
                state.workSocket = client;

                // サーバーからデータ受信
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(InitReceiveCallback), state);
                semaphoreSlim.Wait();  // 受信シグナルになるまで待機

                //
                semaphoreSlim.Dispose();

                StartState = StartState.Started; return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StartClient—{ex.Message}");

                StartState = StartState.None; return false;
            }
        }

        // 关闭客户端
        public async Task StopClient()
        {
            await Task.Yield();

            if (client_to_server is not null)
            {
                client_to_server.Close();
            }
        }

        private void InitConnectCallback(IAsyncResult ar)
        {
            try
            {
                // ソケットを取得
                Socket client = (Socket)ar.AsyncState;

                // 非同期接続を終了
                client.EndConnect(ar);

                // シグナル状態にし、メインスレッドの処理を続行する
                semaphoreSlim.Release();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitConnectCallback—{ex.Message}");

                StartState = StartState.StartFalse; semaphoreSlim.Release();
            }
        }
        private void InitSendCallback(IAsyncResult ar)
        {
            try
            {
                // ソケットを取得
                Socket client = (Socket)ar.AsyncState;

                // 非同期送信を終了
                int bytesSent = client.EndSend(ar);

                // シグナル状態にし、メインスレッドの処理を続行する
                semaphoreSlim.Release();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitSendCallback—{ex.Message}");
            }
        }
        private void InitReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // ソケット情報を保持する為のオブジェクトから情報取得
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // 非同期受信を終了
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // 受信したデータを蓄積
                    state.sb.Append(Encoding.Unicode.GetString(state.buffer, 0, bytesRead));

                    // 蓄積データの終端タグを確認
                    var content = state.sb.ToString();

                    // 終了タグ<EOF>があれば、読み取り完了
                    if (content.EndsWith($"{EOF}"))
                    {
                        Debug.WriteLine($"☆client({base.CharacterName}) -> Server({client_to_server?.RemoteEndPoint})曰く「{content}」");

                        state.sb.Clear();// 需要反复使用该对象，所以清空

                        var jsonObject = this.JsonDeserialize<ClientMessage>(content.TrimEnd($"{EOF}".ToCharArray()));
                        if (jsonObject is not null)
                        {
                            _callback?.Invoke(jsonObject);// 消息回传给上层
                        }

                        semaphoreSlim.Release();

                        OnLoginSuccess(client);
                    }
                    else
                    {
                        // 受信処理再開（まだ受信しているデータがあるため）
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(InitReceiveCallback), state);
                    }
                }
                else
                {
                    // ここにまで辿り着けないよう祈る
                    throw new InvalidOperationException("Client: The code should not reach this point.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Client: InitReceiveCallback—{ex.Message}");
            }
        }
    }

    // 客户端收到消息时的回调函数
    public partial class ChatClient
    {
        // 本客户端已连接到服务端
        private void OnLoginSuccess(Socket client)
        {
            StartState = StartState.Started;

            StateObject state = new StateObject();
            state.workSocket = client;
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(OnServerMessageReceived), state);
        }

        // 收到[服务端]消息后，执行该回调函数（具体行为：收消息，返回给上层，清空缓冲区，继续收消息）
        private void OnServerMessageReceived(IAsyncResult ar)
        {
            try
            {
                // ソケット情報を保持する為のオブジェクトから情報取得
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // 非同期受信を終了
                int bytesRead = client.EndReceive(ar);

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
                        Debug.WriteLine($"client({base.CharacterName}) -> Server({client_to_server?.RemoteEndPoint})曰く「{content}」");

                        state.sb.Clear();//bufferをクリアする

                        // 紧急粘包处理
                        var sidx = content.IndexOf($"{EOF}");
                        if (sidx + EOF.Length < content.Length)
                        {
                            var previous_content = content.Substring(0, sidx + EOF.Length); // 前一个完整的消息
                            var current_content = content.Substring(sidx + EOF.Length);
                            content = previous_content;
                            state.sb.Append(current_content);
                            state.packetSticking = true;

                            Debug.WriteLine($"◆client({base.CharacterName}) -> 粘包处理中...");
                        }
                        else
                        {
                            state.packetSticking = false;
                        }

                        var jsonObject = this.JsonDeserialize<ClientMessage>(content.TrimEnd($"{EOF}".ToCharArray()));
                        if (jsonObject is not null)
                        {
                            _callback?.Invoke(jsonObject); // 上の層にメッセージを送り返す
                        }
                        else
                        {
                            throw new InvalidOperationException("Client: json解析失败");
                        }

                        if (state.packetSticking) { goto fixSticking; } // 我特么直接goto
                    }

                    // 受信処理再開（まだ受信しているデータがあるため）
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(OnServerMessageReceived), state);
                }
                else
                {
                    // ここにまで辿り着けないよう祈る
                    throw new InvalidOperationException("Client: The code should not reach this point.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Client: OnServerMessageReceived—{ex.Message}");

                StartState = StartState.None;
            }
        }

        // 客户端主动将消息发送至服务端后，执行该回调函数
        private void OnSpecificClientSendCallback(IAsyncResult ar)
        {
            try
            {
                // 非同期オブジェクトからソケット情報を取得
                Socket TcpClient = (Socket)ar.AsyncState;

                // クライアントへデータ送信完了
                int bytesSent = TcpClient.EndSend(ar);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Client: OnSpecificClientSendCallback Error—{ex.Message}");
            }
        }
    }

    // 客户端（通过服务端）向客户端发消息
    public partial class ChatClient
    {
        // 单发
        public async Task SendMessageToClient(string targetName, string message, AdditionalPayload additionalPayload)
        {
            if (client_to_server is not null)
            {
                var senderName = base.CharacterName;
                var jsonString = JsonSerialize(new ClientMessage(senderName, targetName, message, additionalPayload));
                byte[] byteData = Encoding.Unicode.GetBytes($"{jsonString}" + $"{EOF}");
                client_to_server.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(OnSpecificClientSendCallback), client_to_server);

                await Task.CompletedTask;
            }
        }

        // 启动标记
        public bool IsStarted => (StartState == StartState.Started);
    }
}
