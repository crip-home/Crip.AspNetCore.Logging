using System.Text;

namespace Crip.AspNetCore.Logging.Tests.Services;

public class JsonStreamModifierTests
{
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
        // Arrange
        IJsonStreamModifier sut = new JsonStreamModifier();

        var bytes = Encoding.UTF8.GetBytes(data);
        var input = new MemoryStream(bytes);
        var output = new MemoryStream();

        // Act
        sut.Modify(input, output);
        using var reader = new StreamReader(output);
        var text = reader.ReadToEnd();

        // Assert
        text.Should().Be(data);
    }


    [Theory, Trait("Category", "Unit")]
    [InlineData("{\"key\":1}")]
    [InlineData("{\"key\":true}")]
    [InlineData("{\"key\":1.2}")]
    [InlineData("{\"key\":\"value\"}")]
    public void JsonContentBuilder_Build_CustomValue(string data)
    {
        // Arrange
        IJsonStreamModifier sut = new JsonStreamModifier();

        var bytes = Encoding.UTF8.GetBytes(data);
        var input = new MemoryStream(bytes);
        var output = new MemoryStream();

        // Act
        sut.Modify(input, output, null, (t, v) => "value");
        using var reader = new StreamReader(output);
        var text = reader.ReadToEnd();

        // Assert
        text.Should().Be("{\"key\":\"value\"}");
    }

    [Theory, Trait("Category", "Unit")]
    [InlineData("{\"a\":\"value\"}")]
    [InlineData("{\"b\":\"value\"}")]
    [InlineData("{\"c\":\"value\"}")]
    [InlineData("{1:\"value\"}")]
    public void JsonContentBuilder_Build_CustomKey(string data)
    {
        // Arrange
        IJsonStreamModifier sut = new JsonStreamModifier();

        var bytes = Encoding.UTF8.GetBytes(data);
        var input = new MemoryStream(bytes);
        var output = new MemoryStream();

        // Act
        sut.Modify(input, output, k => "key");
        using var reader = new StreamReader(output);
        var text = reader.ReadToEnd();

        // Assert
        text.Should().Be("{\"key\":\"value\"}");
    }
}