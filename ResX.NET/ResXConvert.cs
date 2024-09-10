﻿using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ResX.NET
{
    /// <summary>
    /// Converts ResX files to other formats.
    /// </summary>
    public static class ResXConvert
    {
        private static readonly JsonSerializerOptions s_indentedWriter = new() { WriteIndented = true };
        private static readonly JsonWriterOptions s_unsafeEncoder = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        private static readonly JsonWriterOptions s_defaultEncoder = new() { Encoder = JavaScriptEncoder.Default };

        /// <summary>
        /// Converts the ResX file to JSON.
        /// </summary>
        /// <param name="resx">Input ResX file.</param>
        /// <param name="unsafeEncoder">Set to true if ResX file may contain characters other than English.</param>
        /// <param name="prettify">Set to true if you don't want the output JSON file to be minified.</param>
        /// <returns>JSON representation of the ResX file.</returns>
        public static string ToJson(ResXFile resx, bool unsafeEncoder = true, bool prettify = false)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, unsafeEncoder ? s_unsafeEncoder : s_defaultEncoder);

            writer.WriteStartArray();

            foreach (var data in resx.DataList)
            {
                writer.WriteStartObject();

                writer.WriteString("name", data.Name);
                writer.WriteString("value", data.Value);
                writer.WriteString("comment", data.Comment);

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.Flush();
            return JsonPrettify(Encoding.UTF8.GetString(stream.ToArray()), prettify);
        }

        /// <summary>
        /// Converts the ResX file to JSON, asynchronously.
        /// </summary>
        /// <param name="resx">Input ResX file.</param>
        /// <param name="unsafeEncoder">Set to true if ResX file may contain characters other than English.</param>
        /// <param name="prettify">Set to true if you don't want the output JSON file to be minified.</param>
        /// <returns>JSON representation of the ResX file.</returns>
        public static async Task<string> ToJsonAsync(ResXFile resx, bool unsafeEncoder = false, bool prettify = false)
        {
            await using var stream = new MemoryStream();
            await using var writer = new Utf8JsonWriter(stream, unsafeEncoder ? s_unsafeEncoder : s_defaultEncoder);

            writer.WriteStartArray();

            foreach (var data in resx.DataList)
            {
                writer.WriteStartObject();

                writer.WriteString("name", data.Name);
                writer.WriteString("value", data.Value);
                writer.WriteString("comment", data.Comment);

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            await writer.FlushAsync();
            return JsonPrettify(Encoding.UTF8.GetString(stream.ToArray()), prettify);
        }

        /// <summary>
        /// Converts the ResX file to Markdown.
        /// </summary>
        /// <param name="resx">Input ResX file.</param>
        /// <returns>A Markdown representation of the ResX document.</returns>
        public static string ToMarkdown(ResXFile resx)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Converted result from a ResX file");
            builder.AppendLine("*Auto-generated by ResXNET v1.0.0*");

            foreach (var data in resx.DataList)
            {
                builder.AppendLine($"### {data.Name}");
                builder.AppendLine($"*Value*: **{data.Value}**; *comment*: **{data.Comment ?? "null"}**");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Converts the ResX file to C#.
        /// </summary>
        /// <param name="resx">Input ResX file to process.</param>
        /// <returns>C# representation of all data fields of the ResX file.</returns>
        public static string ToCSharp(ResXFile resx)
        {
            var builder = new StringBuilder();
            builder.AppendLine("internal class Class1");
            builder.AppendLine("{");
            foreach (var data in resx.DataList)
            {
                if (data.Comment != null)
                {
                    builder.AppendLine("    /// <summary>");
                    builder.AppendLine($"   /// {data.Comment}");
                    builder.AppendLine("    /// </summary>");
                }
                string name = new(Capitalize(data.Name)
                    .Where(ch => (!char.IsWhiteSpace(ch) && !char.IsPunctuation(ch) && !char.IsControl(ch)) || ch == '_').ToArray());

                builder.AppendLine($"    public string {Capitalize(name)} = \"{data.Value}\";");
            }
            return builder.ToString();
        }

        // source:
        // https://stackoverflow.com/a/4405876/21072788
        // CC BY-SA 4.0
        private static string Capitalize(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };

        // source: https://stackoverflow.com/a/67928315/21072788
        // CC BY-SA 4.0
        private static string JsonPrettify(this string json, bool reallyPrettify = true)
        {
            if (!reallyPrettify)
            {
                return json;
            }

            using var jDoc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(jDoc, s_indentedWriter);
        }
    }
}
