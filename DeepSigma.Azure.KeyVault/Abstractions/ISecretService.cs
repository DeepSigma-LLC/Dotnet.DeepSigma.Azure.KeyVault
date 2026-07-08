using Azure.Security.KeyVault.Secrets;

namespace DeepSigma.Azure.KeyVault.Abstractions;

/// <summary>
/// Provides synchronous and asynchronous operations for managing secrets in Azure Key Vault.
/// </summary>
public interface ISecretService
{
    // Async operations

    /// <summary>
    /// Gets a secret from the vault, including its value and properties.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="version">The version of the secret. If <see langword="null"/>, the latest version is retrieved.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="KeyVaultSecret"/> with its value and properties, or <see langword="null"/> if the secret name is invalid.</returns>
    Task<KeyVaultSecret?> GetSecretAsync(string name, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the value of a secret as a string.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The secret value as a string, or <see langword="null"/> if the secret name is invalid.</returns>
    Task<string?> GetSecretValueAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a secret in the vault, creating a new version if the secret already exists.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="value">The value of the secret.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="KeyVaultSecret"/> that was set, including its properties, or <see langword="null"/> if the secret name is invalid.</returns>
    Task<KeyVaultSecret?> SetSecretAsync(string name, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the properties of a secret without changing its value.
    /// </summary>
    /// <param name="properties">The secret properties to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated <see cref="SecretProperties"/>.</returns>
    Task<SecretProperties> UpdateSecretPropertiesAsync(SecretProperties properties, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a secret from the vault and waits for the operation to complete.
    /// </summary>
    /// <param name="name">The name of the secret to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="DeletedSecret"/> containing deletion information, or <see langword="null"/> if the secret name is invalid.</returns>
    Task<DeletedSecret?> DeleteSecretAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the properties of all secrets in the vault.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of <see cref="SecretProperties"/>.</returns>
    IAsyncEnumerable<SecretProperties> GetPropertiesOfSecretsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the properties of all versions of a secret.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of <see cref="SecretProperties"/> for each version.</returns>
    IAsyncEnumerable<SecretProperties>? GetPropertiesOfSecretVersionsAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a deleted secret, including its recovery information.
    /// </summary>
    /// <param name="name">The name of the deleted secret.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="DeletedSecret"/>, or <see langword="null"/> if the secret name is invalid.</returns>
    Task<DeletedSecret?> GetDeletedSecretAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all deleted secrets in the vault.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of <see cref="DeletedSecret"/>.</returns>
    IAsyncEnumerable<DeletedSecret> GetDeletedSecretsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes a soft-deleted secret. This operation is not recoverable.
    /// </summary>
    /// <param name="name">The name of the deleted secret to purge.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task PurgeDeletedSecretAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recovers a deleted secret to its latest version and waits for the operation to complete.
    /// </summary>
    /// <param name="name">The name of the deleted secret to recover.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="SecretProperties"/> of the recovered secret, or <see langword="null"/> if the secret name is invalid.</returns>
    Task<SecretProperties?> RecoverDeletedSecretAsync(string name, CancellationToken cancellationToken = default);

    // Sync operations

    /// <summary>
    /// Gets a secret from the vault, including its value and properties.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="version">The version of the secret. If <see langword="null"/>, the latest version is retrieved.</param>
    /// <returns>The <see cref="KeyVaultSecret"/> with its value and properties, or <see langword="null"/> if the secret name is invalid.</returns>
    KeyVaultSecret? GetSecret(string name, string? version = null);

    /// <summary>
    /// Gets the value of a secret as a string.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <returns>The secret value as a string, or <see langword="null"/> if the secret name is invalid.</returns>
    string? GetSecretValue(string name);

    /// <summary>
    /// Sets a secret in the vault, creating a new version if the secret already exists.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="value">The value of the secret.</param>
    /// <returns>The <see cref="KeyVaultSecret"/> that was set, including its properties, or <see langword="null"/> if the secret name is invalid.</returns>
    KeyVaultSecret? SetSecret(string name, string value);

    /// <summary>
    /// Updates the properties of a secret without changing its value.
    /// </summary>
    /// <param name="properties">The secret properties to update.</param>
    /// <returns>The updated <see cref="SecretProperties"/>.</returns>
    SecretProperties UpdateSecretProperties(SecretProperties properties);

    /// <summary>
    /// Deletes a secret from the vault and waits for the operation to complete.
    /// </summary>
    /// <param name="name">The name of the secret to delete.</param>
    /// <returns>The <see cref="DeletedSecret"/> containing deletion information, or <see langword="null"/> if the secret name is invalid.</returns>
    DeletedSecret? DeleteSecret(string name);

    /// <summary>
    /// Lists the properties of all secrets in the vault.
    /// </summary>
    /// <returns>An enumerable of <see cref="SecretProperties"/>.</returns>
    IEnumerable<SecretProperties> GetPropertiesOfSecrets();

    /// <summary>
    /// Lists the properties of all versions of a secret.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <returns>An enumerable of <see cref="SecretProperties"/> for each version, or <see langword="null"/> if the secret name is invalid.</returns>
    IEnumerable<SecretProperties>? GetPropertiesOfSecretVersions(string name);

    /// <summary>
    /// Gets a deleted secret, including its recovery information.
    /// </summary>
    /// <param name="name">The name of the deleted secret.</param>
    /// <returns>The <see cref="DeletedSecret"/> or <see langword="null"/> if the secret name is invalid.</returns>
    DeletedSecret? GetDeletedSecret(string name);

    /// <summary>
    /// Lists all deleted secrets in the vault.
    /// </summary>
    /// <returns>An enumerable of <see cref="DeletedSecret"/>.</returns>
    IEnumerable<DeletedSecret> GetDeletedSecrets();

    /// <summary>
    /// Permanently deletes a soft-deleted secret. This operation is not recoverable.
    /// </summary>
    /// <param name="name">The name of the deleted secret to purge.</param>
    void PurgeDeletedSecret(string name);

    /// <summary>
    /// Recovers a deleted secret to its latest version and waits for the operation to complete.
    /// </summary>
    /// <param name="name">The name of the deleted secret to recover.</param>
    /// <returns>The <see cref="SecretProperties"/> of the recovered secret, or <see langword="null"/> if the secret name is invalid.</returns>
    SecretProperties? RecoverDeletedSecret(string name);
}
