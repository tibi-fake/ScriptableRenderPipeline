
namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [GenerateHLSL(needAccessors = false, omitStructDeclaration = true)]
    public struct ShaderVariablesScreenSpaceLightingPerCamera
    {
        // Buffer pyramid
        public Vector4  _ColorPyramidSize;              // (x,y) = Actual Pixel Size, (z,w) = 1 / Actual Pixel Size
        public Vector4  _DepthPyramidSize;              // (x,y) = Actual Pixel Size, (z,w) = 1 / Actual Pixel Size
        public Vector4  _ColorPyramidScale;             // (x,y) = Screen Scale, z = lod count, w = unused
        public Vector4  _DepthPyramidScale;             // (x,y) = Screen Scale, z = lod count, w = unused

        // Ambient occlusion
        public Vector4 _AmbientOcclusionParam; // xyz occlusion color, w directLightStrenght
    }
}

