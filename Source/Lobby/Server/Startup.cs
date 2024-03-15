using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sanguosha.Lobby.Core;

namespace Sanguosha.Lobby.Server;

public class Startup(IConfiguration configuration)
{
    public static readonly JwtSecurityTokenHandler JwtTokenHandler = new();
    public static readonly SymmetricSecurityKey SecurityKey = new([.. Guid.NewGuid().ToByteArray(), .. Guid.NewGuid().ToByteArray()]);
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
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = false,
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.FromSeconds(5),
                    RequireExpirationTime = false,
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
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseAuthentication();
        app.UseEndpoints(endpoint =>
        {
            endpoint.MapGrpcService<LobbyService>();
            endpoint.MapGet("/", () => "这是游戏的服务器端，看到这个表示启动成功了");
        });
    }

    protected string GenerateJwtToken(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException("Name is not specified.");
        }
        var claims = new[] { new Claim(ClaimTypes.Name, name) };
        var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken("SanguoshaServer", "SanguoshaClients", claims, expires: DateTime.Now.AddDays(1), signingCredentials: credentials);
        return JwtTokenHandler.WriteToken(token);
    }
}
