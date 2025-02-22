using System;
using VAMVarRenameTool.EventSystem;

namespace VAMVarRenameTool.Flow
{
    public class FlowManager
    {
        public FlowManager()
        {
            Logic.Instance.EventManager.RegisterEvent<EventStartFixNameInAFolder>(OnStartFixNameInAFolder);
        }

        private void OnStartFixNameInAFolder(EventStartFixNameInAFolder e)
        {
            var flow = new RenameFlow();
            flow.CurrentContext.DirectoryPath = e.DirPath;
            flow.CurrentContext.FilePattern = e.Pattern;
            flow.Execute();
        }
    }
}
