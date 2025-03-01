using VAMVarRenameTool.MetaData;
using VAMVarRenameTool.NameTransform;

namespace VAMVarRenameTool.Misc;

public class VarInfo
{
    private VarMeta meta;
    private VarFileName varFileName;

    public string GetFixedFileName()
    {
        // 优先从meta中取信息. 
        // 如果meta中没有取到.那么用fileName里的信息.
        // meta.CreatorName = Core.Instance.CharTransformer.Transform(meta.CreatorName);
        // meta.PackageName = Core.Instance.CharTransformer.Transform(meta.PackageName);
        //
        // Core.Instance.CreatorPackageNameTransformer.TryTransform(meta);
        // Core.Instance.CreatorNameTransformer.TryTransform(meta);
        // Core.Instance.PackageNameTransformer.TryTransform(meta);
        return string.Empty;
    }
}