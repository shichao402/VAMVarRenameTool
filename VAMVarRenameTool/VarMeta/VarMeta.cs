
namespace VAMVarRenameTool.MetaData
{
    public class VarMeta
    {
        public string CreatorName { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;

        public void CopyFrom(VarMeta meta)
        {
            CreatorName = meta.CreatorName;
            PackageName = meta.PackageName;
        }
    }
}