using Microsoft.IdentityModel.Tokens;

namespace MinimalAPIsMovies.Utilities;

public class KeysHandler
{
    private const string KeysSection = "Authentication:Schemes:Bearer:SigningKeys";
    private const string KeysSection_Issuer = "Issuer";
    private const string KeysSection_Value = "Value";
    private const string _outIssuer = "our-app";

    public static IEnumerable<SecurityKey> GetKeys(IConfiguration configuration) => GetKeys(configuration, _outIssuer);

    public static IEnumerable<SecurityKey> GetKeys(IConfiguration configuration, string issuer)
    {
        // configuration.GetSection(KeysSection)會指向密鑰檔secret.json的根節點
        var signingKey = configuration.GetSection(KeysSection)
            .GetChildren()
            .SingleOrDefault(sk => sk[KeysSection_Issuer] == issuer);

        if (signingKey is not null && signingKey[KeysSection_Value] is string secretKey)
        {
            // yeild return: 會讓這個方法變成一個迭代器，這樣就可以在需要的時候才去計算這個值，不用一次性產生整個序列
            yield return new SymmetricSecurityKey(Convert.FromBase64String(secretKey));
        }
    }

    public static IEnumerable<SecurityKey> GetAllKeys(IConfiguration configuration)
    {
        var signingKeys = configuration.GetSection(KeysSection)
            .GetChildren();

        foreach (var signingKey in signingKeys)
        {
            if (signingKey[KeysSection_Value] is string secretKey)
            {
                yield return new SymmetricSecurityKey(Convert.FromBase64String(secretKey));
            }
        }
    }
}
