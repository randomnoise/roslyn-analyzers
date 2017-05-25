﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines.UnitTests
{
    public partial class RemoveEmptyFinalizersTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicRemoveEmptyFinalizersAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpRemoveEmptyFinalizersAnalyzer();
        }

        [Fact]
        public void CA1821CSharpTestNoWarning()
        {
            VerifyCSharp(@"
using System.Diagnostics;

public class Class1
{
	// No violation because the finalizer contains a method call
	~Class1()
	{
        System.Console.Write("" "");
	}
}

public class Class2
{
	// No violation because the finalizer contains a local declaration statement
	~Class2()
	{
        var x = 1;
	}
}

public class Class3
{
    bool x = true;

	// No violation because the finalizer contains an expression statement
	~Class3()
	{
        x = false;
	}
}

public class Class4
{
	// No violation because the finalizer's body is not empty
	~Class4()
	{
        var x = false;
		Debug.Fail(""Finalizer called!"");
	}
}

public class Class5
{
	// No violation because the finalizer's body is not empty
	~Class5()
	{
        while (true) ;
	}
}

public class Class6
{
	// No violation because the finalizer's body is not empty
	~Class6()
	{
        System.Console.WriteLine();
		Debug.Fail(""Finalizer called!"");
	}
}

public class Class7
{
	// Violation will not occur because the finalizer's body is not empty.
	~Class7()
	{
        if (true)
        {
		    Debug.Fail(""Finalizer called!"");
        }
    }
}
");
        }

        [Fact]
        public void CA1821CSharpTestRemoveEmptyFinalizers()
        {
            VerifyCSharp(@"
public class Class1
{
	// Violation occurs because the finalizer is empty.
	~Class1()
	{
	}
}

public class Class2
{
	// Violation occurs because the finalizer is empty.
	~Class2()
	{
        //// Comments here
	}
}
",
                GetCA1821CSharpResultAt(5, 3),
                GetCA1821CSharpResultAt(13, 3));
        }

        [Fact]
        public void CA1821CSharpTestRemoveEmptyFinalizersWithScope()
        {
            VerifyCSharp(@"
[|
public class Class1
{
	// Violation occurs because the finalizer is empty.
	~Class1()
	{
	}
}
|]

public class Class2
{
	// Violation occurs because the finalizer is empty.
	~Class2()
	{
        //// Comments here
	}
}

",
                GetCA1821CSharpResultAt(6, 3));
        }

        [Fact]
        public void CA1821CSharpTestRemoveEmptyFinalizersWithDebugFail()
        {
            VerifyCSharp(@"
using System.Diagnostics;

public class Class1
{
	// Violation occurs because Debug.Fail is a conditional method. 
	// The finalizer will contain code only if the DEBUG directive 
	// symbol is present at compile time. When the DEBUG 
	// directive is not present, the finalizer will still exist, but 
	// it will be empty.
	~Class1()
	{
		Debug.Fail(""Finalizer called!"");
    }
}
",
            GetCA1821CSharpResultAt(11, 3));
        }

        [Fact]
        public void CA1821CSharpTestRemoveEmptyFinalizersWithDebugFailAndDirective()
        {
            VerifyCSharp(@"
public class Class1
{
#if DEBUG
	// Violation will not occur because the finalizer will exist and 
	// contain code when the DEBUG directive is present. When the 
	// DEBUG directive is not present, the finalizer will not exist, 
	// and therefore not be empty.
	~Class1()
	{
		System.Diagnostics.Debug.Fail(""Finalizer called!"");
    }
#endif
}
");
        }

        [Fact]
        public void CA1821CSharpTestRemoveEmptyFinalizersWithDebugFailAndDirectiveAroundStatements()
        {
            VerifyCSharp(@"
using System.Diagnostics;

public class Class1
{
	// Violation occurs because Debug.Fail is a conditional method.
	~Class1()
	{
		Debug.Fail(""Class1 finalizer called!"");
    }
}

public class Class2
{
	// Violation will not occur because the finalizer's body is not empty.
	~Class2()
	{
		Debug.Fail(""Class2 finalizer called!"");
        Foo();
    }

    void Foo()
    {
    }
}
",
            GetCA1821CSharpResultAt(7, 3));
        }

        [WorkItem(820941, "DevDiv")]
        [Fact]
        public void CA1821CSharpTestRemoveEmptyFinalizersWithNonInvocationBody()
        {
            VerifyCSharp(@"
public class Class1
{
    ~Class1()
	{
		Class2.SomeFlag = true;
    }
}

public class Class2
{
    public static bool SomeFlag;
}
");
        }

        [Fact]
        public void CA1821BasicTestNoWarning()
        {
            VerifyBasic(@"
Imports System.Diagnostics

Public Class Class1
	' No violation because the finalizer contains a method call
    Protected Overrides Sub Finalize()
        System.Console.Write("" "")
    End Sub
End Class

Public Class Class2
	' No violation because the finalizer's body is not empty.
    Protected Overrides Sub Finalize()
        Dim a = true
    End Sub
End Class

Public Class Class3
    Dim a As Boolean = True

	' No violation because the finalizer's body is not empty.
    Protected Overrides Sub Finalize()
        a = False
    End Sub
End Class

Public Class Class4
	' No violation because the finalizer's body is not empty.
    Protected Overrides Sub Finalize()
        Dim a = False
        Debug.Fail(""Finalizer called!"")
    End Sub
End Class

Public Class Class5
	' No violation because the finalizer's body is not empty.
    Protected Overrides Sub Finalize()
        If True Then
            Debug.Fail(""Finalizer called!"")
        End If
    End Sub
End Class
");
        }

        [Fact]
        public void CA1821BasicTestRemoveEmptyFinalizers()
        {
            VerifyBasic(@"
Imports System.Diagnostics

Public Class Class1
    '  Violation occurs because the finalizer is empty.
    Protected Overrides Sub Finalize()

    End Sub
End Class

Public Class Class2
    '  Violation occurs because the finalizer is empty.
    Protected Overrides Sub Finalize()
        ' Comments
    End Sub
End Class

Public Class Class3
    '  Violation occurs because Debug.Fail is a conditional method.
    Protected Overrides Sub Finalize()
        Debug.Fail(""Finalizer called!"")
    End Sub
End Class
",
                GetCA1821BasicResultAt(6, 29),
                GetCA1821BasicResultAt(13, 29),
                GetCA1821BasicResultAt(20, 29));
        }

        [Fact]
        public void CA1821BasicTestRemoveEmptyFinalizersWithScope()
        {
            VerifyBasic(@"
Imports System.Diagnostics

Public Class Class1
    '  Violation occurs because the finalizer is empty.
    Protected Overrides Sub Finalize()

    End Sub
End Class

[|Public Class Class2
    '  Violation occurs because the finalizer is empty.
    Protected Overrides Sub Finalize()
        ' Comments
    End Sub
End Class|]

Public Class Class3
    '  Violation occurs because Debug.Fail is a conditional method.
    Protected Overrides Sub Finalize()
        Debug.Fail(""Finalizer called!"")
    End Sub
End Class
",
                GetCA1821BasicResultAt(13, 29));
        }

        [Fact]
        public void CA1821BasicTestRemoveEmptyFinalizersWithDebugFail()
        {
            VerifyBasic(@"
Imports System.Diagnostics

Public Class Class1
	' Violation occurs because Debug.Fail is a conditional method.
    Protected Overrides Sub Finalize()
        Debug.Fail(""Finalizer called!"")
    End Sub
End Class

Public Class Class2
	' Violation occurs because Debug.Fail is a conditional method.
    Protected Overrides Sub Finalize()
        Dim a = False
        Debug.Fail(""Finalizer called!"")
    End Sub
End Class
",
                GetCA1821BasicResultAt(6, 29));
        }

        [Fact]
        public void CA1821CSharpTestRemoveEmptyFinalizersWithThrowStatement()
        {
            VerifyCSharp(@"
public class Class1
{
    ~Class1()
    {
        throw new System.Exception();
    }
}",
                GetCA1821CSharpResultAt(4, 6));
        }

        [Fact]
        public void CA1821BasicTestRemoveEmptyFinalizersWithThrowStatement()
        {
            VerifyBasic(@"
Public Class Class1
	' Violation occurs because Debug.Fail is a conditional method.
    Protected Overrides Sub Finalize()
        Throw New System.Exception()
    End Sub
End Class
",
                GetCA1821BasicResultAt(4, 29));
        }

        [Fact, WorkItem(1211, "https://github.com/dotnet/roslyn-analyzers/issues/1211")]
        public void CA1821CSharpRemoveEmptyFinalizersInvalidInvocationExpression()
        {
            VerifyCSharp(@"
public class C1
{
    ~C1()
    {
        a
    }
}
",
                TestValidationMode.AllowCompileErrors);
        }

        [Fact, WorkItem(1211, "https://github.com/dotnet/roslyn-analyzers/issues/1211")]
        public void CA1821BasicRemoveEmptyFinalizersInvalidInvocationExpression()
        {
            VerifyBasic(@"
Public Class Class1
    Protected Overrides Sub Finalize()
        a
    End Sub
End Class
",
                TestValidationMode.AllowCompileErrors);
        }

        private static DiagnosticResult GetCA1821CSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, AbstractRemoveEmptyFinalizersAnalyzer.RuleId, MicrosoftQualityGuidelinesAnalyzersResources.RemoveEmptyFinalizers);
        }

        private static DiagnosticResult GetCA1821BasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, AbstractRemoveEmptyFinalizersAnalyzer.RuleId, MicrosoftQualityGuidelinesAnalyzersResources.RemoveEmptyFinalizers);
        }
    }
}
