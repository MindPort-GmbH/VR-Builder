using System;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Utils;
using UnityEngine;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Configurator to set the process runtime configuration which is used by a process during its execution.
    /// There has to be one and only one process runtime configurator game object per scene.
    /// </summary>
    public sealed class ProcessContainer : MonoBehaviour
    {
        /// <summary>
        /// The event that fires when a process mode or runtime configuration changes.
        /// </summary>
        public static event EventHandler<ModeChangedEventArgs> ModeChanged;

        /// <summary>
        /// The event that fires when a process runtime configuration changes.
        /// </summary>
        public static event EventHandler<EventArgs> RuntimeConfigurationChanged;

        /// <summary>
        /// Fully qualified name of the runtime configuration used.
        /// This field is magically filled by <see cref="RuntimeConfiguratorEditor"/>
        /// </summary>
        [SerializeField]
        private string runtimeConfigurationName = typeof(DefaultRuntimeConfiguration).AssemblyQualifiedName;

        /// <summary>
        /// Process name which is selected.
        /// This field is magically filled by <see cref="RuntimeConfiguratorEditor"/>
        /// </summary>
        [SerializeField]
        private string selectedProcessStreamingAssetsPath = "";

        private BaseRuntimeConfiguration runtimeConfiguration;

        /// <summary>
        /// Shortcut to get the <see cref="IRuntimeConfiguration"/> of the instance.
        /// </summary>
        public BaseRuntimeConfiguration Configuration
        {
            get
            {
                if (runtimeConfiguration != null)
                {
                    return runtimeConfiguration;
                }

                Type type = ReflectionUtils.GetTypeFromAssemblyQualifiedName(runtimeConfigurationName);

                if (type == null)
                {
                    Debug.LogErrorFormat("IRuntimeConfiguration type '{0}' cannot be found. Using '{1}' instead.", runtimeConfigurationName, typeof(DefaultRuntimeConfiguration).AssemblyQualifiedName);
                    type = typeof(DefaultRuntimeConfiguration);
                }
#pragma warning disable 0618
                IRuntimeConfiguration config = (IRuntimeConfiguration)ReflectionUtils.CreateInstanceOfType(type);
                if (config is BaseRuntimeConfiguration configuration)
                {
                    Configuration = configuration;
                }
                else
                {
                    Debug.LogWarning("Your runtime configuration only extends the interface IRuntimeConfiguration, please consider moving to BaseRuntimeConfiguration as base class.");
                    Configuration = new RuntimeConfigWrapper(config);
                }
#pragma warning restore 0618
                return runtimeConfiguration;
            }
            set
            {
                if (value == null)
                {
                    Debug.LogError("Process runtime configuration cannot be null.");
                    return;
                }

                if (runtimeConfiguration == value)
                {
                    return;
                }

                if (runtimeConfiguration != null)
                {
                    runtimeConfiguration.Modes.ModeChanged -= RuntimeConfigurationModeChanged;
                }

                value.Modes.ModeChanged += RuntimeConfigurationModeChanged;

                runtimeConfigurationName = value.GetType().AssemblyQualifiedName;
                runtimeConfiguration = value;

                EmitRuntimeConfigurationChanged();
            }
        }

        /// <summary>
        /// Returns the assembly qualified name of the runtime configuration.
        /// </summary>
        public string GetRuntimeConfigurationName()
        {
            return runtimeConfigurationName;
        }

        /// <summary>
        /// Sets the runtime configuration name, expects an assembly qualified name.
        /// </summary>
        public void SetRuntimeConfigurationName(string configurationName)
        {
            runtimeConfigurationName = configurationName;
        }

        /// <summary>
        /// Returns the path to the selected process.
        /// </summary>
        public string GetSelectedProcess()
        {
            return selectedProcessStreamingAssetsPath;
        }

        /// <summary>
        /// Sets the path to the selected process.
        /// </summary>
        public void SetSelectedProcess(string path)
        {
            selectedProcessStreamingAssetsPath = path;
        }

        private void Awake()
        {
            Configuration.SceneObjectRegistry.RegisterAll();
            RuntimeConfigurationChanged += HandleRuntimeConfigurationChanged;
        }

        private void OnDestroy()
        {
            ModeChanged = null;
            RuntimeConfigurationChanged = null;
        }

        private void EmitModeChanged()
        {
            ModeChanged?.Invoke(this, new ModeChangedEventArgs(runtimeConfiguration.Modes.CurrentMode));
        }

        private void EmitRuntimeConfigurationChanged()
        {
            RuntimeConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HandleRuntimeConfigurationChanged(object sender, EventArgs args)
        {
            EmitModeChanged();
        }

        private void RuntimeConfigurationModeChanged(object sender, ModeChangedEventArgs modeChangedEventArgs)
        {
            EmitModeChanged();
        }
    }
}
