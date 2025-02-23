using VAMVarRenameTool.MetaData;

namespace VAMVarRenameTool.NameTransform;

public class CreatorNameTransformer : NameTransformer
{
    public CreatorNameTransformer(string fileName)
    {
        LoadMappings(fileName);
    }
    
    // key是meta里的名字, value是文件名上的名字
    public bool TryTransform(VarMeta input)
    {
        if (_nameMap.TryGetValue(input.CreatorName, out string output))
        {
            input.CreatorName = output;
            return true;
        }

        return false;
    }
}