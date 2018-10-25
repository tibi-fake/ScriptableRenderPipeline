using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    partial class InfluenceVolumeUI : IUpdateable<SerializedInfluenceVolume>
    {
        [Flags]
        internal enum Flag
        {
            None = 0,
            SectionExpandedShape = 1 << 0,
            SectionExpandedShapeSphere = 1 << 1,
            SectionExpandedShapeBox = 1 << 2,
            ShowInfluenceHandle = 1 << 3
        }

        EditorPrefBoolFlags<Flag> m_FlagStorage = new EditorPrefBoolFlags<Flag>("InfluenceVolumeUI");

        public HierarchicalBox boxBaseHandle;
        public HierarchicalBox boxInfluenceHandle;
        public HierarchicalBox boxInfluenceNormalHandle;

        public HierarchicalSphere sphereBaseHandle;
        public HierarchicalSphere sphereInfluenceHandle;
        public HierarchicalSphere sphereInfluenceNormalHandle;

        public bool HasFlag(Flag v) => m_FlagStorage.HasFlag(v);
        public bool SetFlag(Flag f, bool v) => m_FlagStorage.SetFlag(f, v);

        public InfluenceVolumeUI()
        {
            boxBaseHandle = new HierarchicalBox(
                HDReflectionProbeEditor.k_GizmoThemeColorExtent, HDReflectionProbeEditor.k_HandlesColor
            );
            boxInfluenceHandle = new HierarchicalBox(
                HDReflectionProbeEditor.k_GizmoThemeColorInfluenceBlend,
                HDReflectionProbeEditor.k_HandlesColor, parent: boxBaseHandle
            );
            boxInfluenceNormalHandle = new HierarchicalBox(
                HDReflectionProbeEditor.k_GizmoThemeColorInfluenceNormalBlend,
                HDReflectionProbeEditor.k_HandlesColor, parent: boxBaseHandle
            );

            sphereBaseHandle = new HierarchicalSphere(HDReflectionProbeEditor.k_GizmoThemeColorExtent);
            sphereInfluenceHandle = new HierarchicalSphere(
                HDReflectionProbeEditor.k_GizmoThemeColorInfluenceBlend, parent: sphereBaseHandle
            );
            sphereInfluenceNormalHandle = new HierarchicalSphere(
                HDReflectionProbeEditor.k_GizmoThemeColorInfluenceNormalBlend, parent: sphereBaseHandle
            );
        }

        public void Update(SerializedInfluenceVolume v)
        {
            m_FlagStorage.SetFlag(Flag.SectionExpandedShapeBox | Flag.SectionExpandedShapeSphere, false);
            switch ((InfluenceShape)v.shape.intValue)
            {
                case InfluenceShape.Box: m_FlagStorage.SetFlag(Flag.SectionExpandedShapeBox, true); break;
                case InfluenceShape.Sphere: m_FlagStorage.SetFlag(Flag.SectionExpandedShapeSphere, true); break;
            }
        }
    }
}
