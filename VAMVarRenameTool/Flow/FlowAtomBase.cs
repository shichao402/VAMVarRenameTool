namespace VAMVarRenameTool.Flow
{
    public abstract class FlowAtomBase
    {
        protected FlowContext Context => FlowBase.CurrentContext;

        public abstract void Execute();
    }
}
