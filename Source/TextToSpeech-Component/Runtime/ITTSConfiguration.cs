using UnityEngine.Localization;

namespace Source.TextToSpeech_Component.Runtime
{
//    public class TextToSpeechSO: ScriptableObject {}
//    public abstract class TextToSpeechConfiguration<T> : SettingsObject<T> where T : ScriptableObject, new()
    
    public interface ITextToSpeechConfiguration
    {
        static ITextToSpeechConfiguration LoadConfiguration()
        {
            //TODO use reflection api to
            return null;
        }

        object GetUniqueIdentifier();

        bool IsCached(Locale locale);
    }
}