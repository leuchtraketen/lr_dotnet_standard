using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LR.Standard;

public static partial class StringExtensions
{
    private static readonly Random _rand = new();
    private static readonly IReadOnlyList<char> _vovels = new[] { 'a', 'e', 'i', 'o', 'u', 'y', };

    public static string MakeRandomText(int count_words_min = 5, int count_words_max = 25, int count_characters_min = 3, int count_characters_max = 12)
    {
        bool last_character_was_vovel = false;

        char choose_next_character()
        {
            int attempts_left = 100;
            char c;
            do
            {
                c = (char)( 'a' + ( _rand.Next() % ( 'z' - 'a' ) ) );
                if (_vovels.Contains(c) != last_character_was_vovel)
                    break;
            } while (attempts_left-- >= 0);

            last_character_was_vovel = _vovels.Contains(c);

            return c;
        }

        StringBuilder sb = new();
        bool is_beginning_of_sentence = true;
        int count_words = _rand.Next() % ( count_words_max - count_words_min ) + count_words_min;
        for (int word_index = 0; word_index < count_words; word_index++)
        {
            if (word_index != 0)
            {
                if (_rand.Next() % 100 > 95)
                {
                    sb.Append(".");
                    is_beginning_of_sentence = true;
                }

                sb.Append(' ');
            }

            int count_characters = _rand.Next() % ( count_characters_max - count_characters_min ) + count_characters_min;
            for (int character_index = 0; character_index < count_characters; character_index++)
            {
                bool is_uppercase = is_beginning_of_sentence && ( word_index == 0 || ( _rand.Next() % 100 > 90 ) );
                char c = choose_next_character();
                if (is_uppercase)
                    c = char.ToUpper(c);
                sb.Append(c);
                is_beginning_of_sentence = false;
            }
        }

        sb.Append(".");
        return sb.ToString();
    }

    public static string MakeRandomUuid()
    {
        return Guid.NewGuid().ToString("D");
    }

    private static SHA256? _sha256;

    public static string MakeUuidFromString(string input)
    {
        _sha256 ??= SHA256.Create();
        byte[] hash = _sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash.AsSpan()[..16]).ToString("D");
    }

    public static string RemoveDiacritics(string s)
    {
        string normalizedString = s.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();

        foreach (char c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString();
    }

    private static IFormatProvider inv
        = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

    public static string? ToStringInvariant<T>(this T obj, string? format = null)
    {
        return ( format is null )
            ? FormattableString.Invariant($"{obj}")
            : string.Format(inv, $"{{0:{format}}}", obj);
    }

    public static string Base64Encode(this string str)
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
    }

    public static string Base64Decode(this string str)
    {
        byte[] buffer = Convert.FromBase64String(str);
        return System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
    }

    public static string ToStringUTF8(this byte[] byteArray)
    {
        return System.Text.Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
    }

    public static byte[] ToByteArray(this string str)
    {
        return System.Text.Encoding.UTF8.GetBytes(str);
    }

    public static string Truncate(this string value, int maxLength)
    {
        if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
        {
            return value.Substring(0, maxLength);
        }

        return value;
    }

    public static string FormatSortable(this DateTime date)
    {
        return date.ToString("yyyy-MM-ddTHH:mm:ss.fff");
    }

    public static string UppercaseFirst(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return string.Empty;
        }

        char[] a = str.ToCharArray();
        a[0] = char.ToUpper(a[0]);
        return new string(a);
    }

    public static int CountOccurrences(this string str, string needle)
    {
        if (str is null) return 0;
        if (string.IsNullOrEmpty(needle)) throw new ArgumentNullException(nameof(needle));

        int count = 0;
        int index = 0;

        while (( index = str.IndexOf(needle, index, StringComparison.InvariantCulture) ) != -1)
        {
            index += needle.Length;
            ++count;
        }

        return count;
    }

    public static int CountOccurrences(this string str, char needle)
    {
        if (str is null) return 0;

        int count = 0;
        int index = 0;

        while (( index = str.IndexOf(needle, index) ) != -1)
        {
            index += 1;
            ++count;
        }

        return count;
    }

    private static JsonSerializerOptions _createJsonSerializeOptions(JsonSerializerOptions? options_input, bool inline, bool ignore_null, bool preserve_object_references)
    {
        JsonSerializerOptions ret = options_input ?? new();
        ret.ReferenceHandler = preserve_object_references ? ReferenceHandler.Preserve : null;
        ret.WriteIndented = !inline;
        ret.DefaultIgnoreCondition = ignore_null ? JsonIgnoreCondition.WhenWritingDefault : JsonIgnoreCondition.Never;
        return ret;
    }

    public static string ToJson(this object obj,
        bool inline = false,
        bool ignore_null = true,
        bool preserve_object_references = false,
        JsonSerializerOptions? options = null,
        OptionalParameterList _ = default,
        [CallerMemberName] string? member_name = null,
        [CallerFilePath] string? file_path = null,
        [CallerLineNumber] int line_number = 0)
    {
        options = _createJsonSerializeOptions(options_input: options, inline: inline, ignore_null: ignore_null, preserve_object_references: preserve_object_references);
        return JsonSerializer.Serialize(obj, options) ?? string.Empty;
    }

    public static async ValueTask ToJsonAsync(this object obj,
        Stream stream,
        bool inline = false,
        bool ignore_null = true,
        bool preserve_object_references = false,
        JsonSerializerOptions? options = null,
        OptionalParameterList _ = default,
        [CallerMemberName] string? member_name = null,
        [CallerFilePath] string? file_path = null,
        [CallerLineNumber] int line_number = 0)
    {
        options = _createJsonSerializeOptions(options_input: options, inline: inline, ignore_null: ignore_null, preserve_object_references: preserve_object_references);
        await JsonSerializer.SerializeAsync(stream, obj, options);
    }

    public static T FromJson<T>(this string json,
        bool inline = false,
        bool ignore_null = true,
        bool preserve_object_references = false,
        JsonSerializerOptions? options = null,
        OptionalParameterList _ = default,
        [CallerMemberName] string? member_name = null,
        [CallerFilePath] string? file_path = null,
        [CallerLineNumber] int line_number = 0)
    {
        options = _createJsonSerializeOptions(options_input: options, inline: inline, ignore_null: ignore_null, preserve_object_references: preserve_object_references);
        return JsonSerializer.Deserialize<T>(json, options);
    }

    public static async ValueTask<T> FromJsonAsync<T>(this string json,
        Stream stream,
        bool inline = false,
        bool ignore_null = true,
        bool preserve_object_references = false,
        JsonSerializerOptions? options = null,
        OptionalParameterList _ = default,
        [CallerMemberName] string? member_name = null,
        [CallerFilePath] string? file_path = null,
        [CallerLineNumber] int line_number = 0)
    {
        options = _createJsonSerializeOptions(options_input: options, inline: inline, ignore_null: ignore_null, preserve_object_references: preserve_object_references);
        return await JsonSerializer.DeserializeAsync<T>(stream, options);
    }

    public static string Between(this string source, string left, string right)
    {
        return Regex.Match(source, $"{left}(.*){right}").Groups[1].Value;
    }

    public static bool ContainsAny(this string self, params string[] choices)
    {
        if (self == null)
            return false;
        foreach (string choice in choices)
        {
            if (!string.IsNullOrWhiteSpace(choice) && self.Contains(choice))
            {
                return true;
            }
        }

        return false;
    }

    public static bool ContainsAll(this string self, params string[] choices)
    {
        if (self == null)
            return false;
        foreach (string choice in choices)
        {
            if (!string.IsNullOrWhiteSpace(choice) && !self.Contains(choice))
            {
                return false;
            }
        }

        return true;
    }

    public static bool StartsWithAny(this string self, params string[] choices)
    {
        if (self == null)
            return false;
        foreach (string choice in choices)
        {
            if (!string.IsNullOrWhiteSpace(choice) && self.StartsWith(choice))
            {
                return true;
            }
        }

        return false;
    }

    public static bool EndsWithAny(this string self, params string[] choices)
    {
        if (self == null)
            return false;
        foreach (string choice in choices)
        {
            if (!string.IsNullOrWhiteSpace(choice) && self.EndsWith(choice))
            {
                return true;
            }
        }

        return false;
    }

    public static string Multiply(this string source, int multiplier)
    {
        StringBuilder sb = new(multiplier * source.Length);
        for (int i = 0; i < multiplier; i++)
        {
            sb.Append(source);
        }

        return sb.ToString();
    }

    public static string ReplaceStart(this string source, string search, string replacement)
    {
        if (source.StartsWith(search))
        {
            return replacement + source.Substring(search.Length);
        }
        else
        {
            return source;
        }
    }

    public static string ReplaceEnd(this string source, string search, string replacement)
    {
        if (source.EndsWith(search))
        {
            return source.Substring(0, source.Length - search.Length) + replacement;
        }
        else
        {
            return source;
        }
    }

    private static readonly Regex _rgxNumericOnly = new("[^0-9]");

    public static string ToNumericOnly(this string input)
    {
        return _rgxNumericOnly.Replace(input, "");
    }


    private static HashSet<char> _allowedChars = new(new[]
    {
        '.', ':', ',', ';', ' ', '/', '\\', '_', '-', '+', '*', '(', ')', '{', '}', '[', ']', '#', '?', '\'', '"', '!', '^', '=', '%', '@', '$',
    });

    public static string ToUnicodeLettersAndAscii(string s)
    {
        return new string(s.ToCharArray().Where(c => char.IsLetterOrDigit(c) || _allowedChars.Contains(c)).ToArray());
    }

    public static bool EqualsWithIgnoreCase(this string left, string right)
    {
        return left.Equals(right, StringComparison.InvariantCultureIgnoreCase);
    }

    public static void FastSplit(this ReadOnlySpan<byte> bytes, Action<long> callback)
    {
        if (bytes == default)
            return;

        long y = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            byte b = bytes[i];
            if (b == ',')
            {
                callback(y);
                y = 0;
            }
            else
            {
                y = y * 10 + ( b - '0' );
            }
        }

        if (y != 0)
        {
            callback(y);
        }
    }

    public static void FastSplit(this string bytes, Action<long> callback)
    {
        if (bytes is null)
            return;

        long y = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            char b = bytes[i];
            if (b == ',')
            {
                callback(y);
                y = 0;
            }
            else
            {
                y = y * 10 + ( b - '0' );
            }
        }

        if (y != 0)
        {
            callback(y);
        }
    }

    public static long FastConvertToLong(string s)
    {
        long y = 0;
        for (int i = 0; i < s.Length; i++)
            y = y * 10 + ( s[i] - '0' );
        return y;
    }

    public static void FastSplit(this string bytes, Action<string> callback)
    {
        if (bytes is null)
            return;

        foreach (ReadOnlySpan<char> o in bytes.AsSpan().Split(','))
        {
            if (o.Length != 0)
            {
#if NET471
                callback (new string (o.ToArray ()));
#else
                callback(new string(o));
#endif
            }
        }
    }


    private static readonly Regex _rgxAlphaNumericOnlyWithAll = new("[^a-zA-Z0-9_-]");
    private static readonly Regex _rgxAlphaNumericOnlyWithMinus = new("[^a-zA-Z0-9-]");
    private static readonly Regex _rgxAlphaNumericOnlyWithUnderscore = new("[^a-zA-Z0-9-]");
    private static readonly Regex _rgxAlphaNumericOnlyWithNone = new("[^a-zA-Z0-9]");
    private static readonly Regex _rgxNumericOnlyWithAll = new("[^0-9_-]");
    private static readonly Regex _rgxNumericOnlyWithMinus = new("[^0-9-]");
    private static readonly Regex _rgxNumericOnlyWithUnderscore = new("[^0-9-]");
    private static readonly Regex _rgxNumericOnlyWithNone = new("[^0-9]");

    [Flags]
    public enum AlphaNumericFlags
    {
        None = 0,
        WithUnderscore = 1 << 0,
        WithMinus = 1 << 1,

        WithAll = WithUnderscore | WithMinus,
    }

    public static string ToAlphaNumericOnly(this string input, AlphaNumericFlags flags = AlphaNumericFlags.WithUnderscore | AlphaNumericFlags.WithMinus)
    {
        if (( flags & ( AlphaNumericFlags.WithMinus | AlphaNumericFlags.WithUnderscore ) ) != 0)
        {
            return _rgxAlphaNumericOnlyWithAll.Replace(input, "");
        }
        else if (( flags & ( AlphaNumericFlags.WithMinus ) ) != 0)
        {
            return _rgxAlphaNumericOnlyWithMinus.Replace(input, "");
        }
        else if (( flags & ( AlphaNumericFlags.WithUnderscore ) ) != 0)
        {
            return _rgxAlphaNumericOnlyWithUnderscore.Replace(input, "");
        }
        else
        {
            return _rgxAlphaNumericOnlyWithNone.Replace(input, "");
        }
    }

    public static string ToNumericOnly(this string input, AlphaNumericFlags flags = AlphaNumericFlags.WithMinus)
    {
        if (( flags & ( AlphaNumericFlags.WithMinus | AlphaNumericFlags.WithUnderscore ) ) != 0)
        {
            return _rgxNumericOnlyWithAll.Replace(input, "");
        }
        else if (( flags & ( AlphaNumericFlags.WithMinus ) ) != 0)
        {
            return _rgxNumericOnlyWithMinus.Replace(input, "");
        }
        else if (( flags & ( AlphaNumericFlags.WithUnderscore ) ) != 0)
        {
            return _rgxNumericOnlyWithUnderscore.Replace(input, "");
        }
        else
        {
            return _rgxNumericOnlyWithNone.Replace(input, "");
        }
    }

    public static int ParseStringOrGetHashcode(string str)
    {
        if (str == null) str = string.Empty;

        if (int.TryParse(str, out int i))
        {
            return i;
        }

        return GetHashCode(str);
    }

    public static int GetHashCode(string str)
    {
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ( ( hash1 << 5 ) + hash1 ) ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = ( ( hash2 << 5 ) + hash2 ) ^ str[i + 1];
            }

            return hash1 + ( hash2 * 1566083941 );
        }
    }

    public static string LooseDecodeUtf8(this ReadOnlySpan<byte> bytes)
    {
        try
        {
            return Encoding.UTF8.GetString(bytes);
        }
        catch (Exception)
        {
            // the byte array is not a valid UTF-8 string
            StringBuilder sb = new();
            for (int i = 0; i < bytes.Length; i++)
            {
                byte b = bytes[i];
                if (b >= 32 && b <= 126)
                {
                    sb.Append((char)b);
                }
                else
                {
                    sb.Append('[');
                    sb.Append((int)b);
                    sb.Append(']');
                }
            }

            return sb.ToString();
        }
    }

    public static string LooseDecodePrintableCharacters(this ReadOnlySpan<byte> bytes)
    {
        StringBuilder sb = new();
        for (int i = 0; i < bytes.Length; i++)
        {
            byte b = bytes[i];
            if (b >= 32 && b <= 126)
            {
                sb.Append((char)b);
            }
            else
            {
                sb.Append('[');
                sb.Append((int)b);
                sb.Append(']');
            }
        }

        return sb.ToString();
    }
}
