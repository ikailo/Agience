namespace Agience.Authority.Manage.Services
{
    public static class MetadataHelper
    {
        /// <summary>
        /// Gets a value from the metadata dictionary by key.
        /// </summary>
        public static string? GetMetadataValue(Dictionary<string, string> metadata, string key)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

            return metadata.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Sets a value in the metadata dictionary.
        /// </summary>
        public static void SetMetadataValue(Dictionary<string, string> metadata, string key, string value)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

            metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

}
