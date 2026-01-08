using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using CrossEngine.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace CrossEngine.Assemblies;

public static class Scripting
{
    private static Logger log = new Logger("scripting");
    public static (AssemblyLoadContext Context, Assembly Assembly) Compile(Stream stream)
    {
        log.Trace("compiling script");
        
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(stream));
        var references =
            AssemblyLoadContext.Default.Assemblies.Select(assembly =>
                MetadataReference.CreateFromFile(assembly.Location));
        var compilation = CSharpCompilation.Create(
            null,
            new[] {syntaxTree},
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        
        if (!result.Success)
        {
            var diagnostic = "";
            for (int i = 0; i < result.Diagnostics.Length; i++)
            {
                diagnostic += "\n" + result.Diagnostics[i].ToString();
            }
            log.Error("compilation failed:" + diagnostic);
            return (null, null);
        }
        
        ms.Seek(0, SeekOrigin.Begin);

        return AssemblyManager.Load(ms);
    }
}