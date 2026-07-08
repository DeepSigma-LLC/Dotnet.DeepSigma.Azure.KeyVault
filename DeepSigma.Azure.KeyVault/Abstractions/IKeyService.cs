using Azure.Security.KeyVault.Keys;

namespace DeepSigma.Azure.KeyVault.Abstractions;

/// <summary>
/// Provides synchronous and asynchronous operations for managing keys in Azure Key Vault.
/// </summary>
public interface IKeyService
{
    // Async operations

    /// <summary>
    /// Gets a key from the vault, including its cryptographic material and properties.
    /// </summary>
    /// <param name="name">The name of the key.</param>
    /// <param name="version">The version of the key. If <see langword="null"/>, the latest version is retrieved.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="KeyVaultKey"/> with its key material and properties, or <see langword="null"/> if the key name is invalid.</returns>
    Task<KeyVaultKey?> GetKeyAsync(string name, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new key of the specified type in the vault.
    /// </summary>
    /// <param name="name">The name of the key to create.</param>
    /// <param name="keyType">The type of key to create (e.g., RSA, EC).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created <see cref="KeyVaultKey"/>, or <see langword="null"/> if the key name is invalid.</returns>
    Task<KeyVaultKey?> CreateKeyAsync(string name, KeyType keyType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new RSA key in the vault with the specified options.
    /// </summary>
    /// <param name="options">The RSA key creation options, including key size.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created <see cref="KeyVaultKey"/>, or <see langword="null"/> if the key name is invalid.</returns>
    Task<KeyVaultKey> CreateRsaKeyAsync(CreateRsaKeyOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new elliptic curve key in the vault with the specified options.
    /// </summary>
    /// <param name="options">The EC key creation options, including curve name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created <see cref="KeyVaultKey"/>, or <see langword="null"/> if the key name is invalid.</returns>
    Task<KeyVaultKey> CreateEcKeyAsync(CreateEcKeyOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the properties of a key without changing its cryptographic material.
    /// </summary>
    /// <param name="properties">The key properties to update.</param>
    /// <param name="keyOperations">The permitted key operations to set, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated <see cref="KeyVaultKey"/>, or <see langword="null"/> if the key name is invalid.</returns>
    Task<KeyVaultKey> UpdateKeyPropertiesAsync(KeyProperties properties, IEnumerable<KeyOperation>? keyOperations = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a key from the vault and waits for the operation to complete.
    /// </summary>
    /// <param name="name">The name of the key to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="DeletedKey"/> containing deletion information, or <see langword="null"/> if the key name is invalid.</returns>
    Task<DeletedKey?> DeleteKeyAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the properties of all keys in the vault.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of <see cref="KeyProperties"/>.</returns>
    IAsyncEnumerable<KeyProperties> GetPropertiesOfKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the properties of all versions of a key.
    /// </summary>
    /// <param name="name">The name of the key.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of <see cref="KeyProperties"/> for each version.</returns>
    IAsyncEnumerable<KeyProperties>? GetPropertiesOfKeyVersionsAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a deleted key, including its recovery information.
    /// </summary>
    /// <param name="name">The name of the deleted key.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="DeletedKey"/>, or <see langword="null"/> if the key name is invalid.</returns>
    Task<DeletedKey?> GetDeletedKeyAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all deleted keys in the vault.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of <see cref="DeletedKey"/>.</returns>
    IAsyncEnumerable<DeletedKey> GetDeletedKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes a soft-deleted key. This operation is not recoverable.
    /// </summary>
    /// <param name="name">The name of the deleted key to purge.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task PurgeDeletedKeyAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recovers a deleted key to its latest version and waits for the operation to complete.
    /// </summary>
    /// <param name="name">The name of the deleted key to recover.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The recovered <see cref="KeyVaultKey"/>, or <see langword="null"/> if the key name is invalid.</returns>
    Task<KeyVaultKey?> RecoverDeletedKeyAsync(string name, CancellationToken cancellationToken = default);

    // Sync operations

    /// <summary>
    /// Gets a key from the vault, including its cryptographic material and properties.
    /// </summary>
    /// <param name="name">The name of the key.</param>
    /// <param name="version">The version of the key. If <see langword="null"/>, the latest version is retrieved.</param>
    /// <returns>The <see cref="KeyVaultKey"/> with its key material and properties, or <see langword="null"/> if the key name is invalid.</returns>
    KeyVaultKey? GetKey(string name, string? version = null);

    /// <summary>
    /// Creates a new key of the specified type in the vault.
    /// </summary>
    /// <param name="name">The name of the key to create.</param>
    /// <param name="keyType">The type of key to create (e.g., RSA, EC).</param>
    /// <returns>The created <see cref="KeyVaultKey"/>, or <see langword="null"/> if the key name is invalid.</returns>
    KeyVaultKey? CreateKey(string name, KeyType keyType);

    /// <summary>
    /// Creates a new RSA key in the vault with the specified options.
    /// </summary>
    /// <param name="options">The RSA key creation options, including key size.</param>
    /// <returns>The created <see cref="KeyVaultKey"/>, or <see langword="null"/> if the key name is invalid.</returns>
    KeyVaultKey? CreateRsaKey(CreateRsaKeyOptions options);

    /// <summary>
    /// Creates a new elliptic curve key in the vault with the specified options.
    /// </summary>
    /// <param name="options">The EC key creation options, including curve name.</param>
    /// <returns>The created <see cref="KeyVaultKey"/>, or <see langword="null"/> if the key name is invalid.</returns>
    KeyVaultKey? CreateEcKey(CreateEcKeyOptions options);

    /// <summary>
    /// Updates the properties of a key without changing its cryptographic material.
    /// </summary>
    /// <param name="properties">The key properties to update.</param>
    /// <param name="keyOperations">The permitted key operations to set, or <see langword="null"/> to leave unchanged.</param>
    /// <returns>The updated <see cref="KeyVaultKey"/>, or <see langword="null"/> if the key name is invalid.</returns>
    KeyVaultKey? UpdateKeyProperties(KeyProperties properties, IEnumerable<KeyOperation>? keyOperations = null);

    /// <summary>
    /// Deletes a key from the vault and waits for the operation to complete.
    /// </summary>
    /// <param name="name">The name of the key to delete.</param>
    /// <returns>The <see cref="DeletedKey"/> containing deletion information, or <see langword="null"/> if the key name is invalid.</returns>
    DeletedKey? DeleteKey(string name);

    /// <summary>
    /// Lists the properties of all keys in the vault.
    /// </summary>
    /// <returns>An enumerable of <see cref="KeyProperties"/>.</returns>
    IEnumerable<KeyProperties> GetPropertiesOfKeys();

    /// <summary>
    /// Lists the properties of all versions of a key.
    /// </summary>
    /// <param name="name">The name of the key.</param>
    /// <returns>An enumerable of <see cref="KeyProperties"/> for each version, or <see langword="null"/> if the key name is invalid.</returns>
    IEnumerable<KeyProperties>? GetPropertiesOfKeyVersions(string name);

    /// <summary>
    /// Gets a deleted key, including its recovery information.
    /// </summary>
    /// <param name="name">The name of the deleted key.</param>
    /// <returns>The <see cref="DeletedKey"/>, or <see langword="null"/> if the key name is invalid.</returns>
    DeletedKey? GetDeletedKey(string name);

    /// <summary>
    /// Lists all deleted keys in the vault.
    /// </summary>
    /// <returns>An enumerable of <see cref="DeletedKey"/>.</returns>
    IEnumerable<DeletedKey> GetDeletedKeys();

    /// <summary>
    /// Permanently deletes a soft-deleted key. This operation is not recoverable.
    /// </summary>
    /// <param name="name">The name of the deleted key to purge.</param>
    void PurgeDeletedKey(string name);

    /// <summary>
    /// Recovers a deleted key to its latest version and waits for the operation to complete.
    /// </summary>
    /// <param name="name">The name of the deleted key to recover.</param>
    /// <returns>The recovered <see cref="KeyVaultKey"/>, or <see langword="null"/> if the key name is invalid.</returns>
    KeyVaultKey? RecoverDeletedKey(string name);
}
