﻿/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes a tap.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Tap Gesture")]
    public class TapGesture : Gesture
    {
        #region Private variables

        [SerializeField]
        private float timeLimit = float.PositiveInfinity;

        [SerializeField]
        private float distanceLimit = float.PositiveInfinity;

        [SerializeField]
        private float clusterExistenceTime = .3f;

        /// <summary>
        /// The cached screen position. 
        /// Used to keep tap's position which can't be calculated from touch points when the gesture is recognized since all touch points are gone.
        /// </summary>
        protected Vector2 cachedScreenPosition;

        /// <summary>
        /// The cached previous screen position.
        /// Used to keep tap's position which can't be calculated from touch points when the gesture is recognized since all touch points are gone.
        /// </summary>
        protected Vector2 cachedPreviousScreenPosition;

        /// <summary>
        /// The cached target hit result.
        /// Used to keep tap's position which can't be calculated from touch points when the gesture is recognized since all touch points are gone.
        /// </summary>
        protected RaycastHit cachedTargetHitResult;

        private Vector2 totalMovement = Vector2.zero;
        private float startTime;
        private List<TouchPoint> removedPoints = new List<TouchPoint>();
        private List<float> removedPointsTimes = new List<float>();

        #endregion

        #region Public properties

        /// <summary>
        /// Maximum time to hold touches until gesture is considered to be failed.
        /// </summary>
        public float TimeLimit
        {
            get { return timeLimit; }
            set { timeLimit = value; }
        }

        /// <summary>
        /// Maximum distance for touch cluster to move until gesture is considered to be failed.
        /// </summary>
        public float DistanceLimit
        {
            get { return distanceLimit; }
            set { distanceLimit = value; }
        }

        public float ClusterExistenceTime
        {
            get { return clusterExistenceTime; }
            set { clusterExistenceTime = value; }
        }

        /// <inheritdoc />
        public override Vector2 ScreenPosition
        {
            get
            {
                if (TouchPoint.IsInvalidPosition(cachedScreenPosition)) return base.ScreenPosition;
                return cachedScreenPosition;
            }
        }

        /// <inheritdoc />
        public override Vector2 PreviousScreenPosition
        {
            get
            {
                if (TouchPoint.IsInvalidPosition(cachedScreenPosition)) return base.PreviousScreenPosition;
                return cachedPreviousScreenPosition;
            }
        }

        #endregion

        /// <inheritdoc />
        public override bool GetTargetHitResult()
        {
            RaycastHit hit;
            return GetTargetHitResult(out hit);
        }

        /// <inheritdoc />
        public override bool GetTargetHitResult(out RaycastHit hit)
        {
            if (State == GestureState.Ended)
            {
                hit = cachedTargetHitResult;
                return true;
            }

            return base.GetTargetHitResult(out hit);
        }

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void touchesBegan(IList<TouchPoint> touches)
        {
            if (ActiveTouches.Count == touches.Count)
            {
                startTime = Time.time;
            }
        }

        /// <inheritdoc />
        protected override void touchesMoved(IList<TouchPoint> touches)
        {
            totalMovement += ScreenPosition - PreviousScreenPosition;
        }

        /// <inheritdoc />
        protected override void touchesEnded(IList<TouchPoint> touches)
        {
            foreach (var touch in touches)
            {
                removedPoints.Add(touch);
                removedPointsTimes.Add(Time.time);
            }

            if (ActiveTouches.Count == 0)
            {
                if (totalMovement.magnitude/TouchManager.Instance.DotsPerCentimeter >= DistanceLimit || Time.time - startTime > TimeLimit)
                {
                    setState(GestureState.Failed);
                    return;
                }

                // Checking which points were removed in clusterExistenceTime seconds to set their centroid as cached screen position
                var cluster = new List<TouchPoint>();
                var minTime = Time.time - clusterExistenceTime;
                for (var i = removedPoints.Count - 1; i >= 0; i--)
                {
                    var point = removedPoints[i];
                    if (removedPointsTimes[i] >= minTime)
                    {
                        // Points must be over target when released
                        if (base.GetTargetHitResult(point.Position)) cluster.Add(point);
                    } else
                    {
                        break;
                    }
                }

                if (cluster.Count > 0)
                {
                    cachedScreenPosition = Cluster.Get2DCenterPosition(cluster);
                    cachedPreviousScreenPosition = Cluster.GetPrevious2DCenterPosition(cluster);
                    GetTargetHitResult(cachedScreenPosition, out cachedTargetHitResult);
                    setState(GestureState.Recognized);
                } else
                {
                    setState(GestureState.Failed);
                }
            }
        }

        /// <inheritdoc />
        protected override void touchesCancelled(IList<TouchPoint> touches)
        {
            setState(GestureState.Failed);
        }

        /// <inheritdoc />
        protected override void reset()
        {
            totalMovement = Vector2.zero;
            cachedScreenPosition = TouchPoint.InvalidPosition;
            cachedPreviousScreenPosition = TouchPoint.InvalidPosition;
            removedPoints.Clear();
            removedPointsTimes.Clear();
        }

        #endregion

        #region Private functions

        #endregion
    }
}