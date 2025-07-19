using System.Globalization;
using System.Text;
using UnityEngine;

internal class PlayerPrefsProtected
{
	internal static bool AutoSave;

	internal static void DeleteAll()
	{
		PlayerPrefs.DeleteAll();
	}

	internal static void DeleteKey(string key)
	{
		PlayerPrefs.DeleteKey(GetStringHashAsHex(key));
	}

	internal static bool HasKey(string key)
	{
		return PlayerPrefs.HasKey(GetStringHashAsHex(key));
	}

	internal static void Save()
	{
		PlayerPrefs.Save();
	}

	internal static decimal GetDecimal(string key, decimal defaultValue = 0m)
	{
		string text = GetString(key);
		if (string.IsNullOrEmpty(text))
		{
			return defaultValue;
		}
		decimal result;
		if (decimal.TryParse(text, out result))
		{
			return result;
		}
		return defaultValue;
	}

	internal static void SetDecimal(string key, decimal value)
	{
		SetString(key, value.ToString());
	}

	internal static float GetFloat(string key, float defaultValue = 0f)
	{
		string text = GetString(key);
		if (string.IsNullOrEmpty(text))
		{
			return defaultValue;
		}
		float result;
		if (float.TryParse(text, out result))
		{
			return result;
		}
		return defaultValue;
	}

	internal static void SetFloat(string key, float value)
	{
		SetString(key, value.ToString());
	}

	internal static int GetInt(string key, int defaultValue = 0)
	{
		string text = GetString(key);
		if (string.IsNullOrEmpty(text))
		{
			return defaultValue;
		}
		int result;
		if (int.TryParse(text, out result))
		{
			return result;
		}
		return defaultValue;
	}

	internal static void SetInt(string key, int value)
	{
		SetString(key, value.ToString());
	}

	internal static long GetLong(string key, long defaultValue = 0L)
	{
		string text = GetString(key);
		if (string.IsNullOrEmpty(text))
		{
			return defaultValue;
		}
		long result;
		if (long.TryParse(text, out result))
		{
			return result;
		}
		return defaultValue;
	}

	internal static void SetLong(string key, long value)
	{
		SetString(key, value.ToString());
	}

	internal static bool GetBool(string key, bool defaultValue = false)
	{
		string text = GetString(key);
		if (string.IsNullOrEmpty(text))
		{
			return defaultValue;
		}
		int result;
		if (int.TryParse(text, out result))
		{
			return result == 1;
		}
		return defaultValue;
	}

	internal static void SetBool(string key, bool value)
	{
		SetString(key, (value ? 1 : 0).ToString());
	}

	internal static Vector3 GetVector3(string key, Vector3 defaultValue)
	{
		string text = GetString(key);
		if (string.IsNullOrEmpty(text))
		{
			return defaultValue;
		}
		string[] array = text.Split(';');
		if (array.Length != 3)
		{
			return defaultValue;
		}
		float result;
		if (float.TryParse(array[0], out result))
		{
			return defaultValue;
		}
		float result2;
		if (float.TryParse(array[1], out result2))
		{
			return defaultValue;
		}
		float result3;
		if (float.TryParse(array[2], out result3))
		{
			return defaultValue;
		}
		return new Vector3(result, result2, result3);
	}

	internal static void SetVector3(string key, Vector3 value)
	{
		SetString(key, value.x + ";" + value.y + ";" + value.z);
	}

	internal static byte[] GetBuffer(string key)
	{
		return GetBuffer(key, new byte[0]);
	}

	internal static byte[] GetBuffer(string key, byte[] defaultValue)
	{
		string hexString = GetString(key, BytesConvertToHexString(defaultValue));
		return HexStringToBytes(hexString);
	}

	internal static void SetBuffer(string key, byte[] value)
	{
		SetString(key, BytesConvertToHexString(value));
	}

	internal static string GetString(string key)
	{
		return GetString(key, string.Empty);
	}

	internal static string GetString(string key, string defaultValue)
	{
		string text = DecryptString(PlayerPrefs.GetString(GetStringHashAsHex(key), defaultValue), key);
		if (text == null)
		{
			return defaultValue;
		}
		return text;
	}

	internal static void SetString(string key, string value)
	{
		PlayerPrefs.SetString(GetStringHashAsHex(key), EncryptString(value, key));
		if (AutoSave)
		{
			Save();
		}
	}

	private static string GetStringHashAsHex(string s)
	{
		return string.Format("{0:X}", GetStringHash(s));
	}

	private static uint GetStringHash(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return 123u;
		}
		int num = 0;
		int num2 = 352654597;
		int num3 = num2;
		for (int num4 = s.Length; num4 > 0; num4 -= 4)
		{
			num2 = ((num + 1 < s.Length) ? (((num2 << 5) + num2 + (num2 >> 27)) ^ (int)(s[num] | ((uint)s[num + 1] << 16))) : ((num >= s.Length) ? (((num2 << 5) + num2 + (num2 >> 27)) ^ 0) : (((num2 << 5) + num2 + (num2 >> 27)) ^ s[num])));
			if (num4 <= 2)
			{
				break;
			}
			num += 2;
			num3 = ((num + 1 >= s.Length) ? ((num >= s.Length) ? (((num3 << 5) + num3 + (num3 >> 27)) ^ 0) : (((num3 << 5) + num3 + (num3 >> 27)) ^ s[num])) : (((num3 << 5) + num3 + (num3 >> 27)) ^ (int)(s[num] | ((uint)s[num + 1] << 16))));
			num += 2;
		}
		return (uint)(num2 + num3 * 1566083941);
	}

	private static string EncryptString(string str, string key)
	{
		if (str == null || str.Length == 0)
		{
			return str;
		}
		char[] array = str.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (char)(array[i] + (ushort)(5725 + i) + ((key != null && key.Length != 0) ? key[i % key.Length] : '\0'));
		}
		return new string(array);
	}

	private static string DecryptString(string str, string key)
	{
		if (str == null || str.Length == 0)
		{
			return str;
		}
		char[] array = str.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (char)(array[i] - (ushort)(5725 + i) - ((key != null && key.Length != 0) ? key[i % key.Length] : '\0'));
		}
		return new string(array);
	}

	internal static string BytesConvertToHexString(byte[] buff)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte b in buff)
		{
			stringBuilder.Append(string.Format("{0:X2} ", b));
		}
		return stringBuilder.ToString();
	}

	internal static byte[] HexStringToBytes(string hexString)
	{
		if (hexString == null)
		{
			return null;
		}
		if (hexString.Contains("-"))
		{
			hexString = hexString.Replace("-", string.Empty).Trim();
		}
		if (hexString.Contains(" "))
		{
			hexString = hexString.Replace(" ", string.Empty).Trim();
		}
		if (hexString.Contains("\t"))
		{
			hexString = hexString.Replace("\t", string.Empty).Trim();
		}
		if (hexString.Contains("\r"))
		{
			hexString = hexString.Replace("\r", string.Empty).Trim();
		}
		if (hexString.Contains("\n"))
		{
			hexString = hexString.Replace("\n", string.Empty).Trim();
		}
		if ((hexString.Length & 1) != 0)
		{
			hexString += "0";
		}
		byte[] array = new byte[hexString.Length / 2];
		for (int i = 0; i < hexString.Length; i += 2)
		{
			byte result;
			if (!byte.TryParse(hexString.Substring(i, 2), NumberStyles.HexNumber, null, out result))
			{
				return array;
			}
			array[i / 2] = result;
		}
		return array;
	}
}
