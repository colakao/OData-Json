using System;
using System.Collections.Generic;
using System.Text;

public static class MiniJsonParser
{
	public static object Parse(string json)
	{
		return JsonParser(json);
	}

	private static object JsonParser(string json)
	{
		int index = 0;
		return ParseValue(json, ref index);
	}

	private static object ParseValue(string json, ref int index)
	{
		SkipWhitespace(json, ref index);

		if (index >= json.Length) return null;

		char current = json[index];

		if (current == '{') return ParseObject(json, ref index);
		if (current == '[') return ParseArray(json, ref index);
		if (current == '"') return ParseString(json, ref index);
		if (char.IsDigit(current) || current == '-') return ParseNumber(json, ref index);

		if (json.Substring(index).StartsWith("true"))
		{
			index += 4;
			return true;
		}
		if (json.Substring(index).StartsWith("false"))
		{
			index += 5;
			return false;
		}
		if (json.Substring(index).StartsWith("null"))
		{
			index += 4;
			return null;
		}

		throw new FormatException("Invalid JSON format");
	}

	private static Dictionary<string, object> ParseObject(string json, ref int index)
	{
		var dict = new Dictionary<string, object>();
		index++; // Skip '{'

		while (true)
		{
			SkipWhitespace(json, ref index);

			if (index >= json.Length)
			{
				throw new FormatException("Unexpected end of JSON string while parsing object");
			}

			if (json[index] == '}') { index++; break; }

			string key = ParseString(json, ref index);
			SkipWhitespace(json, ref index);

			if (json[index] != ':')
			{
				throw new FormatException($"Expected ':', but found '{json[index]}' at index {index}");
			}
			index++;

			object value = ParseValue(json, ref index);
			dict[key] = value;

			SkipWhitespace(json, ref index);

			if (json[index] == '}') { index++; break; }
			if (json[index] != ',')
			{
				throw new FormatException($"Expected ',', but found '{json[index]}' at index {index}");
			}
			index++;
		}

		return dict;
	}


	private static List<object> ParseArray(string json, ref int index)
	{
		var list = new List<object>();

		index++; // Skip '['
		while (true)
		{
			SkipWhitespace(json, ref index);

			if (json[index] == ']') { index++; break; }

			object value = ParseValue(json, ref index);
			list.Add(value);

			SkipWhitespace(json, ref index);
			if (json[index] == ']') { index++; break; }
			if (json[index] != ',') throw new FormatException("Expected ','");
			index++;
		}

		return list;
	}

	private static string ParseString(string json, ref int index)
	{
		StringBuilder result = new StringBuilder();
		index++; // Skip the opening quote

		while (index < json.Length)
		{
			char currentChar = json[index];

			if (currentChar == '"')  // End of the string
			{
				index++; // Skip the closing quote
				break;
			}

			if (currentChar == '\\') // Handle escape sequences (e.g., \" or \\)
			{
				index++; // Skip the backslash
				if (index < json.Length)
				{
					char escapedChar = json[index];
					if (escapedChar == '"' || escapedChar == '\\')
					{
						result.Append(escapedChar);
					}
					else
					{
						throw new FormatException($"Unexpected escape sequence: \\{escapedChar}");
					}
				}
			}
			else
			{
				result.Append(currentChar);
			}

			index++;
		}

		return result.ToString();
	}

	private static double ParseNumber(string json, ref int index)
	{
		int startIndex = index;
		while (index < json.Length && (char.IsDigit(json[index]) || json[index] == '.' || json[index] == '-')) index++;
		return double.Parse(json.Substring(startIndex, index - startIndex));
	}

	private static void SkipWhitespace(string json, ref int index)
	{
		while (index < json.Length && char.IsWhiteSpace(json[index])) index++;
	}
}
