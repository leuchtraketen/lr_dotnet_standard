using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LR.Standard;

public static class EncryptionExtensions
{
    public static readonly string PASSWORD = "YPev*9apY6U_unub5uVeMuVUWE#AGu4u";
    public static readonly string SALT = "Sa5Yda2AVeVajE6Y";

    private static readonly Random _rand = new();

    public static string GetKey(string? password = null, string? salt = null)
    {
        using SHA256 sha256 = SHA256.Create();

        string raw_key = salt + password;
        string key_hashed;
        key_hashed = sha256.ComputeHash(Encoding.ASCII.GetBytes(raw_key)).ToHexString().ToLower().Substring(0, 32);
        return key_hashed;
    }

    public static string Encrypt(this string? value, string? password = null, string? salt = null, Action<string>? log_func = null)
    {
        value ??= string.Empty;

        string key_hashed = GetKey(password?.IfEmpty(null) ?? PASSWORD, salt?.IfEmpty(null) ?? SALT);

        using Aes aes = Aes.Create();

        byte[] iv = new byte [16];
        _rand.NextBytes(iv);

        aes.Key = Encoding.ASCII.GetBytes(key_hashed);
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        ICryptoTransform aes_cryptor = aes.CreateEncryptor();

        using MemoryStream memory_stream = new();
        using CryptoStream crypto_stream = new(memory_stream, aes_cryptor, CryptoStreamMode.Write);

        byte[] decrypted_bytes = Encoding.UTF8.GetBytes(value);
        crypto_stream.Write(decrypted_bytes, 0, decrypted_bytes.Length);
        crypto_stream.FlushFinalBlock();

        byte[] encrypted_bytes = memory_stream.ToArray();
        string ret = iv.ToHexString() + Convert.ToBase64String(encrypted_bytes, 0, encrypted_bytes.Length);

        log_func?.Invoke("iv:              " + iv.Select(c => (int)c).Join(" "));
        log_func?.Invoke("ret (encrypted_bytes): " + encrypted_bytes.Select(c => (int)c).Join(" "));

        return ret;
    }

    public static string? Decrypt(this string? value, string? password = null, string? salt = null, Action<string>? log_func = null)
    {
        value ??= string.Empty;

        if (value.Length <= 32)
        {
            log_func?.Invoke($"Invalid value for DECRYPT: {value}");
            return null;
        }

        string key_hashed = GetKey(password?.IfEmpty(null) ?? PASSWORD, salt?.IfEmpty(null) ?? SALT);

        using Aes aes = Aes.Create();

        byte[] iv = value.Substring(0, 32).FromHexString();
        byte[] encrypted_bytes = Convert.FromBase64String(value.Substring(32));

        log_func?.Invoke("key:             " + Encoding.ASCII.GetBytes(key_hashed).Select(c => (int)c).Join(" "));
        log_func?.Invoke("iv:              " + iv.Select(c => (int)c).Join(" "));
        log_func?.Invoke("encrypted_bytes: " + encrypted_bytes.Select(c => (int)c).Join(" "));

        aes.Key = Encoding.ASCII.GetBytes(key_hashed);
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        ICryptoTransform aes_cryptor = aes.CreateDecryptor();

        using MemoryStream memory_stream = new(encrypted_bytes);
        using CryptoStream crypto_stream = new(memory_stream, aes_cryptor, CryptoStreamMode.Read);

        byte[] decrypted_bytes;
        using (MemoryStream memory_stream2 = new())
        {
            crypto_stream.CopyTo(memory_stream2);
            decrypted_bytes = memory_stream2.ToArray();
        }

        string ret = Encoding.ASCII.GetString(decrypted_bytes);

        log_func?.Invoke("ret (decrypted string): " + ret);
        return ret;
    }
}
