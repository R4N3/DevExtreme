using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StyleCompiler.ThemeBuilder
{
    class ThemeBuilderLessParser
    {

        const string ThemeBuilderPrefix = "dx-theme-builder-";
        const string MultilineCommentPattern = "(/\\*\\*([\\n\\r])([\\s\\S]*?)\\*\\/)";
        const string NewLinePattern = "([\\n\\r])";

        readonly string _lessContent;

        public ThemeBuilderLessParser(string lessContent)
        {
            _lessContent = lessContent;
        }

        ThemeBuilderMetadata ParseCommentBlock(string commentBlock)
        {
            commentBlock = Regex.Replace(commentBlock, NewLinePattern, "", RegexOptions.Multiline);
            string[] commentLines = commentBlock.Split(new string[] { "*" }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> dataFields = new Dictionary<string, string>();
            foreach (string line in commentLines)
            {
                Match match = Regex.Match(line, "@([a-z]+)\\s(.+)", RegexOptions.IgnoreCase);
                dataFields.Add(match.Groups[1].Value, match.Groups[2].Value);
            }

            ThemeBuilderMetadata data = new ThemeBuilderMetadata
            {
                Name = dataFields["name"].Trim(),
                Type = dataFields["type"].Trim()
            };

            string typeValues = null;
            if(dataFields.TryGetValue("typeValues", out typeValues)) {
                data.TypeValues = typeValues;
            }

            return data;
        }

        public List<ThemeBuilderMetadata> GenerateThemeBuilderMetadata()
        {
            string patternWithConstName = MultilineCommentPattern + "(\\s)*" + NewLinePattern + "*([-@a-z_0-9]+):";
            List<ThemeBuilderMetadata> metadata = new List<ThemeBuilderMetadata>();
            Match match = Regex.Match(_lessContent, patternWithConstName, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            while (match.Success)
            {
                if (!String.IsNullOrEmpty(match.Groups[3].Value))
                {
                    ThemeBuilderMetadata item = ParseCommentBlock(match.Groups[3].Value);
                    item.Key = match.Groups[6].Value;
                    metadata.Add(item);
                }
                match = match.NextMatch();
            }

            return metadata;
        }

        public static string MinifyLess(string less)
        {
            less = Regex.Replace(less, "(?<!(url\\s*\\([^\\)\\n]*))(?<!(http[s]?:))//.*", "");
            less = Regex.Replace(less, "([\\n\\r])", "");
            less = Regex.Replace(less, "\\/\\*(.*?)\\*\\/", "");
            return less;
        }

    }

}


