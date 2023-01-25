using IniParser.Model;
using IniParser.Model.Configuration;
using IniParser.Parser;
using System;
using System.IO;
using System.Text;

namespace Mo3RegUI
{
    internal static class MyIniParserHelper
    {
        public static IniParserConfiguration IniParserConfiguration { get; } = new IniParserConfiguration()
        {
            AllowDuplicateKeys = true,
            AllowDuplicateSections = true,
            AllowKeysWithoutSection = false,
            // CommentRegex = new Regex("a^"), // match nothing
            CaseInsensitive = true,
            AssigmentSpacer = string.Empty,
            // SectionRegex = new Regex("^(\\s*?)\\[{1}\\s*[\\p{L}\\p{P}\\p{M}_\\\"\\'\\{\\}\\#\\+\\;\\*\\%\\(\\)\\=\\?\\&\\$\\^\\<\\>\\`\\^|\\,\\:\\/\\.\\-\\w\\d\\s\\\\\\~]+\\s*\\](\\s*?)$"),
        };

        public static IniDataParser GetIniDataParser() => new(IniParserConfiguration);

        public static IniData GetIniData() => new() { Configuration = IniParserConfiguration };

        public static KeyDataCollection GetSectionOrNew(IniData ini, string sectionName)
        {
            if (!ini.Sections.ContainsSection(sectionName))
            {
                _ = ini.Sections.AddSection(sectionName);
            }
            return ini.Sections[sectionName];
        }

        public static IniData ParseIni(Stream stream)
        {
            var parser = GetIniDataParser();

            using var sr = new StreamReader(stream, new UTF8Encoding(false));
            return parser.Parse(sr.ReadToEnd());
        }

        public static void WriteIni(IniData ini, Stream stream)
        {
            using var sw = new StreamWriter(stream, new UTF8Encoding(false));
            sw.Write(ini.ToString());
        }

        public static void EditIniFile(string filename, Action<IniData> editAction)
        {
            IniData ini = null;
            using (var fs = File.Open(filename, FileMode.Open))
            {
                ini = MyIniParserHelper.ParseIni(fs);
            }

            editAction?.Invoke(ini);

            using (var fs = File.Open(filename, FileMode.Create)) // Note: never use File.OpenWrite
            {
                MyIniParserHelper.WriteIni(ini, fs);
            }
        }
    }
}
