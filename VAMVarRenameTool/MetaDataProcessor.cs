using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VAMVarRenameTool
{
    public class MetaDataProcessor
    {
        private readonly JsonSerializerOptions _options = new()
        {
            AllowTrailingCommas = true, // 允许末尾逗号
            ReadCommentHandling = JsonCommentHandling.Skip, // 允许注释
            PropertyNameCaseInsensitive = true // 忽略属性名大小写
        };
        private readonly List<Regex> _versionPatterns = new()
        {
            new Regex($@".+\.(?<version>[^\s\.]+)[^\.]*\.var$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
            new Regex(@"\.(?<version>latest)\.var$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
            new Regex(@"(?:\.)(?<version>\d+)\.var$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
            new Regex(@".*\..*\.(?<version>\d+)\.var$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        };
        private readonly CharTransformer _charTransformer = new();

        public MetaData ProcessMetaData(string file, out string status, out string newName)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var fullFullName = Path.GetFileName(file);
                var parts = fileName.Split('.');
                if (parts.Length < 3)
                {
                    throw new FormatException("Invalid file name format");
                }

                var originalCreatorName = parts[0];
                var originalPackageName = parts[1];
                var originalVersion = parts[2];

                using var archive = ZipFile.OpenRead(file);
                var metaEntry = archive.GetEntry("meta.json") ??
                                throw new FileNotFoundException("meta.json missing");

                using var stream = metaEntry.Open();
                using var reader = new StreamReader(stream);

                var meta = JsonSerializer.Deserialize<MetaData>(reader.ReadToEnd(), _options) ??
                           throw new InvalidDataException("Invalid meta.json");

                var nameParts = new[] { meta.CreatorName, meta.PackageName, ExtractVersion(fullFullName) };

                var transformedCreatorName = _charTransformer.Transform(originalCreatorName);
                if (string.Equals(transformedCreatorName, meta.CreatorName, StringComparison.OrdinalIgnoreCase))
                {
                    // 如果转换后一样. 就用原始的名字.
                    nameParts[0] = parts[0];
                }

                int error = 0;
                for (int i = 0; i < 2; i++)
                {
                    if (string.IsNullOrEmpty(nameParts[i]))
                    {
                        nameParts[i] = "Unknown";
                        ++error;
                    }
                }

                status = error == 0 ? "Ready" : "Parse Failed";
                newName = string.Join('.', nameParts) + ".var";
                return meta;
            }
            catch (Exception ex)
            {
                status = $"Error: {ex.Message}";
                newName = string.Empty;
                return null;
            }
        }

        public string ExtractVersion(string filename)
        {
            foreach (Regex each in this._versionPatterns)
            {
                var match = each.Match(filename);

                if (match.Success)
                {
                    var version = match.Groups["version"].Value;
                    if (Int32.TryParse(version, out var number))
                    {
                        version = number.ToString();
                    }
                    else
                    {
                        break;
                    }

                    // 清理可能包含的额外字符
                    return Regex.Replace(version, @"[^\w\.-]", "");
                }
            }

            throw new FormatException($"Version pattern not found in: {filename}");
        }
    }
}