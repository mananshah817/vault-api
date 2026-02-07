using System.Security.Cryptography;
using System.Text;

public static class CryptoHelper
{
    private static byte[] NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new Exception("Crypto:Key is missing in environment variables");

        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(key)); // always 32 bytes
    }

    public static string Encrypt(string plainText, string key)
    {
        using var aes = Aes.Create();
        aes.Key = NormalizeKey(key);
        aes.GenerateIV();

        using var enc = aes.CreateEncryptor();
        var plain = Encoding.UTF8.GetBytes(plainText);
        var cipher = enc.TransformFinalBlock(plain, 0, plain.Length);

        return Convert.ToBase64String(aes.IV) + ":" + Convert.ToBase64String(cipher);
    }

    public static string Decrypt(string cipherText, string key)
    {
        var parts = cipherText.Split(':');
        var iv = Convert.FromBase64String(parts[0]);
        var data = Convert.FromBase64String(parts[1]);

        using var aes = Aes.Create();
        aes.Key = NormalizeKey(key);
        aes.IV = iv;

        using var dec = aes.CreateDecryptor();
        var plain = dec.TransformFinalBlock(data, 0, data.Length);

        return Encoding.UTF8.GetString(plain);
    }
}
