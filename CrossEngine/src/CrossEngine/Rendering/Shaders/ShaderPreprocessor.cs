#define SHOW_SOURCES

using CrossEngine.Logging;
using CrossEngine.Platform;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CrossEngine.Rendering.Shaders
{
    public static class ShaderPreprocessor
    {
        internal static Action<Action> ServiceRequest;

        private const string DefaultShaderProgramSource =
@"#type vertex
#version 330 core
layout(location = 0) in vec3 aPosition;
uniform mat4 uViewProjection = mat4(1);
void main() {
    gl_Position = uViewProjection * vec4(aPosition, 1.0);
}

#type fragment
#version 330 core
void main() {
    gl_FragColor = vec4(1, 0, 1, 1);
}";
        public static WeakReference<ShaderProgram> DefaultShaderProgram { get; private set; }

        static Logger _log = new Logger("shader-preproc");

        public struct ShaderSources
        {
            public string Vertex, Fragment;
        }

        internal static void Init()
        {
            // erm
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(DefaultShaderProgramSource);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                DefaultShaderProgram = CreateProgramFromStream(stream);
            }
        }

        internal static void Shutdown()
        {
            DefaultShaderProgram.Dispose();
            DefaultShaderProgram = null;
        }

        public static WeakReference<ShaderProgram> CreateProgramFromFile(string filepath)
        {
            _log.Debug($"processing '{filepath}'");

            Stream IncudeCallback(string path)
            {
                _log.Trace($"including '{path}'");
                return File.OpenRead(Path.Join(Path.GetDirectoryName(filepath), path));
            }

            return CreateProgramFromStream(File.OpenRead(filepath), IncudeCallback);
        }

        public static WeakReference<ShaderProgram> CreateProgramFromStream(Stream stream, Func<string, Stream> includeCallback = null)
        {

            var sources = SplitSources(stream, includeCallback);

            var program = new WeakReference<ShaderProgram>(null);

            ServiceRequest.Invoke(() =>
            {
                var vertex = Shader.Create(sources.Vertex, ShaderType.Vertex);
                var fragment = Shader.Create(sources.Fragment, ShaderType.Fragment);

                program = ShaderProgram.Create(program, vertex.GetValue(), fragment.GetValue());

                vertex.Dispose();
                fragment.Dispose();
            });

            return program;
        }

        public static ShaderSources SplitSources(Stream stream, Func<string, Stream> includeCallback = null)
        {
            StringBuilder builderVertex = new StringBuilder();
            StringBuilder builderFragment = new StringBuilder();
            StringBuilder currentBuilder = null;
            Stack<StreamReader> readersStack = new Stack<StreamReader>(new[] { new StreamReader(stream) });
            int lineNumber = 0;
            while (readersStack.TryPeek(out var reader))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    bool appendLineNumber = false;
                    lineNumber++;
                    switch (line)
                    {
                        case string s when s.StartsWith("#include"):
                            string include = Regex.Match(s, "^#include\\s+\\\"(.+?)\\\"$").Groups[1].Value;
                            stream = includeCallback(include);
                            readersStack.Push(new StreamReader(stream));
                            goto jump_reader; // we need to break while
                        case string s when s.StartsWith("#type"):
                            string type = Regex.Match(s, "^#type\\s+(.+?)$").Groups[1].Value;
                            switch (type)
                            {
                                case "vertex": currentBuilder = builderVertex; break;
                                case "fragment": currentBuilder = builderFragment; break;
                                case "geometry": throw new NotImplementedException();
                                default: Debug.Assert(false, "unknown shader type encountered while preprocessing shader"); break;
                            }
                            continue;
                        case string s when s.StartsWith("#version"):
                            appendLineNumber = true;
                            break;
                    }

                    currentBuilder.AppendLine(line);

                    if (appendLineNumber)
                        currentBuilder.AppendLine($"#line {lineNumber}");
                }
                
                if (reader.EndOfStream)
                    readersStack.Pop().Close();

                jump_reader:;
            }

#if SHOW_SOURCES
            _log.Trace($"vertex:\n{builderVertex.ToString()}");
            _log.Trace($"fragment:\n{builderFragment.ToString()}");
#endif

            return new ShaderSources() { Fragment = builderFragment.ToString(), Vertex = builderVertex.ToString() };
        }

    }
}
