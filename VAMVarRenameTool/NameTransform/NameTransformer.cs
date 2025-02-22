using System;
using System.Collections.Generic;
using System.IO;
using VAMVarRenameTool.NameTransform;

public class NameTransformer
{
    protected Dictionary<string, string> _nameMap = new();

    public bool TryTransform(string input, out string output)
    {
        return _nameMap.TryGetValue(input, out output);
    }
    
    public bool Contains(string key)
    {
        return _nameMap.ContainsKey(key);
    }

    protected void LoadMappings(string fileName)
    {
        MappingFileParser.Parse(fileName, ref _nameMap, true);
    }

    private string GetFilePath(string fileName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var filePath = Path.Combine(baseDirectory, fileName);

        if (!File.Exists(filePath))
        {
            // 尝试在调试目录中查找文件
            filePath = Path.Combine(baseDirectory, "..", "..", "..", fileName);
        }

        return filePath;
    }
}