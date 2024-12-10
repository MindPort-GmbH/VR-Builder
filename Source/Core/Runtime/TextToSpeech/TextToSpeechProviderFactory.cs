using System;
using System.Collections.Generic;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.TextToSpeech.Configuration;
using VRBuilder.Core.TextToSpeech.Providers;
using VRBuilder.Core.Utils;
using VRBuilder.Unity;

namespace VRBuilder.Core.TextToSpeech
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
        public interface ITextToSpeechCreator
        {
            ITextToSpeechProvider Create(ITextToSpeechConfiguration configuration);
        }

        /// <summary>
        /// Non-generic TTS creator.
        /// </summary>
        public class BaseCreator : ITextToSpeechCreator
        {
            private Type textToSpeechProviderType;

            public BaseCreator(Type textToSpeechProviderType)
            {
                if (typeof(ITextToSpeechProvider).IsAssignableFrom(textToSpeechProviderType) == false)
                {
                    throw new InvalidProviderException($"Type '{textToSpeechProviderType.Name}' is not a valid text to speech provider.");
                }

                this.textToSpeechProviderType = textToSpeechProviderType;
            }
            public ITextToSpeechProvider Create(ITextToSpeechConfiguration configuration)
            {
                ITextToSpeechProvider provider = Activator.CreateInstance(textToSpeechProviderType) as ITextToSpeechProvider;
                provider?.SetConfig(configuration);
                return provider;
            }
        }

        private readonly Dictionary<string, ITextToSpeechCreator> registeredProvider = new Dictionary<string, ITextToSpeechCreator>();

        public TextToSpeechProviderFactory()
        {
            IEnumerable<Type> providers = ReflectionUtils.GetConcreteImplementationsOf<ITextToSpeechProvider>();

            foreach (Type provider in providers)
            {
                RegisterProvider(provider);
            }
        }

        /// <summary>
        /// Add a provider of the specified type.
        /// </summary>
        public void RegisterProvider(Type textToSpeechProviderType)
        {
            if (typeof(ITextToSpeechProvider).IsAssignableFrom(textToSpeechProviderType) == false)
            {
                throw new InvalidProviderException($"Type '{textToSpeechProviderType.Name}' is not a valid text to speech provider, therefore it cannot be registered.");
            }

            registeredProvider.Add(textToSpeechProviderType.Name, new BaseCreator(textToSpeechProviderType));
        }

        /// <summary>
        ///  Creates a provider, always loads the actual text to speech config to set it up.
        /// </summary>
        public ITextToSpeechProvider CreateProvider()
        {
            //get selected configuration
            ITextToSpeechConfiguration ttsConfiguration = RuntimeConfigurator.Configuration.GetTextToSpeechConfiguration();
            return CreateProvider(ttsConfiguration);
        }

        /// <summary>
        /// Creates a provider with given config.
        /// </summary>
        public ITextToSpeechProvider CreateProvider(ITextToSpeechConfiguration configuration)
        {
            TextToSpeechSettings settings = RuntimeConfigurator.Configuration.GetTextToSpeechSettings();

            if (string.IsNullOrEmpty(settings.Provider))
            {
                throw new NoConfigurationFoundException($"There is not a valid provider set in '{configuration.GetType().Name}'!");
            }

            if (!registeredProvider.ContainsKey(settings.Provider))
            {
                throw new NoMatchingProviderFoundException($"No matching provider with name '{settings.Provider}' found!");
            }

            ITextToSpeechProvider provider = registeredProvider[settings.Provider].Create(configuration);

            return provider;
        }

        public class NoMatchingProviderFoundException : Exception
        {
            public NoMatchingProviderFoundException(string msg) : base(msg) { }
        }

        public class NoConfigurationFoundException : Exception
        {
            public NoConfigurationFoundException(string msg) : base(msg) { }
        }

        public class InvalidProviderException : Exception
        {
            public InvalidProviderException(string msg) : base(msg) { }
        }
    }
}
