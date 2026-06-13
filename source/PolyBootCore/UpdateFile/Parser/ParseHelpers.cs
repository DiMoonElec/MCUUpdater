using PolyBootCore.PolyBootProtocol.Bootloader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PolyBootCore.UpdateFile.Parser
{
  internal static class ParseHelpers
  {
    // ---------------------------------------------------------------
    // SHA256
    // ---------------------------------------------------------------

    internal static byte[] ComputeSha256(string[] lines, int start, int count)
    {
      using (var sha256 = SHA256.Create())
      {
        for (int i = start; i < start + count; i++)
        {
          byte[] bytes = Encoding.ASCII.GetBytes(lines[i]);
          sha256.TransformBlock(bytes, 0, bytes.Length, null, 0);
        }
        sha256.TransformFinalBlock(new byte[0], 0, 0);
        return sha256.Hash;
      }
    }

    internal static bool HashesEqual(byte[] a, byte[] b)
    {
      if (a == null || b == null || a.Length != b.Length)
        return false;

      int diff = 0;
      for (int i = 0; i < a.Length; i++)
        diff |= a[i] ^ b[i];

      return diff == 0;
    }

    internal static byte[] ParseHex(string hex)
    {
      if (hex.Length % 2 != 0)
        throw new InvalidDataException("Invalid HEX SHA256 length");

      byte[] result = new byte[hex.Length / 2];
      for (int i = 0; i < result.Length; i++)
        result[i] = byte.Parse(
          hex.Substring(i * 2, 2),
          NumberStyles.HexNumber,
          CultureInfo.InvariantCulture);

      return result;
    }

    // ---------------------------------------------------------------
    // Key=Value парсер
    // Поддерживает: KEY=VALUE и KEY="VALUE WITH SPACES"
    // Разделитель — точка с запятой
    // ---------------------------------------------------------------

    internal static Dictionary<string, string> ParseKeyValue(string line)
    {
      var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

      // Убираем ведущий # если есть
      if (line.StartsWith("#", StringComparison.Ordinal))
        line = line.Substring(1);

      int pos = 0;
      while (pos < line.Length)
      {
        // Ищем '='
        int eq = line.IndexOf('=', pos);
        if (eq < 0)
          break;

        string key = line.Substring(pos, eq - pos).Trim();
        pos = eq + 1;

        string value;
        if (pos < line.Length && line[pos] == '"')
        {
          // Значение в кавычках — ищем закрывающую
          pos++; // пропускаем открывающую "
          int closeQuote = line.IndexOf('"', pos);
          if (closeQuote < 0)
            throw new InvalidDataException(
              string.Format("Unclosed quote for key '{0}'", key));

          value = line.Substring(pos, closeQuote - pos);
          pos = closeQuote + 1;

          // Пропускаем разделитель после закрывающей кавычки
          if (pos < line.Length && line[pos] == ';')
            pos++;
        }
        else
        {
          // Значение без кавычек — до ';' или конца строки
          int semi = line.IndexOf(';', pos);
          if (semi < 0)
          {
            value = line.Substring(pos).Trim();
            pos = line.Length;
          }
          else
          {
            value = line.Substring(pos, semi - pos).Trim();
            pos = semi + 1;
          }
        }

        if (!string.IsNullOrEmpty(key))
          result[key] = value;
      }

      return result;
    }

    // ---------------------------------------------------------------
    // Версия x.y.z
    // ---------------------------------------------------------------

    internal static FirmwareVersion ParseVersion(string value, string context)
    {
      string[] parts = value.Split('.');
      if (parts.Length != 3)
        throw new InvalidDataException(
          string.Format("Invalid version format '{0}' in {1}", value, context));

      try
      {
        var major = ushort.Parse(parts[0], CultureInfo.InvariantCulture);
        var minor = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
        var patch = ushort.Parse(parts[2], CultureInfo.InvariantCulture);
        return new FirmwareVersion(major, minor, patch);
      }
      catch (FormatException)
      {
        throw new InvalidDataException(
          string.Format("Invalid version format '{0}' in {1}", value, context));
      }
      catch (OverflowException)
      {
        throw new InvalidDataException(
            string.Format("Version value out of range '{0}' in {1}", value, context));
      }
    }

    internal static int ParseInt(string value, string context)
    {
      int result;
      if (!int.TryParse(value, NumberStyles.Integer,
            CultureInfo.InvariantCulture, out result))
        throw new InvalidDataException(
          string.Format("Invalid integer '{0}' in {1}", value, context));
      return result;
    }

    internal static string RequireKey(Dictionary<string, string> kv,
                                      string key, string context)
    {
      string value;
      if (!kv.TryGetValue(key, out value) || string.IsNullOrEmpty(value))
        throw new InvalidDataException(
          string.Format("Missing required field '{0}' in {1}", key, context));
      return value;
    }
  }
}