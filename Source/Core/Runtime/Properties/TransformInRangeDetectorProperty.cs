// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.Events;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Checks if a transform is in range of another transform. with a provided DetectionRange
    /// </summary>
    public class TransformInRangeDetectorProperty : ProcessSceneObjectProperty, ITransformInRangeDetectorProperty
    {
        private bool isTransformInRange = false;
        private Transform trackedTransform;

        public float DetectionRange { get; set; }

        [SerializeField]
        private UnityEvent<RangeEventArgs> enteredRangeAction;

        [SerializeField]
        private UnityEvent<RangeEventArgs> exitedRangeAction;

        public event Action<RangeEventArgs> EnteredRangeAction;
        public event Action<RangeEventArgs> ExitedRangeAction;

        public void Awake()
        {
            enteredRangeAction.AddListener(OnEnteredRange);
            exitedRangeAction.AddListener(OnExitedRange);
        }

        private void OnExitedRange(RangeEventArgs args)
        {
            ExitedRangeAction?.Invoke(args);
        }

        private void OnEnteredRange(RangeEventArgs args)
        {
            EnteredRangeAction?.Invoke(args);
        }

        private void Update()
        {
            Refresh();
        }

        /// <summary>
        /// Check if there are transforms in range and fire the appropriate events.
        /// </summary>
        public void Refresh()
        {
            if (trackedTransform == null)
            {
                return;
            }

            bool isInsideArea = IsTargetInsideRange();

            if (isInsideArea && isTransformInRange == false)
            {
                EmitEnteredArea();
                isTransformInRange = true;
            }
            else if (isInsideArea == false && isTransformInRange)
            {
                EmitExitedArea();
                isTransformInRange = false;
            }
        }

        public virtual bool IsTargetInsideRange()
        {
            return Vector3.Distance(transform.position, trackedTransform.position) < DetectionRange;
        }

        public void SetTrackedTransform(ISceneObject sceneObject)
        {
            trackedTransform = sceneObject.GameObject().transform;
        }

        public void DestroySelf()
        {
            Destroy(this);
        }

        protected void EmitEnteredArea()
        {
            if (EnteredRangeAction != null)
            {
                EnteredRangeAction.Invoke(new RangeEventArgs(trackedTransform.gameObject));
            }
        }

        protected void EmitExitedArea()
        {
            if (ExitedRangeAction != null)
            {
                ExitedRangeAction.Invoke(new RangeEventArgs(trackedTransform.gameObject));
            }
        }

        public void ForceMoveToTracked()
        {
            transform.position = trackedTransform.position;
        }

        private void OnDisable()
        {
            enteredRangeAction.RemoveListener(OnEnteredRange);
            exitedRangeAction.RemoveListener(OnExitedRange);
        }
    }
}