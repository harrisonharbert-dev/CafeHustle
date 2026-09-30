#if FIMPOSSIBLE_JOBSANDBURST_READY
using FIMSpace.FEditor;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TurboTailAnimator
    {

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            RefreshTransformsList();

            if (_TransformsGhostChain.Count == 0) return;

            if (_Editor_Category == ETailCategory.Setup)
            {
                if (_TransformsGhostChain.Count > 1)
                {
                    Handles.color = new Color(0.5f, 1f, 0.35f, 0.8f);

                    if (_TransformsGhostChain.Count > 1)
                    {
                        FGUI_Handles.DrawBoneHandle(_TransformsGhostChain[0].position, _TransformsGhostChain[1].position, 1f, true, 1f, 1f, 1f, 0.1f);
                        Handles.SphereHandleCap(0, _TransformsGhostChain[0].position, Quaternion.identity, HandleUtility.GetHandleSize(_TransformsGhostChain[0].position) * 0.09f, EventType.Repaint);
                    }

                    for (int i = 1; i < _TransformsGhostChain.Count - 1; i++) // -1 because we painting bones from i to i+1
                    {
                        Handles.color = new Color(0.5f, 1f, 0.35f, 0.8f);
                        FGUI_Handles.DrawBoneHandle(_TransformsGhostChain[i].position, _TransformsGhostChain[i + 1].position, 1f, true, 1f,1f,1f,0.1f);

                        Handles.color = new Color(0.5f, 1f, 0.35f, 0.3f);
                        Handles.SphereHandleCap(0, _TransformsGhostChain[i].position, Quaternion.identity, HandleUtility.GetHandleSize(_TransformsGhostChain[i].position) * 0.09f, EventType.Repaint);
                    }

                    if (_TransformsGhostChain.Count > 1)
                        if (_TransformsGhostChain[0].parent)
                        {
                            Handles.color = new Color(1f, .2f, 0.6f, 0.3f);
                            FGUI_Handles.DrawBoneHandle(_TransformsGhostChain[0].parent.position, _TransformsGhostChain[0].position, 1f, true, 1f, 1f, 1f, 0.1f);
                        }
                }
                else
                {
                    if (_TransformsGhostChain.Count > 0)
                    {
                        if (_TransformsGhostChain[0].parent)
                        {
                            Transform t = _TransformsGhostChain[0]; Transform p = _TransformsGhostChain[0].parent;

                            Handles.color = new Color(0.8f, .8f, 0.2f, 0.8f);
                            FGUI_Handles.DrawBoneHandle(_TransformsGhostChain[0].parent.position, _TransformsGhostChain[0].position, 1f, true, 1f, 1f, 1f, 0.1f);
                            Handles.color = new Color(0.8f, .8f, 0.2f, 0.3f);
                            Handles.SphereHandleCap(0, _TransformsGhostChain[0].position, Quaternion.identity, HandleUtility.GetHandleSize(_TransformsGhostChain[0].position) * 0.135f, EventType.Repaint);
                            Handles.Label(t.position + Vector3.Cross(t.forward, p.forward).normalized * (t.position - p.position).magnitude / 2f, new GUIContent("[i]", "Tail chain with one bone setup - try using End Bone Offset in 'Setup' tab"));
                        }
                    }
                }
            }
            else if (_Editor_Category == ETailCategory.Features)
            {
                if (UseCollision)
                {
                    Color preCol = Gizmos.color;
                    Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.25f);

                    for (int i = 0; i < _TransformsGhostChain.Count; i++)
                    {
                        if (_TransformsGhostChain[i] == null) continue;
                        float radius = 1f;

                        if (Application.isPlaying && initialized) radius = TailSegments[i].GetRadiusScaled();
                        else radius = GetColliderSphereRadiusFor(i) * Mathf.Abs(_TransformsGhostChain[i].lossyScale.x);

                        Gizmos.DrawWireSphere(_TransformsGhostChain[i].position, radius);

                        if (CollisionMode == ECollisionMode.CapsulesAlongBones && i > 0 && _TransformsGhostChain[i - 1])
                        {
                            float previousRadius;

                            if (Application.isPlaying && initialized)
                                previousRadius = TailSegments[i - 1].GetRadiusScaled();
                            else
                                previousRadius = GetColliderSphereRadiusFor(i - 1) * Mathf.Abs(_TransformsGhostChain[i - 1].lossyScale.x);

                            DrawCapsuleCollisionGizmos(_TransformsGhostChain[i - 1].position, _TransformsGhostChain[i].position, previousRadius, radius);
                        }
                    }

                    Handles.color = new Color(0.4f, 1f, 0.25f, 0.22f);

                    int c = Mathf.Min(IncludedColliders.Count, 10); // Drawing max 10 lines toward included colliders

                    for (int i = 0; i < c; i++)
                    {
                        if (IncludedColliders[i] != null)
                        {
                            Handles.DrawDottedLine(_TransformsGhostChain[0].position, IncludedColliders[i].transform.position, 2f);
                            Handles.SphereHandleCap(0, IncludedColliders[i].transform.position, Quaternion.identity, HandleUtility.GetHandleSize(IncludedColliders[i].transform.position) * 0.09f, EventType.Repaint);
                        }
                    }

                    Gizmos.color = preCol;
                }
            }
        }

        static void DrawCapsuleCollisionGizmos(Vector3 start, Vector3 end, float startRadius, float endRadius)
        {
            Vector3 direction = (end - start).normalized;
            if (direction.sqrMagnitude == 0f) return;

            Vector3 reference;

            if (Mathf.Abs(direction.y) < 0.9f) 
                reference = Vector3.up;
            else 
                reference = Vector3.right;

            Vector3 side = Vector3.Cross(direction, reference).normalized;
            Vector3 up = Vector3.Cross(direction, side);
            Gizmos.DrawLine(start + side * startRadius, end + side * endRadius);
            Gizmos.DrawLine(start - side * startRadius, end - side * endRadius);
            Gizmos.DrawLine(start + up * startRadius, end + up * endRadius);
            Gizmos.DrawLine(start - up * startRadius, end - up * endRadius);
        }

#endif

    }
}
#endif
