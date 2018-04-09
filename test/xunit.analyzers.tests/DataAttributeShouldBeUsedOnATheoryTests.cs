using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class DataAttributeShouldBeUsedOnATheoryTests
    {
        readonly DiagnosticAnalyzer analyzer = new DataAttributeShouldBeUsedOnATheory();

        [Fact]
        public async Task DoesNotFindErrorForFactMethodWithNoDataAttributes()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { [Xunit.Fact] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("InlineData")]
        [InlineData("MemberData(\"\")")]
        [InlineData("ClassData(typeof(string))")]
        public async Task DoesNotFindErrorForFactMethodWithDataAttributes(string dataAttribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass { [Xunit.Fact, Xunit." + dataAttribute + "] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("InlineData")]
        [InlineData("MemberData(\"\")")]
        [InlineData("ClassData(typeof(string))")]
        public async Task DoesNotFindErrorForTheoryMethodWithDataAttributes(string dataAttribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass { [Xunit.Theory, Xunit." + dataAttribute + "] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("InlineData")]
        [InlineData("MemberData(\"\")")]
        [InlineData("ClassData(typeof(string))")]
        public async Task FindsErrorForMethodsWithDataAttributesButNotFactOrTheory(string dataAttribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass { [Xunit." + dataAttribute + "] public void TestMethod() { } }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Test data attribute should only be used on a Theory", d.GetMessage());
                    Assert.Equal("xUnit1008", d.Descriptor.Id);
                });
        }
    }
}
