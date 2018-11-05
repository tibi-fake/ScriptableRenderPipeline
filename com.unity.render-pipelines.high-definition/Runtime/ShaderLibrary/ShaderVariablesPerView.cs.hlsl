//
// This file was automatically generated. Please don't edit by hand.
//

#ifndef SHADERVARIABLESPERVIEW_CS_HLSL
#define SHADERVARIABLESPERVIEW_CS_HLSL
//
// UnityEngine.Experimental.Rendering.HDPipeline.ShaderVariablesPerView:  static fields
//
#define DEFAULT_LIGHT_LAYERS (255)

// Generated from UnityEngine.Experimental.Rendering.HDPipeline.ShaderVariablesPerView
// PackingRules = Exact
    float4x4 glstate_matrix_projection;
    float4x4 unity_MatrixV;
    float4x4 unity_MatrixInvV;
    float4x4 unity_MatrixVP;
    float4x4 _ViewMatrix;
    float4x4 _InvViewMatrix;
    float4x4 _ProjMatrix;
    float4x4 _InvProjMatrix;
    float4x4 _ViewProjMatrix;
    float4x4 _InvViewProjMatrix;
    float4x4 _NonJitteredViewProjMatrix;
    float4x4 _PrevViewProjMatrix;
    float4 _TextureWidthScaling;
    float3 _WorldSpaceCameraPos;
    int _Pad;
    float4 _ScreenSize;
    float4 _ScreenToTargetScale;
    float4 _ZBufferParams;
    float4 _ProjectionParams;
    float4 unity_OrthoParams;
    float4 _ScreenParams;
    float4 _FrustumPlanes[6];
    float4 _TaaFrameRotation;
    float4 _Time;
    float4 _LastTime;
    float4 _SinTime;
    float4 _CosTime;
    float4 unity_DeltaTime;
    int _FrameCount;
    uint _EnableLightLayers;


#endif
