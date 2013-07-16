using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenTK.Graphics.OpenGL;
using SphFluid.Properties;

namespace SphFluid.Core.Shaders
{
    public class Shader
        : IReleasable
    {
        private readonly List<int> _shaders;

        /// <summary>
        /// Shader program handle
        /// </summary>
        public int Program { get; private set; }

        private Shader()
        {
            _shaders = new List<int>();
        }

        protected Shader(string vertexShader)
            : this()
        {
            CreateProgram(new Dictionary<ShaderType, string>
            {
                { ShaderType.VertexShader, vertexShader }
            });
        }

        protected Shader(string vertexShader, string fragmentShader)
            : this()
        {
            CreateProgram(new Dictionary<ShaderType, string>
            {
                { ShaderType.VertexShader, vertexShader },
                { ShaderType.FragmentShader, fragmentShader }
            });
        }

        protected Shader(string vertexShader, string geometryShader, string fragmentShader)
            : this()
        {
            CreateProgram(new Dictionary<ShaderType, string>
            {
                { ShaderType.VertexShader, vertexShader },
                { ShaderType.GeometryShader, geometryShader },
                { ShaderType.FragmentShader, fragmentShader }
            });
        }

        private void CreateProgram(Dictionary<ShaderType, string> shaders)
        {
            Trace.TraceInformation("Creating shader program:");
            // create program
            Program = GL.CreateProgram();
            // load and attach all specified shaders
            foreach (var pair in shaders)
            {
                AttachShader(pair.Key, pair.Value);
            }
            // fire event
            ShaderCompiled();
            // link program
            GL.LinkProgram(Program);
            // assert that no link errors occured
            int linkStatus;
            GL.GetProgram(Program, ProgramParameter.LinkStatus, out linkStatus);
            var info = GL.GetProgramInfoLog(Program);
            Utility.Assert(() => linkStatus, 1, string.Format("Error linking program: {0}", info));
        }

        private void AttachShader(ShaderType type, string name)
        {
            Trace.TraceInformation(name);
            // create shader
            var shader = GL.CreateShader(type);
            // load shaders source
            var filename = Path.Combine(Settings.Default.ShaderDir, name + ".glsl");
            using (var reader = new StreamReader(filename))
            {
                GL.ShaderSource(shader, reader.ReadToEnd());
            }
            // compile shader
            GL.CompileShader(shader);
            // assert that no compile error occured
            int compileStatus;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out compileStatus);
            var info = GL.GetShaderInfoLog(shader);
            Utility.Assert(() => compileStatus, 1, string.Format("Error compiling shader: {0}\n{1}", filename, info));
            // attach shader to program
            GL.AttachShader(Program, shader);
            // remember shader to be able to properly release it
            _shaders.Add(shader);
        }

        protected virtual void ShaderCompiled() { }

        protected ShaderUniform<T> GetUniform<T>(string name, Action<int, T> setter)
        {
            var location = GL.GetUniformLocation(Program, name);
            if (location == -1) Trace.TraceWarning(string.Format("Uniform not found or not active: {0}", name));
            return new ShaderUniform<T>(location, setter);
        }

        public void Use()
        {
            GL.UseProgram(Program);
        }

        public void Release()
        {
            GL.DeleteProgram(Program);
            foreach (var shader in _shaders)
            {
                GL.DeleteShader(shader);
            }
        }
    }
}