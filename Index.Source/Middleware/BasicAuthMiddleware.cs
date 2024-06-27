using System.Text;

namespace Index.Source.Middleware;

public class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;

    private readonly IConfiguration _configuration;
    public BasicAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        var authHeaderValue = context.Request.Headers.Authorization;

        if (!string.IsNullOrWhiteSpace(authHeaderValue) && IsAuthHeaderValid(authHeaderValue))
        {
            await _next(context);
        }
        else 
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.WWWAuthenticate = "Basic realm=\"localhost\"";
        }  
    }

    private bool IsAuthHeaderValid(string authHeaderValue)
    {
        if (!authHeaderValue.StartsWith("Basic "))
        {
            return false;
        }

        var encodedValue = authHeaderValue.Substring(6);

        byte[] valueBytes;

        try
        {
            valueBytes = Convert.FromBase64String(encodedValue);
        }
        catch (Exception)
        {
            return false;
        }

        var decodedValue = Encoding.UTF8.GetString(valueBytes);

        if (!decodedValue.Contains(':'))
        {
            return false;
        }

        var authArray = decodedValue.Split(':');

        if (_configuration["Auth:ValidUser"] == authArray[0] && _configuration["Auth:ValidPassword"] == authArray[1])
        {
            return true;
        }

        return false;
    }
}
