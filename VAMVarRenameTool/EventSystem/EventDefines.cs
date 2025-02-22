namespace VAMVarRenameTool.EventSystem
{
    public struct EventStartFixNameInAFolder : IEvent
    {
        public string DirPath { get; }
        public string Pattern { get; }

        public EventStartFixNameInAFolder(string dirPath, string pattern)
        {
            DirPath = dirPath;
            Pattern = pattern;
        }
    }

    // 可以在这里继续添加其他事件定义
}
