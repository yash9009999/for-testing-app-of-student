using System.Security.Cryptography;

namespace AuthServer.Services;

public static class TokenKeyStore
{
    public static RSA LoadOrCreate(string path)
    {
        if (File.Exists(path))
        {
            var xml = File.ReadAllText(path);
            var rsa = RSA.Create();
            rsa.FromXmlString(xml);
            return rsa;
        }

        var newRsa = RSA.Create(2048);
        var xmlString = newRsa.ToXmlString(true);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, xmlString);
        return newRsa;
    }
}
