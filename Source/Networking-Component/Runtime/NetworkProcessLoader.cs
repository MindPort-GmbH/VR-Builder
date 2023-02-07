using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;

namespace VRBuilder.Networking
{
    /// <summary>
    /// Loads and starts the process currently selected in the 'PROCESS_CONFIGURATION' gameObject.
    /// </summary>
    public class NetworkProcessLoader : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                StartCoroutine(StartProcess());
            }
        }

        private IEnumerator StartProcess()
        {
            // Load process from a file.
            string processPath = RuntimeConfigurator.Instance.GetSelectedProcess();

            // Try to load the in the PROCESS_CONFIGURATION selected process.

            Task<IProcess> loadProcess = RuntimeConfigurator.Configuration.LoadProcess(processPath);
            while (!loadProcess.IsCompleted)
            {
                yield return null;
            }

            var process = loadProcess.Result;

            // Initializes the process.
            ProcessRunner.Initialize(process);

            // Runs the process.
            ProcessRunner.Run();
        }
    }
}
