using Azure;
using Azure.Security.KeyVault.Keys;
using DeepSigma.Azure.KeyVault.Services;
using Moq;
using Xunit;

namespace DeepSigma.Azure.KeyVault.Test;

public class KeyServiceTests
{
    private readonly Mock<KeyClient> _mockClient;
    private readonly KeyService _service;

    public KeyServiceTests()
    {
        _mockClient = new Mock<KeyClient>();
        _service = new KeyService(_mockClient.Object);
    }

    [Fact]
    public void Constructor_ThrowsWhenClientIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new KeyService(client: null!));
    }

    // Async tests

    [Fact]
    public async Task GetKeyAsync_ReturnsKey()
    {
        var expected = KeyModelFactory.KeyVaultKey(
            new KeyProperties("test-key"),
            new JsonWebKey([KeyOperation.Encrypt]));
        _mockClient
            .Setup(c => c.GetKeyAsync("test-key", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(expected, Mock.Of<Response>()));

        var result = await _service.GetKeyAsync("test-key");

        Assert.Equal("test-key", result.Name);
    }

    [Fact]
    public async Task GetKeyAsync_WithVersion_ReturnsKey()
    {
        var expected = KeyModelFactory.KeyVaultKey(
            new KeyProperties("test-key"),
            new JsonWebKey([KeyOperation.Encrypt]));
        _mockClient
            .Setup(c => c.GetKeyAsync("test-key", "v1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(expected, Mock.Of<Response>()));

        var result = await _service.GetKeyAsync("test-key", "v1");

        Assert.Equal("test-key", result.Name);
    }

    [Fact]
    public async Task CreateKeyAsync_ReturnsCreatedKey()
    {
        var expected = KeyModelFactory.KeyVaultKey(
            new KeyProperties("rsa-key"),
            new JsonWebKey([KeyOperation.Encrypt]));
        _mockClient
            .Setup(c => c.CreateKeyAsync("rsa-key", KeyType.Rsa, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(expected, Mock.Of<Response>()));

        var result = await _service.CreateKeyAsync("rsa-key", KeyType.Rsa);

        Assert.Equal("rsa-key", result.Name);
    }

    [Fact]
    public async Task CreateRsaKeyAsync_ReturnsCreatedKey()
    {
        var options = new CreateRsaKeyOptions("rsa-key-2048") { KeySize = 2048 };
        var expected = KeyModelFactory.KeyVaultKey(
            new KeyProperties("rsa-key-2048"),
            new JsonWebKey([KeyOperation.Encrypt, KeyOperation.Decrypt]));
        _mockClient
            .Setup(c => c.CreateRsaKeyAsync(options, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(expected, Mock.Of<Response>()));

        var result = await _service.CreateRsaKeyAsync(options);

        Assert.Equal("rsa-key-2048", result.Name);
    }

    [Fact]
    public async Task CreateEcKeyAsync_ReturnsCreatedKey()
    {
        var options = new CreateEcKeyOptions("ec-key");
        var expected = KeyModelFactory.KeyVaultKey(
            new KeyProperties("ec-key"),
            new JsonWebKey([KeyOperation.Sign, KeyOperation.Verify]));
        _mockClient
            .Setup(c => c.CreateEcKeyAsync(options, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(expected, Mock.Of<Response>()));

        var result = await _service.CreateEcKeyAsync(options);

        Assert.Equal("ec-key", result.Name);
    }

    [Fact]
    public async Task GetPropertiesOfKeysAsync_ReturnsAllProperties()
    {
        var items = new List<KeyProperties>
        {
            new("key-1"),
            new("key-2"),
            new("key-3")
        };
        var page = Page<KeyProperties>.FromValues(items, continuationToken: null, Mock.Of<Response>());
        var pageable = AsyncPageable<KeyProperties>.FromPages([page]);

        _mockClient
            .Setup(c => c.GetPropertiesOfKeysAsync(It.IsAny<CancellationToken>()))
            .Returns(pageable);

        var results = new List<KeyProperties>();
        await foreach (var prop in _service.GetPropertiesOfKeysAsync())
        {
            results.Add(prop);
        }

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task GetPropertiesOfKeyVersionsAsync_ReturnsVersionProperties()
    {
        var items = new List<KeyProperties> { new("key-1"), new("key-1") };
        var page = Page<KeyProperties>.FromValues(items, continuationToken: null, Mock.Of<Response>());
        var pageable = AsyncPageable<KeyProperties>.FromPages([page]);

        _mockClient
            .Setup(c => c.GetPropertiesOfKeyVersionsAsync("key-1", It.IsAny<CancellationToken>()))
            .Returns(pageable);

        var results = new List<KeyProperties>();
        await foreach (var prop in _service.GetPropertiesOfKeyVersionsAsync("key-1"))
        {
            results.Add(prop);
        }

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetDeletedKeyAsync_ReturnsDeletedKey()
    {
        var deleted = KeyModelFactory.DeletedKey(
            new KeyProperties("deleted-key"),
            new JsonWebKey([KeyOperation.Encrypt]));
        _mockClient
            .Setup(c => c.GetDeletedKeyAsync("deleted-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(deleted, Mock.Of<Response>()));

        var result = await _service.GetDeletedKeyAsync("deleted-key");

        Assert.Equal("deleted-key", result.Name);
    }

    [Fact]
    public async Task GetDeletedKeysAsync_ReturnsAllDeletedKeys()
    {
        var items = new List<DeletedKey>
        {
            KeyModelFactory.DeletedKey(new KeyProperties("deleted-1"), new JsonWebKey([KeyOperation.Encrypt])),
            KeyModelFactory.DeletedKey(new KeyProperties("deleted-2"), new JsonWebKey([KeyOperation.Encrypt]))
        };
        var page = Page<DeletedKey>.FromValues(items, continuationToken: null, Mock.Of<Response>());
        var pageable = AsyncPageable<DeletedKey>.FromPages([page]);

        _mockClient
            .Setup(c => c.GetDeletedKeysAsync(It.IsAny<CancellationToken>()))
            .Returns(pageable);

        var results = new List<DeletedKey>();
        await foreach (var key in _service.GetDeletedKeysAsync())
        {
            results.Add(key);
        }

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task PurgeDeletedKeyAsync_CallsClient()
    {
        _mockClient
            .Setup(c => c.PurgeDeletedKeyAsync("purge-me", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response>());

        await _service.PurgeDeletedKeyAsync("purge-me");

        _mockClient.Verify(c => c.PurgeDeletedKeyAsync("purge-me", It.IsAny<CancellationToken>()), Times.Once);
    }

    // Sync tests

    [Fact]
    public void GetKey_ReturnsKey()
    {
        var expected = KeyModelFactory.KeyVaultKey(
            new KeyProperties("test-key"),
            new JsonWebKey([KeyOperation.Encrypt]));
        _mockClient
            .Setup(c => c.GetKey("test-key", null, It.IsAny<CancellationToken>()))
            .Returns(Response.FromValue(expected, Mock.Of<Response>()));

        var result = _service.GetKey("test-key");

        Assert.Equal("test-key", result.Name);
    }

    [Fact]
    public void CreateKey_ReturnsCreatedKey()
    {
        var expected = KeyModelFactory.KeyVaultKey(
            new KeyProperties("rsa-key"),
            new JsonWebKey([KeyOperation.Encrypt]));
        _mockClient
            .Setup(c => c.CreateKey("rsa-key", KeyType.Rsa, null, It.IsAny<CancellationToken>()))
            .Returns(Response.FromValue(expected, Mock.Of<Response>()));

        var result = _service.CreateKey("rsa-key", KeyType.Rsa);

        Assert.Equal("rsa-key", result.Name);
    }

    [Fact]
    public void CreateRsaKey_ReturnsCreatedKey()
    {
        var options = new CreateRsaKeyOptions("rsa-key-4096") { KeySize = 4096 };
        var expected = KeyModelFactory.KeyVaultKey(
            new KeyProperties("rsa-key-4096"),
            new JsonWebKey([KeyOperation.Encrypt, KeyOperation.Decrypt]));
        _mockClient
            .Setup(c => c.CreateRsaKey(options, It.IsAny<CancellationToken>()))
            .Returns(Response.FromValue(expected, Mock.Of<Response>()));

        var result = _service.CreateRsaKey(options);

        Assert.Equal("rsa-key-4096", result.Name);
    }

    [Fact]
    public void CreateEcKey_ReturnsCreatedKey()
    {
        var options = new CreateEcKeyOptions("ec-key");
        var expected = KeyModelFactory.KeyVaultKey(
            new KeyProperties("ec-key"),
            new JsonWebKey([KeyOperation.Sign, KeyOperation.Verify]));
        _mockClient
            .Setup(c => c.CreateEcKey(options, It.IsAny<CancellationToken>()))
            .Returns(Response.FromValue(expected, Mock.Of<Response>()));

        var result = _service.CreateEcKey(options);

        Assert.Equal("ec-key", result.Name);
    }

    [Fact]
    public void GetPropertiesOfKeys_ReturnsAllProperties()
    {
        var items = new List<KeyProperties>
        {
            new("key-1"),
            new("key-2"),
            new("key-3")
        };
        var page = Page<KeyProperties>.FromValues(items, continuationToken: null, Mock.Of<Response>());
        var pageable = Pageable<KeyProperties>.FromPages([page]);

        _mockClient
            .Setup(c => c.GetPropertiesOfKeys(It.IsAny<CancellationToken>()))
            .Returns(pageable);

        var results = _service.GetPropertiesOfKeys().ToList();

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void GetPropertiesOfKeyVersions_ReturnsVersionProperties()
    {
        var items = new List<KeyProperties> { new("key-1"), new("key-1") };
        var page = Page<KeyProperties>.FromValues(items, continuationToken: null, Mock.Of<Response>());
        var pageable = Pageable<KeyProperties>.FromPages([page]);

        _mockClient
            .Setup(c => c.GetPropertiesOfKeyVersions("key-1", It.IsAny<CancellationToken>()))
            .Returns(pageable);

        var results = _service.GetPropertiesOfKeyVersions("key-1").ToList();

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void GetDeletedKey_ReturnsDeletedKey()
    {
        var deleted = KeyModelFactory.DeletedKey(
            new KeyProperties("deleted-key"),
            new JsonWebKey([KeyOperation.Encrypt]));
        _mockClient
            .Setup(c => c.GetDeletedKey("deleted-key", It.IsAny<CancellationToken>()))
            .Returns(Response.FromValue(deleted, Mock.Of<Response>()));

        var result = _service.GetDeletedKey("deleted-key");

        Assert.Equal("deleted-key", result.Name);
    }

    [Fact]
    public void GetDeletedKeys_ReturnsAllDeletedKeys()
    {
        var items = new List<DeletedKey>
        {
            KeyModelFactory.DeletedKey(new KeyProperties("deleted-1"), new JsonWebKey([KeyOperation.Encrypt])),
            KeyModelFactory.DeletedKey(new KeyProperties("deleted-2"), new JsonWebKey([KeyOperation.Encrypt]))
        };
        var page = Page<DeletedKey>.FromValues(items, continuationToken: null, Mock.Of<Response>());
        var pageable = Pageable<DeletedKey>.FromPages([page]);

        _mockClient
            .Setup(c => c.GetDeletedKeys(It.IsAny<CancellationToken>()))
            .Returns(pageable);

        var results = _service.GetDeletedKeys().ToList();

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void PurgeDeletedKey_CallsClient()
    {
        _mockClient
            .Setup(c => c.PurgeDeletedKey("purge-me", It.IsAny<CancellationToken>()))
            .Returns(Mock.Of<Response>());

        _service.PurgeDeletedKey("purge-me");

        _mockClient.Verify(c => c.PurgeDeletedKey("purge-me", It.IsAny<CancellationToken>()), Times.Once);
    }
}
