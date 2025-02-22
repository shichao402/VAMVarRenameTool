using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace VAMVarRenameTool.MetaData
{
    public class VarMetaDataProcessor
    {
        private readonly JsonSerializerOptions _options = new()
        {
            AllowTrailingCommas = true, // 允许末尾逗号
            ReadCommentHandling = JsonCommentHandling.Skip, // 允许注释
            PropertyNameCaseInsensitive = true // 忽略属性名大小写
        };

        public bool ProcessVarMeta(string file, ref VarMeta varMeta)
        {
            try
            {
                using var archive = ZipFile.OpenRead(file);
                var metaEntry = archive.GetEntry("meta.json") ??
                                throw new FileNotFoundException("meta.json missing");

                using var stream = metaEntry.Open();
                using var reader = new StreamReader(stream);

                var meta = JsonSerializer.Deserialize<VarMeta>(reader.ReadToEnd(), _options) ??
                           throw new InvalidDataException("Invalid meta.json");

                if (string.IsNullOrEmpty(meta.CreatorName))
                {
                    meta.CreatorName = "Unknown";
                }

                if (string.IsNullOrEmpty(meta.PackageName))
                {
                    meta.CreatorName = "Unknown";
                }

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
            return false;
        }
    }
}