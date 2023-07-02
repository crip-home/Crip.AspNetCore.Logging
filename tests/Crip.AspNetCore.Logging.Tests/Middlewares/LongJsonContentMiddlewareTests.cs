using System.Text;
using System.Text.RegularExpressions;
using Crip.Extensions.Tests;
using Microsoft.Extensions.Options;

namespace Crip.AspNetCore.Logging.Tests.Middlewares;

public class LongJsonContentMiddlewareTests
{
    private readonly LongJsonContentOptions _jsonContentOptions = new()
    {
        MaxCharCountInField = 50,
        LeaveOnTrimCharCountInField = 10,
    };

    private readonly IOptions<RequestLoggingOptions> _options;

    public LongJsonContentMiddlewareTests()
    {
        _options = Options.Create(new RequestLoggingOptions
        {
            LongJsonContent = _jsonContentOptions,
        });

    }

    [Fact, Trait("Category", "Unit")]
    public void Constructor_OverridesDefaultsWithValuesFromConfig()
    {
        // Arrange
        JsonStreamModifier jsonBuilder = new();
        _jsonContentOptions.MaxCharCountInField = 999;
        _jsonContentOptions.LeaveOnTrimCharCountInField = 888;

        // Act
        var middleware = new LongJsonContentMiddleware(jsonBuilder, _options);

        // Assert
        middleware.MaxCharCountInField.Should().Be(999);
        middleware.LeaveOnTrim.Should().Be(888);
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_ProperlyCreateComplex()
    {
        // Arrange
        JsonStreamModifier jsonBuilder = new();
        _jsonContentOptions.MaxCharCountInField = 50;
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        var content = this.LoadResource("sis_vehicle_response.json");
        content = Regex.Replace(content, @"\s", "");
        var bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        // Act
        sut.Modify(new MemoryStream(bytes), output);

        // Assert
        var expected = this.LoadResource("sis_vehicle_response_log.json");
        expected = Regex.Replace(expected, @"\s", "");

        new StreamReader(output).ReadToEnd()
            .Should().NotBeEmpty()
            .And.BeEquivalentTo(expected);
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_ProperlyCreateArray()
    {
        // Arrange
        JsonStreamModifier jsonBuilder = new();
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        var content = "[\"1\",\"2\"]";
        var bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        // Act
        sut.Modify(new MemoryStream(bytes), output);

        // Assert
        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("[\"1\",\"2\"]");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_ProperlyCreateObject()
    {
        // Arrange
        JsonStreamModifier jsonBuilder = new();
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        var content = "{\"1\":1,\"2\":true}";
        var bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        // Act
        sut.Modify(new MemoryStream(bytes), output);

        // Assert
        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("{\"1\":1,\"2\":true}");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_ProperlyHandlesNullValue()
    {
        // Arrange
        JsonStreamModifier jsonBuilder = new();
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        var content = "{\"1\":null,\"2\":true}";
        var bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        // Act
        sut.Modify(new MemoryStream(bytes), output);

        // Assert
        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("{\"1\":null,\"2\":true}");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_ProperlyCreateNested()
    {
        // Arrange
        JsonStreamModifier jsonBuilder = new();
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        var content = "{\"1\":1.1,\"object\":[\"1\",{\"2\":null}]}";
        var bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        // Act
        sut.Modify(new MemoryStream(bytes), output);

        // Assert
        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("{\"1\":1.1,\"object\":[\"1\",{\"2\":null}]}");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_IgnoresNonJsonContentType()
    {
        // Arrange
        JsonStreamModifier jsonBuilder = new();
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        var content = "This message is not a JSON string";
        var bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        // Act
        sut.Modify(new MemoryStream(bytes), output);

        // Assert
        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("This message is not a JSON string");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_IgnoresNonJson()
    {
        // Arrange
        JsonStreamModifier jsonBuilder = new();
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        var content = "This message is not a JSON string";
        var bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        // Act
        sut.Modify(new MemoryStream(bytes), output);

        // Assert
        new StreamReader(output).ReadToEnd().Should()
            .Be("This message is not a JSON string");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_DoesNotModifySmallJson()
    {
        // Arrange
        JsonStreamModifier jsonBuilder = new();
        _jsonContentOptions.MaxCharCountInField = 15;
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        var content = """{"key1":1,"key2":"some content"}""";
        var bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        // Act
        sut.Modify(new MemoryStream(bytes), output);

        // Assert
        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("""{"key1":1,"key2":"some content"}""");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_TrimsValue()
    {
        // Arrange
        var jsonBuilder = new JsonStreamModifier();
        _jsonContentOptions.MaxCharCountInField = 10;
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        var content = """{"key1":"short","key2":"some long content"}""";
        var bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        // Act
        sut.Modify(new MemoryStream(bytes), output);

        // Assert
        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("""{"key1":"short","key2":"some long ..."}""");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_TrimsCustomLengthValue()
    {
        // Arrange
        JsonStreamModifier jsonBuilder = new();
        _jsonContentOptions.MaxCharCountInField = 10;
        _jsonContentOptions.LeaveOnTrimCharCountInField = 12;
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);

        var content = """{"key1":"short","key2":"some long content"}""";
        var bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        // Act
        sut.Modify(new MemoryStream(bytes), output);

        // Assert
        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("""{"key1":"short","key2":"some long co..."}""");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_TrimsValueOnMultiDimension()
    {
        // Arrange
        JsonStreamModifier jsonBuilder = new();
        _jsonContentOptions.MaxCharCountInField = 10;
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        var content = """{"key1":"short","key2":{"key1":"some long content"}}""";
        var bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        // Act
        sut.Modify(new MemoryStream(bytes), output);

        // Assert
        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("""{"key1":"short","key2":{"key1":"some long ..."}}""");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_ProperlyFailsOnInvalidTrimValue()
    {
        // Arrange
        JsonStreamModifier jsonBuilder = new();

        // Act
        _jsonContentOptions.MaxCharCountInField = 0;
        Func<IRequestContentLogMiddleware> act = () => new LongJsonContentMiddleware(jsonBuilder, _options);

        // Assert
        act.Should()
            .ThrowExactly<ArgumentOutOfRangeException>()
            .WithMessage("Specified argument was out of the range of valid values. (Parameter 'maxCharCountInField')");
    }
}