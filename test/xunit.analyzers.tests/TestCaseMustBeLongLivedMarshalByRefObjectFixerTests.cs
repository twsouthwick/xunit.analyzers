﻿using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TestCaseMustBeLongLivedMarshalByRefObjectFixerTests
    {
        readonly DiagnosticAnalyzer analyzer = new TestCaseMustBeLongLivedMarshalByRefObject();
        readonly CodeFixProvider fixer = new TestCaseMustBeLongLivedMarshalByRefObjectFixer();

        [Fact]
        public async Task WithNoBaseClass_WithoutUsing_AddsBaseClass()
        {
            var code = "public class MyTestCase : Xunit.Abstractions.ITestCase { }";

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, code, CompilationReporting.IgnoreErrors, XunitReferences.PkgExecutionExtensibility);

            Assert.Equal("public class MyTestCase : Xunit.LongLivedMarshalByRefObject, Xunit.Abstractions.ITestCase { }", result);
        }

        [Fact]
        public async Task WithNoBaseClass_WithUsing_AddsBaseClass()
        {
            var code = "using Xunit; using Xunit.Abstractions; public class MyTestCase : ITestCase { }";

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, code, CompilationReporting.IgnoreErrors, XunitReferences.PkgExecutionExtensibility);

            Assert.Equal("using Xunit; using Xunit.Abstractions; public class MyTestCase : LongLivedMarshalByRefObject, ITestCase { }", result);
        }

        [Fact]
        public async Task WithBadBaseClass_WithoutUsing_ReplacesBaseClass()
        {
            var code = "public class Foo { } public class MyTestCase : Foo, Xunit.Abstractions.ITestCase { }";

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, code, CompilationReporting.IgnoreErrors, XunitReferences.PkgExecutionExtensibility);

            Assert.Equal("public class Foo { } public class MyTestCase : Xunit.LongLivedMarshalByRefObject, Xunit.Abstractions.ITestCase { }", result);
        }

        [Fact]
        public async Task WithBadBaseClass_WithUsing_ReplacesBaseClass()
        {
            var code = "using Xunit; using Xunit.Abstractions; public class Foo { } public class MyTestCase : Foo, ITestCase { }";

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, code, CompilationReporting.IgnoreErrors, XunitReferences.PkgExecutionExtensibility);

            Assert.Equal("using Xunit; using Xunit.Abstractions; public class Foo { } public class MyTestCase : LongLivedMarshalByRefObject, ITestCase { }", result);
        }
    }
}
