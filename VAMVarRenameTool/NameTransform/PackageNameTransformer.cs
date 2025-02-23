using VAMVarRenameTool.MetaData;

namespace VAMVarRenameTool.NameTransform;

public class PackageNameTransformer : NameTransformer
{
    public PackageNameTransformer(string fileName) : base(fileName)
    {
    }
    
    // key是meta里的名字, value是文件名上的名字
    public bool TryTransform(VarMeta meta)
    {
        if (_nameMap.TryGetValue(meta.PackageName, out string output))
        {
            meta.PackageName = output;
            return true;
        }

        return false;
    }
}