using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Networking
{
    public class NetworkRuntimeConfiguration : DefaultRuntimeConfiguration
    {
        public override ProcessSceneObject User
        {
            get
            {
                IEnumerable<UserSceneObject> users = GameObject.FindObjectsOfType<UserSceneObject>();

                IEnumerable<NetworkObject> networkUsers = users.Select(user => user.GetComponentInParent<NetworkObject>()).Where(networkObject => networkObject != null);

                NetworkObject user = networkUsers.FirstOrDefault(user => user.IsOwner);

                return user != null ? user.GetComponentInChildren<UserSceneObject>() : users.FirstOrDefault();   
            }
        }
    }
}