using MagicOnion.Server;
using Reactive.Bindings;

var builder = WebApplication.CreateBuilder(args);

// DI�R���e�i��ReactiveProperty��o�^
builder.Services.AddSingleton(new ReactiveProperty<string>(""));

// gRPC��MagicOnion�̐ݒ�
builder.Services.AddGrpc();
builder.Services.AddMagicOnion();

var app = builder.Build();

// MagicOnion�T�[�r�X��o�^
app.MapMagicOnionService();

// �T�[�o�[���N��
Console.WriteLine("Server is running...");
app.Run();
