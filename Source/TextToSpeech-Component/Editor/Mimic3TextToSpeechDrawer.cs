using System;
using VRBuilder.Editor.UI.Drawers;
using VRBuilder.TextToSpeech;

namespace VRBuilder.Editor.TextToSpeech.UI
{
    ///<author email="a.schaub@lefx.de">Aron Schaub</author>
    public class Mimic3TextToSpeechDrawer : TextToSpeechDrawerAttribute
    {
        public Mimic3TextToSpeechDrawer() : base(typeof(Mimic3TextToSpeechProvider))
        {
        }
    }
}