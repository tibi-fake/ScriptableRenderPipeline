using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityEditor.Experimental.Rendering.LightweightPipeline.ShaderGUI
{
    public static class UnlitGUI
    {
        public static class Styles
        {
            public static GUIContent sampleGILabel = new GUIContent("Global Illumination",
                "If enabled Global Illumination will be sampled from Ambient lighting, Lightprobes or Lightmap.");

        }

        public struct UnlitProperties
        {
            // Surface Input Props
            public MaterialProperty sampleGIProp;
            public MaterialProperty bumpMapProp;

            public UnlitProperties(MaterialProperty[] properties)
            {
                // Surface Input Props
                sampleGIProp = BaseShaderGUI.FindProperty("_SampleGI", properties, false);
                bumpMapProp = BaseShaderGUI.FindProperty("_BumpMap", properties, false);
            }
        }

        public static void Inputs(UnlitProperties properties, MaterialEditor materialEditor)
        {
            BaseShaderGUI.DoNormalArea(materialEditor, properties.bumpMapProp);
        }
        
        public static void Advanced(UnlitProperties properties)
        {
            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUILayout.Toggle(Styles.sampleGILabel, properties.sampleGIProp.floatValue > 0);
            if (EditorGUI.EndChangeCheck())
                properties.sampleGIProp.floatValue = enabled ? 1f : 0f;
        }

        public static void SetMaterialKeywords(Material material)
        {
            bool sampleGI = material.GetFloat("_SampleGI") >= 1.0f;
            bool normalMap = material.GetTexture("_BumpMap");

            CoreUtils.SetKeyword(material, "_SAMPLE_GI", sampleGI && !normalMap);
            CoreUtils.SetKeyword(material, "_SAMPLE_GI_NORMALMAP", sampleGI && normalMap);
        }
    }
}
