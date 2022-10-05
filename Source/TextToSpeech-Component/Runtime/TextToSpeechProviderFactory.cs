using System;
using System.Collections.Generic;
using VRBuilder.Core.Utils;
using VRBuilder.Unity;

namespace VRBuilder.TextToSpeech
{
    /// <summary>
    /// This factory creates and provides <see cref="ITextToSpeechProvider"/>s.
    /// They are chosen by name, the following providers are registered by default:
    /// - MicrosoftSapiTextToSpeechProvider
    /// - WatsonTextToSpeechProvider
    /// - GoogleTextToSpeechProvider
    /// </summary>
    public class TextToSpeechProviderFactory : Singleton<TextToSpeechProviderFactory>
    {
        private readonly Dictionary<string, Type> registeredProvider = new();

        public TextToSpeechProviderFactory()
        {
            foreach (var type in ReflectionUtils.GetFinalImplementationsOf<ITextToSpeechProvider>()) 
                registeredProvider.Add(type.Name, type);
        }
        
        /// <summary>
        /// Creates an provider with given config.
        /// </summary>
        public ITextToSpeechProvider CreateProvider(TextToSpeechConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.Provider))
            {
                throw new NoConfigurationFoundException($"There is not a valid provider set in '{configuration.GetType().Name}'!");
            }

            if (!registeredProvider.ContainsKey(configuration.Provider))
            {
                throw new NoMatchingProviderFoundException($"No matching provider with name '{configuration.Provider}' found!");
            }

            var type = registeredProvider[configuration.Provider];
            if (Activator.CreateInstance(type) is ITextToSpeechProvider provider)
            {
                provider.SetConfig(configuration);

                if (configuration.UseStreamingAssetFolder)
                {
                    provider = new FileTextToSpeechProvider(provider, configuration);
                }

                return provider;
            }

            return null;
        }

        public class NoMatchingProviderFoundException : Exception
        {
            public NoMatchingProviderFoundException(string msg) : base (msg) { }
        }

        public class NoConfigurationFoundException : Exception
        {
            public NoConfigurationFoundException(string msg) : base(msg) { }
        }
    }
}