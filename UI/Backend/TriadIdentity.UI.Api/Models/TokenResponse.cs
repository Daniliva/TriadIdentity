namespace TriadIdentity.UI.Api.Models;

public class TokenResponse
{
    public string Token { get; set; }
    public DateTime Expires { get; set; }
}
