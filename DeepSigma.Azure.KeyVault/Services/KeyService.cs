using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using DeepSigma.Azure.KeyVault.Abstractions;

namespace DeepSigma.Azure.KeyVault.Services;

/// <summary>
/// Provides simplified synchronous and asynchronous operations for managing keys in Azure Key Vault.
/// Wraps <see cref="KeyClient"/> and unwraps responses for convenience.
/// </summary>
public class KeyService : IKeyService
{
    private readonly KeyClient _client;

    /// <summary>
    /// Initializes a new instance of <see cref="KeyService"/> using an existing <see cref="KeyClient"/>.
    /// </summary>
    /// <param name="client">The <see cref="KeyClient"/> to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> is <see langword="null"/>.</exception>
    public KeyService(KeyClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Initializes a new instance of <see cref="KeyService"/> using a vault URI and credential.
    /// </summary>
    /// <param name="vaultUri">The URI of the Azure Key Vault (e.g., <c>https://myvault.vault.azure.net</c>).</param>
    /// <param name="credential">The <see cref="TokenCredential"/> used to authenticate requests.</param>
    public KeyService(Uri vaultUri, TokenCredential credential)
        : this(new KeyClient(vaultUri, credential))
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="KeyService"/> using a vault URI and <see cref="DefaultAzureCredential"/>.
    /// </summary>
    /// <param name="vaultUri">The URI of the Azure Key Vault (e.g., <c>https://myvault.vault.azure.net</c>).</param>
    public KeyService(Uri vaultUri)
        : this(vaultUri, new DefaultAzureCredential())
    {
    }

    // Async operations

    /// <inheritdoc />
    public async Task<KeyVaultKey> GetKeyAsync(string name, string? version = null, CancellationToken cancellationToken = default)
    {
        Response<KeyVaultKey> response = await _client.GetKeyAsync(name, version, cancellationToken);
        return response.Value;
    }

    /// <inheritdoc />
    public async Task<KeyVaultKey> CreateKeyAsync(string name, KeyType keyType, CancellationToken cancellationToken = default)
    {
        Response<KeyVaultKey> response = await _client.CreateKeyAsync(name, keyType, null, cancellationToken);
        return response.Value;
    }

    /// <inheritdoc />
    public async Task<KeyVaultKey> CreateRsaKeyAsync(CreateRsaKeyOptions options, CancellationToken cancellationToken = default)
    {
        Response<KeyVaultKey> response = await _client.CreateRsaKeyAsync(options, cancellationToken);
        return response.Value;
    }

    /// <inheritdoc />
    public async Task<KeyVaultKey> CreateEcKeyAsync(CreateEcKeyOptions options, CancellationToken cancellationToken = default)
    {
        Response<KeyVaultKey> response = await _client.CreateEcKeyAsync(options, cancellationToken);
        return response.Value;
    }

    /// <inheritdoc />
    public async Task<KeyVaultKey> UpdateKeyPropertiesAsync(KeyProperties properties, IEnumerable<KeyOperation>? keyOperations = null, CancellationToken cancellationToken = default)
    {
        Response<KeyVaultKey> response = await _client.UpdateKeyPropertiesAsync(properties, keyOperations, cancellationToken);
        return response.Value;
    }

    /// <inheritdoc />
    public async Task<DeletedKey> DeleteKeyAsync(string name, CancellationToken cancellationToken = default)
    {
        DeleteKeyOperation operation = await _client.StartDeleteKeyAsync(name, cancellationToken);
        Response<DeletedKey> response = await operation.WaitForCompletionAsync(cancellationToken);
        return response.Value;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<KeyProperties> GetPropertiesOfKeysAsync(CancellationToken cancellationToken = default)
    {
        return _client.GetPropertiesOfKeysAsync(cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<KeyProperties> GetPropertiesOfKeyVersionsAsync(string name, CancellationToken cancellationToken = default)
    {
        return _client.GetPropertiesOfKeyVersionsAsync(name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DeletedKey> GetDeletedKeyAsync(string name, CancellationToken cancellationToken = default)
    {
        Response<DeletedKey> response = await _client.GetDeletedKeyAsync(name, cancellationToken);
        return response.Value;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<DeletedKey> GetDeletedKeysAsync(CancellationToken cancellationToken = default)
    {
        return _client.GetDeletedKeysAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task PurgeDeletedKeyAsync(string name, CancellationToken cancellationToken = default)
    {
        await _client.PurgeDeletedKeyAsync(name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<KeyVaultKey> RecoverDeletedKeyAsync(string name, CancellationToken cancellationToken = default)
    {
        RecoverDeletedKeyOperation operation = await _client.StartRecoverDeletedKeyAsync(name, cancellationToken);
        Response<KeyVaultKey> response = await operation.WaitForCompletionAsync(cancellationToken);
        return response.Value;
    }

    // Sync operations

    /// <inheritdoc />
    public KeyVaultKey GetKey(string name, string? version = null)
    {
        Response<KeyVaultKey> response = _client.GetKey(name, version);
        return response.Value;
    }

    /// <inheritdoc />
    public KeyVaultKey CreateKey(string name, KeyType keyType)
    {
        Response<KeyVaultKey> response = _client.CreateKey(name, keyType, null);
        return response.Value;
    }

    /// <inheritdoc />
    public KeyVaultKey CreateRsaKey(CreateRsaKeyOptions options)
    {
        Response<KeyVaultKey> response = _client.CreateRsaKey(options);
        return response.Value;
    }

    /// <inheritdoc />
    public KeyVaultKey CreateEcKey(CreateEcKeyOptions options)
    {
        Response<KeyVaultKey> response = _client.CreateEcKey(options);
        return response.Value;
    }

    /// <inheritdoc />
    public KeyVaultKey UpdateKeyProperties(KeyProperties properties, IEnumerable<KeyOperation>? keyOperations = null)
    {
        Response<KeyVaultKey> response = _client.UpdateKeyProperties(properties, keyOperations);
        return response.Value;
    }

    /// <inheritdoc />
    public DeletedKey DeleteKey(string name)
    {
        DeleteKeyOperation operation = _client.StartDeleteKey(name);
        operation.WaitForCompletion();
        return operation.Value;
    }

    /// <inheritdoc />
    public IEnumerable<KeyProperties> GetPropertiesOfKeys()
    {
        return _client.GetPropertiesOfKeys();
    }

    /// <inheritdoc />
    public IEnumerable<KeyProperties> GetPropertiesOfKeyVersions(string name)
    {
        return _client.GetPropertiesOfKeyVersions(name);
    }

    /// <inheritdoc />
    public DeletedKey GetDeletedKey(string name)
    {
        Response<DeletedKey> response = _client.GetDeletedKey(name);
        return response.Value;
    }

    /// <inheritdoc />
    public IEnumerable<DeletedKey> GetDeletedKeys()
    {
        return _client.GetDeletedKeys();
    }

    /// <inheritdoc />
    public void PurgeDeletedKey(string name)
    {
        _client.PurgeDeletedKey(name);
    }

    /// <inheritdoc />
    public KeyVaultKey RecoverDeletedKey(string name)
    {
        RecoverDeletedKeyOperation operation = _client.StartRecoverDeletedKey(name);
        operation.WaitForCompletion();
        return operation.Value;
    }
}
