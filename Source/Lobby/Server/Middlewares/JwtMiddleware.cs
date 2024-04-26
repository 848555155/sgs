using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
namespace Sanguosha.Lobby.Server;

public class JwtMiddleware(IMemoryCache memoryCache): IMiddleware
{
    private readonly IMemoryCache cache = memoryCache;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // context.Request.Headers[JwtBearerDefaults.AuthenticationScheme]
        // var result = await base.ValidateTokenAsync(token, validationParameters);
        // if (result.IsValid == false)
        //     return result;
        // if (!cache.TryGetValue("", out string? cacheToken) || cacheToken != token)
        // {
        //     result.IsValid = false;
        //     return result;
        // }
        // return result;
        await next(context);
    }
}
