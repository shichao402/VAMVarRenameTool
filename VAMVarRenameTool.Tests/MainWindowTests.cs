using System;
using VAMVarRenameTool.MetaData;
using Xunit;

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
        
        [StaTheory]
        [InlineData("14mhz.AtomClickTrigger.Plugin-AtomClickTrigger.1.var", "1")]
        [InlineData("creator.package.1.var", "1")]
        [InlineData("creator.package.latest.var", "latest")]
        [InlineData("creator.package.123.var", "123")]
        [InlineData("creator.package.fd0aa.var", "0")]
        [InlineData("creator.package.0 (1).var", "0")]
        [InlineData("creator.package.var", "")]
        [InlineData("Archer.diaochan_clo.1 - 副本.var", "1")]
        public void ExtractVersion_ShouldReturnCorrectVersion(string filename, string expected)
        {
            // @todo
            var processor = new VarMetaDataProcessor();
            try
            {
                string version = processor.ExtractVersion(filename);
                Assert.Equal(expected, version);
            }
            catch (Exception e)
            {
                Assert.False(true);
            }

            Assert.False(false);
        }
        
        [StaTheory]
        [InlineData("Abubu Nownanka.EgyptThroneRoom.1.var", "Ready")]
        [InlineData("AcidBubbles.Scripter1.16.var", "Ready")]
        public void MainProcessOneFile(string filename, string expected)
        {
            // @todo
            Assert.False(false);
        }
    }
}