using Azure;
using Azure.Security.KeyVault.Secrets;
using DeepSigma.Azure.KeyVault.Services;
using Moq;
using Xunit;

namespace DeepSigma.Azure.KeyVault.Test;

public class SecretServiceTests
{
    private readonly Mock<SecretClient> _mockClient;
    private readonly SecretService _service;

    public SecretServiceTests()
    {
        _mockClient = new Mock<SecretClient>();
        _service = new SecretService(_mockClient.Object);
    }

    [Fact]
    public void Constructor_ThrowsWhenClientIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new SecretService(client: null!));
    }

    // Async tests

    [Fact]
    public async Task GetSecretAsync_ReturnsSecret()
    {
        var expected = new KeyVaultSecret("test-secret", "test-value");
        _mockClient
            .Setup(c => c.GetSecretAsync("test-secret", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(expected, Mock.Of<Response>()));

        var result = await _service.GetSecretAsync("test-secret");

        Assert.Equal("test-secret", result.Name);
        Assert.Equal("test-value", result.Value);
    }

    [Fact]
    public async Task GetSecretAsync_WithVersion_ReturnsSecret()
    {
        var expected = new KeyVaultSecret("test-secret", "versioned-value");
        _mockClient
            .Setup(c => c.GetSecretAsync("test-secret", "v1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(expected, Mock.Of<Response>()));

        var result = await _service.GetSecretAsync("test-secret", "v1");

        Assert.Equal("versioned-value", result.Value);
    }

    [Fact]
    public async Task GetSecretValueAsync_ReturnsOnlyValue()
    {
        var secret = new KeyVaultSecret("test-secret", "secret-value");
        _mockClient
            .Setup(c => c.GetSecretAsync("test-secret", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(secret, Mock.Of<Response>()));

        var result = await _service.GetSecretValueAsync("test-secret");

        Assert.Equal("secret-value", result);
    }

    [Fact]
    public async Task SetSecretAsync_SetsAndReturnsSecret()
    {
        var expected = new KeyVaultSecret("new-secret", "new-value");
        _mockClient
            .Setup(c => c.SetSecretAsync("new-secret", "new-value", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(expected, Mock.Of<Response>()));

        var result = await _service.SetSecretAsync("new-secret", "new-value");

        Assert.Equal("new-secret", result.Name);
        Assert.Equal("new-value", result.Value);
    }

    [Fact]
    public async Task UpdateSecretPropertiesAsync_ReturnsUpdatedProperties()
    {
        var properties = new SecretProperties("test-secret");
        _mockClient
            .Setup(c => c.UpdateSecretPropertiesAsync(properties, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(properties, Mock.Of<Response>()));

        var result = await _service.UpdateSecretPropertiesAsync(properties);

        Assert.Equal(properties.Name, result.Name);
    }

    [Fact]
    public async Task GetPropertiesOfSecretsAsync_ReturnsAllProperties()
    {
        var items = new List<SecretProperties>
        {
            new("secret-1"),
            new("secret-2"),
            new("secret-3")
        };
        var page = Page<SecretProperties>.FromValues(items, continuationToken: null, Mock.Of<Response>());
        var pageable = AsyncPageable<SecretProperties>.FromPages([page]);

        _mockClient
            .Setup(c => c.GetPropertiesOfSecretsAsync(It.IsAny<CancellationToken>()))
            .Returns(pageable);

        var results = new List<SecretProperties>();
        await foreach (var prop in _service.GetPropertiesOfSecretsAsync())
        {
            results.Add(prop);
        }

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task GetPropertiesOfSecretVersionsAsync_ReturnsVersionProperties()
    {
        var items = new List<SecretProperties> { new("secret-1"), new("secret-1") };
        var page = Page<SecretProperties>.FromValues(items, continuationToken: null, Mock.Of<Response>());
        var pageable = AsyncPageable<SecretProperties>.FromPages([page]);

        _mockClient
            .Setup(c => c.GetPropertiesOfSecretVersionsAsync("secret-1", It.IsAny<CancellationToken>()))
            .Returns(pageable);

        var results = new List<SecretProperties>();
        await foreach (var prop in _service.GetPropertiesOfSecretVersionsAsync("secret-1"))
        {
            results.Add(prop);
        }

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetDeletedSecretAsync_ReturnsDeletedSecret()
    {
        var deleted = SecretModelFactory.DeletedSecret(
            new SecretProperties("deleted-secret"),
            value: "old-value");
        _mockClient
            .Setup(c => c.GetDeletedSecretAsync("deleted-secret", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(deleted, Mock.Of<Response>()));

        var result = await _service.GetDeletedSecretAsync("deleted-secret");

        Assert.Equal("deleted-secret", result.Name);
    }

    [Fact]
    public async Task GetDeletedSecretsAsync_ReturnsAllDeletedSecrets()
    {
        var items = new List<DeletedSecret>
        {
            SecretModelFactory.DeletedSecret(new SecretProperties("deleted-1")),
            SecretModelFactory.DeletedSecret(new SecretProperties("deleted-2"))
        };
        var page = Page<DeletedSecret>.FromValues(items, continuationToken: null, Mock.Of<Response>());
        var pageable = AsyncPageable<DeletedSecret>.FromPages([page]);

        _mockClient
            .Setup(c => c.GetDeletedSecretsAsync(It.IsAny<CancellationToken>()))
            .Returns(pageable);

        var results = new List<DeletedSecret>();
        await foreach (var secret in _service.GetDeletedSecretsAsync())
        {
            results.Add(secret);
        }

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task PurgeDeletedSecretAsync_CallsClient()
    {
        _mockClient
            .Setup(c => c.PurgeDeletedSecretAsync("purge-me", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response>());

        await _service.PurgeDeletedSecretAsync("purge-me");

        _mockClient.Verify(c => c.PurgeDeletedSecretAsync("purge-me", It.IsAny<CancellationToken>()), Times.Once);
    }

    // Sync tests

    [Fact]
    public void GetSecret_ReturnsSecret()
    {
        var expected = new KeyVaultSecret("test-secret", "test-value");
        _mockClient
            .Setup(c => c.GetSecret("test-secret", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Response.FromValue(expected, Mock.Of<Response>()));

        var result = _service.GetSecret("test-secret");

        Assert.Equal("test-secret", result.Name);
        Assert.Equal("test-value", result.Value);
    }

    [Fact]
    public void GetSecretValue_ReturnsOnlyValue()
    {
        var secret = new KeyVaultSecret("test-secret", "secret-value");
        _mockClient
            .Setup(c => c.GetSecret("test-secret", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Response.FromValue(secret, Mock.Of<Response>()));

        var result = _service.GetSecretValue("test-secret");

        Assert.Equal("secret-value", result);
    }

    [Fact]
    public void SetSecret_SetsAndReturnsSecret()
    {
        var expected = new KeyVaultSecret("new-secret", "new-value");
        _mockClient
            .Setup(c => c.SetSecret("new-secret", "new-value", It.IsAny<CancellationToken>()))
            .Returns(Response.FromValue(expected, Mock.Of<Response>()));

        var result = _service.SetSecret("new-secret", "new-value");

        Assert.Equal("new-secret", result.Name);
        Assert.Equal("new-value", result.Value);
    }

    [Fact]
    public void UpdateSecretProperties_ReturnsUpdatedProperties()
    {
        var properties = new SecretProperties("test-secret");
        _mockClient
            .Setup(c => c.UpdateSecretProperties(properties, It.IsAny<CancellationToken>()))
            .Returns(Response.FromValue(properties, Mock.Of<Response>()));

        var result = _service.UpdateSecretProperties(properties);

        Assert.Equal(properties.Name, result.Name);
    }

    [Fact]
    public void GetPropertiesOfSecrets_ReturnsAllProperties()
    {
        var items = new List<SecretProperties>
        {
            new("secret-1"),
            new("secret-2"),
            new("secret-3")
        };
        var page = Page<SecretProperties>.FromValues(items, continuationToken: null, Mock.Of<Response>());
        var pageable = Pageable<SecretProperties>.FromPages([page]);

        _mockClient
            .Setup(c => c.GetPropertiesOfSecrets(It.IsAny<CancellationToken>()))
            .Returns(pageable);

        var results = _service.GetPropertiesOfSecrets().ToList();

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void GetPropertiesOfSecretVersions_ReturnsVersionProperties()
    {
        var items = new List<SecretProperties> { new("secret-1"), new("secret-1") };
        var page = Page<SecretProperties>.FromValues(items, continuationToken: null, Mock.Of<Response>());
        var pageable = Pageable<SecretProperties>.FromPages([page]);

        _mockClient
            .Setup(c => c.GetPropertiesOfSecretVersions("secret-1", It.IsAny<CancellationToken>()))
            .Returns(pageable);

        var results = _service.GetPropertiesOfSecretVersions("secret-1").ToList();

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void GetDeletedSecret_ReturnsDeletedSecret()
    {
        var deleted = SecretModelFactory.DeletedSecret(
            new SecretProperties("deleted-secret"),
            value: "old-value");
        _mockClient
            .Setup(c => c.GetDeletedSecret("deleted-secret", It.IsAny<CancellationToken>()))
            .Returns(Response.FromValue(deleted, Mock.Of<Response>()));

        var result = _service.GetDeletedSecret("deleted-secret");

        Assert.Equal("deleted-secret", result.Name);
    }

    [Fact]
    public void GetDeletedSecrets_ReturnsAllDeletedSecrets()
    {
        var items = new List<DeletedSecret>
        {
            SecretModelFactory.DeletedSecret(new SecretProperties("deleted-1")),
            SecretModelFactory.DeletedSecret(new SecretProperties("deleted-2"))
        };
        var page = Page<DeletedSecret>.FromValues(items, continuationToken: null, Mock.Of<Response>());
        var pageable = Pageable<DeletedSecret>.FromPages([page]);

        _mockClient
            .Setup(c => c.GetDeletedSecrets(It.IsAny<CancellationToken>()))
            .Returns(pageable);

        var results = _service.GetDeletedSecrets().ToList();

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void PurgeDeletedSecret_CallsClient()
    {
        _mockClient
            .Setup(c => c.PurgeDeletedSecret("purge-me", It.IsAny<CancellationToken>()))
            .Returns(Mock.Of<Response>());

        _service.PurgeDeletedSecret("purge-me");

        _mockClient.Verify(c => c.PurgeDeletedSecret("purge-me", It.IsAny<CancellationToken>()), Times.Once);
    }
}
