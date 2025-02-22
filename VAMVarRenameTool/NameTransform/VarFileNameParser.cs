using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace VAMVarRenameTool.NameTransform
{
    public class VarFileNameParser
    {
        public VarFileNameParser(string creatorFilePath, string packageFilePath)
        {
        }

        public VarFileName Parse(string fileName)
        {
            var knownCreators =  Logic.Instance.CreatorNameTransformer;
            var knownPackages =  Logic.Instance.PackageNameTransformer;
            if (!fileName.EndsWith(".var", StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException("Invalid file name format: must end with .var");
            }

            var nameWithoutExtension = fileName.Substring(0, fileName.Length - 4);
            var parts = nameWithoutExtension.Split('.', StringSplitOptions.TrimEntries);

            var result = new VarFileName();

            for (int i = 0; i < parts.Length; i++)
            {
                if (string.IsNullOrEmpty(result.Creator) && knownCreators.Contains(parts[i]))
                {
                    result.Creator = parts[i];
                }
                else if (string.IsNullOrEmpty(result.Package) && knownPackages.Contains(parts[i]))
                {
                    result.Package = parts[i];
                    result.Version = string.Join('.', parts.Skip(i + 1));
                    break;
                }
            }

            if (string.IsNullOrEmpty(result.Creator) && parts.Length > 0)
            {
                result.Creator = parts[0];
            }

            if (string.IsNullOrEmpty(result.Package) && parts.Length > 1)
            {
                result.Package = parts[1];
            }

            if (string.IsNullOrEmpty(result.Version) && parts.Length > 2)
            {
                result.Version = parts[2];
            }

            if (result.Version != "latest")
            {
                // 尝试从版本号中提取数字部分
                var match = Regex.Match(result.Version, @"\d+");
                if (match.Success)
                {
                    result.Version = match.Value;
                }
                else
                {
                    throw new FormatException("Invalid version format: must contain an integer");
                }
            }

            return result;
        }
    }
}