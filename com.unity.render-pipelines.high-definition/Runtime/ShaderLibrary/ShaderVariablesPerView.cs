namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [GenerateHLSL(needAccessors = false, omitStructDeclaration = true)]
    public unsafe struct ShaderVariablesPerView
    {
        // ================================
        //     PER FRAME CONSTANTS
        // ================================
//    #if !defined(USING_STEREO_MATRICES)
        public Matrix4x4 glstate_matrix_projection;
        public Matrix4x4 unity_MatrixV;
        public Matrix4x4 unity_MatrixInvV;
        public Matrix4x4 unity_MatrixVP;
//        public Vector4 unity_StereoScaleOffset;
//    #endif

        // ================================
        //     PER VIEW CONSTANTS
        // ================================
        // TODO: all affine matrices should be 3x4.
        public Matrix4x4 _ViewMatrix;
        public Matrix4x4 _InvViewMatrix;
        public Matrix4x4 _ProjMatrix;
        public Matrix4x4 _InvProjMatrix;
        public Matrix4x4 _ViewProjMatrix;
        public Matrix4x4 _InvViewProjMatrix;
        public Matrix4x4 _NonJitteredViewProjMatrix;
        public Matrix4x4 _PrevViewProjMatrix;       // non-jittered

        public Vector4 _TextureWidthScaling; // 0.5 for SinglePassDoubleWide (stereo) and 1.0 otherwise

        // TODO: put commonly used vars together (below), and then sort them by the frequency of use (descending).
        // Note: a matrix is 4 * 4 * 4 = 64 bytes (1x cache line), so no need to sort those.
//#if defined(USING_STEREO_MATRICES)
//      float3 _Align16;
//#else
        public Vector3 _WorldSpaceCameraPos;

        public int _Pad;
//#endif
        public Vector4 _ScreenSize;                 // { w, h, 1 / w, 1 / h }
        public Vector4 _ScreenToTargetScale;        // { w / RTHandle.maxWidth, h / RTHandle.maxHeight } : xy = currFrame, zw = prevFrame

        // Values used to linearize the Z buffer (http://www.humus.name/temp/Linearize%20depth.txt)
        // x = 1 - f/n
        // y = f/n
        // z = 1/f - 1/n
        // w = 1/n
        // or in case of a reversed depth buffer (UNITY_REVERSED_Z is 1)
        // x = -1 + f/n
        // y = 1
        // z = -1/n + -1/f
        // w = 1/f
        public Vector4 _ZBufferParams;

        // x = 1 or -1 (-1 if projection is flipped)
        // y = near plane
        // z = far plane
        // w = 1/far plane
        public Vector4 _ProjectionParams;

        // x = orthographic camera's width
        // y = orthographic camera's height
        // z = unused
        // w = 1.0 if camera is ortho, 0.0 if perspective
        public Vector4 unity_OrthoParams;

        // x = width
        // y = height
        // z = 1 + 1.0/width
        // w = 1 + 1.0/height
        public Vector4 _ScreenParams;

        [HLSLArray(6, typeof(Vector4))]
        public fixed float _FrustumPlanes[6 * 4];           // { (a, b, c) = N, d = -dot(N, P) } [L, R, T, B, N, F]

        // TAA Frame Index ranges from 0 to 7. This gives you two rotations per cycle.
        public Vector4 _TaaFrameRotation;           // { sin(taaFrame * PI/2), cos(taaFrame * PI/2), 0, 0 }
        // t = animateMaterials ? Time.realtimeSinceStartup : 0.
        public Vector4 _Time;                       // { t/20, t, t*2, t*3 }
        public Vector4 _LastTime;                   // { t/20, t, t*2, t*3 }
        public Vector4 _SinTime;                    // { sin(t/8), sin(t/4), sin(t/2), sin(t) }
        public Vector4 _CosTime;                    // { cos(t/8), cos(t/4), cos(t/2), cos(t) }
        public Vector4 unity_DeltaTime;             // { dt, 1/dt, smoothdt, 1/smoothdt }
        public int _FrameCount;

        public const int DEFAULT_LIGHT_LAYERS = 0xFF;
        public uint _EnableLightLayers;
    }
}
