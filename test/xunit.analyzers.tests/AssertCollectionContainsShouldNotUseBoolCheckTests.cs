using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertCollectionContainsShouldNotUseBoolCheckTests
    {
        private readonly DiagnosticAnalyzer analyzer = new AssertCollectionContainsShouldNotUseBoolCheck();

        public static TheoryData<string> Collections { get; } = new TheoryData<string>
        {
            "new System.Collections.Generic.List<int>()",
            "new System.Collections.Generic.HashSet<int>()",
            "new System.Collections.ObjectModel.Collection<int>()"
        };

        public static TheoryData<string> Enumerables { get; } = new TheoryData<string>
        {
            "new int[0]",
            "System.Linq.Enumerable.Empty<int>()"
        };

        private static void CheckDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Contains() to check if a value exists in a collection.", d.GetMessage());
                Assert.Equal("xUnit2017", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task FindsWarningForTrueCollectionContainsCheck(string collection)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() { 
    Xunit.Assert.True(" + collection + @".Contains(1));
} }");

            CheckDiagnostics(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task FindsWarningForFalseCollectionContainsCheck(string collection)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() { 
    Xunit.Assert.False(" + collection + @".Contains(1));
} }");

            CheckDiagnostics(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Enumerables))]
        public async Task FindsWarningForTrueLinqContainsCheck(string enumerable)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.True(" + enumerable + @".Contains(1));
} }");

            CheckDiagnostics(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Enumerables))]
        public async Task FindsWarningForTrueLinqContainsCheckWithEqualityComparer(string enumerable)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.True(" + enumerable + @".Contains(1, System.Collections.Generic.EqualityComparer<int>.Default));
} }");

            CheckDiagnostics(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Enumerables))]
        public async Task FindsWarningForFalseLinqContainsCheck(string enumerable)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.False(" + enumerable + @".Contains(1));
} }");

            CheckDiagnostics(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Enumerables))]
        public async Task FindsWarningForFalseLinqContainsCheckWithEqualityComparer(string enumerable)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.False(" + enumerable + @".Contains(1, System.Collections.Generic.EqualityComparer<int>.Default));
} }");

            CheckDiagnostics(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task DoesNotFindWarningForTrueCollectionContainsCheckWithAssertionMessage(string collection)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() { 
    Xunit.Assert.True(" + collection + @".Contains(1), ""Custom message"");
} }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task DoesNotFindWarningForFalseCollectionContainsCheckWithAssertionMessage(string collection)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() { 
    Xunit.Assert.False(" + collection + @".Contains(1), ""Custom message"");
} }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Enumerables))]
        public async Task DoesNotFindWarningForTrueLinqContainsCheckWithAssertionMessage(string enumerable)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.True(" + enumerable + @".Contains(1), ""Custom message"");
} }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Enumerables))]
        public async Task DoesNotFindWarningForFalseLinqContainsCheckWithAssertionMessage(string enumerable)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.False(" + enumerable + @".Contains(1), ""Custom message"");
} }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DoesNotCrashForCollectionWithDifferentTypeParametersThanICollectionImplementation_ZeroParameters()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Collections.Generic;
class IntList : List<int> { }
class TestClass { void TestMethod() {
    Xunit.Assert.False(new IntList().Contains(1));
} }");

            CheckDiagnostics(diagnostics);
        }

        [Fact]
        public async Task DoesNotCrashForCollectionWithDifferentTypeParametersThanICollectionImplementation_TwoParameters()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Collections.Generic;
class TestClass { void TestMethod() {
    Xunit.Assert.False(new Dictionary<int, int>().ContainsKey(1));
} }");

            Assert.Empty(diagnostics);
        }
    }
}
