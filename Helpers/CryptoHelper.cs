using System.Security.Cryptography;
using System.Text;

public static class CryptoHelper
{
    private static byte[] NormalizeKey(string key)
    {
        // Derive 32 bytes key from any length string using SHA256
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(key));
    }

    public static string Encrypt(string plainText, string key)
    {
        using var aes = Aes.Create();
        aes.Key = NormalizeKey(key);     // ✅ always 32 bytes
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(aes.IV) + ":" + Convert.ToBase64String(cipherBytes);
    }

    public static string Decrypt(string cipherText, string key)
    {
        var parts = cipherText.Split(':');
        var iv = Convert.FromBase64String(parts[0]);
        var cipherBytes = Convert.FromBase64String(parts[1]);

        using var aes = Aes.Create();
        aes.Key = NormalizeKey(key);     // ✅ always 32 bytes
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}