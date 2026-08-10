using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Appegy.Storage
{
    internal static class DebugJsonWriter
    {
        private const int IndentWidth = 2;
        private const int MaxDepth = 64;

        private static readonly BindingFlags _memberFlags = BindingFlags.Public | BindingFlags.Instance;

        public static string ToJson(IReadOnlyDictionary<string, Record> data)
        {
            var builder = new StringBuilder();
            var visited = new HashSet<object>(ReferenceComparer.Instance);
            WriteEntries(builder, ProjectRoot(data), 0, visited);
            builder.Append('\n');
            return builder.ToString();
        }

        private static IEnumerable<KeyValuePair<string, object>> ProjectRoot(IReadOnlyDictionary<string, Record> data)
        {
            foreach (var pair in data)
            {
                yield return new KeyValuePair<string, object>(pair.Key, pair.Value.Object);
            }
        }

        private static void WriteValue(StringBuilder builder, object value, int depth, HashSet<object> visited)
        {
            switch (value)
            {
                case null:
                    builder.Append("null");
                    return;
                case bool boolean:
                    builder.Append(boolean ? "true" : "false");
                    return;
                case string text:
                    WriteString(builder, text);
                    return;
                case char character:
                    WriteString(builder, character.ToString());
                    return;
                case Enum enumeration:
                    WriteString(builder, enumeration.ToString());
                    return;
                case float single:
                    WriteFloating(builder, single, float.IsNaN(single) || float.IsInfinity(single), single.ToString("R", CultureInfo.InvariantCulture));
                    return;
                case double real:
                    WriteFloating(builder, real, double.IsNaN(real) || double.IsInfinity(real), real.ToString("R", CultureInfo.InvariantCulture));
                    return;
                case sbyte or byte or short or ushort or int or uint or long or ulong or decimal:
                    builder.Append(((IFormattable)value).ToString(null, CultureInfo.InvariantCulture));
                    return;
                case DateTime dateTime:
                    WriteString(builder, dateTime.ToString("o", CultureInfo.InvariantCulture));
                    return;
                case DateTimeOffset dateTimeOffset:
                    WriteString(builder, dateTimeOffset.ToString("o", CultureInfo.InvariantCulture));
                    return;
                case TimeSpan timeSpan:
                    WriteString(builder, timeSpan.ToString());
                    return;
                case Guid guid:
                    WriteString(builder, guid.ToString());
                    return;
                case Vector2 vector2:
                    WriteEntries(builder, Members(("x", vector2.x), ("y", vector2.y)), depth, visited);
                    return;
                case Vector3 vector3:
                    WriteEntries(builder, Members(("x", vector3.x), ("y", vector3.y), ("z", vector3.z)), depth, visited);
                    return;
                case Vector4 vector4:
                    WriteEntries(builder, Members(("x", vector4.x), ("y", vector4.y), ("z", vector4.z), ("w", vector4.w)), depth, visited);
                    return;
                case Quaternion quaternion:
                    WriteEntries(builder, Members(("x", quaternion.x), ("y", quaternion.y), ("z", quaternion.z), ("w", quaternion.w)), depth, visited);
                    return;
                case Vector2Int vector2Int:
                    WriteEntries(builder, Members(("x", vector2Int.x), ("y", vector2Int.y)), depth, visited);
                    return;
                case Vector3Int vector3Int:
                    WriteEntries(builder, Members(("x", vector3Int.x), ("y", vector3Int.y), ("z", vector3Int.z)), depth, visited);
                    return;
            }

            if (TryGetDictionaryEntries(value, out var entries))
            {
                WriteEntries(builder, entries, depth, visited);
                return;
            }

            if (value is IEnumerable enumerable)
            {
                WriteArray(builder, enumerable, depth, visited);
                return;
            }

            WriteReflected(builder, value, depth, visited);
        }

        private static void WriteEntries(StringBuilder builder, IEnumerable<KeyValuePair<string, object>> entries, int depth, HashSet<object> visited)
        {
            builder.Append('{');
            var first = true;
            foreach (var entry in entries)
            {
                AppendSeparator(builder, ref first);
                Indent(builder, depth + 1);
                WriteString(builder, entry.Key);
                builder.Append(": ");
                WriteValue(builder, entry.Value, depth + 1, visited);
            }
            CloseBlock(builder, depth, first, '}');
        }

        private static void WriteArray(StringBuilder builder, IEnumerable enumerable, int depth, HashSet<object> visited)
        {
            builder.Append('[');
            var first = true;
            foreach (var item in enumerable)
            {
                AppendSeparator(builder, ref first);
                Indent(builder, depth + 1);
                WriteValue(builder, item, depth + 1, visited);
            }
            CloseBlock(builder, depth, first, ']');
        }

        private static void WriteReflected(StringBuilder builder, object value, int depth, HashSet<object> visited)
        {
            var type = value.GetType();
            var isReference = !type.IsValueType;
            if (depth >= MaxDepth || (isReference && !visited.Add(value)))
            {
                WriteString(builder, value.ToString());
                return;
            }
            try
            {
                var members = CollectMembers(type, value);
                if (members.Count == 0)
                {
                    WriteString(builder, value.ToString());
                    return;
                }
                WriteEntries(builder, members, depth, visited);
            }
            finally
            {
                if (isReference)
                {
                    visited.Remove(value);
                }
            }
        }

        private static List<KeyValuePair<string, object>> CollectMembers(Type type, object value)
        {
            var members = new List<KeyValuePair<string, object>>();
            foreach (var field in type.GetFields(_memberFlags))
            {
                if (TryReadMember(() => field.GetValue(value), out var fieldValue))
                {
                    members.Add(new KeyValuePair<string, object>(field.Name, fieldValue));
                }
            }
            foreach (var property in type.GetProperties(_memberFlags))
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                {
                    continue;
                }
                if (TryReadMember(() => property.GetValue(value), out var propertyValue))
                {
                    members.Add(new KeyValuePair<string, object>(property.Name, propertyValue));
                }
            }
            return members;
        }

        private static bool TryReadMember(Func<object> getter, out object result)
        {
            try
            {
                result = getter();
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        private static bool TryGetDictionaryEntries(object value, out IEnumerable<KeyValuePair<string, object>> entries)
        {
            foreach (var contract in value.GetType().GetInterfaces())
            {
                if (!contract.IsGenericType)
                {
                    continue;
                }
                var definition = contract.GetGenericTypeDefinition();
                if (definition == typeof(IReadOnlyDictionary<,>) || definition == typeof(IDictionary<,>))
                {
                    entries = EnumerateDictionary((IEnumerable)value);
                    return true;
                }
            }
            entries = Array.Empty<KeyValuePair<string, object>>();
            return false;
        }

        private static IEnumerable<KeyValuePair<string, object>> EnumerateDictionary(IEnumerable pairs)
        {
            PropertyInfo keyProperty = null;
            PropertyInfo valueProperty = null;
            foreach (var pair in pairs)
            {
                if (pair == null)
                {
                    continue;
                }
                if (keyProperty == null)
                {
                    var pairType = pair.GetType();
                    keyProperty = pairType.GetProperty("Key");
                    valueProperty = pairType.GetProperty("Value");
                }
                var key = keyProperty?.GetValue(pair);
                var entryValue = valueProperty?.GetValue(pair);
                yield return new KeyValuePair<string, object>(key?.ToString() ?? "null", entryValue);
            }
        }

        private static void WriteFloating(StringBuilder builder, object value, bool isSpecial, string formatted)
        {
            if (isSpecial)
            {
                WriteString(builder, value.ToString());
            }
            else
            {
                builder.Append(formatted);
            }
        }

        private static void WriteString(StringBuilder builder, string text)
        {
            if (text == null)
            {
                builder.Append("null");
                return;
            }
            builder.Append('"');
            foreach (var character in text)
            {
                switch (character)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (character < 0x20)
                        {
                            builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            builder.Append(character);
                        }
                        break;
                }
            }
            builder.Append('"');
        }

        private static void AppendSeparator(StringBuilder builder, ref bool first)
        {
            if (!first)
            {
                builder.Append(',');
            }
            first = false;
            builder.Append('\n');
        }

        private static void CloseBlock(StringBuilder builder, int depth, bool empty, char closing)
        {
            if (!empty)
            {
                builder.Append('\n');
                Indent(builder, depth);
            }
            builder.Append(closing);
        }

        private static void Indent(StringBuilder builder, int depth)
        {
            builder.Append(' ', depth * IndentWidth);
        }

        private static KeyValuePair<string, object>[] Members(params (string Key, object Value)[] items)
        {
            var members = new KeyValuePair<string, object>[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                members[i] = new KeyValuePair<string, object>(items[i].Key, items[i].Value);
            }
            return members;
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new();

            public new bool Equals(object left, object right) => ReferenceEquals(left, right);

            public int GetHashCode(object value) => RuntimeHelpers.GetHashCode(value);
        }
    }
}
