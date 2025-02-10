using Xunit;

namespace VAMVarRenameTool.Tests
{
    public class MainWindowTests
    {
        [Theory]
        [InlineData("creator.package.1.var", "1")]
        [InlineData("creator.package.latest.var", "latest")]
        [InlineData("creator.package.123.var", "123")]
        [InlineData("creator.package.1.0.0.var", "1.0.0")]
        [InlineData("creator.package.var", "")]
        public void ExtractVersion_ShouldReturnCorrectVersion(string filename, string expected)
        {
            var meta = new MetaData { CreatorName = "creator", PackageName = "package" };
            var mainWindow = new MainWindow();
            var result = mainWindow.ExtractVersion(filename, meta);
            Assert.Equal(expected, result);
        }
    }
}
