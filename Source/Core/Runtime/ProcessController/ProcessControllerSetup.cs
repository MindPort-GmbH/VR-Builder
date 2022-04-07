using System.IO;
using UnityEngine;

namespace VRBuilder.ProcessController
{
    /// <summary>
    /// Allows to select a desired ProcessController.
    /// </summary>
    public class ProcessControllerSetup : MonoBehaviour
    {
        private enum ProcessMode
        {
            Default = 0,
            Standalone = 1
        }
        
        [SerializeField]
        private ProcessMode processMode;
        
        [SerializeField, HideInInspector]
        private GameObject processControllerPrefab = null;

        private GameObject currentControllerInstance = null;

        protected virtual void Start()
        {
            InstantiateSpectator();
        }

        private void InstantiateSpectator()
        {
            if (processControllerPrefab == null)
            {
                throw new FileNotFoundException($"No process controller prefabs set." );
            }
            
            if (currentControllerInstance != null)
            {
                Destroy(currentControllerInstance);
            }
            
            currentControllerInstance = Instantiate(processControllerPrefab);
        }
    }
}
