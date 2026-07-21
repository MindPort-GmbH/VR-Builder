// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.Events;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Properties
{
    public class ColliderWithTriggerProperty : ProcessSceneObjectProperty, IColliderWithTriggerProperty
    {
        [Header("Events")]
        [SerializeField]
        private UnityEvent<ColliderWithTriggerEventArgs> triggerEntered = new UnityEvent<ColliderWithTriggerEventArgs>();

        [SerializeField]
        private UnityEvent<ColliderWithTriggerEventArgs> triggerExited = new UnityEvent<ColliderWithTriggerEventArgs>();

        private Action<ColliderWithTriggerEventArgs> enteredTriggerAction;
        private Action<ColliderWithTriggerEventArgs> exitedTriggerAction;

        /// <inheritdoc/>
        public event Action<ColliderWithTriggerEventArgs> EnteredTriggerAction
        {
            add => enteredTriggerAction += value;
            remove => enteredTriggerAction -= value;
        }

        /// <inheritdoc/>
        public event Action<ColliderWithTriggerEventArgs> ExitedTriggerAction
        {
            add => exitedTriggerAction += value;
            remove => exitedTriggerAction -= value;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            triggerEntered.AddListener(OnTriggerEnter);
            triggerExited.AddListener(OnTriggerExit);

            Collider[] colliders = GetComponents<Collider>();
            if (colliders.Length == 0)
            {
                Debug.LogErrorFormat("Object '{0}' with ColliderProperty must have at least one Collider attached.", SceneObject);
            }
            else
            {
                if (CheckIfObjectHasTriggerCollider() == false)
                {
                    Debug.LogErrorFormat("Object '{0}' with ColliderProperty must have at least one Collider with isTrigger set to true.", SceneObject);
                }
            }
        }

        private void OnTriggerEnter(ColliderWithTriggerEventArgs args)
        {
            enteredTriggerAction?.Invoke(args);
        }

        private void OnTriggerExit(ColliderWithTriggerEventArgs args)
        {
            exitedTriggerAction?.Invoke(args);
        }

        private bool CheckIfObjectHasTriggerCollider()
        {
            bool hasTriggerCollider = false;

            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                if (col.enabled && col.isTrigger)
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
        /// <param name="sceneObject">the Scene Object. gets the transform whose world position should be evaluated.</param>
        /// <returns>
        /// True if <see cref="Collider.ClosestPoint(Vector3)"/> equals the transform position for at least one
        /// enabled trigger collider; otherwise false.
        /// </returns>
        public bool IsTransformInsideTrigger(ISceneObject sceneObject)
        {
            var targetTransform = sceneObject.GameObject().transform;
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider co in colliders)
            {
                if (co.enabled && co.isTrigger)
                {
                    Vector3 closest = co.ClosestPoint(targetTransform.position);
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
            OnTriggerEnter(new ColliderWithTriggerEventArgs(other.gameObject));
        }

        private void OnTriggerExit(Collider other)
        {
            OnTriggerExit(new ColliderWithTriggerEventArgs(other.gameObject));
        }

        /// <summary>
        /// Instantaneously move target inside the collider and fire the event.
        /// </summary>
        /// <param name="sceneObject"></param>
        public void FastForwardEnter(ISceneObject sceneObject)
        {
            var collidedObject = sceneObject.GameObject();
            collidedObject.transform.rotation = transform.rotation;
            collidedObject.transform.position = transform.position;

            OnTriggerExit(new ColliderWithTriggerEventArgs(collidedObject));
        }
    }
}