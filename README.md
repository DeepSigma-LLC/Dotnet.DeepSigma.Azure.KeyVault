# DeepSigma.Azure.KeyVault

A .NET library that simplifies interactions with Azure Key Vault by providing a clean, interface-based wrapper around the Azure SDK's `SecretClient` and `KeyClient`. Both synchronous and asynchronous operations are supported out of the box.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/) [![Azure Key Vault](https://img.shields.io/badge/Azure-Key%20Vault-0078D4)](https://azure.microsoft.com/services/key-vault/)

---

## Features

- **Secrets management** - get, set, update, delete, purge, recover, and list secrets and their versions.
- **Keys management** - get, create (RSA / EC), update, delete, purge, recover, and list keys and their versions.
- **Sync + Async** - every operation is available in both synchronous and asynchronous variants.
- **Interface-driven** - `ISecretService` and `IKeyService` make dependency injection and unit testing straightforward.
- **Response unwrapping** - Azure SDK `Response<T>` objects are automatically unwrapped so you work directly with domain types.
- **Long-running operation handling** - delete and recover methods wait for the operation to complete before returning.
- **Multiple constructors** - pass your own `SecretClient`/`KeyClient`, supply a `Uri` + `TokenCredential`, or just a `Uri` (uses `DefaultAzureCredential`).

---

## Project Structure

```text
Dotnet.DeepSigma.Azure.KeyVault/
|-- DeepSigma.Azure.KeyVault/              # Main library
|   |-- Abstractions/
|   |   |-- ISecretService.cs              # Secret operations interface
|   |   |-- IKeyService.cs                 # Key operations interface
|   |-- Services/
|   |   |-- SecretService.cs               # SecretClient wrapper
|   |   |-- KeyService.cs                  # KeyClient wrapper
|   |-- DeepSigma.Azure.KeyVault.csproj
|-- DeepSigma.Azure.KeyVault.Test/         # Unit tests
|   |-- SecretServiceTests.cs
|   |-- KeyServiceTests.cs
|   |-- DeepSigma.Azure.KeyVault.Test.csproj
```

---

## Prerequisites

| Requirement | Version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 or later |
| An [Azure subscription](https://azure.microsoft.com/free/) | -- |
| An [Azure Key Vault](https://learn.microsoft.com/azure/key-vault/general/quick-create-portal) instance | -- |

---

## Installation

Clone the repository and build:

```bash
git clone https://github.com/DeepSigma-LLC/Dotnet.DeepSigma.Azure.KeyVault.git
cd Dotnet.DeepSigma.Azure.KeyVault
dotnet build
```

Or add a project reference from your own solution:

```bash
dotnet add reference path/to/DeepSigma.Azure.KeyVault/DeepSigma.Azure.KeyVault.csproj
```

---

## Authentication

The library uses [Azure.Identity](https://learn.microsoft.com/dotnet/api/overview/azure/identity-readme) for authentication. The following credential types are supported:

| Credential | When to use |
|---|---|
| `DefaultAzureCredential` | Recommended for most scenarios -- automatically chains multiple credential types (managed identity, Visual Studio, Azure CLI, etc.). |
| Any `TokenCredential` | Pass your own credential (e.g., `ClientSecretCredential`, `ManagedIdentityCredential`) for fine-grained control. |

Ensure your identity has the appropriate Key Vault access policies or RBAC roles:

- **Key Vault Secrets User / Officer** -- for secret operations
- **Key Vault Crypto User / Officer** -- for key operations

---

## Getting Started

### Creating a Service Instance

There are three ways to construct a service:

```csharp
using DeepSigma.Azure.KeyVault.Services;

// 1. Simplest -- uses DefaultAzureCredential
var secretService = new SecretService(new Uri("https://myvault.vault.azure.net"));
var keyService    = new KeyService(new Uri("https://myvault.vault.azure.net"));

// 2. Custom credential
var credential    = new ClientSecretCredential(tenantId, clientId, clientSecret);
var secretService = new SecretService(new Uri("https://myvault.vault.azure.net"), credential);
var keyService    = new KeyService(new Uri("https://myvault.vault.azure.net"), credential);

// 3. Bring your own client
var secretClient  = new SecretClient(new Uri("https://myvault.vault.azure.net"), credential);
var secretService = new SecretService(secretClient);
```

### Dependency Injection

Register the services in your DI container:

```csharp
using DeepSigma.Azure.KeyVault.Abstractions;
using DeepSigma.Azure.KeyVault.Services;

builder.Services.AddSingleton<ISecretService>(
    new SecretService(new Uri("https://myvault.vault.azure.net")));

builder.Services.AddSingleton<IKeyService>(
    new KeyService(new Uri("https://myvault.vault.azure.net")));
```

---

## Usage

### Secrets

```csharp
// Set a secret
KeyVaultSecret created = await secretService.SetSecretAsync("my-secret", "s3cr3t-value");

// Get a secret
KeyVaultSecret secret = await secretService.GetSecretAsync("my-secret");
Console.WriteLine(secret.Value);

// Get just the value
string value = await secretService.GetSecretValueAsync("my-secret");

// Get a specific version
KeyVaultSecret versioned = await secretService.GetSecretAsync("my-secret", version: "abc123");

// Update properties
SecretProperties props = secret.Properties;
props.ExpiresOn = DateTimeOffset.UtcNow.AddDays(90);
await secretService.UpdateSecretPropertiesAsync(props);

// List all secrets
await foreach (SecretProperties item in secretService.GetPropertiesOfSecretsAsync())
{
    Console.WriteLine(item.Name);
}

// List versions of a secret
await foreach (SecretProperties ver in secretService.GetPropertiesOfSecretVersionsAsync("my-secret"))
{
    Console.WriteLine($"{ver.Version} - Created: {ver.CreatedOn}");
}

// Delete a secret (waits for completion)
DeletedSecret deleted = await secretService.DeleteSecretAsync("my-secret");

// Purge a deleted secret
await secretService.PurgeDeletedSecretAsync("my-secret");

// Recover a deleted secret (waits for completion)
SecretProperties recovered = await secretService.RecoverDeletedSecretAsync("my-secret");
```

### Keys

```csharp
// Create a key
KeyVaultKey rsaKey = await keyService.CreateRsaKeyAsync(new CreateRsaKeyOptions("my-rsa-key"));
KeyVaultKey ecKey  = await keyService.CreateEcKeyAsync(new CreateEcKeyOptions("my-ec-key"));
KeyVaultKey key    = await keyService.CreateKeyAsync("my-key", KeyType.Rsa);

// Get a key
KeyVaultKey fetched = await keyService.GetKeyAsync("my-rsa-key");
Console.WriteLine(fetched.KeyType);

// Get a specific version
KeyVaultKey versioned = await keyService.GetKeyAsync("my-rsa-key", version: "abc123");

// Update properties
KeyProperties props = fetched.Properties;
props.ExpiresOn = DateTimeOffset.UtcNow.AddYears(1);
await keyService.UpdateKeyPropertiesAsync(props);

// List all keys
await foreach (KeyProperties item in keyService.GetPropertiesOfKeysAsync())
{
    Console.WriteLine(item.Name);
}

// List versions of a key
await foreach (KeyProperties ver in keyService.GetPropertiesOfKeyVersionsAsync("my-rsa-key"))
{
    Console.WriteLine($"{ver.Version} - Created: {ver.CreatedOn}");
}

// Delete a key (waits for completion)
DeletedKey deleted = await keyService.DeleteKeyAsync("my-rsa-key");

// Purge a deleted key
await keyService.PurgeDeletedKeyAsync("my-rsa-key");

// Recover a deleted key (waits for completion)
KeyVaultKey recovered = await keyService.RecoverDeletedKeyAsync("my-rsa-key");
```

### Synchronous Operations

Every async method has a synchronous counterpart:

```csharp
// Sync secret operations
KeyVaultSecret secret = secretService.GetSecret("my-secret");
string value           = secretService.GetSecretValue("my-secret");
KeyVaultSecret created = secretService.SetSecret("my-secret", "value");
DeletedSecret deleted   = secretService.DeleteSecret("my-secret");

// Sync key operations
KeyVaultKey key         = keyService.GetKey("my-key");
KeyVaultKey rsaKey      = keyService.CreateRsaKey(new CreateRsaKeyOptions("my-rsa-key"));
DeletedKey deletedKey   = keyService.DeleteKey("my-key");
```

---

## API Reference

### ISecretService

| Method | Returns | Description |
|---|---|---|
| `GetSecretAsync(name, version?, ct)` | `Task<KeyVaultSecret>` | Gets a secret by name, optionally by version. |
| `GetSecretValueAsync(name, ct)` | `Task<string>` | Gets the plain-text value of a secret. |
| `SetSecretAsync(name, value, ct)` | `Task<KeyVaultSecret>` | Sets or updates a secret. |
| `UpdateSecretPropertiesAsync(props, ct)` | `Task<SecretProperties>` | Updates metadata on a secret. |
| `DeleteSecretAsync(name, ct)` | `Task<DeletedSecret>` | Deletes a secret and waits for completion. |
| `GetPropertiesOfSecretsAsync(ct)` | `IAsyncEnumerable<SecretProperties>` | Lists properties of all secrets. |
| `GetPropertiesOfSecretVersionsAsync(name, ct)` | `IAsyncEnumerable<SecretProperties>` | Lists all versions of a secret. |
| `GetDeletedSecretAsync(name, ct)` | `Task<DeletedSecret>` | Gets a deleted secret. |
| `GetDeletedSecretsAsync(ct)` | `IAsyncEnumerable<DeletedSecret>` | Lists all deleted secrets. |
| `PurgeDeletedSecretAsync(name, ct)` | `Task` | Permanently removes a deleted secret. |
| `RecoverDeletedSecretAsync(name, ct)` | `Task<SecretProperties>` | Recovers a deleted secret and waits for completion. |

All async methods have synchronous equivalents (e.g., `GetSecret`, `SetSecret`, etc.).

### IKeyService

| Method | Returns | Description |
|---|---|---|
| `GetKeyAsync(name, version?, ct)` | `Task<KeyVaultKey>` | Gets a key by name, optionally by version. |
| `CreateKeyAsync(name, keyType, ct)` | `Task<KeyVaultKey>` | Creates a key with the specified type. |
| `CreateRsaKeyAsync(options, ct)` | `Task<KeyVaultKey>` | Creates an RSA key with detailed options. |
| `CreateEcKeyAsync(options, ct)` | `Task<KeyVaultKey>` | Creates an EC key with detailed options. |
| `UpdateKeyPropertiesAsync(props, ct)` | `Task<KeyProperties>` | Updates metadata on a key. |
| `DeleteKeyAsync(name, ct)` | `Task<DeletedKey>` | Deletes a key and waits for completion. |
| `GetPropertiesOfKeysAsync(ct)` | `IAsyncEnumerable<KeyProperties>` | Lists properties of all keys. |
| `GetPropertiesOfKeyVersionsAsync(name, ct)` | `IAsyncEnumerable<KeyProperties>` | Lists all versions of a key. |
| `GetDeletedKeyAsync(name, ct)` | `Task<DeletedKey>` | Gets a deleted key. |
| `GetDeletedKeysAsync(ct)` | `IAsyncEnumerable<DeletedKey>` | Lists all deleted keys. |
| `PurgeDeletedKeyAsync(name, ct)` | `Task` | Permanently removes a deleted key. |
| `RecoverDeletedKeyAsync(name, ct)` | `Task<KeyVaultKey>` | Recovers a deleted key and waits for completion. |

All async methods have synchronous equivalents (e.g., `GetKey`, `CreateKey`, etc.).

---

## Testing

The test project uses **xUnit v3** and **Moq** to unit-test every operation without hitting Azure.

### Running Tests

```bash
cd DeepSigma.Azure.KeyVault.Test
dotnet test
```

### Mocking in Your Own Tests

Because every operation is exposed through an interface, you can easily mock the services:

```csharp
using DeepSigma.Azure.KeyVault.Abstractions;
using Moq;

var mockSecrets = new Mock<ISecretService>();
mockSecrets
    .Setup(s => s.GetSecretValueAsync("api-key", It.IsAny<CancellationToken>()))
    .ReturnsAsync("test-value");

var service = new MyService(mockSecrets.Object);
```

The library implementations wrap the Azure SDK clients whose methods are `virtual`, so you can also mock at the client level if needed:

```csharp
using Azure.Security.KeyVault.Secrets;
using Moq;

var mockClient = new Mock<SecretClient>();
mockClient
    .Setup(c => c.GetSecretAsync("my-secret", null, It.IsAny<CancellationToken>()))
    .ReturnsAsync(Response.FromValue(secret, Mock.Of<Response>()));

var secretService = new SecretService(mockClient.Object);
```

---

## Dependencies

### Library

| Package | Version |
|---|---|
| [Azure.Identity](https://www.nuget.org/packages/Azure.Identity) | 1.20.0 |
| [Azure.Security.KeyVault.Keys](https://www.nuget.org/packages/Azure.Security.KeyVault.Keys) | 4.9.0 |
| [Azure.Security.KeyVault.Secrets](https://www.nuget.org/packages/Azure.Security.KeyVault.Secrets) | 4.9.0 |

### Tests

| Package | Version |
|---|---|
| [xunit.v3](https://www.nuget.org/packages/xunit.v3) | 3.2.2 |
| [Moq](https://www.nuget.org/packages/Moq) | 4.20.72 |
| [Microsoft.NET.Test.Sdk](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk) | 18.3.0 |

---

## Contributing

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature/my-feature`.
3. Commit your changes: `git commit -m 'Add my feature'`.
4. Push to the branch: `git push origin feature/my-feature`.
5. Open a pull request.

Please ensure all existing tests pass and add tests for any new functionality.

---

## License

This project is maintained by [DeepSigma LLC](https://github.com/DeepSigma-LLC).
