using System.Text;
using System.Text.RegularExpressions;
using Crip.AspNetCore.Logging.LongJsonContent.Configuration;
using Crip.AspNetCore.Logging.LongJsonContent.Services;
using Crip.Extensions.Tests;
using Microsoft.Extensions.Options;

namespace Crip.AspNetCore.Logging.LongJsonContent.Tests;

public class LongJsonContentMiddlewareShould
{
    private readonly LongJsonContentOptions _jsonContentOptions = new()
    {
        MaxCharCountInField = 20,
        LeaveOnTrimCharCountInField = 10,
    };

    private readonly IOptions<LongJsonContentOptions> _options;

    public LongJsonContentMiddlewareShould()
    {
        _options = Options.Create(_jsonContentOptions);
    }

    [Fact, Trait("Category", "Unit")]
    public void Constructor_OverridesDefaultsWithValuesFromConfig()
    {
        JsonStreamModifier jsonBuilder = new();
        _jsonContentOptions.MaxCharCountInField = 999;
        _jsonContentOptions.LeaveOnTrimCharCountInField = 888;

        var middleware = new LongJsonContentMiddleware(jsonBuilder, _options);

        middleware.MaxCharCountInField.Should().Be(999);
        middleware.LeaveOnTrim.Should().Be(888);
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_ProperlyCreateComplex()
    {

        JsonStreamModifier jsonBuilder = new();
        _jsonContentOptions.MaxCharCountInField = 50;
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        string content = this.LoadResource("sis_vehicle_response.json");
        content = Regex.Replace(content, @"\s", "");
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        sut.Modify(new MemoryStream(bytes), output);

        string expected = this.LoadResource("sis_vehicle_response_log.json");
        expected = Regex.Replace(expected, @"\s", "");

        new StreamReader(output).ReadToEnd()
            .Should().NotBeEmpty()
            .And.BeEquivalentTo(expected);
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_ProperlyCreateArray()
    {

        JsonStreamModifier jsonBuilder = new();
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        string content = "[\"1\",\"2\"]";
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        sut.Modify(new MemoryStream(bytes), output);

        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("[\"1\",\"2\"]");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_ProperlyCreateObject()
    {

        JsonStreamModifier jsonBuilder = new();
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        string content = "{\"1\":1,\"2\":true}";
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        sut.Modify(new MemoryStream(bytes), output);

        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("{\"1\":1,\"2\":true}");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_ProperlyHandlesNullValue()
    {

        JsonStreamModifier jsonBuilder = new();
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        string content = "{\"1\":null,\"2\":true}";
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        sut.Modify(new MemoryStream(bytes), output);

        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("{\"1\":null,\"2\":true}");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_ProperlyCreateNested()
    {

        JsonStreamModifier jsonBuilder = new();
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        string content = "{\"1\":1.1,\"object\":[\"1\",{\"2\":null}]}";
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        sut.Modify(new MemoryStream(bytes), output);

        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("{\"1\":1.1,\"object\":[\"1\",{\"2\":null}]}");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_IgnoresNonJsonContentType()
    {

        JsonStreamModifier jsonBuilder = new();
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        string content = "This message is not a JSON string";
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        sut.Modify(new MemoryStream(bytes), output);

        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo("This message is not a JSON string");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_IgnoresNonJson()
    {

        JsonStreamModifier jsonBuilder = new();
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        string content = "This message is not a JSON string";
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        sut.Modify(new MemoryStream(bytes), output);

        new StreamReader(output).ReadToEnd().Should()
            .Be("This message is not a JSON string");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_DoesNotModifySmallJson()
    {

        JsonStreamModifier jsonBuilder = new();
        _jsonContentOptions.MaxCharCountInField = 15;
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        string content = @"{""key1"":1,""key2"":""some content""}";
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        sut.Modify(new MemoryStream(bytes), output);

        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo(@"{""key1"":1,""key2"":""some content""}");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_TrimsValue()
    {

        var jsonBuilder = new JsonStreamModifier();
        _jsonContentOptions.MaxCharCountInField = 10;
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        string content = @"{""key1"":""short"",""key2"":""some long content""}";
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        sut.Modify(new MemoryStream(bytes), output);

        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo(@"{""key1"":""short"",""key2"":""some long ...""}");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_TrimsCustomLengthValue()
    {

        JsonStreamModifier jsonBuilder = new();
        _jsonContentOptions.MaxCharCountInField = 10;
        _jsonContentOptions.LeaveOnTrimCharCountInField = 12;

        LongJsonContentMiddleware sut = new(jsonBuilder, _options);

        string content = @"{""key1"":""short"",""key2"":""some long content""}";
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        sut.Modify(new MemoryStream(bytes), output);

        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo(@"{""key1"":""short"",""key2"":""some long co...""}");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_TrimsValueOnMultiDimension()
    {

        JsonStreamModifier jsonBuilder = new();
        _jsonContentOptions.MaxCharCountInField = 10;
        LongJsonContentMiddleware sut = new(jsonBuilder, _options);
        string content = @"{""key1"":""short"",""key2"":{""key1"":""some long content""}}";
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream output = new();

        sut.Modify(new MemoryStream(bytes), output);

        new StreamReader(output).ReadToEnd().Should()
            .NotBeEmpty()
            .And.BeEquivalentTo(@"{""key1"":""short"",""key2"":{""key1"":""some long ...""}}");
    }

    [Fact, Trait("Category", "Unit")]
    public void Modify_ProperlyFailsOnInvalidTrimValue()
    {
        JsonStreamModifier jsonBuilder = new();

        _jsonContentOptions.MaxCharCountInField = 0;
        Func<IRequestContentLogMiddleware> act = () => new LongJsonContentMiddleware(jsonBuilder, _options);

        act.Should()
            .ThrowExactly<ArgumentOutOfRangeException>()
            .WithMessage("Specified argument was out of the range of valid values. (Parameter 'maxCharCountInField')");
    }
}