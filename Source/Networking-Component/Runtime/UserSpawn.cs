using UnityEngine;

namespace VRBuilder.Networking
{
    public class UserSpawn : MonoBehaviour
    {
        [SerializeField]
        private int id = 0;

        public int Id => id;
    }
}