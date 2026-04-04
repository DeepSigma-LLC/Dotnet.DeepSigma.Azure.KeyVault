using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DeepSigma.Azure.KeyVault.Abstractions;

namespace DeepSigma.Azure.KeyVault.Services;

/// <summary>
/// Provides simplified synchronous and asynchronous operations for managing secrets in Azure Key Vault.
/// Wraps <see cref="SecretClient"/> and unwraps responses for convenience.
/// </summary>
public class SecretService : ISecretService
{
    private readonly SecretClient _client;

    /// <summary>
    /// Initializes a new instance of <see cref="SecretService"/> using an existing <see cref="SecretClient"/>.
    /// </summary>
    /// <param name="client">The <see cref="SecretClient"/> to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> is <see langword="null"/>.</exception>
    public SecretService(SecretClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SecretService"/> using a vault URI and credential.
    /// </summary>
    /// <param name="vaultUri">The URI of the Azure Key Vault (e.g., <c>https://myvault.vault.azure.net</c>).</param>
    /// <param name="credential">The <see cref="TokenCredential"/> used to authenticate requests.</param>
    public SecretService(Uri vaultUri, TokenCredential credential)
        : this(new SecretClient(vaultUri, credential))
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SecretService"/> using a vault URI and <see cref="DefaultAzureCredential"/>.
    /// </summary>
    /// <param name="vaultUri">The URI of the Azure Key Vault (e.g., <c>https://myvault.vault.azure.net</c>).</param>
    public SecretService(Uri vaultUri)
        : this(vaultUri, new DefaultAzureCredential())
    {
    }

    // Async operations

    /// <inheritdoc />
    public async Task<KeyVaultSecret> GetSecretAsync(string name, string? version = null, CancellationToken cancellationToken = default)
    {
        Response<KeyVaultSecret> response = await _client.GetSecretAsync(name, version, cancellationToken);
        return response.Value;
    }

    /// <inheritdoc />
    public async Task<string> GetSecretValueAsync(string name, CancellationToken cancellationToken = default)
    {
        KeyVaultSecret secret = await GetSecretAsync(name, cancellationToken: cancellationToken);
        return secret.Value;
    }

    /// <inheritdoc />
    public async Task<KeyVaultSecret> SetSecretAsync(string name, string value, CancellationToken cancellationToken = default)
    {
        Response<KeyVaultSecret> response = await _client.SetSecretAsync(name, value, cancellationToken);
        return response.Value;
    }

    /// <inheritdoc />
    public async Task<SecretProperties> UpdateSecretPropertiesAsync(SecretProperties properties, CancellationToken cancellationToken = default)
    {
        Response<SecretProperties> response = await _client.UpdateSecretPropertiesAsync(properties, cancellationToken);
        return response.Value;
    }

    /// <inheritdoc />
    public async Task<DeletedSecret> DeleteSecretAsync(string name, CancellationToken cancellationToken = default)
    {
        DeleteSecretOperation operation = await _client.StartDeleteSecretAsync(name, cancellationToken);
        Response<DeletedSecret> response = await operation.WaitForCompletionAsync(cancellationToken);
        return response.Value;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<SecretProperties> GetPropertiesOfSecretsAsync(CancellationToken cancellationToken = default)
    {
        return _client.GetPropertiesOfSecretsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<SecretProperties> GetPropertiesOfSecretVersionsAsync(string name, CancellationToken cancellationToken = default)
    {
        return _client.GetPropertiesOfSecretVersionsAsync(name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DeletedSecret> GetDeletedSecretAsync(string name, CancellationToken cancellationToken = default)
    {
        Response<DeletedSecret> response = await _client.GetDeletedSecretAsync(name, cancellationToken);
        return response.Value;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<DeletedSecret> GetDeletedSecretsAsync(CancellationToken cancellationToken = default)
    {
        return _client.GetDeletedSecretsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task PurgeDeletedSecretAsync(string name, CancellationToken cancellationToken = default)
    {
        await _client.PurgeDeletedSecretAsync(name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SecretProperties> RecoverDeletedSecretAsync(string name, CancellationToken cancellationToken = default)
    {
        RecoverDeletedSecretOperation operation = await _client.StartRecoverDeletedSecretAsync(name, cancellationToken);
        Response<SecretProperties> response = await operation.WaitForCompletionAsync(cancellationToken);
        return response.Value;
    }

    // Sync operations

    /// <inheritdoc />
    public KeyVaultSecret GetSecret(string name, string? version = null)
    {
        Response<KeyVaultSecret> response = _client.GetSecret(name, version, CancellationToken.None);
        return response.Value;
    }

    /// <inheritdoc />
    public string GetSecretValue(string name)
    {
        return GetSecret(name).Value;
    }

    /// <inheritdoc />
    public KeyVaultSecret SetSecret(string name, string value)
    {
        Response<KeyVaultSecret> response = _client.SetSecret(name, value);
        return response.Value;
    }

    /// <inheritdoc />
    public SecretProperties UpdateSecretProperties(SecretProperties properties)
    {
        Response<SecretProperties> response = _client.UpdateSecretProperties(properties);
        return response.Value;
    }

    /// <inheritdoc />
    public DeletedSecret DeleteSecret(string name)
    {
        DeleteSecretOperation operation = _client.StartDeleteSecret(name);
        operation.WaitForCompletion();
        return operation.Value;
    }

    /// <inheritdoc />
    public IEnumerable<SecretProperties> GetPropertiesOfSecrets()
    {
        return _client.GetPropertiesOfSecrets();
    }

    /// <inheritdoc />
    public IEnumerable<SecretProperties> GetPropertiesOfSecretVersions(string name)
    {
        return _client.GetPropertiesOfSecretVersions(name);
    }

    /// <inheritdoc />
    public DeletedSecret GetDeletedSecret(string name)
    {
        Response<DeletedSecret> response = _client.GetDeletedSecret(name);
        return response.Value;
    }

    /// <inheritdoc />
    public IEnumerable<DeletedSecret> GetDeletedSecrets()
    {
        return _client.GetDeletedSecrets();
    }

    /// <inheritdoc />
    public void PurgeDeletedSecret(string name)
    {
        _client.PurgeDeletedSecret(name);
    }

    /// <inheritdoc />
    public SecretProperties RecoverDeletedSecret(string name)
    {
        RecoverDeletedSecretOperation operation = _client.StartRecoverDeletedSecret(name);
        operation.WaitForCompletion();
        return operation.Value;
    }
}
