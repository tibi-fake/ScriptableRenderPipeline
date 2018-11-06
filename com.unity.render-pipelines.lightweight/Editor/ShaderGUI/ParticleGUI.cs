using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System.Linq;
using System.Collections.Generic;

namespace UnityEditor.Experimental.Rendering.LightweightPipeline.ShaderGUI
{
    public static class ParticleGUI
    {
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

        public static class Styles
        {
            public static GUIContent colorMode = new GUIContent("Color Mode",
                "Determines the blending mode between the particle color and the base.");

            public static GUIContent flipbookMode = new GUIContent("Flip-Book Blending",
                "Smooths out the transition between two frames of a flip-book animation if used.");

            public static GUIContent softParticlesEnabled = new GUIContent("Soft Particles",
                "Fade out particle geometry when it gets close to the surface of objects written into the depth buffer.");

            public static GUIContent softParticlesNearFadeDistanceText =
                new GUIContent("Near fade", "Soft Particles near fade distance.");

            public static GUIContent softParticlesFarFadeDistanceText =
                new GUIContent("Far fade", "Soft Particles far fade distance.");

            public static GUIContent cameraFadingEnabled = new GUIContent("Camera Fading",
                "Fade out particle geometry when it gets close to the camera.");

            public static GUIContent cameraNearFadeDistanceText =
                new GUIContent("Near Fade", "Camera near fade distance.");

            public static GUIContent cameraFarFadeDistanceText =
                new GUIContent("Far Fade", "Camera far fade distance.");

            public static GUIContent distortionEnabled = new GUIContent("Distortion",
                "This makes use of the 'CameraOpaque' texture from teh pipeline to give the ability yo distort the background pixels");

            public static GUIContent distortionStrength = new GUIContent("Strength",
                "How much the normal map affects the warping of the background pixels.");

            public static GUIContent distortionBlend = new GUIContent("Blend",
                "Weighting between the Base color of the particle and the backround color.");

            public static GUIContent VertexStreams = new GUIContent("Vertex Streams");

            public static string streamPositionText = "Position (POSITION.xyz)";
            public static string streamNormalText = "Normal (NORMAL.xyz)";
            public static string streamColorText = "Color (COLOR.xyzw)";
            public static string streamUVText = "UV (TEXCOORD0.xy)";
            public static string streamUV2Text = "UV2 (TEXCOORD0.zw)";
            public static string streamAnimBlendText = "AnimBlend (TEXCOORD1.x)";
            public static string streamTangentText = "Tangent (TANGENT.xyzw)";

            public static GUIContent streamApplyToAllSystemsText = new GUIContent("Sync Systems",
                "Apply the vertex stream layout to all Particle Systems using this material");

            public static GUIStyle vertexStreamIcon = new GUIStyle();
        }
        
        public struct ParticleProperties
        {
            // Surface Option Props
            public MaterialProperty colorMode;

            // Advanced Props
            public MaterialProperty flipbookMode;
            public MaterialProperty softParticlesEnabled;
            public MaterialProperty cameraFadingEnabled;
            public MaterialProperty distortionEnabled;
            public MaterialProperty softParticlesNearFadeDistance;
            public MaterialProperty softParticlesFarFadeDistance;
            public MaterialProperty cameraNearFadeDistance;
            public MaterialProperty cameraFarFadeDistance;
            public MaterialProperty distortionBlend;
            public MaterialProperty distortionStrength;

            public ParticleProperties(MaterialProperty[] properties)
            {
                // Surface Option Props
                colorMode = BaseShaderGUI.FindProperty("_ColorMode", properties, false);
                // Advanced Props
                flipbookMode = BaseShaderGUI.FindProperty("_FlipbookBlending", properties);
                softParticlesEnabled = BaseShaderGUI.FindProperty("_SoftParticlesEnabled", properties);
                cameraFadingEnabled = BaseShaderGUI.FindProperty("_CameraFadingEnabled", properties);
                distortionEnabled = BaseShaderGUI.FindProperty("_DistortionEnabled", properties, false);
                softParticlesNearFadeDistance = BaseShaderGUI.FindProperty("_SoftParticlesNearFadeDistance", properties);
                softParticlesFarFadeDistance = BaseShaderGUI.FindProperty("_SoftParticlesFarFadeDistance", properties);
                cameraNearFadeDistance = BaseShaderGUI.FindProperty("_CameraNearFadeDistance", properties);
                cameraFarFadeDistance = BaseShaderGUI.FindProperty("_CameraFarFadeDistance", properties);
                distortionBlend = BaseShaderGUI.FindProperty("_DistortionBlend", properties, false);
                distortionStrength = BaseShaderGUI.FindProperty("_DistortionStrength", properties, false); 
            }
        }
        
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
        
        public static void FadingOptions(Material material, MaterialEditor materialEditor, ParticleProperties properties)
        {
            // Z write doesn't work with fading
            bool hasZWrite = (material.GetInt("_ZWrite") != 0);
            if(!hasZWrite)
            {
                // Soft Particles
                {
                    EditorGUI.showMixedValue = properties.softParticlesEnabled.hasMixedValue;
                    var enabled = properties.softParticlesEnabled.floatValue;

                    EditorGUI.BeginChangeCheck();
                    enabled = EditorGUILayout.Toggle(Styles.softParticlesEnabled, enabled != 0.0f) ? 1.0f : 0.0f;
                    if (EditorGUI.EndChangeCheck())
                    {
                        materialEditor.RegisterPropertyChangeUndo("Soft Particles Enabled");
                        properties.softParticlesEnabled.floatValue = enabled;
                    }

                    if (enabled != 0.0f)
                    {
                        EditorGUI.indentLevel++;
                        materialEditor.ShaderProperty(properties.softParticlesNearFadeDistance, Styles.softParticlesNearFadeDistanceText);
                        materialEditor.ShaderProperty(properties.softParticlesFarFadeDistance, Styles.softParticlesFarFadeDistanceText);
                        EditorGUI.indentLevel--;
                    }
                }

                // Camera Fading
                {
                    EditorGUI.showMixedValue = properties.cameraFadingEnabled.hasMixedValue;
                    var enabled = properties.cameraFadingEnabled.floatValue;

                    EditorGUI.BeginChangeCheck();
                    enabled = EditorGUILayout.Toggle(Styles.cameraFadingEnabled, enabled != 0.0f) ? 1.0f : 0.0f;
                    if (EditorGUI.EndChangeCheck())
                    {
                        materialEditor.RegisterPropertyChangeUndo("Camera Fading Enabled");
                        properties.cameraFadingEnabled.floatValue = enabled;
                    }

                    if (enabled != 0.0f)
                    {
                        EditorGUI.indentLevel++;
                        materialEditor.ShaderProperty(properties.cameraNearFadeDistance, Styles.cameraNearFadeDistanceText);
                        materialEditor.ShaderProperty(properties.cameraFarFadeDistance, Styles.cameraFarFadeDistanceText);
                        EditorGUI.indentLevel--;
                    }
                }
                
                // Distortion
                if (properties.distortionEnabled != null)
                {
                    EditorGUI.BeginChangeCheck();
                    var enabled = properties.distortionEnabled.floatValue;
                    enabled = EditorGUILayout.Toggle(Styles.distortionEnabled, enabled != 0.0f) ? 1.0f : 0.0f;
                    if (EditorGUI.EndChangeCheck())
                    {
                        materialEditor.RegisterPropertyChangeUndo("Distortion Enabled");
                        properties.distortionEnabled.floatValue = enabled;
                    }
                    
                    if (enabled != 0.0f)
                    {
                        EditorGUI.indentLevel++;
                        materialEditor.ShaderProperty(properties.distortionStrength, Styles.distortionStrength);
                        EditorGUI.BeginChangeCheck();
                        var blend = EditorGUILayout.Slider(Styles.distortionBlend, properties.distortionBlend.floatValue, 0f, 1f);
                        if(EditorGUI.EndChangeCheck())
                            properties.distortionBlend.floatValue = blend;
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.showMixedValue = false;
            }
        }
        
        public static void DoVertexStreamsArea(Material material, List<ParticleSystemRenderer> renderers)
        {
            // Display list of streams required to make this shader work
            bool useLighting = true;//(material.GetFloat("_LightingEnabled") > 0.0f);
            bool useFlipbookBlending = (material.GetFloat("_FlipbookBlending") > 0.0f);
            bool useTangents = material.GetTexture("_BumpMap") && useLighting;

            if (useLighting)
                GUILayout.Label(Styles.streamNormalText, EditorStyles.label);

            GUILayout.Label(Styles.streamColorText, EditorStyles.label);
            GUILayout.Label(Styles.streamUVText, EditorStyles.label);

            if (useFlipbookBlending)
            {
                GUILayout.Label(Styles.streamUV2Text, EditorStyles.label);
                GUILayout.Label(Styles.streamAnimBlendText, EditorStyles.label);
            }

            if (useTangents)
                GUILayout.Label(Styles.streamTangentText, EditorStyles.label);

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
            if (GUILayout.Button(Styles.streamApplyToAllSystemsText, EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
            {
                foreach (ParticleSystemRenderer renderer in renderers)
                {
                    renderer.SetActiveVertexStreams(streams);
                }
            }

            // Display a warning if any renderers have incorrect vertex streams
            string Warnings = "";
            List<ParticleSystemVertexStream> rendererStreams = new List<ParticleSystemVertexStream>();
            foreach (ParticleSystemRenderer renderer in renderers)
            {
                renderer.GetActiveVertexStreams(rendererStreams);
                if (!rendererStreams.SequenceEqual(streams))
                    Warnings += "  " + renderer.name + "\n";
            }
            if (!string.IsNullOrEmpty(Warnings))
            {
                EditorGUILayout.HelpBox("The following Particle System Renderers are using this material with incorrect Vertex Streams:\n" + Warnings + "Use the Apply to Systems button to fix this", MessageType.Warning, true);
            }

            EditorGUILayout.Space();
        }
        
        public static void SetMaterialKeywords(Material material)
        {
            // Z write doesn't work with distortion/fading
            bool hasZWrite = (material.GetInt("_ZWrite") != 0);

            // Lit shader?
            //bool useLighting = (material.GetFloat("_LightingEnabled") > 0.0f);
            material.EnableKeyword("_RECEIVE_SHADOWS_OFF");

            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            //BaseShaderGUI.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));
            //BaseShaderGUI.SetKeyword(material, "_METALLICGLOSSMAP", (material.GetTexture("_MetallicGlossMap") != null));

            // A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
            // or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
            // The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
            MaterialEditor.FixupEmissiveFlag(material);
            bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            CoreUtils.SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);

            // Set the define for flipbook blending
            bool useFlipbookBlending = (material.GetFloat("_FlipbookBlending") > 0.0f);
            BaseShaderGUI.SetKeyword(material, "_FLIPBOOKBLENDING_OFF", useFlipbookBlending);

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
            BaseShaderGUI.SetKeyword(material, "_FADING_ON", useFading);
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
            bool useDistortion = (material.GetFloat("_DistortionEnabled") > 0.0f) && (BaseShaderGUI.SurfaceType)material.GetFloat("_Surface") != BaseShaderGUI.SurfaceType.Opaque;
            BaseShaderGUI.SetKeyword(material, "_DISTORTION_ON", useDistortion);
            if(useDistortion)
                material.SetFloat("_DistortionStrengthScaled", material.GetFloat("_DistortionStrength") * 0.1f);
            
            // Set the define for distortion + grabpass (Distortion not supported)
            material.SetShaderPassEnabled("Always", false);
        }

        //List<ParticleSystemRenderer> m_RenderersUsingThisMaterial = new List<ParticleSystemRenderer>();

/*        public override void FindProperties(MaterialProperty[] properties)
    {
        base.FindProperties(properties);
        
        colorMode = FindProperty("_ColorMode", properties, false);
        flipbookMode = FindProperty("_FlipbookBlending", properties);
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
    }*/

        /*public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");
            
            material.shaderKeywords = null;
            //SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Blend"));
            if (colorMode != null)
                SetupMaterialWithColorMode(material);
            SetupMaterialBlendMode(material);
            SetMaterialKeywords(material);
        }*/

        /*public override void DrawSurfaceOptions(Material material)
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
        }*/

        /*public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            
            EditorGUI.BeginChangeCheck();
            {
                //materialEditor.TexturePropertySingleLine(StylesParticle.normalMapText, bumpMapProp);
    
                DrawEmissionProperties(material, true);
    
                DrawBaseTileOffset();
                EditorGUI.BeginChangeCheck();
            }
        }*/

        /*public override void DrawAdvancedOptions(Material material)
        {
            EditorGUI.BeginChangeCheck();
            {
                materialEditor.ShaderProperty(flipbookMode, StylesParticle.flipbookMode);
                FadingPopup(material);
                EditorGUI.BeginChangeCheck();
            }
            base.DrawAdvancedOptions(material);
        }*/

        /*public override void DrawAdditionalFoldouts(Material material)
        {
            var vertexStreams = EditorGUILayout.BeginFoldoutHeaderGroup(GetHeaderState(3), StylesParticle.VertexStreams, EditorStyles.foldoutHeader, NullThing, StylesParticle.vertexStreamIcon);
            if (vertexStreams)
            {
                DoVertexStreamsArea(material);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            StoreHeaderState(vertexStreams, 3);
        }*/

        //private static void NullThing(Rect rect){}

        /*public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            if (m_FirstTimeApply)
            {
                CacheRenderersUsingThisMaterial(materialEditor.target as Material);
                m_FirstTimeApply = false;
            }
            
            base.OnGUI(materialEditor, props);
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

        /*void FadingPopup(Material material)
        {
            // Z write doesn't work with fading
            bool hasZWrite = (material.GetInt("_ZWrite") != 0);
            if(!hasZWrite)
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
                        EditorGUI.indentLevel++;
                        materialEditor.ShaderProperty(softParticlesNearFadeDistance, StylesParticle.softParticlesNearFadeDistanceText);
                        materialEditor.ShaderProperty(softParticlesFarFadeDistance, StylesParticle.softParticlesFarFadeDistanceText);
                        EditorGUI.indentLevel--;
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
                        EditorGUI.indentLevel++;
                        materialEditor.ShaderProperty(cameraNearFadeDistance, StylesParticle.cameraNearFadeDistanceText);
                        materialEditor.ShaderProperty(cameraFarFadeDistance, StylesParticle.cameraFarFadeDistanceText);
                        EditorGUI.indentLevel--;
                    }
                }
                
                // Distortion
                if (distortionEnabled != null)
                {
                    EditorGUI.BeginChangeCheck();
                    var enabled = distortionEnabled.floatValue;
                    enabled = EditorGUILayout.Toggle(StylesParticle.distortionEnabled, enabled != 0.0f) ? 1.0f : 0.0f;
                    if (EditorGUI.EndChangeCheck())
                    {
                        materialEditor.RegisterPropertyChangeUndo("Distortion Enabled");
                        distortionEnabled.floatValue = enabled;
                    }
                    
                    if (enabled != 0.0f)
                    {
                        int indentation = 2;
                        materialEditor.ShaderProperty(distortionStrength, StylesParticle.distortionStrength, indentation);
                        materialEditor.ShaderProperty(distortionBlend, StylesParticle.distortionBlend, indentation);
                    }
                }
    
                EditorGUI.showMixedValue = false;
            }
        }*/

/*        void DoAlbedoArea(Material material)
    {
        m_MaterialEditor.TexturePropertyWithHDRColor(Styles.albedoText, albedoMap, albedoColor, true);
        if (((BlendMode)material.GetFloat("_Blend") == BlendMode.Cutout))
        {
            m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText, MaterialEditor.kMiniTextureFieldLabelIndentLevel);
        }
    }*/

        /*void DoSpecularMetallicArea(Material material)
        {
            if (metallicMap == null)
                return;
    
    
                bool hasGlossMap = metallicMap.textureValue != null;
                materialEditor.TexturePropertySingleLine(StylesParticle.metallicMapText, metallicMap, hasGlossMap ? null : metallic);
    
                int indentation = 2; // align with labels of texture properties
                bool showSmoothnessScale = hasGlossMap;
                materialEditor.ShaderProperty(smoothness, showSmoothnessScale ? StylesParticle.smoothnessScaleText : StylesParticle.smoothnessText, indentation);
        }*/

        /*void DoVertexStreamsArea(Material material)
        {
            // Display list of streams required to make this shader work
            bool useLighting = true;//(material.GetFloat("_LightingEnabled") > 0.0f);
            bool useFlipbookBlending = (material.GetFloat("_FlipbookBlending") > 0.0f);
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
            if (!string.IsNullOrEmpty(Warnings))
            {
                EditorGUILayout.HelpBox("The following Particle System Renderers are using this material with incorrect Vertex Streams:\n" + Warnings + "Use the Apply to Systems button to fix this", MessageType.Warning, true);
            }
    
            EditorGUILayout.Space();
        }*/

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

        /*public static void SetupMaterialWithColorMode(Material material)
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
        }*/

        /*static void SetMaterialKeywords(Material material)
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
            bool useFlipbookBlending = (material.GetFloat("_FlipbookBlending") > 0.0f);
            SetKeyword(material, "_FLIPBOOKBLENDING_OFF", useFlipbookBlending);
    
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
        }*/

        /*void CacheRenderersUsingThisMaterial(Material material)
        {
            m_RenderersUsingThisMaterial.Clear();
    
            ParticleSystemRenderer[] renderers = UnityEngine.Object.FindObjectsOfType(typeof(ParticleSystemRenderer)) as ParticleSystemRenderer[];
            foreach (ParticleSystemRenderer renderer in renderers)
            {
                if (renderer.sharedMaterial == material)
                    m_RenderersUsingThisMaterial.Add(renderer);
            }
        }*/

        /*static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }*/
    }
} // namespace UnityEditor
