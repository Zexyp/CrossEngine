using CrossEngine.Assemblies;
using CrossEngine.Debugging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Shaders;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CrossEngine.Display.WindowService;

namespace CrossEngine.Platform.Windows
{
    class GdiShader : Shader
    {
        internal Script script;

        public GdiShader(string source, ShaderType type) : base(type)
        {
            GC.KeepAlive(this);
            GPUGC.Register(this);

            if (CheckDiagnose(source))
                return;
            
            // crazy 💀
            var options = ScriptOptions.Default.WithReferences(typeof(Color).Assembly, typeof(Matrix4x4).Assembly).WithImports("System.Drawing", "System.Numerics");
            script = CSharpScript.Create(source, options, typeof(ShaderVariables));
        }

        private bool CheckDiagnose(string source)
        {
            var tree = CSharpSyntaxTree.ParseText(source);
            var diagnostics = tree.GetDiagnostics();
            if (diagnostics.Any())
            {
                var log = $"shader syntax diagnostic ({Type}):";
                foreach (var item in diagnostics)
                {
                    log += "\n" + item.ToString();
                }
                GdiRendererApi.Log.Warn(log);
                
                return true;
            }

            return false;
        }
    }
}
