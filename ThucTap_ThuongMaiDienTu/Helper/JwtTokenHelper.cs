using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ThucTap_ThuongMaiDienTu.Models;

public static class JwtTokenHelper
{
    private const string Path = "appsettings.json";

    public static string GenerateToken(Account user)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile(Path)
            .Build()
            .GetSection("Jwt");

        var key = Encoding.UTF8.GetBytes(config["SecretKey"]);
        var issuer = config["Issuer"];
        var audience = config["Audience"]; 

        if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || key.Length < 32)
            throw new InvalidOperationException("JWT configuration is missing or invalid.");

        // Define user-specific claims (information embedded in the token)
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.AccountType),
            new Claim("UserId", user.Id.ToString())
        };

        // Describe the token parameters
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1), // Set token expiration
            Issuer = issuer,    // Set Issuer
            Audience = audience, // Set Audience
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
    public static ClaimsPrincipal? ValidateTokenFromRequest(HttpRequest request, out SecurityToken validatedToken)
    {
        validatedToken = null;
        var jwtToken = request.Cookies["jwtToken"];

        if (string.IsNullOrEmpty(jwtToken))
        {
            Console.WriteLine("JWT token not found.");
            return null;
        }

        var secretKey = new ConfigurationBuilder()
            .AddJsonFile(Path)
            .Build().GetSection("Jwt")["SecretKey"];
        return ValidateToken(jwtToken, secretKey, out validatedToken);
    }
    public static ClaimsPrincipal? ValidateToken(string token, string secretKey, out SecurityToken validatedToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        try
        {
            var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,  // Set to `true` if you validate the issuer
                ValidateAudience = false, // Set to `true` if you validate the audience
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Optional: adjust for server time differences
            }, out validatedToken);

            return claimsPrincipal;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
            validatedToken = null;
            return null;
        }
    }

    public static string? GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        return principal?.FindFirst(claimType)?.Value;
    }public static bool TryAuthenticateUser(HttpRequest request, HttpContext context, out ClaimsPrincipal? principal)
    {
        principal = ValidateTokenFromRequest(request, out SecurityToken validatedToken);

        if (principal == null)
        {
            Console.WriteLine("Invalid JWT token.");
            return false;
        }

        // Assign the validated principal to HttpContext.User
        context.User = principal;
        return true;
    }
}
