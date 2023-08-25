namespace VRBuilder.Core.Localization
{
    /// <summary>
    /// Interface for localized text content.
    /// </summary>
    public interface ILocalizedContent
    {
        /// <summary>
        /// Returns localized content according to the localization and process configuration.
        /// </summary>        
        string GetLocalizedContent();
    }
}
