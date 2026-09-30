using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {
        Vector3 _limiting_limitPosition = Vector3.zero;
        Vector3 _limiting_influenceOffset = Vector3.zero;

        /// <summary> Helping stretching limiting be more responsible (lerp value help) </summary>
        float _limiting_stretchingHelperTooLong = 0f;
        float _limiting_stretchingHelperTooShort = 0f;

        /// <summary>
        /// If tail is too long we making it shorter if too short - back to default scale
        /// </summary>
        protected void StretchingLimiting(TailSegment bone)
        {
            Vector3 backDir = (bone.ParentBone.ProceduralPosition) - (bone.ProceduralPosition);
            float dist = backDir.magnitude;

            if (dist > 0f)
            {
                float maxDist = bone.BoneLengthScaled + bone.BoneLengthScaled * 2.5f * MaxStretching;

                if (dist > maxDist) // If tail too long
                {
                    if (MaxStretching == 0f)
                    {
                        _limiting_limitPosition = bone.ProceduralPosition + backDir * ((dist - bone.BoneLengthScaled) / dist);
                        bone.ProceduralPosition = _limiting_limitPosition;
                    }
                    else
                    {
                        _limiting_limitPosition = bone.ParentBone.ProceduralPosition - backDir.normalized * maxDist;
                        //bone.ProceduralPosition = _limiting_limitPosition;
                        float limValue = 1f - maxDist / dist + _limiting_stretchingHelperTooLong;
                        limValue = Mathf.Min(0.99f, Mathf.Max(0f, limValue));
                        if (ReactionSpeed < 0.5f) limValue *= deltaForLerps * (10f + ReactionSpeed * 30f);
                        bone.ProceduralPosition = Vector3.Lerp(bone.ProceduralPosition, _limiting_limitPosition, limValue);
                    }
                }
                else // If tail too short
                {
                    maxDist = bone.BoneLengthScaled + bone.BoneLengthScaled * 1.1f * MaxStretching;

                    if (dist < maxDist)
                    {
                        _limiting_limitPosition = bone.ProceduralPosition + backDir * ((dist - bone.BoneLengthScaled) / dist);

                        if (MaxStretching == 0f)
                            bone.ProceduralPosition = _limiting_limitPosition;
                        else
                        {
                            float limValue = 1f - dist / maxDist + _limiting_stretchingHelperTooShort;
                            bone.ProceduralPosition = Vector3.Lerp(bone.ProceduralPosition, _limiting_limitPosition, Mathf.Clamp01(limValue));
                        }
                    }
                }

            }

        }

        Quaternion _limiting_angle_ToTargetRot;
        Quaternion _limiting_angle_targetInLocal;
        Quaternion _limiting_angle_newLocal;

        /// <summary>
        /// If segment rotation is in too big angle we straighten it
        /// </summary>
        protected Vector3 AngleLimiting(TailSegment child, Vector3 targetPos)
        {
            _limiting_limitPosition = targetPos;

            Transform parentTransform = child.ParentBone.transform;
            Quaternion parentRotation = parentTransform.rotation;

            _limiting_angle_ToTargetRot = (
             Quaternion.FromToRotation
             (
                 parentTransform.TransformDirection( TailSegment_GetLocalPosition( child ) ),
                 targetPos - child.ParentBone.ProceduralPosition)
             )
             * parentRotation;

            _limiting_angle_targetInLocal = FEngineering.QToLocal( parentRotation, _limiting_angle_ToTargetRot ); // Quaternion.Inverse(child.ParentBone.PreviousRotation) * _limiting_angle_ToTargetRot;

            // Limiting all axis or one
            float angleDiffToInitPose = 0f;

            if (AngleLimitAxis.sqrMagnitude == 0f) // All axis limit angle
                angleDiffToInitPose = Quaternion.Angle(_limiting_angle_targetInLocal, child.LastKeyframeLocalRotation);
            else // Selective axis
            {
                // Work in a swing/twist decomposition instead of Euler angles
                // Euler magnitude loses the direction around an axis at the +/-180 seam and was the source of selective-axis jitter
                // Use the current source pose for both limit modes
                Vector3 axis = AngleLimitAxis.normalized;
                Quaternion relative = Quaternion.Inverse(child.LastKeyframeLocalRotation) * _limiting_angle_targetInLocal;
                Vector3 relativeVector = new Vector3(relative.x, relative.y, relative.z);
                Vector3 projectedVector = Vector3.Project(relativeVector, axis);
                Quaternion twist = new Quaternion(projectedVector.x, projectedVector.y, projectedVector.z, relative.w);
                float twistMagnitude = Mathf.Sqrt(twist.x * twist.x + twist.y * twist.y + twist.z * twist.z + twist.w * twist.w);

                if (twistMagnitude > Mathf.Epsilon)
                {
                    twist.x /= twistMagnitude; twist.y /= twistMagnitude; twist.z /= twistMagnitude; twist.w /= twistMagnitude;
                    float signedTwist = Mathf.Atan2(Vector3.Dot(new Vector3(twist.x, twist.y, twist.z), axis), twist.w) * Mathf.Rad2Deg * 2f;
                    if (signedTwist > 180f) signedTwist -= 360f;
                    else if (signedTwist < -180f) signedTwist += 360f;

                    float minLimit = -AngleLimit;
                    float maxLimit = AngleLimit;
                    if (Mathf.Abs(LimitAxisRange.x - LimitAxisRange.y) > Mathf.Epsilon)
                    {
                        minLimit += LimitAxisRange.x;
                        maxLimit += LimitAxisRange.y;
                    }

                    float clampedTwist = Mathf.Clamp(signedTwist, minLimit, maxLimit);
                    angleDiffToInitPose = Mathf.Abs(signedTwist - clampedTwist);

                    if (angleDiffToInitPose > Mathf.Epsilon)
                    {
                        Quaternion swing = relative * Quaternion.Inverse(twist);
                        _limiting_angle_newLocal = child.LastKeyframeLocalRotation * swing * Quaternion.AngleAxis(clampedTwist, axis);
                    }
                }
            }


            #region Debug
            //Debug.Log("Atarget in local = " +
            //    FEngineering.WrapVector(_limiting_angle_targetInLocal.eulerAngles) + " last key local = " +
            //    FEngineering.WrapVector(child.lastKeyframeLocalRotation.eulerAngles) + " angle = " + angleDiffToInitPose);
            #endregion

            // Finding rotate back to limited angle coordinates
            if (AngleLimitAxis.sqrMagnitude != 0f && angleDiffToInitPose > Mathf.Epsilon)
            {
                // The selective branch already has its exact bounded rotation.
                if (LimitSmoothing > Mathf.Epsilon)
                {
                    float smooth = Mathf.Lerp(55f, 15f, LimitSmoothing);
                    float correctionWeight = Mathf.InverseLerp(0f, Mathf.Max(AngleLimit, 0.0001f), angleDiffToInitPose);

                    _limiting_angle_newLocal = Quaternion.Slerp(_limiting_angle_targetInLocal, _limiting_angle_newLocal, Mathf.Clamp01(deltaForLerps * smooth * correctionWeight));
                }

                _limiting_angle_ToTargetRot = FEngineering.QToWorld(parentRotation, _limiting_angle_newLocal);
                
                Vector3 limitedDirection = _limiting_angle_ToTargetRot * Vector3.Scale(child.transform.lossyScale, TailSegment_GetLocalPosition(child));
                float limitedDirectionLength = limitedDirection.magnitude;
                float targetDistance = (targetPos - child.ParentBone.ProceduralPosition).magnitude;

                if (limitedDirectionLength > Mathf.Epsilon && targetDistance > Mathf.Epsilon)
                    _limiting_limitPosition = child.ParentBone.ProceduralPosition + limitedDirection * (targetDistance / limitedDirectionLength);

                return _limiting_limitPosition;
            }

            if (AngleLimitAxis.sqrMagnitude == 0f && angleDiffToInitPose > AngleLimit)
            {

                #region Debug

                //Debug.DrawLine(child.ParentBone.ParentBone.transform.position + child.ParentBone.ParentBone.ProceduralRotation * child.ParentBone.transform.localPosition,
                //child.ProceduralPosition, Color.red, 1f);

                //Debug.Log("[" + child.Index + "] diff = " 
                //    + angleDiffToInitPose + " exc =  " 
                //    + exceededAngle + " fact = " 
                //    + angleFactor);

                #endregion


                // Move only as far as is necessary to reach the limit
                float targetBlend = AngleLimit > Mathf.Epsilon ? Mathf.Clamp01( AngleLimit / angleDiffToInitPose ) : 0f;
                _limiting_angle_newLocal = Quaternion.Slerp( child.LastKeyframeLocalRotation, _limiting_angle_targetInLocal, targetBlend );

                if (LimitSmoothing > Mathf.Epsilon)
                {
                    // Keep the optional soft response, but never allow its interpolation to overshoot
                    float correction = Quaternion.Angle( _limiting_angle_targetInLocal, _limiting_angle_newLocal );
                    float correctionWeight = Mathf.InverseLerp( 0f, Mathf.Max( AngleLimit, 0.0001f ), correction );
                    float smooth = Mathf.Lerp( 55f, 15f, LimitSmoothing );

                    _limiting_angle_newLocal = Quaternion.Slerp( _limiting_angle_targetInLocal, _limiting_angle_newLocal, Mathf.Clamp01( deltaForLerps * smooth * correctionWeight ) );
                }

                _limiting_angle_ToTargetRot = FEngineering.QToWorld( parentRotation, _limiting_angle_newLocal );

                // Angular limiting must not also change the segment length
                // Preserving the constrained position distance avoids fighting StretchingLimiting every frame
                Vector3 limitedDirection = _limiting_angle_ToTargetRot * Vector3.Scale( child.transform.lossyScale, TailSegment_GetLocalPosition( child ) );
                float limitedDirectionLength = limitedDirection.magnitude;
                float targetDistance = (targetPos - child.ParentBone.ProceduralPosition).magnitude;

                if (limitedDirectionLength > Mathf.Epsilon && targetDistance > Mathf.Epsilon)
                    _limiting_limitPosition = child.ParentBone.ProceduralPosition + limitedDirection * (targetDistance / limitedDirectionLength);

                return _limiting_limitPosition;
            }

            return targetPos;
        }


        /// <summary>
        /// Limiting tail motion in world space position movement
        /// </summary>
        void MotionInfluenceLimiting()
        {
            if (MotionInfluence != 1f)
            {   // one - param: param = 1 -> 0  param = 0 -> 1
                Vector3 basePosition = BaseTransform.position;

                _limiting_influenceOffset = ( basePosition - previousWorldPosition ) * ( 1f - MotionInfluence );

                if (MotionInfluenceInY < 1f)
                    _limiting_influenceOffset.y = ( basePosition.y - previousWorldPosition.y) * (1f - MotionInfluenceInY);

                for (int i = 0; i < TailSegments.Count; i++)
                {
                    TailSegments[i].ProceduralPosition += _limiting_influenceOffset;
                    TailSegments[i].PreviousPosition += _limiting_influenceOffset;
                }

                GhostChild.ProceduralPosition += _limiting_influenceOffset;
                GhostChild.PreviousPosition += _limiting_influenceOffset;
            }
        }


        /// <summary>
        /// Bend the target before interpolation and smoothing so a steady force has a steady resting pose.
        /// </summary>
        Vector3 CalculateGravityPositionOffsetForSegment(TailSegment bone, Vector3 targetPosition)
        {
            Vector3 force = bone.Gravity;
            if (UseWind) force += WindEffect;
            if (FEngineering.VIsZero(force)) return targetPosition;

            // Preserve the force strength relative to the spring at the reference 60 Hz step.
            float springStrength = _sg_curly;
            if (UnifyBendiness > 0f) springStrength *= TailSegment_GetUnifiedBendinessMultiplier(bone);
            force /= Mathf.Max(0.0001f, springStrength);

            Vector3 toBone = targetPosition - bone.ParentBone.ProceduralPosition;
            Vector3 direction = toBone + force * bone.BoneLengthScaled;
            return bone.ParentBone.ProceduralPosition + direction.normalized * toBone.magnitude;
        }


        /// <summary>
        /// Limiting movement of tail bones in selected axis
        /// </summary>
        void Axis2DLimit(TailSegment child)
        {
            child.ProceduralPosition -=
                FEngineering.VAxis2DLimit(
                    child.ParentBone.transform,
                    child.ParentBone.ProceduralPosition,
                    child.ProceduralPosition, Axis2D);

        }


        #region Distance Limiting Calculations

        [Tooltip("If you want to use max distance fade option to smoothly disable tail animator when object is going far away from camera")]
        public bool UseMaxDistance = false;

        [Tooltip("(By default camera transform) Measuring distance from this object to define if object is too far and not need to update tail animator")]
        public Transform DistanceFrom;
        [HideInInspector]
        public Transform _distanceFrom_Auto;

        [Tooltip("Max distance to main camera / target object to smoothly turn off tail animator.")]
        public float MaximumDistance = 35f;

        [Tooltip("If object in range should be detected only when is nearer than 'MaxDistance' to avoid stuttery enabled - disable switching")]
        [Range(0.0f, 1f)]
        public float MaxOutDistanceFactor = 0f;

        [Tooltip("If distance should be measured not using Up (y) axis")]
        public bool DistanceWithoutY = false;

        [Tooltip("Offsetting point from which we want to measure distance to target")]
        public Vector3 DistanceMeasurePoint;

        [Tooltip("Disable fade duration in seconds")]
        [Range(0.25f, 2f)]
        public float FadeDuration = 0.75f;

        private bool maxDistanceExceed = false;
        private Transform finalDistanceFrom;
        private bool wasCameraSearch = false;

        /// <summary> Multiplier for blend weight when tail animator is far from camera or provided object </summary>
        private float distanceWeight = 1f;


        /// <summary>
        /// Getting distance value from distance measure point to target position
        /// </summary>
        public float GetDistanceMeasure(Vector3 targetPosition)
        {
            if (DistanceWithoutY)
            {
                Vector3 p = BaseTransform.position + BaseTransform.TransformVector(DistanceMeasurePoint);
                Vector2 p2 = new Vector2(p.x, p.z);
                return Vector2.Distance(p2, new Vector2(targetPosition.x, targetPosition.z));
            }
            else
                return Vector3.Distance(BaseTransform.position + BaseTransform.TransformVector(DistanceMeasurePoint), targetPosition);
        }


        /// <summary>
        /// Handling max distance feature
        /// </summary>
        private void MaxDistanceCalculations()
        {

            if (DistanceFrom != null)
                finalDistanceFrom = DistanceFrom;
            #region Defining distance measure reference if not found
            else
            {
                if (finalDistanceFrom == null)
                {
                    if (_distanceFrom_Auto == null)
                    {
                        Camera c = Camera.main;
                        if (c) _distanceFrom_Auto = c.transform;
                        else
                        {
                            if (!wasCameraSearch)
                            {
                                c = FUVH.FindSceneObject<Camera>();
                                if (c) _distanceFrom_Auto = c.transform;
                                wasCameraSearch = true;
                            }
                        }
                    }

                    finalDistanceFrom = _distanceFrom_Auto;
                }
            }
            #endregion


            // If we are using distance limitation
            if (MaximumDistance > 0f && finalDistanceFrom != null)
            {
                if (!maxDistanceExceed) // If look motion is not out of look range etc.
                {
                    float distance = GetDistanceMeasure(finalDistanceFrom.position);

                    if (distance > MaximumDistance + MaximumDistance * MaxOutDistanceFactor)
                        maxDistanceExceed = true;

                    distanceWeight += Time.unscaledDeltaTime * (1f / FadeDuration);
                    if (distanceWeight > 1f) distanceWeight = 1f;
                }
                else // When disabling tail animator
                {
                    // Entering back distance range
                    float distance = GetDistanceMeasure(finalDistanceFrom.position);
                    if (distance <= MaximumDistance) maxDistanceExceed = false;

                    distanceWeight -= Time.unscaledDeltaTime * (1f / FadeDuration);
                    if (distanceWeight < 0f) distanceWeight = 0f;
                }
            }
            else // If we don't use max of distance feature
            {
                maxDistanceExceed = false;
                distanceWeight = 1f;
            }
        }


        #endregion


    }

}
