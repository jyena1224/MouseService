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
    // �N���C�A���gA�ւ̑��M�p�X�g���[����ێ�
    private readonly ReactiveProperty<string> _mouseData;

    // �R���X�g���N�^�ŃV���O���g����ReactiveProperty�𒍓�
    public MouseService(ReactiveProperty<string> mouseData)
    {
        _mouseData = mouseData;
    }
    // �N���C�A���gA�p: �}�E�X�f�[�^�����A���^�C���Ŕz�M
    public async Task<DuplexStreamingResult<string, string>> SubscribeMouseData()
    {
        var streamingContext = GetDuplexStreamingContext<string, string>();

        // �N���C�A���gA�Ƀf�[�^�𑗐M���鏈����ݒ�
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

        // �N���C�A���gA����̃��N�G�X�g�X�g���[�����Ď��i��ł͎g�p���Ȃ��j
        await foreach (var message in streamingContext.ReadAllAsync())
        {
            Console.WriteLine($"Received from Client A: {message}");
        }

        return streamingContext.Result();
    }

    // �N���C�A���gB�p: �}�E�X�f�[�^���T�[�o�[�ɑ��M
    public async Task<DuplexStreamingResult<string, string>> SendMouseData()
    {
        // DuplexStreamingResult<TRequest, TResponse> �̃C���X�^���X���擾
        var streamingResult = GetDuplexStreamingContext<string, string>();

        // �N���C�A���gB����̃��N�G�X�g�X�g���[�����Ď�
        await foreach (var mouseData in streamingResult.ReadAllAsync())
        {
            Console.WriteLine($"Received from Client B: {mouseData}");
            _mouseData.Value = mouseData; // ReactiveProperty�Ƀf�[�^���i�[
        }

        return streamingResult.Result();
    }
}
