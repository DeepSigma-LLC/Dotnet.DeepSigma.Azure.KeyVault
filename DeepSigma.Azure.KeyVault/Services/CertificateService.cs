using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using DeepSigma.Azure.KeyVault.Abstractions;
using System.Security.Cryptography.X509Certificates;


namespace DeepSigma.Azure.KeyVault.Services;

/// <summary>
/// Provides simplified synchronous and asynchronous operations for managing certificates in Azure Key Vault.
/// Wraps <see cref="CertificateClient"/> and unwraps responses for convenience.
/// </summary>
/// <remarks>
/// GetCertificateAsync gets the certificate object and policy. DownloadCertificateAsync creates an X509Certificate2,
/// and Microsoft notes that because the certificate’s Cer property only contains the public key, downloading the full certificate attempts to access the managed secret that contains the full certificate;
/// that requires both certificates/get and secrets/get permissions. The download content type (PFX vs PEM) is determined by Key Vault
/// from the stored secret and cannot be chosen by the caller; the only download option the SDK exposes is <see cref="X509KeyStorageFlags"/>,
/// which is often required on Linux/macOS (e.g. EphemeralKeySet/MachineKeySet).
/// For importing, Key Vault expects a PFX or ASCII PEM certificate containing the private key, and the SDK uses ImportCertificateOptions(string, byte[]).
/// </remarks>
public class CertificateService : ICertificateService
{
    private readonly CertificateClient _client;

    /// <summary>
    /// Initializes a new instance of <see cref="CertificateService"/> using an existing <see cref="CertificateClient"/>.
    /// </summary>
    /// <param name="client"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public CertificateService(CertificateClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CertificateService"/> using a vault URI and credential.
    /// </summary>
    /// <param name="vaultUri"></param>
    /// <param name="credential"></param>
    public CertificateService(Uri vaultUri, TokenCredential credential)
        : this(new CertificateClient(vaultUri, credential))
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CertificateService"/> using a vault URI and <see cref="DefaultAzureCredential"/>.
    /// </summary>
    /// <param name="vaultUri"></param>
    public CertificateService(Uri vaultUri)
        : this(vaultUri, new DefaultAzureCredential())
    {
    }

    // Async operations

    /// <inheritdoc />
    public async Task<KeyVaultCertificateWithPolicy?> GetCertificateAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        Response<KeyVaultCertificateWithPolicy> response =
            await _client.GetCertificateAsync(name, cancellationToken);

        return response.Value;
    }

    /// <inheritdoc />
    public async Task<KeyVaultCertificate?> GetCertificateVersionAsync(
        string name,
        string version,
        CancellationToken cancellationToken = default)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        if (string.IsNullOrWhiteSpace(version))
            return null;

        Response<KeyVaultCertificate> response =
            await _client.GetCertificateVersionAsync(name, version, cancellationToken);

        return response.Value;
    }

    /// <inheritdoc />
    public async Task<X509Certificate2?> DownloadCertificateAsync(
        string name,
        string? version = null,
        X509KeyStorageFlags? keyStorageFlags = null,
        CancellationToken cancellationToken = default)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        var options = new DownloadCertificateOptions(name);

        if (!string.IsNullOrWhiteSpace(version))
            options.Version = version;

        if (keyStorageFlags.HasValue)
            options.KeyStorageFlags = keyStorageFlags.Value;

        Response<X509Certificate2> response =
            await _client.DownloadCertificateAsync(options, cancellationToken);

        return response.Value;
    }

    /// <inheritdoc />
    public async Task<KeyVaultCertificateWithPolicy?> CreateCertificateAsync(
        string name,
        CertificatePolicy policy,
        CancellationToken cancellationToken = default)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        ArgumentNullException.ThrowIfNull(policy);

        CertificateOperation operation =
            await _client.StartCreateCertificateAsync(
                name,
                policy,
                cancellationToken: cancellationToken);

        Response<KeyVaultCertificateWithPolicy> response =
            await operation.WaitForCompletionAsync(cancellationToken);

        return response.Value;
    }

    /// <inheritdoc />
    public async Task<KeyVaultCertificateWithPolicy?> ImportCertificateAsync(
        string name,
        byte[] certificateBytes,
        string? password = null,
        CertificatePolicy? policy = null,
        bool? enabled = null,
        CancellationToken cancellationToken = default)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        if (certificateBytes is null || certificateBytes.Length == 0)
            return null;

        var options = new ImportCertificateOptions(name, certificateBytes)
        {
            Password = password,
            Policy = policy,
            Enabled = enabled
        };

        Response<KeyVaultCertificateWithPolicy> response =
            await _client.ImportCertificateAsync(options, cancellationToken);

        return response.Value;
    }

    /// <inheritdoc />
    public async Task<KeyVaultCertificate> UpdateCertificatePropertiesAsync(
        CertificateProperties properties,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(properties);

        Response<KeyVaultCertificate> response =
            await _client.UpdateCertificatePropertiesAsync(properties, cancellationToken);

        return response.Value;
    }

    /// <inheritdoc />
    public async Task<CertificatePolicy?> GetCertificatePolicyAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        Response<CertificatePolicy> response =
            await _client.GetCertificatePolicyAsync(name, cancellationToken);

        return response.Value;
    }

    /// <inheritdoc />
    public async Task<CertificatePolicy?> UpdateCertificatePolicyAsync(
        string name,
        CertificatePolicy policy,
        CancellationToken cancellationToken = default)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        ArgumentNullException.ThrowIfNull(policy);

        Response<CertificatePolicy> response =
            await _client.UpdateCertificatePolicyAsync(name, policy, cancellationToken);

        return response.Value;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<CertificateProperties> GetPropertiesOfCertificatesAsync(
        bool includePending = false,
        CancellationToken cancellationToken = default)
    {
        return _client.GetPropertiesOfCertificatesAsync(includePending, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<CertificateProperties>? GetPropertiesOfCertificateVersionsAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        return _client.GetPropertiesOfCertificateVersionsAsync(name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DeletedCertificate?> DeleteCertificateAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        DeleteCertificateOperation operation =
            await _client.StartDeleteCertificateAsync(name, cancellationToken);

        Response<DeletedCertificate> response =
            await operation.WaitForCompletionAsync(cancellationToken);

        return response.Value;
    }

    /// <inheritdoc />
    public async Task<DeletedCertificate?> GetDeletedCertificateAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        Response<DeletedCertificate> response =
            await _client.GetDeletedCertificateAsync(name, cancellationToken);

        return response.Value;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<DeletedCertificate> GetDeletedCertificatesAsync(
        bool includePending = false,
        CancellationToken cancellationToken = default)
    {
        return _client.GetDeletedCertificatesAsync(includePending, cancellationToken);
    }

    /// <inheritdoc />
    public async Task PurgeDeletedCertificateAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            throw new ArgumentException("Invalid certificate name.", nameof(name));

        await _client.PurgeDeletedCertificateAsync(name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<KeyVaultCertificateWithPolicy?> RecoverDeletedCertificateAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        RecoverDeletedCertificateOperation operation =
            await _client.StartRecoverDeletedCertificateAsync(name, cancellationToken);

        Response<KeyVaultCertificateWithPolicy> response =
            await operation.WaitForCompletionAsync(cancellationToken);

        return response.Value;
    }

    // Sync operations

    /// <inheritdoc />
    public KeyVaultCertificateWithPolicy? GetCertificate(string name)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        Response<KeyVaultCertificateWithPolicy> response =
            _client.GetCertificate(name);

        return response.Value;
    }

    /// <inheritdoc />
    public KeyVaultCertificate? GetCertificateVersion(string name, string version)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        if (string.IsNullOrWhiteSpace(version))
            return null;

        Response<KeyVaultCertificate> response =
            _client.GetCertificateVersion(name, version);

        return response.Value;
    }

    /// <inheritdoc />
    public X509Certificate2? DownloadCertificate(
        string name,
        string? version = null,
        X509KeyStorageFlags? keyStorageFlags = null)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        var options = new DownloadCertificateOptions(name);

        if (!string.IsNullOrWhiteSpace(version))
            options.Version = version;

        if (keyStorageFlags.HasValue)
            options.KeyStorageFlags = keyStorageFlags.Value;

        Response<X509Certificate2> response =
            _client.DownloadCertificate(options);

        return response.Value;
    }

    /// <inheritdoc />
    public KeyVaultCertificateWithPolicy? CreateCertificate(
        string name,
        CertificatePolicy policy)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        ArgumentNullException.ThrowIfNull(policy);

        CertificateOperation operation =
            _client.StartCreateCertificate(name, policy);

        operation.WaitForCompletion();

        return operation.Value;
    }

    /// <inheritdoc />
    public KeyVaultCertificateWithPolicy? ImportCertificate(
        string name,
        byte[] certificateBytes,
        string? password = null,
        CertificatePolicy? policy = null,
        bool? enabled = null)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        if (certificateBytes is null || certificateBytes.Length == 0)
            return null;

        var options = new ImportCertificateOptions(name, certificateBytes)
        {
            Password = password,
            Policy = policy,
            Enabled = enabled
        };

        Response<KeyVaultCertificateWithPolicy> response =
            _client.ImportCertificate(options);

        return response.Value;
    }

    /// <inheritdoc />
    public KeyVaultCertificate UpdateCertificateProperties(CertificateProperties properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        Response<KeyVaultCertificate> response =
            _client.UpdateCertificateProperties(properties);

        return response.Value;
    }

    /// <inheritdoc />
    public CertificatePolicy? GetCertificatePolicy(string name)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        Response<CertificatePolicy> response =
            _client.GetCertificatePolicy(name);

        return response.Value;
    }

    /// <inheritdoc />
    public CertificatePolicy? UpdateCertificatePolicy(string name, CertificatePolicy policy)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        ArgumentNullException.ThrowIfNull(policy);

        Response<CertificatePolicy> response =
            _client.UpdateCertificatePolicy(name, policy);

        return response.Value;
    }

    /// <inheritdoc />
    public IEnumerable<CertificateProperties> GetPropertiesOfCertificates(
        bool includePending = false)
    {
        return _client.GetPropertiesOfCertificates(includePending);
    }

    /// <inheritdoc />
    public IEnumerable<CertificateProperties>? GetPropertiesOfCertificateVersions(string name)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        return _client.GetPropertiesOfCertificateVersions(name);
    }

    /// <inheritdoc />
    public DeletedCertificate? DeleteCertificate(string name)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        DeleteCertificateOperation operation =
            _client.StartDeleteCertificate(name);

        operation.WaitForCompletion();

        return operation.Value;
    }

    /// <inheritdoc />
    public DeletedCertificate? GetDeletedCertificate(string name)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        Response<DeletedCertificate> response =
            _client.GetDeletedCertificate(name);

        return response.Value;
    }

    /// <inheritdoc />
    public IEnumerable<DeletedCertificate> GetDeletedCertificates(
        bool includePending = false)
    {
        return _client.GetDeletedCertificates(includePending);
    }

    /// <inheritdoc />
    public void PurgeDeletedCertificate(string name)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            throw new ArgumentException("Invalid certificate name.", nameof(name));

        _client.PurgeDeletedCertificate(name);
    }

    /// <inheritdoc />
    public KeyVaultCertificateWithPolicy? RecoverDeletedCertificate(string name)
    {
        if (!KeyVaultValidationService.IsValidCertificateName(name))
            return null;

        RecoverDeletedCertificateOperation operation =
            _client.StartRecoverDeletedCertificate(name);

        operation.WaitForCompletion();

        return operation.Value;
    }
}
