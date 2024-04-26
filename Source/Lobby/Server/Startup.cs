using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sanguosha.Lobby.Core;

namespace Sanguosha.Lobby.Server;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();
        services.AddDbContext<AccountContext>(options => options.UseInMemoryDatabase(Configuration.GetConnectionString("DefaultConnection") ?? "memory"));
        services.AddSingleton<LobbyManager>();
        services.AddGrpc();
        services.AddHostedService<DeadRoomCleanupService>();
        services.AddAuthorizationBuilder()
            .AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireClaim(ClaimTypes.Name);
            });
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = "SanguoshaServer",
                    ValidateAudience = true,
                    ValidAudience = "SanguoshaClients",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey([.. Guid.Empty.ToByteArray(), .. Guid.Empty.ToByteArray()]),
                    //ValidAlgorithms = ["HmacSha256"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RequireExpirationTime = true,
                };
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoint =>
        {
            endpoint.MapGrpcService<LobbyService>();
            endpoint.MapGet("/", () => "这是游戏的服务器端，看到这个表示启动成功了");
        });
    }

}
