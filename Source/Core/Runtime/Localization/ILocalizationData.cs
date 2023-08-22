namespace VRBuilder.Core.Localization
{
    /// <summary>
    /// Entity data that contains localization data.
    /// </summary>
    public interface ILocalizationData
    {
        /// <summary>
        /// Returns the string localization table for the current entity.
        /// </summary>
        string StringLocalizationTable { get; set; }

        /// <summary>
        /// Returns the asset localization table for the current entity.
        /// </summary>
        string AssetLocalizationTable { get; set; }
    }
}
