﻿using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class FactMethodMustNotHaveParametersTests
    {
        readonly DiagnosticAnalyzer analyzer = new FactMethodMustNotHaveParameters();

        [Fact]
        public async Task DoesNotFindErrorForFactWithNoParameters()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { [Xunit.Fact] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DoesNotFindErrorForTheoryWithParameters()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { [Xunit.Theory] public void TestMethod(string p) { } }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task FindsErrorForPrivateClass()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { [Xunit.Fact] public void TestMethod(string p) { } }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Fact methods cannot have parameters", d.GetMessage());
                    Assert.Equal("xUnit1001", d.Descriptor.Id);
                });
        }
    }
}
