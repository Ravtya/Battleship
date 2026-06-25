using Battleship.Api.Hubs;
using Battleship.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<PlayerService>();
builder.Services.AddSingleton<GameService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapGet("/health", () => Results.Ok("ok"));
app.MapHub<BattleHub>("/Battle");

app.Run();
