using System.Linq;
using UnityEngine.Localization;
using VRBuilder.Core.Utils;
using VRBuilder.TextToSpeech;

namespace Source.TextToSpeech_Component.Runtime
{
//    public class TextToSpeechSO: ScriptableObject {}
//    public abstract class TextToSpeechConfiguration<T> : SettingsObject<T> where T : ScriptableObject, new()
    
    public interface ITextToSpeechConfiguration
    {
        static ITextToSpeechConfiguration LoadConfiguration()
        {
            return null;
        }

        string GetUniqueIdentifier(string text, string md5Hash, Locale locale);

        bool IsCached(Locale locale, string localizedContent);
    }
}