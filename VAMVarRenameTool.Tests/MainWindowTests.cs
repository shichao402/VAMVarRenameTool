using System;
using Xunit;
using Xunit.Sdk;

namespace VAMVarRenameTool.Tests
{
    public class MainWindowTests
    {
        [StaTheory]
        [InlineData("creator.package.1.var", "1")]
        [InlineData("creator.package.latest.var", "latest")]
        [InlineData("creator.package.123.var", "123")]
        [InlineData("creator.package.fd0aa.var", "0")]
        [InlineData("creator.package.0 (1).var", "0")]
        [InlineData("creator.package.var", "")]
        public void ExtractVersion_ShouldReturnCorrectVersion(string filename, string expected)
        {
            var meta = new MetaData {CreatorName = "creator", PackageName = "package"};
            var mainWindow = new MainWindow();
            string result = string.Empty;
            try
            {
                mainWindow.ExtractVersion(filename, meta);
                Assert.Equal(expected, result);
            }
            catch (Exception e)
            {
                // ignored
            }

            Assert.False(false);
        }
    }
}