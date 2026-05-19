using System.Text;
using System.Text.RegularExpressions;
using NADA.VFX.Core.State;

namespace NADA.VFX.Core.Visuals
{
    internal static class VfxStyleJson
    {
        internal static string Write(VfxStyleSave save)
        {
            var builder = new StringBuilder();

            builder.AppendLine("{");
            builder.AppendLine($"  \"Version\": {save.Version},");
            builder.AppendLine($"  \"Name\": \"{Escape(save.Name)}\",");
            builder.AppendLine("  \"Entries\": [");

            for (int i = 0; i < save.Entries.Count; i++)
            {
                VfxStyleEntry entry = save.Entries[i];

                builder.Append("    { ");
                builder.Append($"\"Key\": \"{Escape(entry.Key)}\", ");
                builder.Append($"\"Value\": \"{Escape(entry.Value)}\"");
                builder.Append(i < save.Entries.Count - 1 ? " }," : " }");
                builder.AppendLine();
            }

            builder.AppendLine("  ]");
            builder.AppendLine("}");

            return builder.ToString();
        }

        internal static VfxStyleSave Read(string json)
        {
            var save = new VfxStyleSave();

            Match nameMatch = Regex.Match(json, "\"Name\"\\s*:\\s*\"(.*?)\"");
            if (nameMatch.Success)
                save.Name = Unescape(nameMatch.Groups[1].Value);

            MatchCollection entryMatches = Regex.Matches(
                json,
                "\\{\\s*\"Key\"\\s*:\\s*\"(.*?)\"\\s*,\\s*\"Value\"\\s*:\\s*\"(.*?)\"\\s*\\}");

            foreach (Match match in entryMatches)
            {
                save.Entries.Add(new VfxStyleEntry(
                    Unescape(match.Groups[1].Value),
                    Unescape(match.Groups[2].Value)));
            }

            return save;
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }

        private static string Unescape(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }
    }
}