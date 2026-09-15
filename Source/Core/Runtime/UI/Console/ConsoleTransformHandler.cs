using UnityEngine;
using VRBuilder.Core.Configuration;

namespace VRBuilder.UI.Console
{
    /// <summary>
    /// Ensures the console is displayed in a position relevant to the user.
    /// </summary>
    public class ConsoleTransformHandler : MonoBehaviour
    {
        [SerializeField] private float distanceFromHead = 2f;

        void Update()
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            if (RuntimeConfigurator.Configuration.User == null)
            {
                return;
            }

            transform.position = RuntimeConfigurator.Configuration.User.Head.position + new Vector3(0, 0, distanceFromHead);
        }
    }
}

