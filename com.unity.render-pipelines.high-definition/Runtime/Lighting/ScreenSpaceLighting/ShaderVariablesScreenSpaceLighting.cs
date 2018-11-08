
namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [GenerateHLSL(needAccessors = false, omitStructDeclaration = true)]
    public struct ShaderVariablesScreenSpaceLighting
    {
        // Things previously here (Color/DepthPyramidSize/Scale, AmbientOcclusionParam) are in PerCamera struct now
        public Vector4  _CameraMotionVectorsSize;       // (x,y) = Actual Pixel Size, (z,w) = 1 / Actual Pixel Size
        public Vector4  _CameraMotionVectorsScale;      // (x,y) = Screen Scale, z = lod count, w = unused

        public Vector4 _IndirectLightingMultiplier; // .x indirect diffuse multiplier (use with indirect lighting volume controler)

        // Screen space refraction
        public float   _SSRefractionInvScreenWeightDistance; // Distance for screen space smoothstep with fallback

        public int _SSLPad1, _SSLPad2, _SSLPad3;
    }
}

