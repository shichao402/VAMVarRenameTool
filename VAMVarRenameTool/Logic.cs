using System;
using VAMVarRenameTool.NameTransform;
using VAMVarRenameTool.EventSystem;
using VAMVarRenameTool.Flow;

namespace VAMVarRenameTool
{
    public class Logic
    {
        private static readonly Lazy<Logic> _instance = new(() => new Logic());

        public static Logic Instance => _instance.Value;

        public PackageNameTransformer PackageNameTransformer { get; }
        public CreatorNameTransformer CreatorNameTransformer { get; }
        public EventManager EventManager { get; }
        public FlowManager FlowManager { get; }

        private Logic()
        {
            PackageNameTransformer = new PackageNameTransformer("Config/package_mappings.txt");
            CreatorNameTransformer = new CreatorNameTransformer("Config/creator_mappings.txt");
            EventManager = new EventManager();
            FlowManager = new FlowManager();
        }
    }
}
