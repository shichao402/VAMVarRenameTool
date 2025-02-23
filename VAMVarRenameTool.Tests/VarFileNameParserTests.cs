using System;
using VAMVarRenameTool.NameTransform;
using Xunit;

namespace VAMVarRenameTool.Tests
{
    public class VarFileNameParserTests
    {
        private readonly VarFileNameParser _parser;

        public VarFileNameParserTests()
        {
            _parser = Core.Instance.VarFileNameParser;
        }

        [Theory]
        [InlineData("14mhz.AtomClickTrigger.Plugin-AtomClickTrigger.1.var", "14mhz", "AtomClickTrigger", "1")]
        [InlineData("Abubu Nownanka.EgyptThroneRoom.1.var", "Abubu Nownanka", "EgyptThroneRoom", "1")]
        [InlineData("AcidBubbles.Scripter1.16.var", "AcidBubbles", "Scripter1", "16")]
        public void Parse_SpecialVar(string filename, string expectedCreator, string expectedPackage, string expectedVersion)
        {
            var result = _parser.Parse(filename);

            Assert.Equal(expectedCreator, result.Creator);
            Assert.Equal(expectedPackage, result.Package);
            Assert.Equal(expectedVersion, result.Version);
        }

        [Theory]
        [InlineData("creator.package.1.var", "creator", "package", "1")]
        [InlineData("creator.package.latest.var", "creator", "package", "latest")]
        [InlineData("creator.package.123.var", "creator", "package", "123")]
        [InlineData("creator.package.fd0aa.var", "creator", "package", "0")]
        [InlineData("creator.package.0 (1).var", "creator", "package", "0")]
        [InlineData("creator.package.var", "creator", "package", "100000")]
        [InlineData("Archer.diaochan_clo.1 - 副本.var", "Archer", "diaochan_clo", "1")]
        [InlineData("14mhz.AtomClickTrigger.Plugin-AtomClickTrigger.1.var", "14mhz", "AtomClickTrigger", "1")]
        [InlineData("Abubu Nownanka.EgyptThroneRoom.1.var", "Abubu Nownanka", "EgyptThroneRoom", "1")]
        public void Parse_ShouldReturnCorrectVarFileName(string filename, string expectedCreator, string expectedPackage, string expectedVersion)
        {
            var result = _parser.Parse(filename);

            Assert.Equal(expectedCreator, result.Creator);
            Assert.Equal(expectedPackage, result.Package);
            Assert.Equal(expectedVersion, result.Version);
        }

        [Theory]
        [InlineData("invalidfile.txt")]
        [InlineData("creator.package.var")]
        [InlineData("creator.package")]
        [InlineData("creator..1.var")]
        [InlineData(".package.1.var")]
        [InlineData("creator.package.1")]
        public void Parse_ShouldThrowFormatException_ForInvalidFileName(string filename)
        {
            // Assert.Throws<FormatException>(() => _parser.Parse(filename));
        }
    }
}