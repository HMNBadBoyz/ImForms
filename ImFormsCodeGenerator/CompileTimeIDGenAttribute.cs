using CodeGeneration.Roslyn;
using System;
using System.Diagnostics;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
[CodeGenerationAttribute(typeof(CompileTimeIDGenGenerator))]
[Conditional("CodeGeneration")]
public class CompileTimeIDGenAttribute : Attribute
{
    public CompileTimeIDGenAttribute()
    {
        
    }

   
}
