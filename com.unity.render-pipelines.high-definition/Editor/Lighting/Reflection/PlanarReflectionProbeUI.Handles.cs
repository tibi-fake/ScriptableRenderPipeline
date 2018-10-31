using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    partial class PlanarReflectionProbeUI
    {
        static readonly Color k_GizmoMirrorPlaneCamera = new Color(128f / 255f, 128f / 255f, 233f / 255f, 128f / 255f);

        internal static void DrawHandlesOverride(PlanarReflectionProbeUI s, SerializedPlanarReflectionProbe d, Editor o)
        {
            //Note: HDProbeUI.DrawHandles is called in parent 
            PlanarReflectionProbe probe = d.target;

            switch (EditMode.editMode)
            {
                case EditBaseShape:
                    if ((InfluenceShape)d.influenceVolume.shape.intValue != InfluenceShape.Box)
                        return;

                    //override base handle behavior to also translate object along x and z axis and offset the y axis
                    using (new Handles.DrawingScope(Matrix4x4.TRS(Vector3.zero, d.target.transform.rotation, Vector3.one)))
                    {
                        //contained must be initialized in all case
                        s.influenceVolume.boxBaseHandle.center = Quaternion.Inverse(d.target.transform.rotation) * d.target.transform.position + d.influenceVolume.offset.vector3Value;
                        s.influenceVolume.boxBaseHandle.size = probe.influenceVolume.boxSize;

                        EditorGUI.BeginChangeCheck();
                        s.influenceVolume.boxBaseHandle.DrawHandle();
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObjects(new Object[] { d.target, d.target.transform }, "Modified Planar Base Volume AABB");

                            probe.influenceVolume.boxSize = s.influenceVolume.boxBaseHandle.size;

                            d.target.influenceVolume.offset = new Vector3(0, s.influenceVolume.boxBaseHandle.center.y, 0);
                            Vector3 centerXZ = s.influenceVolume.boxBaseHandle.center;
                            centerXZ.y = 0;
                            Vector3 deltaXZ = d.target.transform.rotation * centerXZ - d.target.transform.position;
                            d.target.transform.position += deltaXZ;
                        }
                    }
                    break;
            }
        }

        [DrawGizmo(GizmoType.Selected)]
        internal static void DrawGizmos(PlanarReflectionProbe d, GizmoType gizmoType)
        {
            HDProbeUI.DrawGizmos(d, gizmoType);

            HDProbeUI s;
            if (!HDProbeEditor.TryGetUIStateFor(d, out s))
                return;
            
            var mat = Matrix4x4.TRS(d.transform.position + d.transform.rotation * d.influenceVolume.offset, d.transform.rotation, Vector3.one);

            //gizmo overrides
            switch (EditMode.editMode)
            {
                case EditBaseShape:
                    if (d.influenceVolume.shape != InfluenceShape.Box)
                        break;

                    using (new Handles.DrawingScope(mat))
                    {
                        s.influenceVolume.boxBaseHandle.center = Vector3.zero;
                        s.influenceVolume.boxBaseHandle.size = d.influenceVolume.boxSize;
                        s.influenceVolume.boxBaseHandle.DrawHull(true);
                        s.influenceVolume.boxInfluenceHandle.center = d.influenceVolume.boxBlendOffset;
                        s.influenceVolume.boxInfluenceHandle.size = d.influenceVolume.boxSize + d.influenceVolume.boxBlendSize;
                        s.influenceVolume.boxInfluenceHandle.DrawHull(false);
                        s.influenceVolume.boxInfluenceNormalHandle.center = d.influenceVolume.boxBlendNormalOffset;
                        s.influenceVolume.boxInfluenceNormalHandle.size = d.influenceVolume.boxSize + d.influenceVolume.boxBlendNormalSize;
                        s.influenceVolume.boxInfluenceNormalHandle.DrawHull(false);
                    }
                    break;
            }

            if (!HDProbeEditor.TryGetUIStateFor(d, out s))
                return;

            if (s.showCaptureHandles || EditMode.editMode == EditCenter)
                DrawGizmos_CaptureFrustrum(d);

            if (d.useMirrorPlane)
                DrawGizmos_CaptureMirror(d);
        }

        static void DrawGizmos_CaptureMirror(PlanarReflectionProbe d)
        {
            var c = Gizmos.color;
            var m = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(
                    d.captureMirrorPlanePosition,
                    Quaternion.LookRotation(d.captureMirrorPlaneNormal, Vector3.up),
                    Vector3.one);
            Gizmos.color = k_GizmoMirrorPlaneCamera;

            Gizmos.DrawCube(Vector3.zero, new Vector3(1, 1, 0));

            Gizmos.matrix = m;
            Gizmos.color = c;
        }

        static void DrawGizmos_CaptureFrustrum(PlanarReflectionProbe d)
        {
            var viewerCamera = Camera.current;
            var c = Gizmos.color;
            var m = Gizmos.matrix;

            float nearClipPlane, farClipPlane, aspect, fov;
            Color backgroundColor;
            CameraClearFlags clearFlags;
            Vector3 capturePosition;
            Quaternion captureRotation;
            Matrix4x4 worldToCameraRHS, projection;

            ReflectionSystem.CalculateCaptureCameraProperties(d,
                out nearClipPlane, out farClipPlane,
                out aspect, out fov, out clearFlags, out backgroundColor,
                out worldToCameraRHS, out projection,
                out capturePosition, out captureRotation, viewerCamera);

            Gizmos.DrawSphere(capturePosition, HandleUtility.GetHandleSize(capturePosition) * 0.2f);
            Gizmos.color = c;
        }
    }
}
