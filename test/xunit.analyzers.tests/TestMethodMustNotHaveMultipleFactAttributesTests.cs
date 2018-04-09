﻿using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TestMethodMustNotHaveMultipleFactAttributesTests
    {
        readonly DiagnosticAnalyzer analyzer = new TestMethodMustNotHaveMultipleFactAttributes();

        [Theory]
        [InlineData("Fact")]
        [InlineData("Theory")]
        public async Task DoesNotFindErrorForMethodWithSingleAttribute(string attribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass { [Xunit." + attribute + "] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task FindsErrorForMethodWithTheoryAndFact()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass { [Xunit.Fact, Xunit.Theory] public void TestMethod() { } }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Test methods cannot have multiple Fact or Theory attributes", d.GetMessage());
                    Assert.Equal("xUnit1002", d.Descriptor.Id);
                });
        }

        [Fact]
        public async Task FindsErrorForMethodWithCustomFactAttribute()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass { [Xunit.Fact, CustomFact] public void TestMethod() { } }",
                "public class CustomFactAttribute : Xunit.FactAttribute { }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Test methods cannot have multiple Fact or Theory attributes", d.GetMessage());
                    Assert.Equal("xUnit1002", d.Descriptor.Id);
                });
        }
    }
}
