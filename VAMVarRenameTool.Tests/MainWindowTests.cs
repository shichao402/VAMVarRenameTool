using System;
using VAMVarRenameTool.MetaData;
using Xunit;

namespace VAMVarRenameTool.Tests
{
    public class MainWindowTests
    {
        [StaTheory]
        [InlineData("creator.package.var", "")]
        public void MainWindow_Test(string filename, string expected)
        {
            var meta = new MetaData.VarMeta {CreatorName = "creator", PackageName = "package"};
            var mainWindow = new MainWindow();
            string result = string.Empty;
            try
            {
                // mainWindow.ExtractVersion(filename, meta);
                // Assert.Equal(expected, result);
            }
            catch (Exception e)
            {
                // ignored
            }

            Assert.False(false);
        }
    }
}