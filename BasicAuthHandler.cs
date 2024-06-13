using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ProjectBoard.Models;

namespace ProjectBoard.Auth;

public class BasicAuthHandler: AuthenticationHandler<AuthenticationSchemeOptions> {

    private ProjectContext projectContext;

    public BasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        ProjectContext projectContext
    ) : base(options, logger, encoder, clock) {
        this.projectContext = projectContext;
    }
    

    protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
        if (!Request.Headers.ContainsKey("Authorization")) {
            return AuthenticateResult.Fail("Authorization header not found");
        }

        string username;
        string password;
        string hash;

        try {
            // Extract the header value and convert to bytes.
            var headerVal = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(headerVal.Parameter);
            var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(":");

            // Extract the raw decoded username and password for validation. 
            username = credentials[0];
            password = credentials[1];
            // Generate the has to compare aggainst.
            hash = SharedHashAlgorithm(credentialBytes);
        } catch (System.Exception) {
            return AuthenticateResult.Fail("Ran into a problem while dycrypting user login credentials");
        }

        // Check for the user in the database and validate crededntial details match. 
        using (var dbContext = projectContext) {
            var user = dbContext.Users.FirstOrDefault(user => user.Username == username);
            if (user == null) {
                return await Task.FromResult(AuthenticateResult.Fail("Incorrect username"));
            }

            if (user.Password != password) {
                return await Task.FromResult(AuthenticateResult.Fail("Incorrect password"));
            }

            if (user.Hash != hash) {
                return await Task.FromResult(AuthenticateResult.Fail("Invalid credentials"));
            }

            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }

    public static string SharedHashAlgorithm(byte[] bytesBuffer) {
        using (SHA256 sha256Hash = SHA256.Create()) {
            byte[] bytes = sha256Hash.ComputeHash(bytesBuffer);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++) {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
