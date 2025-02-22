using System;
using System.Collections.Generic;
using System.IO;

namespace VAMVarRenameTool.Flow
{
    public class FileScanAtom : FlowAtomBase
    {
        public string DirectoryPath { get; set; }
        public string SearchPattern { get; set; }

        public override void Execute()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {DirectoryPath}");
            }

            var files = Directory.GetFiles(DirectoryPath, SearchPattern, SearchOption.AllDirectories);
            if (Context is VarRenameFileContext context)
            {
                context.Files = new List<string>(files);
            }
        }
    }
}
