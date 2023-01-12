using System;
using System.Collections.Generic;
using System.Linq;
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
        public interface ITextToSpeechCreator
        {
            ITextToSpeechProvider Create(TextToSpeechConfiguration configuration);
        }

        /// <summary>
        /// Easy basic creator which requires an empty constructor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class BaseCreator<T> : ITextToSpeechCreator where T : ITextToSpeechProvider, new()
        {
            public ITextToSpeechProvider Create(TextToSpeechConfiguration configuration)
            {
                T provider = new T();
                provider.SetConfig(configuration);
                return provider;
            }
        }

        public class BaseCreator : ITextToSpeechCreator
        {
            private Type textToSpeechProviderType;

            public BaseCreator(Type textToSpeechProviderType)
            {
                this.textToSpeechProviderType= textToSpeechProviderType;
            }
            public ITextToSpeechProvider Create(TextToSpeechConfiguration configuration)
            {
                ITextToSpeechProvider provider = Activator.CreateInstance(textToSpeechProviderType) as ITextToSpeechProvider;
                provider.SetConfig(configuration);
                return provider;
            }
        }

        private readonly Dictionary<string, ITextToSpeechCreator> registeredProvider = new Dictionary<string, ITextToSpeechCreator>();

        public TextToSpeechProviderFactory()
        {
            IEnumerable<Type> providers = ReflectionUtils.GetConcreteImplementationsOf<ITextToSpeechProvider>();

            foreach(Type provider in providers)
            {
                RegisterProvider(provider);
            }

            //RegisterProvider<WatsonTextToSpeechProvider>();
            //RegisterProvider<GoogleTextToSpeechProvider>();
            //RegisterProvider<MicrosoftSapiTextToSpeechProvider>();
        }

        /// <summary>
        /// Add or overwrites a provider of type T.
        /// </summary>
        public void RegisterProvider<T>() where T : ITextToSpeechProvider, new()
        {
            registeredProvider.Add(typeof(T).Name, new BaseCreator<T>());
        }

        public void RegisterProvider(Type textToSpeechProviderType)
        {
            registeredProvider.Add(textToSpeechProviderType.Name, new BaseCreator(textToSpeechProviderType));
        }

        /// <summary>
        ///  Creates a provider, always loads the actual text to speech config to set it up.
        /// </summary>
        public ITextToSpeechProvider CreateProvider()
        {
            TextToSpeechConfiguration ttsConfiguration = TextToSpeechConfiguration.LoadConfiguration();
            return CreateProvider(ttsConfiguration);
        }

        /// <summary>
        /// Creates a provider with given config.
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

            ITextToSpeechProvider provider = registeredProvider[configuration.Provider].Create(configuration);

            //if (configuration.UseStreamingAssetFolder)
            //{
            //    provider = new FileTextToSpeechProvider(provider, configuration);
            //}

            return provider;
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