using MagicOnion.Server;
using Reactive.Bindings;

var builder = WebApplication.CreateBuilder(args);

// DIコンテナにReactivePropertyを登録
builder.Services.AddSingleton(new ReactiveProperty<string>(""));

// gRPCとMagicOnionの設定
builder.Services.AddGrpc();
builder.Services.AddMagicOnion();

var app = builder.Build();

// MagicOnionサービスを登録
app.MapMagicOnionService();

// サーバーを起動
Console.WriteLine("Server is running...");
app.Run();
