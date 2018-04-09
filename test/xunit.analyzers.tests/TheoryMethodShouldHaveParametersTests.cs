using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TheoryMethodShouldHaveParametersTests
    {
        readonly DiagnosticAnalyzer analyzer = new TheoryMethodShouldHaveParameters();

        [Fact]
        public async Task DoesNotFindErrorForFactMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { [Xunit.Fact] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DoesNotFindErrorForTheoryMethodWithParameters()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass { [Xunit.Theory] public void TestMethod(string s) { } }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task FindsErrorForTheoryMethodWithoutParameters()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "class TestClass { [Xunit.Theory] public void TestMethod() { } }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Theory methods should have parameters", d.GetMessage());
                    Assert.Equal("xUnit1006", d.Descriptor.Id);
                });
        }
    }
}
