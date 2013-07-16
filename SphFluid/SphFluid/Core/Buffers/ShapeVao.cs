﻿using System;
using OpenTK.Graphics.OpenGL;
using SphFluid.Core.Shapes;

namespace SphFluid.Core.Buffers
{
    public class ShapeVao
        : Vao
    {
        private readonly Vbo _vertexBuffer;

        protected ShapeVao(Shape shape, BeginMode mode, int drawCount)
            : base(mode, drawCount)
        {
            GL.BindVertexArray(VaoHandle);
            // create vertex buffer
            GL.EnableClientState(ArrayCap.VertexArray);
            _vertexBuffer = new Vbo();
            _vertexBuffer.UploadData(BufferTarget.ArrayBuffer, shape.Vertices, 3 * sizeof(float));
            GL.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);
            // unbind vertex array object
            GL.BindVertexArray(0);
        }

        public ShapeVao(Shape shape, BeginMode mode)
            : this(shape, mode, shape.Vertices.Length) { }

        public override void Release()
        {
            base.Release();
            _vertexBuffer.Release();
        }

        public override void Render()
        {
            GL.BindVertexArray(VaoHandle);
            GL.DrawArrays(Mode, 0, DrawCount);
            GL.BindVertexArray(0);
        }

        public override void RenderInstanced(int instances)
        {
            GL.BindVertexArray(VaoHandle);
            GL.DrawArraysInstanced(Mode, 0, DrawCount, instances);
            GL.BindVertexArray(0);
        }
    }
}