using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    public static class CBArrayAccessor
    {
        public static unsafe void Set(float *dst, Vector4[] newValArr, int arrSizeInFloats, ref bool dirtyFlag)
        {
            fixed (Vector4* newVals = newValArr)
            {
                if (!dirtyFlag)
                {
                    // Do memory compare first
                    float* a = dst;
                    float* b = (float*) newVals;
                    for (int i = 0; i < arrSizeInFloats; ++i)
                    {
                        if (*(a++) != *(b++))
                        {
                            dirtyFlag = true;
                            break;
                        }
                    }

                    if (!dirtyFlag)
                        return;
                }

                Buffer.MemoryCopy(newVals, dst, arrSizeInFloats * 4, arrSizeInFloats * 4);
            }
        }

    }

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
            public ShaderVariablesSubsurfaceScattering m_SS;
            public ShaderVariablesDecal m_Decal;
            public ShaderVariablesVolumetricLighting m_Vol;
            public ShaderVariablesScreenSpaceLighting m_SSL;
        }

        private GlobalsCB m_CB;

        // Subsurface scattering
        public uint _EnableSubsurfaceScattering { set { CBAccessor<uint>.Set(() => ref m_CB.m_SS._EnableSubsurfaceScattering, value, ref IsDirty); } }
        public uint _TexturingModeFlags { set { CBAccessor<uint>.Set(() => ref m_CB.m_SS._TexturingModeFlags, value, ref IsDirty); } }
        public uint _TransmissionFlags { set { CBAccessor<uint>.Set(() => ref m_CB.m_SS._TransmissionFlags, value, ref IsDirty); } }

        public unsafe Vector4[] _ThicknessRemaps {
            set
            {
                fixed (float* dst = m_CB.m_SS._ThicknessRemaps)
                {
                    CBArrayAccessor.Set(dst, value,
                        DiffusionProfileConstants.DIFFUSION_PROFILE_COUNT * 4, ref IsDirty);
                }
            }
        }

        public unsafe Vector4[] _ShapeParams {
            set
            {
                fixed (float* dst = m_CB.m_SS._ShapeParams)
                {
                    CBArrayAccessor.Set(dst, value,
                        DiffusionProfileConstants.DIFFUSION_PROFILE_COUNT * 4, ref IsDirty);
                }
            }
        }

        public unsafe Vector4[] _TransmissionTintsAndFresnel0 {
            set
            {
                fixed (float* dst = m_CB.m_SS._TransmissionTintsAndFresnel0)
                {
                    CBArrayAccessor.Set(dst, value,
                        DiffusionProfileConstants.DIFFUSION_PROFILE_COUNT * 4, ref IsDirty);
                }
            }
        }

        public unsafe Vector4[] _WorldScales {
            set
            {
                fixed (float* dst = m_CB.m_SS._WorldScales)
                {
                    CBArrayAccessor.Set(dst, value,
                        DiffusionProfileConstants.DIFFUSION_PROFILE_COUNT * 4, ref IsDirty);
                }
            }
        }

        // Decals
        public Vector2 _DecalAtlasResolution { set { CBAccessor<Vector2>.Set(() => ref m_CB.m_Decal._DecalAtlasResolution, value, ref IsDirty); } }
        public uint _EnableDecals { set { CBAccessor<uint>.Set(() => ref m_CB.m_Decal._EnableDecals, value, ref IsDirty); } }
//        public uint _DecalCount { set { CBAccessor<uint>.Set(() => ref m_CB.m_Decal._DecalCount, value, ref IsDirty); } }

        // Volumetric lighting
        public unsafe Vector4[] _AmbientProbeCoeffs {
            set
            {
                fixed (float* dst = m_CB.m_Vol._AmbientProbeCoeffs)
                {
                    CBArrayAccessor.Set(dst, value,
                        7 * 4, ref IsDirty);
                }
            }
        }

/*    // These are in atmospheric scattering
        public Vector3 _HeightFogBaseScattering { set { CBAccessor<Vector3>.Set(() => ref m_CB.m_Vol._HeightFogBaseScattering, value, ref IsDirty); } }
        public float _HeightFogBaseExtinction { set { CBAccessor<float>.Set(() => ref m_CB.m_Vol._HeightFogBaseExtinction, value, ref IsDirty); } }

        public Vector2 _HeightFogExponents { set { CBAccessor<Vector2>.Set(() => ref m_CB.m_Vol._HeightFogExponents, value, ref IsDirty); } }
        public float _HeightFogBaseHeight { set { CBAccessor<float>.Set(() => ref m_CB.m_Vol._HeightFogBaseHeight, value, ref IsDirty); } }
        public float _GlobalFogAnisotropy { set { CBAccessor<float>.Set(() => ref m_CB.m_Vol._GlobalFogAnisotropy, value, ref IsDirty); } }
*/
        public Vector4 _VBufferResolution { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_Vol._VBufferResolution, value, ref IsDirty); } }
        public Vector4 _VBufferSliceCount { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_Vol._VBufferSliceCount, value, ref IsDirty); } }
        public Vector4 _VBufferUvScaleAndLimit { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_Vol._VBufferUvScaleAndLimit, value, ref IsDirty); } }
        public Vector4 _VBufferDepthEncodingParams { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_Vol._VBufferDepthEncodingParams, value, ref IsDirty); } }
        public Vector4 _VBufferDepthDecodingParams { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_Vol._VBufferDepthDecodingParams, value, ref IsDirty); } }

        public Vector4 _VBufferPrevResolution { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_Vol._VBufferPrevResolution, value, ref IsDirty); } }
        public Vector4 _VBufferPrevSliceCount { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_Vol._VBufferPrevSliceCount, value, ref IsDirty); } }
        public Vector4 _VBufferPrevUvScaleAndLimit { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_Vol._VBufferPrevUvScaleAndLimit, value, ref IsDirty); } }
        public Vector4 _VBufferPrevDepthEncodingParams { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_Vol._VBufferPrevDepthEncodingParams, value, ref IsDirty); } }
        public Vector4 _VBufferPrevDepthDecodingParams { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_Vol._VBufferPrevDepthDecodingParams, value, ref IsDirty); } }

        public float _VBufferMaxLinearDepth { set { CBAccessor<float>.Set(() => ref m_CB.m_Vol._VBufferMaxLinearDepth, value, ref IsDirty); } }
        public int _EnableDistantFog { set { CBAccessor<int>.Set(() => ref m_CB.m_Vol._EnableDistantFog, value, ref IsDirty); } }

        // ScreenSpaceLighting
//        public Vector4 _ColorPyramidSize { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_SSL._ColorPyramidSize, value, ref IsDirty); } }
//        public Vector4 _DepthPyramidSize { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_SSL._DepthPyramidSize, value, ref IsDirty); } }
        public Vector4 _CameraMotionVectorsSize { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_SSL._CameraMotionVectorsSize, value, ref IsDirty); } }
//        public Vector4 _ColorPyramidScale { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_SSL._ColorPyramidScale, value, ref IsDirty); } }
//        public Vector4 _DepthPyramidScale { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_SSL._DepthPyramidScale, value, ref IsDirty); } }
        public Vector4 _CameraMotionVectorsScale { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_SSL._CameraMotionVectorsScale, value, ref IsDirty); } }

//        public Vector4 _AmbientOcclusionParam { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_SSL._AmbientOcclusionParam, value, ref IsDirty); } }
        public Vector4 _IndirectLightingMultiplier { set { CBAccessor<Vector4>.Set(() => ref m_CB.m_SSL._IndirectLightingMultiplier, value, ref IsDirty); } }
        public float _SSRefractionInvScreenWeightDistance { set { CBAccessor<float>.Set(() => ref m_CB.m_SSL._SSRefractionInvScreenWeightDistance, value, ref IsDirty); } }


        public HDGlobalsConstantBuffer() : base(HDShaderIDs.HDRPGlobals, Marshal.SizeOf<GlobalsCB>(), "HDRPGlobals")
        {
        }

        public void UploadAndBind(CommandBuffer cmd)
        {
            UploadAndBind(cmd, ref m_CB);
        }
    }
}
