using Azure.Security.KeyVault.Certificates;
using System.Security.Cryptography.X509Certificates;

namespace DeepSigma.Azure.KeyVault.Abstractions;

/// <summary>
/// Defines simplified synchronous and asynchronous operations for managing certificates in Azure Key Vault.
/// Methods that take a certificate name return <c>null</c> (or throw, where noted) when the name fails validation.
/// </summary>
public interface ICertificateService
{
    // Async operations

    /// <summary>
    /// Gets the latest version of a certificate along with its policy.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The certificate with its policy, or <c>null</c> if the name is invalid.</returns>
    Task<KeyVaultCertificateWithPolicy?> GetCertificateAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific version of a certificate.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <param name="version">The version of the certificate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The requested certificate version, or <c>null</c> if the name or version is invalid.</returns>
    Task<KeyVaultCertificate?> GetCertificateVersionAsync(
        string name,
        string version,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a certificate as an <see cref="X509Certificate2"/>, including the private key when available.
    /// Requires both certificates/get and secrets/get permissions.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <param name="version">The version to download, or <c>null</c> for the latest version.</param>
    /// <param name="keyStorageFlags">
    /// Optional key storage flags controlling how the private key is loaded.
    /// Often required on Linux/macOS (for example, EphemeralKeySet or MachineKeySet).
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The downloaded certificate, or <c>null</c> if the name is invalid.</returns>
    Task<X509Certificate2?> DownloadCertificateAsync(
        string name,
        string? version = null,
        X509KeyStorageFlags? keyStorageFlags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts creation of a new certificate (or new version) with the specified policy and waits for completion.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <param name="policy">The policy governing certificate creation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created certificate with its policy, or <c>null</c> if the name is invalid.</returns>
    Task<KeyVaultCertificateWithPolicy?> CreateCertificateAsync(
        string name,
        CertificatePolicy policy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports a certificate from raw bytes. Key Vault expects a PFX or ASCII PEM certificate containing the private key.
    /// </summary>
    /// <param name="name">The name to assign to the imported certificate.</param>
    /// <param name="certificateBytes">The certificate content to import.</param>
    /// <param name="password">The password protecting the certificate, if any.</param>
    /// <param name="policy">An optional policy to apply to the imported certificate.</param>
    /// <param name="enabled">Whether the imported certificate should be enabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The imported certificate with its policy, or <c>null</c> if the name is invalid or no bytes were supplied.</returns>
    Task<KeyVaultCertificateWithPolicy?> ImportCertificateAsync(
        string name,
        byte[] certificateBytes,
        string? password = null,
        CertificatePolicy? policy = null,
        bool? enabled = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the mutable properties of a certificate.
    /// </summary>
    /// <param name="properties">The properties to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated certificate.</returns>
    Task<KeyVaultCertificate> UpdateCertificatePropertiesAsync(
        CertificateProperties properties,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the policy of a certificate.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The certificate policy, or <c>null</c> if the name is invalid.</returns>
    Task<CertificatePolicy?> GetCertificatePolicyAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the policy of a certificate.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <param name="policy">The new policy.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated certificate policy, or <c>null</c> if the name is invalid.</returns>
    Task<CertificatePolicy?> UpdateCertificatePolicyAsync(
        string name,
        CertificatePolicy policy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates the properties of all certificates in the vault.
    /// </summary>
    /// <param name="includePending">Whether to include certificates that are still being created.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async stream of certificate properties.</returns>
    IAsyncEnumerable<CertificateProperties> GetPropertiesOfCertificatesAsync(
        bool includePending = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates the properties of all versions of a certificate.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async stream of certificate version properties, or <c>null</c> if the name is invalid.</returns>
    IAsyncEnumerable<CertificateProperties>? GetPropertiesOfCertificateVersionsAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a certificate and waits for the operation to complete.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deleted certificate, or <c>null</c> if the name is invalid.</returns>
    Task<DeletedCertificate?> DeleteCertificateAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a deleted certificate from a soft-delete-enabled vault.
    /// </summary>
    /// <param name="name">The name of the deleted certificate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deleted certificate, or <c>null</c> if the name is invalid.</returns>
    Task<DeletedCertificate?> GetDeletedCertificateAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates deleted certificates in a soft-delete-enabled vault.
    /// </summary>
    /// <param name="includePending">Whether to include certificates that are still being created.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async stream of deleted certificates.</returns>
    IAsyncEnumerable<DeletedCertificate> GetDeletedCertificatesAsync(
        bool includePending = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently purges a deleted certificate. This action is irreversible.
    /// </summary>
    /// <param name="name">The name of the deleted certificate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="ArgumentException">Thrown when the certificate name is invalid.</exception>
    Task PurgeDeletedCertificateAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recovers a deleted certificate and waits for the operation to complete.
    /// </summary>
    /// <param name="name">The name of the deleted certificate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The recovered certificate with its policy, or <c>null</c> if the name is invalid.</returns>
    Task<KeyVaultCertificateWithPolicy?> RecoverDeletedCertificateAsync(
        string name,
        CancellationToken cancellationToken = default);

    // Sync operations

    /// <summary>
    /// Gets the latest version of a certificate along with its policy.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <returns>The certificate with its policy, or <c>null</c> if the name is invalid.</returns>
    KeyVaultCertificateWithPolicy? GetCertificate(string name);

    /// <summary>
    /// Gets a specific version of a certificate.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <param name="version">The version of the certificate.</param>
    /// <returns>The requested certificate version, or <c>null</c> if the name or version is invalid.</returns>
    KeyVaultCertificate? GetCertificateVersion(string name, string version);

    /// <summary>
    /// Downloads a certificate as an <see cref="X509Certificate2"/>, including the private key when available.
    /// Requires both certificates/get and secrets/get permissions.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <param name="version">The version to download, or <c>null</c> for the latest version.</param>
    /// <param name="keyStorageFlags">
    /// Optional key storage flags controlling how the private key is loaded.
    /// Often required on Linux/macOS (for example, EphemeralKeySet or MachineKeySet).
    /// </param>
    /// <returns>The downloaded certificate, or <c>null</c> if the name is invalid.</returns>
    X509Certificate2? DownloadCertificate(
        string name,
        string? version = null,
        X509KeyStorageFlags? keyStorageFlags = null);

    /// <summary>
    /// Creates a new certificate (or new version) with the specified policy and waits for completion.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <param name="policy">The policy governing certificate creation.</param>
    /// <returns>The created certificate with its policy, or <c>null</c> if the name is invalid.</returns>
    KeyVaultCertificateWithPolicy? CreateCertificate(string name, CertificatePolicy policy);

    /// <summary>
    /// Imports a certificate from raw bytes. Key Vault expects a PFX or ASCII PEM certificate containing the private key.
    /// </summary>
    /// <param name="name">The name to assign to the imported certificate.</param>
    /// <param name="certificateBytes">The certificate content to import.</param>
    /// <param name="password">The password protecting the certificate, if any.</param>
    /// <param name="policy">An optional policy to apply to the imported certificate.</param>
    /// <param name="enabled">Whether the imported certificate should be enabled.</param>
    /// <returns>The imported certificate with its policy, or <c>null</c> if the name is invalid or no bytes were supplied.</returns>
    KeyVaultCertificateWithPolicy? ImportCertificate(
        string name,
        byte[] certificateBytes,
        string? password = null,
        CertificatePolicy? policy = null,
        bool? enabled = null);

    /// <summary>
    /// Updates the mutable properties of a certificate.
    /// </summary>
    /// <param name="properties">The properties to update.</param>
    /// <returns>The updated certificate.</returns>
    KeyVaultCertificate UpdateCertificateProperties(CertificateProperties properties);

    /// <summary>
    /// Gets the policy of a certificate.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <returns>The certificate policy, or <c>null</c> if the name is invalid.</returns>
    CertificatePolicy? GetCertificatePolicy(string name);

    /// <summary>
    /// Updates the policy of a certificate.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <param name="policy">The new policy.</param>
    /// <returns>The updated certificate policy, or <c>null</c> if the name is invalid.</returns>
    CertificatePolicy? UpdateCertificatePolicy(string name, CertificatePolicy policy);

    /// <summary>
    /// Enumerates the properties of all certificates in the vault.
    /// </summary>
    /// <param name="includePending">Whether to include certificates that are still being created.</param>
    /// <returns>A sequence of certificate properties.</returns>
    IEnumerable<CertificateProperties> GetPropertiesOfCertificates(bool includePending = false);

    /// <summary>
    /// Enumerates the properties of all versions of a certificate.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <returns>A sequence of certificate version properties, or <c>null</c> if the name is invalid.</returns>
    IEnumerable<CertificateProperties>? GetPropertiesOfCertificateVersions(string name);

    /// <summary>
    /// Deletes a certificate and waits for the operation to complete.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <returns>The deleted certificate, or <c>null</c> if the name is invalid.</returns>
    DeletedCertificate? DeleteCertificate(string name);

    /// <summary>
    /// Gets a deleted certificate from a soft-delete-enabled vault.
    /// </summary>
    /// <param name="name">The name of the deleted certificate.</param>
    /// <returns>The deleted certificate, or <c>null</c> if the name is invalid.</returns>
    DeletedCertificate? GetDeletedCertificate(string name);

    /// <summary>
    /// Enumerates deleted certificates in a soft-delete-enabled vault.
    /// </summary>
    /// <param name="includePending">Whether to include certificates that are still being created.</param>
    /// <returns>A sequence of deleted certificates.</returns>
    IEnumerable<DeletedCertificate> GetDeletedCertificates(bool includePending = false);

    /// <summary>
    /// Permanently purges a deleted certificate. This action is irreversible.
    /// </summary>
    /// <param name="name">The name of the deleted certificate.</param>
    /// <exception cref="ArgumentException">Thrown when the certificate name is invalid.</exception>
    void PurgeDeletedCertificate(string name);

    /// <summary>
    /// Recovers a deleted certificate and waits for the operation to complete.
    /// </summary>
    /// <param name="name">The name of the deleted certificate.</param>
    /// <returns>The recovered certificate with its policy, or <c>null</c> if the name is invalid.</returns>
    KeyVaultCertificateWithPolicy? RecoverDeletedCertificate(string name);
}
