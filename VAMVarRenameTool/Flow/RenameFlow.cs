namespace VAMVarRenameTool.Flow
{
    public class RenameFlow : FlowBase<VarRenameFileContext>
    {
        public RenameFlow()
        {
            AddFlowAtom(new FileScanAtom());
            // 可以在这里继续添加其他的流程原子
        }
    }
}
