using UnityEngine.Serialization;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    public sealed partial class HDAdditionalReflectionData : IVersionable<HDAdditionalReflectionData.Version>
    {
        enum Version
        {
            First,
            RemoveUsageOfLegacyProbeParamsForStocking,
            HDProbeChild,
            UseInfluenceVolume,
            MergeEditors,
            AddCaptureSettingsAndFrameSettings,
            ModeAndTextures,
            ProbeSettings
        }

        static readonly MigrationDescription<Version, HDAdditionalReflectionData> k_Migration
            = MigrationDescription.New(
                MigrationStep.New(Version.RemoveUsageOfLegacyProbeParamsForStocking, (HDAdditionalReflectionData t) =>
                {
#pragma warning disable 618 // Type or member is obsolete
                    t.m_ObsoleteBlendDistancePositive = t.m_ObsoleteBlendDistanceNegative = Vector3.one * t.reflectionProbe.blendDistance;
                    t.weight = t.reflectionProbe.importance;
                    t.multiplier = t.reflectionProbe.intensity;
                    switch (t.reflectionProbe.refreshMode)
                    {
                        case UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame: t.realtimeMode = ProbeSettings.RealtimeMode.EveryFrame; break;
                        case UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake: t.realtimeMode = ProbeSettings.RealtimeMode.OnEnable; break;
                    }
                    // size and center were kept in legacy until Version.UseInfluenceVolume
                    //   and mode until Version.ModeAndTextures
                    //   and all capture settings are done in Version.AddCaptureSettingsAndFrameSettings
#pragma warning restore 618 // Type or member is obsolete
                }),
                MigrationStep.New(Version.UseInfluenceVolume, (HDAdditionalReflectionData t) =>
                {
                    t.influenceVolume.boxSize = t.reflectionProbe.size;
#pragma warning disable 618
                    t.influenceVolume.obsoleteOffset = t.reflectionProbe.center;
                    t.influenceVolume.sphereRadius = t.m_ObsoleteInfluenceSphereRadius;
                    t.influenceVolume.shape = t.m_ObsoleteInfluenceShape; //must be done after each size transfert
                    t.influenceVolume.boxBlendDistancePositive = t.m_ObsoleteBlendDistancePositive;
                    t.influenceVolume.boxBlendDistanceNegative = t.m_ObsoleteBlendDistanceNegative;
                    t.influenceVolume.boxBlendNormalDistancePositive = t.m_ObsoleteBlendNormalDistancePositive;
                    t.influenceVolume.boxBlendNormalDistanceNegative = t.m_ObsoleteBlendNormalDistanceNegative;
                    t.influenceVolume.boxSideFadePositive = t.m_ObsoleteBoxSideFadePositive;
                    t.influenceVolume.boxSideFadeNegative = t.m_ObsoleteBoxSideFadeNegative;
#pragma warning restore 618
                }),
                MigrationStep.New(Version.MergeEditors, (HDAdditionalReflectionData t) =>
                {
                    t.m_ProbeSettings.proxySettings.useInfluenceVolumeAsProxyVolume
                        = t.reflectionProbe.boxProjection;
                    t.reflectionProbe.boxProjection = false;
                }),
                MigrationStep.New(Version.AddCaptureSettingsAndFrameSettings, (HDAdditionalReflectionData t) =>
                {
#pragma warning disable 618
                    t.m_ObsoleteCaptureSettings = t.m_ObsoleteCaptureSettings ?? new ObsoleteCaptureSettings();
                    t.m_ObsoleteCaptureSettings.cullingMask = t.reflectionProbe.cullingMask;
#if UNITY_EDITOR //m_UseOcclusionCulling is not exposed in c# !
                    var serializedReflectionProbe = new UnityEditor.SerializedObject(t.reflectionProbe);
                    t.m_ObsoleteCaptureSettings.useOcclusionCulling = serializedReflectionProbe.FindProperty("m_UseOcclusionCulling").boolValue;
#endif
                    t.m_ObsoleteCaptureSettings.nearClipPlane = t.reflectionProbe.nearClipPlane;
                    t.m_ObsoleteCaptureSettings.farClipPlane = t.reflectionProbe.farClipPlane;
#pragma warning restore 618
                }),
                MigrationStep.New(Version.ModeAndTextures, (HDAdditionalReflectionData t) =>
                {
                    t.mode = (ProbeSettings.Mode)t.reflectionProbe.mode;
                    t.SetTexture(ProbeSettings.Mode.Baked, t.reflectionProbe.bakedTexture);
                    t.SetTexture(ProbeSettings.Mode.Custom, t.reflectionProbe.customBakedTexture);
                }),
                MigrationStep.New(Version.ProbeSettings, (HDAdditionalReflectionData t) =>
                {
#pragma warning disable 618
                    // Migrate capture position
                    // Previously, the capture position of a reflection probe was the position of the game object
                    //   and the center of the influence volume is (transform.position + t.influenceVolume.m_ObsoleteOffset) in world space
                    // Now, the center of the influence volume is the position of the transform and the capture position
                    //   is t.probeSettings.proxySettings.capturePositionProxySpace and is in capture space

                    var capturePositionWS = t.transform.position;
                    // set the transform position to the influence position world space
                    t.transform.position += t.influenceVolume.obsoleteOffset;

                    var capturePositionPS = t.proxyToWorld.inverse * capturePositionWS;
                    t.m_ProbeSettings.proxySettings.capturePositionProxySpace = capturePositionPS;
                    t.m_ProbeSettings.proxySettings.mirrorPositionProxySpace = capturePositionPS;

                    // Migrating Capture Settings
                    t.m_ProbeSettings.camera.bufferClearing.clearColorMode = t.m_ObsoleteCaptureSettings.clearColorMode;
                    t.m_ProbeSettings.camera.bufferClearing.clearDepth = t.m_ObsoleteCaptureSettings.clearDepth;
                    t.m_ProbeSettings.camera.culling.cullingMask = t.m_ObsoleteCaptureSettings.cullingMask;
                    t.m_ProbeSettings.camera.culling.useOcclusionCulling = t.m_ObsoleteCaptureSettings.useOcclusionCulling;
                    t.m_ProbeSettings.camera.frustum.nearClipPlane = t.m_ObsoleteCaptureSettings.nearClipPlane;
                    t.m_ProbeSettings.camera.frustum.farClipPlane = t.m_ObsoleteCaptureSettings.farClipPlane;
                    t.m_ProbeSettings.camera.volumes.layerMask = t.m_ObsoleteCaptureSettings.volumeLayerMask;
                    t.m_ProbeSettings.camera.volumes.anchorOverride = t.m_ObsoleteCaptureSettings.volumeAnchorOverride;
                    t.m_ProbeSettings.camera.frustum.fieldOfView = t.m_ObsoleteCaptureSettings.fieldOfView;
                    t.m_ProbeSettings.camera.renderingPath = t.m_ObsoleteCaptureSettings.renderingPath;
#pragma warning restore 618
                })
            );

        [SerializeField, FormerlySerializedAs("version"), FormerlySerializedAs("m_Version")]
        int m_ReflectionProbeVersion;
        Version IVersionable<Version>.version { get => (Version)m_ReflectionProbeVersion; set => m_ReflectionProbeVersion = (int)value; }

        #region Deprecated Fields
#pragma warning disable 649 //never assigned
        //data only kept for migration, to be removed in future version
        [SerializeField, FormerlySerializedAs("influenceShape"), System.Obsolete("influenceShape is deprecated, use influenceVolume parameters instead")]
        InfluenceShape m_ObsoleteInfluenceShape;
        [SerializeField, FormerlySerializedAs("influenceSphereRadius"), System.Obsolete("influenceSphereRadius is deprecated, use influenceVolume parameters instead")]
        float m_ObsoleteInfluenceSphereRadius = 3.0f;
        [SerializeField, FormerlySerializedAs("blendDistancePositive"), System.Obsolete("blendDistancePositive is deprecated, use influenceVolume parameters instead")]
        Vector3 m_ObsoleteBlendDistancePositive = Vector3.zero;
        [SerializeField, FormerlySerializedAs("blendDistanceNegative"), System.Obsolete("blendDistanceNegative is deprecated, use influenceVolume parameters instead")]
        Vector3 m_ObsoleteBlendDistanceNegative = Vector3.zero;
        [SerializeField, FormerlySerializedAs("blendNormalDistancePositive"), System.Obsolete("blendNormalDistancePositive is deprecated, use influenceVolume parameters instead")]
        Vector3 m_ObsoleteBlendNormalDistancePositive = Vector3.zero;
        [SerializeField, FormerlySerializedAs("blendNormalDistanceNegative"), System.Obsolete("blendNormalDistanceNegative is deprecated, use influenceVolume parameters instead")]
        Vector3 m_ObsoleteBlendNormalDistanceNegative = Vector3.zero;
        [SerializeField, FormerlySerializedAs("boxSideFadePositive"), System.Obsolete("boxSideFadePositive is deprecated, use influenceVolume parameters instead")]
        Vector3 m_ObsoleteBoxSideFadePositive = Vector3.one;
        [SerializeField, FormerlySerializedAs("boxSideFadeNegative"), System.Obsolete("boxSideFadeNegative is deprecated, use influenceVolume parameters instead")]
        Vector3 m_ObsoleteBoxSideFadeNegative = Vector3.one;
        #pragma warning restore 649 //never assigned
        #endregion
    }
}
