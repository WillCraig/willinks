using System.Security.Cryptography;

namespace Willinks.Api.Services;

// TODO: remove
public static class ShortCodeGenerator
{
    private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int Length = 6;

    public static string Generate()
    {
        var result = new char[Length];
        var bytes = RandomNumberGenerator.GetBytes(Length);
        for (int i = 0; i < Length; i++)
            result[i] = Chars[bytes[i] % Chars.Length];
        return new string(result);
    }
}
