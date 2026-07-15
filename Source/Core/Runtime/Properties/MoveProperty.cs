using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Primitives;
using VRBuilder.Core.Runtime.Utils;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Properties
{
    public class MoveProperty : ProcessSceneObjectProperty, IMoveProperty
    {
        public void DisablePhysics()
        {
            //TODO: we are not really waiting for the authority here. was always like that
            RuntimeConfigurator.Configuration.SceneObjectManager.RequestAuthority(SceneObject);

            Rigidbody movingRigidbody = gameObject.GetComponent<Rigidbody>();
            if (movingRigidbody != null && movingRigidbody.isKinematic == false)
            {
#if UNITY_6000
                movingRigidbody.linearVelocity = Vector3.zero;
#else
                    movingRigidbody.velocity = Vector3.zero;
#endif
                movingRigidbody.angularVelocity = Vector3.zero;
            }
        }

        public void MoveTo(ISceneObject finalPositionValue, float progress, IAnimationCurve animationCurve = null)
        {
            Transform movingTransform = gameObject.transform;
            Transform targetPositionTransform = finalPositionValue.GameObject.transform;

            Vector3 initialPosition = movingTransform.position;
            Quaternion initialRotation = movingTransform.rotation;
            RuntimeConfigurator.Configuration.SceneObjectManager.RequestAuthority(SceneObject);
            if (animationCurve != null)
            {
                progress = animationCurve.ToUnity().Evaluate(progress);
            }
            if (progress < 1f)
            {
                movingTransform.position = initialPosition + (targetPositionTransform.position - initialPosition) * progress;
                movingTransform.rotation = Quaternion.Euler(initialRotation.eulerAngles + (targetPositionTransform.rotation.eulerAngles - initialRotation.eulerAngles) * progress);
            }
            else
            {
                movingTransform.position = targetPositionTransform.position;
                movingTransform.rotation = targetPositionTransform.rotation;
            }
        }

        public void EnablePhysics()
        {
            RuntimeConfigurator.Configuration.SceneObjectManager.RequestAuthority(SceneObject);
            Rigidbody movingRigidbody = gameObject.GetComponent<Rigidbody>();
            if (movingRigidbody != null && movingRigidbody.isKinematic == false)
            {
#if UNITY_6000
                movingRigidbody.linearVelocity = Vector3.zero;
#else
                    movingRigidbody.velocity = Vector3.zero;
#endif
                movingRigidbody.angularVelocity = Vector3.zero;
            }
        }
    }
}