namespace VAMVarRenameTool.NameTransform;

public struct VarFileName
{
    public string Creator { get; set; }
    public string Package { get; set; }
    public string Version { get; set; }
    public string OriginalFileName { get; set; }
    public string Extension { get; set; }

    public string NewFileName => $"{Creator}.{Package}.{Version}{Extension}";
}