using UnityEngine;

namespace VRBuilder.Core.Utils
{
    public static class GameObjectExtension
    {
        /// <summary>
        /// Set layer of GameObject and children.
        /// For more efficiency Generic by <Component> is used. In case for specific Component GetComponentsInChildren executes faster, then Transform by default.
        /// </summary>
        /// <param name="gameObject">GameObject</param>
        /// <param name="layer">layer num.</param>
        /// <param name="includeChildren">Toggle set for children</param>
        public static void SetLayer<T>(this GameObject gameObject, int layer, bool includeChildren = false) where T : Component
        {
            gameObject.layer = layer;
            if (includeChildren == false) return;

            T[] children = gameObject.GetComponentsInChildren<T>(true);
            for (int i = 0; i < children.Length; i++)
                children[i].gameObject.layer = layer;
        }

        /// <summary>
        /// Removes a gameobject child with a specific name.
        /// As it uses DestroyImmediate it should only be used during editor time.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="name of child to remove"></param>
        public static void RemoveChildWithNameImmediate(this GameObject gameObject, string name)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Transform child = gameObject.transform.GetChild(i);
                if (child.name == name)
                {
                    Object.DestroyImmediate(child.gameObject);
                    i--;
                }
            }
        }
    }
}
