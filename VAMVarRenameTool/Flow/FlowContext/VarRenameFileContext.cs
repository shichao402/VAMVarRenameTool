using System.Collections.Generic;

namespace VAMVarRenameTool.Flow
{
    public class VarRenameFileContext : FlowContext
    {
        // 定义特定于重命名文件流程的上下文属性和方法
        public string OriginalFileName { get; set; }
        public string NewFileName { get; set; }
        public string DirectoryPath { get; set; }
        public List<string> Files { get; set; } = new();
        public string FilePattern { get; set; }
    }
}
