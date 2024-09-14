using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using xObsAsyncImageSource.SocketUtils.Helper;

namespace xObsAsyncImageSource.SocketUtils.Role
{
    // 非同期処理でソケット情報を保持する為のオブジェクト
    public class StateObject
    {
        // 受信バッファサイズ
        public const int BufferSize = 1024;

        // 受信バッファ
        public byte[] buffer = new byte[BufferSize];

        // 受信データ
        public StringBuilder sb = new StringBuilder();

        // ソケット
        public Socket workSocket = null;

        // 粘包flag
        public bool packetSticking = false;
    }

    // 启动标志
    public enum StartState
    {
        Started = 0,
        Starting,
        StartFalse,
        Stopped,
        None
    }

    // 作为客户端，向服务端报道时，的信件格式
    public record ClientInitialAuthentication([property: JsonPropertyName("Name")] string Name,
                                              [property: JsonPropertyName("Age")] string Age);

    // 发消息，转发消息，收消息，使用通用格式
    public record ClientMessage([property: JsonPropertyName("SenderName")] string SenderName,
                                [property: JsonPropertyName("ReceiverName")] string ReceiverName,
                                [property: JsonPropertyName("Message")] string Message,
                                [property: JsonPropertyName("AdditionalPayload")] AdditionalPayload AdditionalPayload);

    // 追加消息
    public class AdditionalPayload
    {
        [JsonPropertyName("SenderColor")]
        public string SenderColor { get; set; }

        [JsonPropertyName("SenderAvatarIdx")]
        public int SenderAvatarIdx { get; set; }

        [JsonPropertyName("ExMessageType")]
        public ExMessageType ExMessageType { get; set; }

        [JsonPropertyName("ExMessage")]
        public string ExMessage { get; set; }

        [JsonPropertyName("ExObject")]
        public string? ExObject { get; set; }

        /// <summary>
        /// ExObject类型改为string后，添加一个MatchObject用来临时储存ExObject反序列化后的结果以便UI绑定
        /// </summary>
        [JsonIgnore]
        public object? MatchObject { get; set; }

        public AdditionalPayload(string senderColor, int senderAvatarIdx)
        {
            // 必填
            SenderColor = senderColor;
            SenderAvatarIdx = senderAvatarIdx;

            // 选填
            ExMessageType = ExMessageType.Chat;
            ExMessage = string.Empty;
            ExObject = null;
            MatchObject = null;
        }
    }

    // 追加消息的类型
    [JsonConverter(typeof(JsonStringEnumConverter<ExMessageType>))]
    public enum ExMessageType
    {
        Chat = 0,          //聊天消息
        SystemAlert,       //系统公告 ExMessage
        SystemReply,       //首登回复 ExMessage
        OnlineUsersCount,  //在线人数 ExObject
        GameMatching,      //发起对战 ExObject
        GameConfirm,       //确认对战 ExObject
        GameInProgress,    //正在对战 ExObject
        WinnerAlert,       //结算公告 ExObject
    }

    // Working at AOT
    [JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(ClientInitialAuthentication))]
    [JsonSerializable(typeof(ClientMessage))]
    public partial class JsonSourceGenerationContext : JsonSerializerContext
    {
    }
}

namespace xObsAsyncImageSource.SocketUtils.Role
{
    public partial class ChatBase
    {
        protected StartState StartState = StartState.None;
        protected string CharacterName { get; init; }
        protected string CharacterAge { get; init; }
        protected readonly string EOF = "<EOF>";

        public CharacterInfo CharacterInfo { get; init; }

        // Working at AOT
        protected JsonSerializerOptions JsonOptions => new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        //// 序列化
        protected string JsonSerialize<T>(T jsonObject)
        {
            return JsonSerializer.Serialize(jsonObject, typeof(T), new JsonSourceGenerationContext(JsonOptions));
        }
        //// 反序列化
        protected T? JsonDeserialize<T>(string jsonText)
        {
            return (T?)JsonSerializer.Deserialize(jsonText, new JsonSourceGenerationContext().GetTypeInfo(typeof(T))!);
        }

        //// 压缩
        //protected byte[] DeflateCompressText(string text)
        //{
        //    return DeflateService.CompressText(text);
        //}
        //protected byte[] DeflateCompressText(byte[] text)
        //{
        //    return DeflateService.CompressText(text);
        //}
        //// 解压缩
        //protected string DeflateDecompressData(byte[] compressedData)
        //{
        //    return DeflateService.DecompressData(compressedData);
        //}
    }
}
