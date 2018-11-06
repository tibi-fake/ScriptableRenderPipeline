using NUnit.Framework;
using System;
using System.IO;
using UnityEditor;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline.Tests
{
    public partial class HDAdditionalReflectionDataTests
    {
        static string ToYAML(Vector3 v) => $"{{x: {v.x}, y: {v.y}, z: {v.z}}}";

        struct DefaultTest : IDisposable
        {
            string m_GeneratedPrefabFileName;

            public DefaultTest(string id, string YAML, out GameObject instance)
            {
                m_GeneratedPrefabFileName = $"Assets/Temporary/{id}.prefab";

                var fileInfo = new FileInfo(m_GeneratedPrefabFileName);
                if (!fileInfo.Directory.Exists)
                    fileInfo.Directory.Create();
                
                File.WriteAllText(m_GeneratedPrefabFileName, YAML);

                AssetDatabase.ImportAsset(m_GeneratedPrefabFileName);

                instance = AssetDatabase.LoadAssetAtPath<GameObject>(m_GeneratedPrefabFileName);
            }

            public void Dispose() => AssetDatabase.DeleteAsset(m_GeneratedPrefabFileName);
        }

        public class MigrateFromLegacyProbe
        {
            public class LegacyProbeData
            {
                public Vector3 boxOffset;
                public Vector3 capturePositionWS;
                public Vector3 boxSize;
                public float blendDistance;
                public float importance;
                public float intensity;
                public bool boxProjection;
                public int cullingMask;
                public bool useOcclusionCulling;
                public float nearClipPlane;
                public float farClipPlane;
                public int resolution;
                public int mode;
                public int refreshMode;
            }

            static object[] s_LegacyProbeDatas =
            {
                new LegacyProbeData
                {
                    blendDistance = 1.2f,
                    boxOffset = new Vector3(2, 3, 4),
                    boxProjection = true,
                    boxSize = new Vector3(1, 2, 3),
                    capturePositionWS = new Vector3(2, 3, 4),
                    cullingMask = 308,
                    farClipPlane = 850,
                    nearClipPlane = 1.5f,
                    importance = 12,
                    intensity = 1.4f,
                    mode = (int)ReflectionProbeMode.Realtime,
                    refreshMode = (int)ReflectionProbeRefreshMode.EveryFrame,
                    resolution = 256,
                    useOcclusionCulling = false
                }
            };

            [Test, TestCaseSource(nameof(s_LegacyProbeDatas))]
            public void Test(LegacyProbeData legacyProbeData)
            {
                using (new DefaultTest(
                    nameof(MigrateFromLegacyProbe),
                    GeneratePrefabYAML(legacyProbeData),
                    out GameObject instance
                ))
                {
                    var influencePositionWS = legacyProbeData.capturePositionWS + legacyProbeData.boxOffset;
                    var capturePositionPS = legacyProbeData.boxProjection ? -legacyProbeData.boxOffset : Vector3.zero;

                    var probe = instance.GetComponent<HDAdditionalReflectionData>();
                    var settings = probe.settings;
                    Assert.AreEqual(influencePositionWS, probe.transform.position);
                    Assert.AreEqual(capturePositionPS, settings.proxySettings.capturePositionProxySpace);
                    Assert.AreEqual(Vector3.one * legacyProbeData.blendDistance, settings.influence.boxBlendDistancePositive);
                    Assert.AreEqual(Vector3.one * legacyProbeData.blendDistance, settings.influence.boxBlendDistanceNegative);
                    Assert.AreEqual(legacyProbeData.importance, settings.lighting.weight);
                    Assert.AreEqual(legacyProbeData.intensity, settings.lighting.multiplier);
                    Assert.AreEqual(legacyProbeData.boxSize, settings.influence.boxSize);
                    Assert.AreEqual(legacyProbeData.boxProjection, settings.proxySettings.useInfluenceVolumeAsProxyVolume);
                    Assert.AreEqual(legacyProbeData.useOcclusionCulling, settings.camera.culling.useOcclusionCulling);
                    Assert.AreEqual(legacyProbeData.nearClipPlane, settings.camera.frustum.nearClipPlane);
                    Assert.AreEqual(legacyProbeData.farClipPlane, settings.camera.frustum.farClipPlane);
                    Assert.AreEqual(ProbeSettings.ProbeType.ReflectionProbe, settings.type);

                    var targetMode = ProbeSettings.Mode.Baked;
                    switch ((ReflectionProbeMode)legacyProbeData.mode)
                    {
                        case ReflectionProbeMode.Baked: targetMode = ProbeSettings.Mode.Baked; break;
                        case ReflectionProbeMode.Custom: targetMode = ProbeSettings.Mode.Custom; break;
                        case ReflectionProbeMode.Realtime: targetMode = ProbeSettings.Mode.Realtime; break;
                    }
                    Assert.AreEqual(settings.mode, targetMode);

                    var targetRealtimeMode = ProbeSettings.RealtimeMode.EveryFrame;
                    switch ((ReflectionProbeRefreshMode)legacyProbeData.refreshMode)
                    {
                        case ReflectionProbeRefreshMode.EveryFrame:
                        case ReflectionProbeRefreshMode.ViaScripting: targetRealtimeMode = ProbeSettings.RealtimeMode.EveryFrame; break;
                        case ReflectionProbeRefreshMode.OnAwake: targetRealtimeMode = ProbeSettings.RealtimeMode.OnEnable; break;
                    }
                    Assert.AreEqual(settings.realtimeMode, targetRealtimeMode);
                }
            }

            string GeneratePrefabYAML(LegacyProbeData legacyProbeData)
                => $@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &4579176910221717176
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 6741578724909752953}}
  - component: {{fileID: 1787267906489536894}}
  m_Layer: 0
  m_Name: Reflection Probe
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &6741578724909752953
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 4579176910221717176}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {ToYAML(legacyProbeData.capturePositionWS)}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_Children: []
  m_Father: {{fileID: 0}}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!215 &1787267906489536894
ReflectionProbe:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 4579176910221717176}}
  m_Enabled: 1
  serializedVersion: 2
  m_Type: 0
  m_Mode: {legacyProbeData.mode}
  m_RefreshMode: {legacyProbeData.refreshMode}
  m_TimeSlicingMode: 0
  m_Resolution: {legacyProbeData.resolution}
  m_UpdateFrequency: 0
  m_BoxSize: {ToYAML(legacyProbeData.boxSize)}
  m_BoxOffset: {ToYAML(legacyProbeData.boxOffset)}
  m_NearClip: {legacyProbeData.nearClipPlane}
  m_FarClip: {legacyProbeData.farClipPlane}
  m_ShadowDistance: 100
  m_ClearFlags: 1
  m_BackGroundColor: {{r: 0.20, g: 0.30, b: 0.50, a: 0}}
  m_CullingMask:
    serializedVersion: 2
    m_Bits: {legacyProbeData.cullingMask}
  m_IntensityMultiplier: {legacyProbeData.intensity}
  m_BlendDistance: {legacyProbeData.blendDistance}
  m_HDR: 1
  m_BoxProjection: {(legacyProbeData.boxProjection ? 1 : 0)}
  m_RenderDynamicObjects: 0
  m_UseOcclusionCulling: {(legacyProbeData.useOcclusionCulling ? 1 : 0)}
  m_Importance: {legacyProbeData.importance}
  m_CustomBakedTexture: {{fileID: 0}}
";
        }
    }
}
