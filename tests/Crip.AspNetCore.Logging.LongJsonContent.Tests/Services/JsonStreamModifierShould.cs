using System.Text;
using Crip.AspNetCore.Logging.LongJsonContent.Services;

namespace Crip.AspNetCore.Logging.LongJsonContent.Tests.Services;

public class JsonStreamModifierShould
{
    private readonly IJsonStreamModifier _modifier = new JsonStreamModifier();

    [Theory, Trait("Category", "Unit")]
    [InlineData("")]
    [InlineData("[]")]
    [InlineData("[1,2]")]
    [InlineData("{\"key\":1}")]
    [InlineData("{\"key\":true}")]
    [InlineData("{\"key\":1.2}")]
    [InlineData("{\"key\":\"value\"}")]
    public void JsonStreamModifier_Modify_WritesAsOriginal(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var input = new MemoryStream(bytes);
        var output = new MemoryStream();

        _modifier.Modify(input, output);
        using var reader = new StreamReader(output);
        var text = reader.ReadToEnd();

        text.Should().Be(data);
    }


    [Theory, Trait("Category", "Unit")]
    [InlineData("{\"key\":1}")]
    [InlineData("{\"key\":true}")]
    [InlineData("{\"key\":1.2}")]
    [InlineData("{\"key\":\"value\"}")]
    public void Modify_CustomValue(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var input = new MemoryStream(bytes);
        var output = new MemoryStream();

        _modifier.Modify(input, output, null, (t, v) => "value");
        using var reader = new StreamReader(output);
        var text = reader.ReadToEnd();

        text.Should().Be("{\"key\":\"value\"}");
    }

    [Theory, Trait("Category", "Unit")]
    [InlineData("{\"a\":\"value\"}")]
    [InlineData("{\"b\":\"value\"}")]
    [InlineData("{\"c\":\"value\"}")]
    [InlineData("{1:\"value\"}")]
    public void Modify_CustomKey(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var input = new MemoryStream(bytes);
        var output = new MemoryStream();

        _modifier.Modify(input, output, k => "key");
        using var reader = new StreamReader(output);
        var text = reader.ReadToEnd();

        text.Should().Be("{\"key\":\"value\"}");
    }
}