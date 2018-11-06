using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    public static class CBAccessor<T>
    {
        public delegate ref T Accessor();

        public static void Set(Accessor accessor, T newVal, ref bool dirtyFlag)
        {
            var oldVal = accessor();
            // Dirty tracking, skips compare when already dirty
            if (!dirtyFlag && EqualityComparer<T>.Default.Equals(newVal, oldVal)) return;
            dirtyFlag = true;
            accessor() = newVal;
        }
    }

    public class GPUConstantBuffer : IDisposable
    {
        protected bool IsDirty = true;
        private ComputeBuffer m_GPUBuffer = null;
        private int m_ConstantBufferNameID;
        private int m_ConstantBufferSize;

        public GPUConstantBuffer(int cbName, int cbSize, string name)
        {
            m_ConstantBufferNameID = cbName;
            m_ConstantBufferSize = cbSize;
            if (SystemInfo.supportsSetConstantBuffer)
            {
                m_GPUBuffer = new ComputeBuffer(1, cbSize, ComputeBufferType.Constant, ComputeBufferMode.Immutable);
                m_GPUBuffer.name = name;
            }
        }

        protected void UploadAndBind<T>(CommandBuffer cmd, ref T data)
        {
            if (m_GPUBuffer == null)
                return;

            if (IsDirty)
            {
                m_GPUBuffer.SetData(new T[] {data});
                IsDirty = false;
            }

            cmd.SetGlobalConstantBuffer(m_GPUBuffer, m_ConstantBufferNameID, 0, m_ConstantBufferSize);
        }

        public void Dispose()
        {
            Shader.SetGlobalConstantBuffer(m_ConstantBufferNameID, null, 0, 0);
            m_GPUBuffer?.Dispose();
        }
    }

    public class HDGlobalsConstantBuffer: GPUConstantBuffer
    {
        private struct GlobalsCB
        {
            public ShaderVariablesDecal m_Decal;
        }

        private GlobalsCB m_CB;

        public Vector2 _DecalAtlasResolution { set { CBAccessor<Vector2>.Set(() => ref m_CB.m_Decal._DecalAtlasResolution, value, ref IsDirty); } }
        public uint _EnableDecals { set { CBAccessor<uint>.Set(() => ref m_CB.m_Decal._EnableDecals, value, ref IsDirty); } }
//        public uint _DecalCount { set { CBAccessor<uint>.Set(() => ref m_CB.m_Decal._DecalCount, value, ref IsDirty); } }

        public HDGlobalsConstantBuffer() : base(HDShaderIDs.HDRPGlobals, Marshal.SizeOf<GlobalsCB>(), "HDRPGlobals")
        {
        }

        public void UploadAndBind(CommandBuffer cmd)
        {
            UploadAndBind(cmd, ref m_CB);
        }
    }
}
