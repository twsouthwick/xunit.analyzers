﻿using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class PublicMethodShouldBeMarkedAsTestTests
    {
        readonly DiagnosticAnalyzer analyzer = new PublicMethodShouldBeMarkedAsTest();

        [Fact]
        public async Task DoesNotFindErrorForPublicMethodInNonTestClass()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("Xunit.Fact")]
        [InlineData("Xunit.Theory")]
        public async Task DoesNotFindErrorForTestMethods(string attribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { [" + attribute + "] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DoesNotFindErrorForIDisposableDisposeMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"public class TestClass : System.IDisposable {
    [Xunit.Fact] public void TestMethod() { }
    public void Dispose() { }
}");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DoesNotFindErrorForPublicAbstractMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"public abstract class TestClass {
    [Xunit.Fact] public void TestMethod() { }
    public abstract void AbstractMethod();
}");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DoesNotFindErrorForPublicAbstractMethodMarkedWithFact()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"public abstract class TestClass {
    [Xunit.Fact] public void TestMethod() { }
    [Xunit.Fact] public abstract void AbstractMethod();
}");
            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DoesNotFindErrorForIDisposableDisposeMethodOverrideFromParentClass()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"public class BaseClass : System.IDisposable {
    public virtual void Dispose() { }
}
public class TestClass : BaseClass {
    [Xunit.Fact] public void TestMethod() { }
    public override void Dispose() { }
}");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DoesNotFindErrorForIDisposableDisposeMethodOverrideFromParentClassWithRepeatedInterfaceDeclaration()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"public class BaseClass : System.IDisposable {
    public virtual void Dispose() { }
}
public class TestClass : BaseClass, System.IDisposable {
    [Xunit.Fact] public void TestMethod() { }
    public override void Dispose() { }
}");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DoesNotFindErrorForIDisposableDisposeMethodOverrideFromGrandParentClass()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"public abstract class BaseClass : System.IDisposable {
    public abstract void Dispose();
}
public abstract class IntermediateClass : BaseClass {
}
public class TestClass : IntermediateClass {
    [Xunit.Fact] public void TestMethod() { }
    public override void Dispose() { }
}");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DoesNotFindErrorForIAsyncLifetimeMethods()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"public class TestClass : Xunit.IAsyncLifetime {
    [Xunit.Fact] public void TestMethod() { }
    public System.Threading.Tasks.Task DisposeAsync()
    {
        throw new System.NotImplementedException();
    }
    public System.Threading.Tasks.Task InitializeAsync()
    {
        throw new System.NotImplementedException();
    }
}");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DoesNotFindErrorForPublicMethodMarkedWithAttributeWhichIsMarkedWithIgnoreXunitAnalyzersRule1013()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"public class IgnoreXunitAnalyzersRule1013Attribute : System.Attribute { }

[IgnoreXunitAnalyzersRule1013]
public class CustomTestTypeAttribute : System.Attribute { }

public class TestClass { [Xunit.Fact] public void TestMethod() { } [CustomTestType] public void CustomTestMethod() {} }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task FindsWarningForPublicMethodMarkedWithAttributeWhichInheritsFromAttributeMarkedWithIgnoreXunitAnalyzersRule1013()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"public class IgnoreXunitAnalyzersRule1013Attribute : System.Attribute { }

[IgnoreXunitAnalyzersRule1013]
public class BaseCustomTestTypeAttribute : System.Attribute { }

public class DerivedCustomTestTypeAttribute : BaseCustomTestTypeAttribute { }

public class TestClass { [Xunit.Fact] public void TestMethod() { } [DerivedCustomTestType] public void CustomTestMethod() {} }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Public method 'CustomTestMethod' on test class 'TestClass' should be marked as a Fact.", d.GetMessage());
                    Assert.Equal("xUnit1013", d.Id);
                    Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                });
        }

        [Theory]
        [InlineData("Xunit.Fact")]
        [InlineData("Xunit.Theory")]

        public async Task FindsWarningForPublicMethodWithoutParametersInTestClass(string attribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass { [" + attribute + "] public void TestMethod() { } public void Method() {} }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Public method 'Method' on test class 'TestClass' should be marked as a Fact.", d.GetMessage());
                    Assert.Equal("xUnit1013", d.Id);
                    Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                });
        }

        [Theory]
        [InlineData("Xunit.Fact")]
        [InlineData("Xunit.Theory")]

        public async Task FindsWarningForPublicMethodWithParametersInTestClass(string attribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass { [" + attribute + "] public void TestMethod() { } public void Method(int a) {} }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Public method 'Method' on test class 'TestClass' should be marked as a Theory.", d.GetMessage());
                    Assert.Equal("xUnit1013", d.Id);
                    Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                });
        }
    }
}
