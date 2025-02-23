using System;
using VAMVarRenameTool.NameTransform;
using VAMVarRenameTool.EventSystem;
using VAMVarRenameTool.MetaData;

namespace VAMVarRenameTool
{
    public class Core
    {
        private static readonly Lazy<Core> _instance = new(() => new Core());

        public static Core Instance => _instance.Value;

        public CreatorPackageNameTransformer CreatorPackageNameTransformer { get; set; }
        public PackageNameTransformer PackageNameTransformer { get; }
        public CreatorNameTransformer CreatorNameTransformer { get; }
        public CharTransformer CharTransformer { get; }
        public VarFileNameParser VarFileNameParser { get; }
        public EventManager EventManager { get; }
        public VarMetaProcessor VarMetaProcessor { get; }

        private Core()
        {
            try
            {
                // 初始化代码
                CreatorNameTransformer = new CreatorNameTransformer("Config/meta2name_creator.txt");
                PackageNameTransformer = new PackageNameTransformer("Config/meta2name_package.txt");
                CreatorPackageNameTransformer = new CreatorPackageNameTransformer("Config/meta2name_creator_package.txt");
                VarFileNameParser = new VarFileNameParser(CreatorNameTransformer, PackageNameTransformer);
                EventManager = new EventManager();
                CharTransformer = new CharTransformer();
                VarMetaProcessor = new VarMetaProcessor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"构造函数异常: {ex}");
                throw; // 保留原始堆栈信息
            }
        }

        public void ReloadConfigs()
        {
            CreatorNameTransformer.ReloadConfigs();
            PackageNameTransformer.ReloadConfigs();
            CreatorPackageNameTransformer.ReloadConfigs();
        }
    }
}