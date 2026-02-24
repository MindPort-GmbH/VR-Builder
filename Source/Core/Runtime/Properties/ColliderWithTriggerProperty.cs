using System;
using UnityEngine;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Properties
{
    public class ColliderWithTriggerProperty : ProcessSceneObjectProperty
    {
        public class ColliderWithTriggerEventArgs : EventArgs
        {
            public readonly GameObject CollidedObject;
            public ColliderWithTriggerEventArgs(GameObject collidedObject)
            {
                CollidedObject = collidedObject;
            }
        }

        public event EventHandler<ColliderWithTriggerEventArgs> EnteredTrigger;
        public event EventHandler<ColliderWithTriggerEventArgs> ExitedTrigger;

        protected override void OnEnable()
        {
            base.OnEnable();

            Collider[] colliders = GetComponents<Collider>();
            if (colliders.Length == 0)
            {
                Debug.LogErrorFormat("Object '{0}' with ColliderProperty must have at least one Collider attached.", SceneObject.GameObject.name);
            }
            else
            {
                if (CheckIfObjectHasTriggerCollider() == false)
                {
                    Debug.LogErrorFormat("Object '{0}' with ColliderProperty must have at least one Collider with isTrigger set to true.", SceneObject.GameObject.name);
                }
            }
        }

        private bool CheckIfObjectHasTriggerCollider()
        {
            bool hasTriggerCollider = false;

            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider collider in colliders)
            {
                if (collider.enabled && collider.isTrigger)
                {
                    hasTriggerCollider = true;
                    break;
                }
            }

            return hasTriggerCollider;
        }

        /// <summary>
        /// Checks whether a transform position is inside or on any enabled trigger collider on this object.
        /// </summary>
        /// <param name="targetTransform">The transform whose world position should be evaluated.</param>
        /// <returns>
        /// True if <see cref="Collider.ClosestPoint(Vector3)"/> equals the transform position for at least one
        /// enabled trigger collider; otherwise false.
        /// </returns>
        public bool IsTransformInsideTrigger(Transform targetTransform)
        {
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider collider in colliders)
            {
                if (collider.enabled && collider.isTrigger)
                {
                    Vector3 closest = collider.ClosestPoint(targetTransform.position);
                    bool inside = closest == targetTransform.position;

                    if (inside)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (EnteredTrigger != null)
            {
                EnteredTrigger.Invoke(this, new ColliderWithTriggerEventArgs(other.gameObject));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (ExitedTrigger != null)
            {
                ExitedTrigger.Invoke(this, new ColliderWithTriggerEventArgs(other.gameObject));
            }
        }

        /// <summary>
        /// Instantaneously move target inside the collider and fire the event.
        /// </summary>
        /// <param name="target"></param>
        public void FastForwardEnter(ISceneObject target)
        {
            target.GameObject.transform.rotation = transform.rotation;
            target.GameObject.transform.position = transform.position;

            if (EnteredTrigger != null)
            {
                EnteredTrigger.Invoke(this, new ColliderWithTriggerEventArgs(target.GameObject));
            }
        }
    }
}
