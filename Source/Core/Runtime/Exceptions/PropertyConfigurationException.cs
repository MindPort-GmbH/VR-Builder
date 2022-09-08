using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Exceptions
{
    ///<author email="a.schaub@lefx.de">Aron Schaub</author>
    public class PropertyConfigurationException : ProcessException
    {
        public PropertyConfigurationException(string message) : base(message)
        {
        }

        public PropertyConfigurationException(ISceneObjectProperty sourceObject) : base($"Exception while configure '{sourceObject}'")
        {
        }
    }
}
