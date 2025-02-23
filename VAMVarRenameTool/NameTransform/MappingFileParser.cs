using System;
using System.Collections.Generic;
using System.IO;

namespace VAMVarRenameTool.NameTransform
{
    public class MappingFileParser
    {
        public static bool Parse(string filePath, ref Dictionary<string, string> map, bool clearFirst = false)
        {
            if (File.Exists(filePath))
            {
                if (clearFirst)
                    map.Clear();
                foreach (var line in File.ReadAllLines(filePath))
                {
                    var parts = line.Split(new[] { ",,," }, StringSplitOptions.TrimEntries);
                    var key = parts[0].Trim();
                    var value = parts.Length > 1 ? parts[1].Trim() : key;
                    map[key] = value;
                }

                return true;
            }
            return false;
        }
    }
}
