using CloudStorage.Common.Enums;

namespace CloudStorage.DomainService.Interfaces
{
    /// <summary>
    /// Defines a factory for creating storage service instances based on the specified provider type and credentials.
    /// </summary>
    /// <remarks>This interface is designed to abstract the creation of storage services, allowing clients to
    /// obtain an appropriate implementation of <see cref="IStorageService"/> for a given <see
    /// cref="StorageProviderType"/>.</remarks>
    public interface IStorageProvideFactory
    {

        /// <summary>
        /// Creates an appropriate storage service based on provider type
        /// </summary>
        /// <param name="providerType">Type of storage provider</param>
        /// <param name="credentialName">Name of the credential configuration</param>
        /// <returns>Storage service instance</returns>
        IStorageService CreateStorageService(StorageProviderType providerType, string credentialName);
    }
}

