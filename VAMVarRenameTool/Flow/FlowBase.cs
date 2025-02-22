using System.Collections.Generic;

namespace VAMVarRenameTool.Flow
{
    public abstract class FlowBase<TContext> where TContext : FlowContext, new()
    {
        protected readonly List<FlowAtomBase> _flowAtoms = new();
        private Stack<FlowContext> _contextStack = new();

        protected FlowBase()
        {
            _contextStack.Push(new TContext());
        }
        
        ~FlowBase()
        {
            _contextStack.Pop();
        }

        public void AddFlowAtom(FlowAtomBase flowAtom)
        {
            _flowAtoms.Add(flowAtom);
        }

        public void Execute()
        {
            foreach (var flowAtom in _flowAtoms)
            {
                flowAtom.Execute();
            }
        }

        protected virtual TContext CreateContext()
        {
            return new TContext();
        }

        public TContext CurrentContext => (TContext)_contextStack.Peek();
    }
}
