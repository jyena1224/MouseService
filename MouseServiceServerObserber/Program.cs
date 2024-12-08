using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using MouseServiceContracts;

class Program
{
    private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
    private static HookProc _mouseProc = MouseHookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    private static DuplexStreamingResult<string, string> _requestStream;

    //[STAThread]
    static async Task Main(string[] args)
    {
        //if (args.Length < 1)
        //{
        //    Console.WriteLine("サーバーのURLを引数として渡してください。");
        //    return;
        //}

        //string serverUrl = args[0];

        // gRPCサーバーに接続
        using var channel = GrpcChannel.ForAddress("http://localhost:5000");
        var mouseService = MagicOnionClient.Create<IMouseService>(channel);

        Console.WriteLine("クライアントBが起動しました。マウス操作を監視中...");

        // 双方向ストリームを開始
        using var streaming = await mouseService.SendMouseData();
        _requestStream = streaming;

        // マウスフックを設定
        _hookID = SetHook(MouseHookCallback);

        // メッセージループを開始
        Application.Run();

        // アプリケーション終了時にフックを解除
        UnhookWindowsHookEx(_hookID);

        // ストリームを完了
        await streaming.RequestStream.CompleteAsync();
    }

    private static IntPtr SetHook(HookProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

            // マウスの座標を取得
            string mouseData = $"X: {hookStruct.pt.x}, Y: {hookStruct.pt.y}";
            Console.WriteLine(mouseData);
            // サーバーにデータを送信
            _requestStream.RequestStream.WriteAsync(mouseData);
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private const int WH_MOUSE_LL = 14;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private struct POINT
    {
        public int x;
        public int y;
    }

    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public int mouseData;
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;
    }
}
