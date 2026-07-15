using UnityEngine;
using VRBuilder.Core.Primitives;
using VRBuilder.Core.Runtime.Utils;
using VRBuilder.Core.Utils.Bezier;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Path property that generates a path from a <see cref="BezierSpline"/>.
    /// </summary>
    [RequireComponent(typeof(BezierSpline))]
    public class BezierSplinePathProperty : ProcessSceneObjectProperty, IPathProperty
    {
        private BezierSpline spline;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (spline == null)
            {
                spline = GetComponent<BezierSpline>();
            }
        }

        /// <inheritdoc/>
        public IVector3 GetPoint(float t)
        {
            return spline.GetPoint(t).ToVector3Data();
        }

        /// <inheritdoc/>
        public IVector3 GetDirection(float t)
        {
            return spline.GetDirection(t).ToVector3Data();
        }
    }
}
