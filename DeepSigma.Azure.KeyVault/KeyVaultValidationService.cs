using System.Text.RegularExpressions;

namespace DeepSigma.Azure.KeyVault;

/// <summary>
/// Provides validation methods for Azure Key Vault resource names, object names, and URIs.
/// </summary>
public static class KeyVaultValidationService
{
    /*
     * Key Vault resource name:
     * - 3-24 characters
     * - letters, numbers, hyphens
     * - no consecutive hyphens
     * - starts with a letter
     * - ends with letter or number
     */
    private static readonly Regex VaultNameRegex = new(
        @"^(?!.*--)[A-Za-z][A-Za-z0-9-]{1,22}[A-Za-z0-9]$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /*
     * Key Vault object name:
     * Applies to secrets, keys, and certificates.
     *
     * Practical safe rule:
     * - 1-127 characters
     * - letters, numbers, hyphens only
     */
    private static readonly Regex ObjectNameRegex = new(
        @"^[A-Za-z0-9-]{1,127}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Validates whether the provided vault name is valid according to Azure Key Vault naming rules.
    /// </summary>
    /// <param name="vaultName"></param>
    /// <returns></returns>
    public static bool IsValidVaultName(string? vaultName)
    {
        return !string.IsNullOrWhiteSpace(vaultName)
            && VaultNameRegex.IsMatch(vaultName);
    }

    /// <summary>
    /// Validates whether the provided secret name is valid according to Azure Key Vault naming rules.
    /// </summary>
    /// <param name="secretName"></param>
    /// <returns></returns>
    public static bool IsValidSecretName(string? secretName)
    {
        return IsValidObjectName(secretName);
    }

    /// <summary>
    /// Validates whether the provided key name is valid according to Azure Key Vault naming rules.
    /// </summary>
    /// <param name="keyName"></param>
    /// <returns></returns>
    public static bool IsValidKeyName(string? keyName)
    {
        return IsValidObjectName(keyName);
    }


    /// <summary>
    /// Validates whether the provided certificate name is valid according to Azure Key Vault naming rules.
    /// </summary>
    /// <param name="certificateName"></param>
    /// <returns></returns>
    public static bool IsValidCertificateName(string? certificateName)
    {
        return IsValidObjectName(certificateName);
    }
    
    /// <summary>
    /// Validates whether the provided secret value is valid according to Azure Key Vault rules.
    /// </summary>
    /// <param name="secretValue"></param>
    /// <returns></returns>
    public static bool IsValidSecretValue(string? secretValue)
    {
        // Change this if your app wants to allow empty secrets.
        return !string.IsNullOrEmpty(secretValue);
    }

    /// <summary>
    /// Validates whether the provided vault URI is valid according to Azure Key Vault rules.
    /// </summary>
    /// <param name="vaultUri"></param>
    /// <returns></returns>
    public static bool IsValidVaultUri(string? vaultUri)
    {
        if (string.IsNullOrWhiteSpace(vaultUri))
            return false;

        if (!Uri.TryCreate(vaultUri, UriKind.Absolute, out var uri))
            return false;

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return false;

        return uri.Host.EndsWith(".vault.azure.net", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Converts a valid Azure Key Vault name to its corresponding URI.
    /// </summary>
    /// <param name="vaultName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string ToVaultUri(string vaultName)
    {
        if (!IsValidVaultName(vaultName))
            throw new ArgumentException("Invalid Azure Key Vault name.", nameof(vaultName));

        return $"https://{vaultName}.vault.azure.net/";
    }

    /// <summary>
    /// Converts a configuration key to a valid Azure Key Vault secret name.
    /// </summary>
    /// <param name="configKey"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string ToSecretNameFromConfigKey(string configKey)
    {
        if (string.IsNullOrWhiteSpace(configKey))
            throw new ArgumentException("Configuration key cannot be null or whitespace.", nameof(configKey));

        var secretName = configKey.Replace(":", "--");

        if (!IsValidSecretName(secretName))
            throw new ArgumentException($"Invalid Azure Key Vault secret name: {secretName}", nameof(configKey));

        return secretName;
    }

    /// <summary>
    /// Validates whether the provided object name (secret, key, or certificate) is valid according to Azure Key Vault naming rules.
    /// </summary>
    /// <param name="objectName"></param>
    /// <returns></returns>
    private static bool IsValidObjectName(string? objectName)
    {
        return !string.IsNullOrWhiteSpace(objectName)
            && ObjectNameRegex.IsMatch(objectName);
    }
}