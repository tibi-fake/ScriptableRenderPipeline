
namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [GenerateHLSL(needAccessors = false, omitStructDeclaration = true)]
    public unsafe struct ShaderVariablesVolumetricLighting
    {
        // Volumetric lighting.
        [HLSLArray(7, typeof(Vector4))]
        public fixed float _AmbientProbeCoeffs[7 * 4];      // 3 bands of SH, packed, rescaled and convolved with the phase function

        public Vector4 _VBufferResolution;          // { w, h, 1/w, 1/h }
        public Vector4 _VBufferSliceCount;          // { count, 1/count, 0, 0 }
        public Vector4 _VBufferUvScaleAndLimit;     // Necessary us to work with sub-allocation (resource aliasing) in the RTHandle system
        public Vector4 _VBufferDepthEncodingParams; // See the call site for description
        public Vector4 _VBufferDepthDecodingParams; // See the call site for description

        // TODO: these are only used for reprojection.
        // Once reprojection is performed in a separate pass, we should probably
        // move these to a dedicated CBuffer to avoid polluting the global one.
        public Vector4 _VBufferPrevResolution;
        public Vector4 _VBufferPrevSliceCount;
        public Vector4 _VBufferPrevUvScaleAndLimit;
        public Vector4 _VBufferPrevDepthEncodingParams;
        public Vector4 _VBufferPrevDepthDecodingParams;
        public float  _VBufferMaxLinearDepth;      // The Z coordinate of the middle of the last slice
        public int    _EnableDistantFog;           // bool...

        public int VolPad1, VolPad2;
    }
}

