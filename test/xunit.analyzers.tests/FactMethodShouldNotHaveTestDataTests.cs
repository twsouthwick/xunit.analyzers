﻿using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class FactMethodShouldNotHaveTestDataTests
    {
        readonly DiagnosticAnalyzer analyzer = new FactMethodShouldNotHaveTestData();

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
        public async Task DoesNotFindErrorForDerviedFactMethodWithDataAttributes(string dataAttribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class DerivedFactAttribute: Xunit.FactAttribute {}",
                "public class TestClass { [DerivedFactAttribute, Xunit." + dataAttribute + "] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("InlineData")]
        [InlineData("MemberData(\"\")")]
        [InlineData("ClassData(typeof(string))")]
        public async Task FindsErrorForFactMethodsWithDataAttributes(string dataAttribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass { [Xunit.Fact, Xunit." + dataAttribute + "] public void TestMethod() { } }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Fact methods should not have test data", d.GetMessage());
                    Assert.Equal("xUnit1005", d.Descriptor.Id);
                });
        }
    }
}
