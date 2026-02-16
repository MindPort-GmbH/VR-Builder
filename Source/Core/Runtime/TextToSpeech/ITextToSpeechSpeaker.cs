using System.Collections.Generic;
using VRBuilder.Core.TextToSpeech.Configuration;

namespace VRBuilder.Core.TextToSpeech
{
    /// <summary>
    /// ITextToSpeechSpeaker allows supporting different voices for Text-To-Speech
    /// </summary>
    public interface ITextToSpeechSpeaker
    {
        /// <summary>
        /// Returns a list of all speakers that are set in the <see cref="ITextToSpeechConfiguration"/>
        /// </summary>
        /// <returns></returns>
        public List<string> GetSpeaker();
    }
}