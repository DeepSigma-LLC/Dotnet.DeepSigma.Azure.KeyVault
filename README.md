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
