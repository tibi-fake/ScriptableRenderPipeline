using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityEditor.Experimental.Rendering.LightweightPipeline
{
    internal class ParticlesLitShaderGUI : BaseShaderGUI
    {
/*        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent, // Physically plausible transparency mode, implemented as alpha pre-multiply
            Additive,
            Subtractive,
            Modulate
        }*/

        public enum FlipbookMode
        {
            Simple,
            Blended
        }

        public enum ColorMode
        {
            Multiply,
            Additive,
            Subtractive,
            Overlay,
            Color,
            Difference
        }

        private static class StylesParticle
        {
            public static GUIContent albedoText = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A).");
            public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff.");
            public static GUIContent metallicMapText = new GUIContent("Metallic", "Metallic (R) and Smoothness (A).");
            public static GUIContent smoothnessText = new GUIContent("Smoothness", "Smoothness value.");
            public static GUIContent smoothnessScaleText = new GUIContent("Smoothness", "Smoothness scale factor.");
            public static GUIContent normalMapText = new GUIContent("Normal Map", "Normal Map.");
            public static GUIContent emissionText = new GUIContent("Color", "Emission (RGB).");

            public static GUIContent renderingMode = new GUIContent("Rendering Mode", "Determines the transparency and blending method for drawing the object to the screen.");
            public static GUIContent[] blendNames = Array.ConvertAll(Enum.GetNames(typeof(BlendMode)), item => new GUIContent(item));

            public static GUIContent colorMode = new GUIContent("Color Mode", "Determines the blending mode between the particle color and the texture albedo.");
            //public static GUIContent[] colorNames = Array.ConvertAll(Enum.GetNames(typeof(ColorMode)), item => new GUIContent(item));

            public static GUIContent flipbookMode = new GUIContent("Flip-Book Mode", "Determines the blending mode used for animated texture sheets.");
            public static GUIContent[] flipbookNames = Array.ConvertAll(Enum.GetNames(typeof(FlipbookMode)), item => new GUIContent(item));

            public static GUIContent twoSidedEnabled = new GUIContent("Two Sided", "Render both front and back faces of the particle geometry.");

            public static GUIContent softParticlesEnabled = new GUIContent("Enable Soft Particles", "Fade out particle geometry when it gets close to the surface of objects written into the depth buffer.");
            public static GUIContent softParticlesNearFadeDistanceText = new GUIContent("Near fade", "Soft Particles near fade distance.");
            public static GUIContent softParticlesFarFadeDistanceText = new GUIContent("Far fade", "Soft Particles far fade distance.");

            public static GUIContent cameraFadingEnabled = new GUIContent("Enable Camera Fading", "Fade out particle geometry when it gets close to the camera.");
            public static GUIContent cameraNearFadeDistanceText = new GUIContent("Near fade", "Camera near fade distance.");
            public static GUIContent cameraFarFadeDistanceText = new GUIContent("Far fade", "Camera far fade distance.");
            
            public static GUIContent distortionEnabled = new GUIContent("Distortion", "This makes use of the 'CameraOpaque' texture from teh pipeline to give the ability yo distort the background pixels");
            public static GUIContent distortionStrength = new GUIContent("Strength", "How much the normal map affects the warping of the background pixels.");
            public static GUIContent distortionBlend = new GUIContent("Blend", "Weighting between the Base color of the particle and the backround color.");

            public static GUIContent emissionEnabled = new GUIContent("Emission");

            public static string blendingOptionsText = "Blending Options";
            public static string mainOptionsText = "Main Options";
            public static string mapsOptionsText = "Maps";
            public static string advancedOptionsText = "Advanced Options";
            public static GUIContent VertexStreams = new GUIContent("Vertex Streams");

            public static string streamPositionText = "Position (POSITION.xyz)";
            public static string streamNormalText = "Normal (NORMAL.xyz)";
            public static string streamColorText = "Color (COLOR.xyzw)";
            public static string streamUVText = "UV (TEXCOORD0.xy)";
            public static string streamUV2Text = "UV2 (TEXCOORD0.zw)";
            public static string streamAnimBlendText = "AnimBlend (TEXCOORD1.x)";
            public static string streamTangentText = "Tangent (TANGENT.xyzw)";

            public static GUIContent streamApplyToAllSystemsText = new GUIContent("Apply to Systems", "Apply the vertex stream layout to all Particle Systems using this material");
        }

        MaterialProperty colorMode;
        MaterialProperty flipbookMode;
        MaterialProperty metallicMap;
        MaterialProperty metallic;
        MaterialProperty smoothness;
        MaterialProperty bumpScale;
        MaterialProperty bumpMapProp;
        MaterialProperty softParticlesEnabled;
        MaterialProperty cameraFadingEnabled;
        MaterialProperty distortionEnabled;
        MaterialProperty softParticlesNearFadeDistance;
        MaterialProperty softParticlesFarFadeDistance;
        MaterialProperty cameraNearFadeDistance;
        MaterialProperty cameraFarFadeDistance;
        private MaterialProperty distortionBlend;
        private MaterialProperty distortionStrength;

        List<ParticleSystemRenderer> m_RenderersUsingThisMaterial = new List<ParticleSystemRenderer>();

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            
            colorMode = FindProperty("_ColorMode", properties, false);
            flipbookMode = FindProperty("_FlipbookMode", properties);
            metallicMap = FindProperty("_MetallicGlossMap", properties, false);
            metallic = FindProperty("_Metallic", properties, false);
            smoothness = FindProperty("_Smoothness", properties, false);
            bumpScale = FindProperty("_BumpScale", properties, false);
            bumpMapProp = FindProperty("_BumpMap", properties, false);
            softParticlesEnabled = FindProperty("_SoftParticlesEnabled", properties);
            cameraFadingEnabled = FindProperty("_CameraFadingEnabled", properties);
            distortionEnabled = FindProperty("_DistortionEnabled", properties, false);
            softParticlesNearFadeDistance = FindProperty("_SoftParticlesNearFadeDistance", properties);
            softParticlesFarFadeDistance = FindProperty("_SoftParticlesFarFadeDistance", properties);
            cameraNearFadeDistance = FindProperty("_CameraNearFadeDistance", properties);
            cameraFarFadeDistance = FindProperty("_CameraFarFadeDistance", properties);
            distortionBlend = FindProperty("_DistortionBlend", properties, false);
            distortionStrength = FindProperty("_DistortionStrength", properties, false);           
        }
        
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");
            
            material.shaderKeywords = null;
            //SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Blend"));
            if (colorMode != null)
                SetupMaterialWithColorMode(material);
            SetupMaterialBlendMode(material);
            SetMaterialKeywords(material);
        }
        
        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                base.DrawSurfaceOptions(material);
                DoPopup(StylesParticle.colorMode, colorMode, Enum.GetNames(typeof(ColorMode)));
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendModeProp.targets)
                    MaterialChanged((Material)obj);
            }
        }
        
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            
            EditorGUI.BeginChangeCheck();
            {
                materialEditor.TexturePropertySingleLine(StylesParticle.normalMapText, bumpMapProp);

                DrawEmissionProperties(material, true);

                DrawBaseTileOffset();
                EditorGUI.BeginChangeCheck();
            }
        }

        public override void DrawAdditionalFoldouts(Material material)
        {
            var vertexStreams = EditorGUILayout.BeginFoldoutHeaderGroup(GetHeaderState(3), StylesParticle.VertexStreams, EditorStyles.foldoutHeader, null, new GUIStyle());
            if (vertexStreams)
            {
                DoVertexStreamsArea(material);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            StoreHeaderState(vertexStreams, 3);
        }

/*        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            if (materialEditor == null)
                throw new ArgumentNullException("materialEditor");

            FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;

            if (m_FirstTimeApply)
            {
                MaterialChanged(material);
                CacheRenderersUsingThisMaterial(material);
                m_FirstTimeApply = false;
            }

            ShaderPropertiesGUI(material);
        }*/

/*        public void ShaderPropertiesGUI(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.Label(Styles.blendingOptionsText, EditorStyles.boldLabel);

                BlendModePopup();
                ColorModePopup();

                EditorGUILayout.Space();
                GUILayout.Label(Styles.mainOptionsText, EditorStyles.boldLabel);

                FlipbookModePopup();
                TwoSidedPopup(material);
                FadingPopup(material);

                if (distortionEnabled != null)
                {
                    EditorGUI.BeginChangeCheck();
                    var enabled = distortionEnabled.floatValue;
                    enabled = EditorGUILayout.Toggle(Styles.distortionEnabled, enabled != 0.0f) ? 1.0f : 0.0f;
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_MaterialEditor.RegisterPropertyChangeUndo("Distortion Enabled");
                        distortionEnabled.floatValue = enabled;
                    }
                    
                    if (enabled != 0.0f)
                    {
                        int indentation = 2;
                        m_MaterialEditor.ShaderProperty(distortionStrength, Styles.distortionStrength, indentation);
                        m_MaterialEditor.ShaderProperty(distortionBlend, Styles.distortionBlend, indentation);
                    }
                }

                EditorGUILayout.Space();
                GUILayout.Label(Styles.mapsOptionsText, EditorStyles.boldLabel);

                DoAlbedoArea(material);
                DoSpecularMetallicArea(material);
                DoNormalMapArea(material);
                DoEmissionArea(material);

                if (!flipbookMode.hasMixedValue && (FlipbookMode)flipbookMode.floatValue != FlipbookMode.Blended)
                {
                    EditorGUI.BeginChangeCheck();
                    m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
                    if (EditorGUI.EndChangeCheck())
                        emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset; // Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendMode.targets)
                    MaterialChanged((Material)obj);
            }

            EditorGUILayout.Space();

            GUILayout.Label(Styles.requiredVertexStreamsText, EditorStyles.boldLabel);
            DoVertexStreamsArea(material);
        }*/

        /*public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            if (material == null)
                throw new ArgumentNullException("oldShader");

            if (newShader == null)
                throw new ArgumentNullException("newShader");

            // Sync the lighting flag for the unlit shader
/*            if (newShader.name.Contains("Unlit"))
                material.SetFloat("_LightingEnabled", 0.0f);
            else
                material.SetFloat("_LightingEnabled", 1.0f);#1#

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Blend"));
                return;
            }

            BlendMode blendMode = BlendMode.Opaque;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                blendMode = BlendMode.Cutout;
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                blendMode = BlendMode.Fade;
            }
            material.SetFloat("_Blend", (float)blendMode);

            MaterialChanged(material);
        }*/

/*        void BlendModePopup()
        {
            EditorGUI.showMixedValue = blendMode.hasMixedValue;
            var mode = (BlendMode)blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (BlendMode)EditorGUILayout.Popup(StylesParticle.renderingMode, (int)mode, StylesParticle.blendNames);
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                blendMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;
        }*/

/*        void ColorModePopup()
        {
            if (colorMode != null)
            {
                EditorGUI.showMixedValue = colorMode.hasMixedValue;
                var mode = (ColorMode)colorMode.floatValue;

                EditorGUI.BeginChangeCheck();
                mode = (ColorMode)EditorGUILayout.Popup(StylesParticle.colorMode, (int)mode, StylesParticle.colorNames);
                if (EditorGUI.EndChangeCheck())
                {
                    materialEditor.RegisterPropertyChangeUndo("Color Mode");
                    colorMode.floatValue = (float)mode;
                }

                EditorGUI.showMixedValue = false;
            }
        }*/

        void FlipbookModePopup()
        {
            EditorGUI.showMixedValue = flipbookMode.hasMixedValue;
            var mode = (FlipbookMode)flipbookMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (FlipbookMode)EditorGUILayout.Popup(StylesParticle.flipbookMode, (int)mode, StylesParticle.flipbookNames);
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo("Flip-Book Mode");
                flipbookMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;
        }

/*        void TwoSidedPopup(Material material)
        {
            EditorGUI.showMixedValue = cullMode.hasMixedValue;
            var enabled = (cullMode.floatValue == (float)UnityEngine.Rendering.CullMode.Off);

            EditorGUI.BeginChangeCheck();
            enabled = EditorGUILayout.Toggle(StylesParticle.twoSidedEnabled, enabled);
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo("Two Sided Enabled");
                cullMode.floatValue = enabled ? (float)UnityEngine.Rendering.CullMode.Off : (float)UnityEngine.Rendering.CullMode.Back;
            }

            EditorGUI.showMixedValue = false;
        }*/

        void FadingPopup(Material material)
        {
            // Z write doesn't work with fading
            bool hasZWrite = (material.GetInt("_ZWrite") != 0);
            if (!hasZWrite)
            {
                // Soft Particles
                {
                    EditorGUI.showMixedValue = softParticlesEnabled.hasMixedValue;
                    var enabled = softParticlesEnabled.floatValue;

                    EditorGUI.BeginChangeCheck();
                    enabled = EditorGUILayout.Toggle(StylesParticle.softParticlesEnabled, enabled != 0.0f) ? 1.0f : 0.0f;
                    if (EditorGUI.EndChangeCheck())
                    {
                        materialEditor.RegisterPropertyChangeUndo("Soft Particles Enabled");
                        softParticlesEnabled.floatValue = enabled;
                    }

                    if (enabled != 0.0f)
                    {
                        int indentation = 2;
                        materialEditor.ShaderProperty(softParticlesNearFadeDistance, StylesParticle.softParticlesNearFadeDistanceText, indentation);
                        materialEditor.ShaderProperty(softParticlesFarFadeDistance, StylesParticle.softParticlesFarFadeDistanceText, indentation);
                    }
                }

                // Camera Fading
                {
                    EditorGUI.showMixedValue = cameraFadingEnabled.hasMixedValue;
                    var enabled = cameraFadingEnabled.floatValue;

                    EditorGUI.BeginChangeCheck();
                    enabled = EditorGUILayout.Toggle(StylesParticle.cameraFadingEnabled, enabled != 0.0f) ? 1.0f : 0.0f;
                    if (EditorGUI.EndChangeCheck())
                    {
                        materialEditor.RegisterPropertyChangeUndo("Camera Fading Enabled");
                        cameraFadingEnabled.floatValue = enabled;
                    }

                    if (enabled != 0.0f)
                    {
                        int indentation = 2;
                        materialEditor.ShaderProperty(cameraNearFadeDistance, StylesParticle.cameraNearFadeDistanceText, indentation);
                        materialEditor.ShaderProperty(cameraFarFadeDistance, StylesParticle.cameraFarFadeDistanceText, indentation);
                    }
                }

                EditorGUI.showMixedValue = false;
            }
        }

/*        void DoAlbedoArea(Material material)
        {
            m_MaterialEditor.TexturePropertyWithHDRColor(Styles.albedoText, albedoMap, albedoColor, true);
            if (((BlendMode)material.GetFloat("_Blend") == BlendMode.Cutout))
            {
                m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText, MaterialEditor.kMiniTextureFieldLabelIndentLevel);
            }
        }*/

        void DoSpecularMetallicArea(Material material)
        {
            if (metallicMap == null)
                return;


                bool hasGlossMap = metallicMap.textureValue != null;
                materialEditor.TexturePropertySingleLine(StylesParticle.metallicMapText, metallicMap, hasGlossMap ? null : metallic);

                int indentation = 2; // align with labels of texture properties
                bool showSmoothnessScale = hasGlossMap;
                materialEditor.ShaderProperty(smoothness, showSmoothnessScale ? StylesParticle.smoothnessScaleText : StylesParticle.smoothnessText, indentation);
        }

        void DoVertexStreamsArea(Material material)
        {
            // Display list of streams required to make this shader work
            bool useLighting = true;//(material.GetFloat("_LightingEnabled") > 0.0f);
            bool useFlipbookBlending = (material.GetFloat("_FlipbookMode") > 0.0f);
            bool useTangents = material.GetTexture("_BumpMap") && useLighting;

            if (useLighting)
                GUILayout.Label(StylesParticle.streamNormalText, EditorStyles.label);

            GUILayout.Label(StylesParticle.streamColorText, EditorStyles.label);
            GUILayout.Label(StylesParticle.streamUVText, EditorStyles.label);

            if (useFlipbookBlending)
            {
                GUILayout.Label(StylesParticle.streamUV2Text, EditorStyles.label);
                GUILayout.Label(StylesParticle.streamAnimBlendText, EditorStyles.label);
            }

            if (useTangents)
                GUILayout.Label(StylesParticle.streamTangentText, EditorStyles.label);

            // Build the list of expected vertex streams
            List<ParticleSystemVertexStream> streams = new List<ParticleSystemVertexStream>();
            streams.Add(ParticleSystemVertexStream.Position);

            if (useLighting)
                streams.Add(ParticleSystemVertexStream.Normal);

            streams.Add(ParticleSystemVertexStream.Color);
            streams.Add(ParticleSystemVertexStream.UV);

            if (useFlipbookBlending)
            {
                streams.Add(ParticleSystemVertexStream.UV2);
                streams.Add(ParticleSystemVertexStream.AnimBlend);
            }

            if (useTangents)
                streams.Add(ParticleSystemVertexStream.Tangent);

            // Set the streams on all systems using this material
            if (GUILayout.Button(StylesParticle.streamApplyToAllSystemsText, EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
            {
                foreach (ParticleSystemRenderer renderer in m_RenderersUsingThisMaterial)
                {
                    renderer.SetActiveVertexStreams(streams);
                }
            }

            // Display a warning if any renderers have incorrect vertex streams
            string Warnings = "";
            List<ParticleSystemVertexStream> rendererStreams = new List<ParticleSystemVertexStream>();
            foreach (ParticleSystemRenderer renderer in m_RenderersUsingThisMaterial)
            {
                renderer.GetActiveVertexStreams(rendererStreams);
                if (!rendererStreams.SequenceEqual(streams))
                    Warnings += "  " + renderer.name + "\n";
            }
            if (string.IsNullOrEmpty(Warnings))
            {
                EditorGUILayout.HelpBox("The following Particle System Renderers are using this material with incorrect Vertex Streams:\n" + Warnings + "Use the Apply to Systems button to fix this", MessageType.Warning, true);
            }

            EditorGUILayout.Space();
        }

        /*public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Additive:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Subtractive:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.ReverseSubtract);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Modulate:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.EnableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }*/

        public static void SetupMaterialWithColorMode(Material material)
        {
            var colorMode = (ColorMode) material.GetFloat("_ColorMode");
            
            switch (colorMode)
            {
                case ColorMode.Multiply:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.DisableKeyword("_COLORADDSUBDIFF_ON");
                    break;
                case ColorMode.Overlay:
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.DisableKeyword("_COLORADDSUBDIFF_ON");
                    material.EnableKeyword("_COLOROVERLAY_ON");
                    break;
                case ColorMode.Color:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORADDSUBDIFF_ON");
                    material.EnableKeyword("_COLORCOLOR_ON");
                    break;
                case ColorMode.Difference:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.EnableKeyword("_COLORADDSUBDIFF_ON");
                    material.SetVector("_BaseColorAddSubDiff", new Vector4(-1.0f, 1.0f, 0.0f, 0.0f));
                    break;
                case ColorMode.Additive:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.EnableKeyword("_COLORADDSUBDIFF_ON");
                    material.SetVector("_BaseColorAddSubDiff", new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
                    break;
                case ColorMode.Subtractive:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.EnableKeyword("_COLORADDSUBDIFF_ON");
                    material.SetVector("_BaseColorAddSubDiff", new Vector4(-1.0f, 0.0f, 0.0f, 0.0f));
                    break;
            }
        }

        static void SetMaterialKeywords(Material material)
        {
            // Z write doesn't work with distortion/fading
            bool hasZWrite = (material.GetInt("_ZWrite") != 0);

            // Lit shader?
            //bool useLighting = (material.GetFloat("_LightingEnabled") > 0.0f);

            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));
            SetKeyword(material, "_METALLICGLOSSMAP", (material.GetTexture("_MetallicGlossMap") != null));

            // A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
            // or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
            // The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
            MaterialEditor.FixupEmissiveFlag(material);
            bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            CoreUtils.SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);

            // Set the define for flipbook blending
            bool useFlipbookBlending = (material.GetFloat("_FlipbookMode") > 0.0f);
            SetKeyword(material, "_REQUIRE_UV2", useFlipbookBlending);

            // Clamp fade distances
            bool useSoftParticles = (material.GetFloat("_SoftParticlesEnabled") > 0.0f);
            bool useCameraFading = (material.GetFloat("_CameraFadingEnabled") > 0.0f);
            float softParticlesNearFadeDistance = material.GetFloat("_SoftParticlesNearFadeDistance");
            float softParticlesFarFadeDistance = material.GetFloat("_SoftParticlesFarFadeDistance");
            float cameraNearFadeDistance = material.GetFloat("_CameraNearFadeDistance");
            float cameraFarFadeDistance = material.GetFloat("_CameraFarFadeDistance");

            if (softParticlesNearFadeDistance < 0.0f)
            {
                softParticlesNearFadeDistance = 0.0f;
                material.SetFloat("_SoftParticlesNearFadeDistance", 0.0f);
            }
            if (softParticlesFarFadeDistance < 0.0f)
            {
                softParticlesFarFadeDistance = 0.0f;
                material.SetFloat("_SoftParticlesFarFadeDistance", 0.0f);
            }
            if (cameraNearFadeDistance < 0.0f)
            {
                cameraNearFadeDistance = 0.0f;
                material.SetFloat("_CameraNearFadeDistance", 0.0f);
            }
            if (cameraFarFadeDistance < 0.0f)
            {
                cameraFarFadeDistance = 0.0f;
                material.SetFloat("_CameraFarFadeDistance", 0.0f);
            }

            // Set the define for fading
            bool useFading = (useSoftParticles || useCameraFading) && !hasZWrite;
            SetKeyword(material, "_FADING_ON", useFading);
            if (useSoftParticles)
            {
                material.SetVector("_SoftParticleFadeParams",
                    new Vector4(softParticlesNearFadeDistance,
                        1.0f / (softParticlesFarFadeDistance - softParticlesNearFadeDistance), 0.0f, 0.0f));
                material.EnableKeyword("SOFTPARTICLES_ON");
            }
            else
                material.SetVector("_SoftParticleFadeParams", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            if (useCameraFading)
                material.SetVector("_CameraFadeParams", new Vector4(cameraNearFadeDistance, 1.0f / (cameraFarFadeDistance - cameraNearFadeDistance), 0.0f, 0.0f));
            else
                material.SetVector("_CameraFadeParams", new Vector4(0.0f, Mathf.Infinity, 0.0f, 0.0f));

            // distortion
            bool useDistortion = (material.GetFloat("_DistortionEnabled") > 0.0f) && (SurfaceType)material.GetFloat("_Surface") != SurfaceType.Opaque;
            SetKeyword(material, "_DISTORTION_ON", useDistortion);
            if(useDistortion)
                material.SetFloat("_DistortionStrengthScaled", material.GetFloat("_DistortionStrength") * 0.1f);
            
            // Set the define for distortion + grabpass (Distortion not supported)
            material.SetShaderPassEnabled("Always", false);
        }

        void CacheRenderersUsingThisMaterial(Material material)
        {
            m_RenderersUsingThisMaterial.Clear();

            ParticleSystemRenderer[] renderers = UnityEngine.Object.FindObjectsOfType(typeof(ParticleSystemRenderer)) as ParticleSystemRenderer[];
            foreach (ParticleSystemRenderer renderer in renderers)
            {
                if (renderer.sharedMaterial == material)
                    m_RenderersUsingThisMaterial.Add(renderer);
            }
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }
    }
} // namespace UnityEditor
