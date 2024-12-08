using Grpc.Core;
using MagicOnion;

namespace MouseServiceContracts
{
   
    public interface IMouseService : IService<IMouseService>
    {
        // クライアントA用: サーバーからマウスデータを受信
        Task<DuplexStreamingResult<string, string>> SubscribeMouseData();

        // クライアントB用: サーバーにマウスデータを送信
        Task<DuplexStreamingResult<string, string>> SendMouseData();

    }
}
