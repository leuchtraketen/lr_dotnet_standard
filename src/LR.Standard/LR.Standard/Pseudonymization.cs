using System;
using System.Collections.Generic;
using System.Text;

namespace LR.Standard;

public static class Pseudonymization
{
    public static string MakeVowelConsonantHash(string value, int length)
    {
        using (System.Security.Cryptography.SHA512 sha512 = System.Security.Cryptography.SHA512.Create())
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
            bytes = sha512.ComputeHash(bytes);
            return MakeVowelConsonantHash(bytes, length);
        }
    }

    public static string MakeVowelConsonantHash(byte[] bytes, int length)
    {
        string result = "";

        char[] vowels = new[] { 'A', 'E', 'I', 'O', 'U', };
        char[] consonants = new[] { 'B', 'C', 'D', 'F', 'H', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'W', 'Z', };

        bool should_be_vowel = false;
        foreach (byte fullbyte in bytes)
        {
            int b1 = ( fullbyte & 0xF0 ) >> 4;
            int b2 = ( fullbyte & 0x0F );

            foreach (int b in new[] { b1, b2 })
            {
                if (should_be_vowel)
                {
                    char c = vowels[( (int)b ) % vowels.Length];
                    result += c;
                }
                else
                {
                    char c = consonants[( (int)b ) % consonants.Length];
                    result += c;
                }

                should_be_vowel = !should_be_vowel;
            }
        }

        return result.Substring(0, length);
    }
}
