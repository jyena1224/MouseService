using MagicOnion.Server;
using MagicOnion;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Grpc.Core;
using MouseServiceContracts;
using Reactive.Bindings;
using System.Reactive.Linq;
using System.Runtime.Serialization;

public class MouseService : ServiceBase<IMouseService>, IMouseService
{
    // クライアントAへの送信用ストリームを保持
    private readonly ReactiveProperty<string> _mouseData;

    // コンストラクタでシングルトンのReactivePropertyを注入
    public MouseService(ReactiveProperty<string> mouseData)
    {
        _mouseData = mouseData;
    }
    // クライアントA用: マウスデータをリアルタイムで配信
    public async Task<DuplexStreamingResult<string, string>> SubscribeMouseData()
    {
        var streamingContext = GetDuplexStreamingContext<string, string>();

        // クライアントAにデータを送信する処理を設定
        _mouseData
            .Subscribe(data =>
            {
                if (!string.IsNullOrEmpty(data))
                {
                    try
                    {
                        streamingContext.WriteAsync(data).Wait();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending data to Client A: {ex.Message}");
                    }
                }
            });

        // クライアントAからのリクエストストリームを監視（例では使用しない）
        await foreach (var message in streamingContext.ReadAllAsync())
        {
            Console.WriteLine($"Received from Client A: {message}");
        }

        return streamingContext.Result();
    }

    // クライアントB用: マウスデータをサーバーに送信
    public async Task<DuplexStreamingResult<string, string>> SendMouseData()
    {
        // DuplexStreamingResult<TRequest, TResponse> のインスタンスを取得
        var streamingResult = GetDuplexStreamingContext<string, string>();

        // クライアントBからのリクエストストリームを監視
        await foreach (var mouseData in streamingResult.ReadAllAsync())
        {
            Console.WriteLine($"Received from Client B: {mouseData}");
            _mouseData.Value = mouseData; // ReactivePropertyにデータを格納
        }

        return streamingResult.Result();
    }
}
