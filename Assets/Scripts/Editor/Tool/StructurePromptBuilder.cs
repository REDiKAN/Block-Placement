using System.Linq;
using System.Text;

namespace Game.Editor.Tool
{
    public static class StructurePromptBuilder
    {
        public static string Build(StructurePromptItem[] structures)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"structures\": [");

            var validStructures = structures.Where(s => s.Config is not null).ToArray();

            for (var i = 0; i < validStructures.Length; i++)
            {
                var item = validStructures[i];
                var config = item.Config;

                sb.AppendLine("    {");
                sb.AppendLine($"      \"name\": \"{config.DisplayName}\",");
                sb.AppendLine($"      \"maxCount\": {item.MaxCount},");
                sb.Append("      \"cells\": [");

                if (config.LocalCoordinates is not null && config.LocalCoordinates.Length > 0)
                {
                    for (var j = 0; j < config.LocalCoordinates.Length; j++)
                    {
                        var coord = config.LocalCoordinates[j];
                        sb.Append($"{{\"x\": {coord.x}, \"y\": {coord.y}, \"z\": {coord.z}}}");
                        if (j < config.LocalCoordinates.Length - 1)
                            sb.Append(", ");
                    }
                }

                sb.AppendLine("]");
                sb.Append("    }");

                if (i < validStructures.Length - 1)
                    sb.Append(",");

                sb.AppendLine();
            }

            sb.AppendLine("  ]");
            sb.Append("}");

            return sb.ToString();
        }
    }
}