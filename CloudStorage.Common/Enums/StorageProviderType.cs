namespace CloudStorage.Common.Enums
{
    /// <summary>
    /// Specifies the type of storage provider used for storing data.
    /// </summary>
    /// <remarks>This enumeration defines the supported storage providers. Additional providers may be added
    /// in the future.</remarks>
    public enum StorageProviderType
    {
        AzureBlob = 1,

        AwsS3 = 2
        //Future providers can be added here like GoogleCloudStorage, IBMCloudStorage etc.
    }
}
