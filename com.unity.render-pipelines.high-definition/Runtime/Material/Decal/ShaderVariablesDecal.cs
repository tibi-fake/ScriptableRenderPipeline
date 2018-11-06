
namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [GenerateHLSL(needAccessors = false, omitStructDeclaration = true)]
    public struct ShaderVariablesDecal
    {
        public Vector2  _DecalAtlasResolution;
        public uint    _EnableDecals;

        public uint _DecalPad; // Need padding to be float4-aligned
        // Decal count moved to light loop struct
    }
}

