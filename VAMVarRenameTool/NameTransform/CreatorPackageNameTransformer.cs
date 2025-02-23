using VAMVarRenameTool.MetaData;

namespace VAMVarRenameTool.NameTransform;

public class CreatorPackageNameTransformer : NameTransformer
{
    public CreatorPackageNameTransformer (string fileName)
    {
        LoadMappings(fileName);
    }
    
    // key是meta里的名字, value是文件名上的名字
    public bool TryTransform(VarMeta meta)
    {
        string inputName = $"{meta.CreatorName}.{meta.PackageName}";
        if (_nameMap.TryGetValue(inputName, out string output))
        {
            var spilit = output.Split('.');
            meta.CreatorName = spilit[0];
            meta.PackageName = spilit[1];
            return true;
        }

        return false;
    }
}