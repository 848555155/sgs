﻿using Microsoft.EntityFrameworkCore;
using ProtoBuf.Grpc.Server;
using Sanguosha.Lobby.Server;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddDbContext<AccountContext>(
    options=>
    {
        options.UseInMemoryDatabase("InMemory");
    });
builder.Services.AddGrpc();
builder.Services.AddCodeFirstGrpc();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<LobbyService>();
//app.MapGrpcService<LobbyServiceImpl>();
app.MapGet("/", () => "This is Sanduosha game server");

app.Run();
