using System.Text;
using System.Text.RegularExpressions;
using Crip.Extensions.Tests;
using Microsoft.Extensions.Configuration;

namespace Crip.AspNetCore.Logging.Tests
{
    public class LongJsonContentMiddlewareTests
    {
        [Fact, Trait("Category", "Unit")]
        public void LongJsonContentMiddleware_Constructor_OverridesDefaultsWithValuesFromConfig()
        {
            // Arrange
            JsonStreamModifier jsonBuilder = new();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Logging:Request:MaxCharCountInField", "999" },
                    { "Logging:Request:LeaveOnTrimCharCountInField", "888" },
                })
                .Build();

            // Act
            var middleware = new LongJsonContentMiddleware(jsonBuilder, configuration);

            // Assert
            middleware.MaxCharCountInField.Should().Be(999);
            middleware.LeaveOnTrim.Should().Be(888);
        }

        [Fact, Trait("Category", "Unit")]
        public void LongJsonContentMiddleware_Modify_ProperlyCreateComplex()
        {
            // Arrange
            JsonStreamModifier jsonBuilder = new();
            LongJsonContentMiddleware sut = new(jsonBuilder, 50);
            string content = this.LoadResource("sis_vehicle_response.json");
            content = Regex.Replace(content, @"\s", "");
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            MemoryStream output = new();

            // Act
            sut.Modify(new MemoryStream(bytes), output);

            // Assert
            string expected = this.LoadResource("sis_vehicle_response_log.json");
            expected = Regex.Replace(expected, @"\s", "");

            new StreamReader(output).ReadToEnd()
                .Should().NotBeEmpty()
                .And.BeEquivalentTo(expected);
        }

        [Fact, Trait("Category", "Unit")]
        public void LongJsonContentMiddleware_Modify_ProperlyCreateArray()
        {
            // Arrange
            JsonStreamModifier jsonBuilder = new();
            LongJsonContentMiddleware sut = new(jsonBuilder, null);
            string content = "[\"1\",\"2\"]";
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            MemoryStream output = new();

            // Act
            sut.Modify(new MemoryStream(bytes), output);

            // Assert
            new StreamReader(output).ReadToEnd().Should()
                .NotBeEmpty()
                .And.BeEquivalentTo("[\"1\",\"2\"]");
        }

        [Fact, Trait("Category", "Unit")]
        public void LongJsonContentMiddleware_Modify_ProperlyCreateObject()
        {
            // Arrange
            JsonStreamModifier jsonBuilder = new();
            LongJsonContentMiddleware sut = new(jsonBuilder, null);
            string content = "{\"1\":1,\"2\":true}";
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            MemoryStream output = new();

            // Act
            sut.Modify(new MemoryStream(bytes), output);

            // Assert
            new StreamReader(output).ReadToEnd().Should()
                .NotBeEmpty()
                .And.BeEquivalentTo("{\"1\":1,\"2\":true}");
        }

        [Fact, Trait("Category", "Unit")]
        public void LongJsonContentMiddleware_Modify_ProperlyHandlesNullValue()
        {
            // Arrange
            JsonStreamModifier jsonBuilder = new();
            LongJsonContentMiddleware sut = new(jsonBuilder, null);
            string content = "{\"1\":null,\"2\":true}";
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            MemoryStream output = new();

            // Act
            sut.Modify(new MemoryStream(bytes), output);

            // Assert
            new StreamReader(output).ReadToEnd().Should()
                .NotBeEmpty()
                .And.BeEquivalentTo("{\"1\":null,\"2\":true}");
        }

        [Fact, Trait("Category", "Unit")]
        public void LongJsonContentMiddleware_Modify_ProperlyCreateNested()
        {
            // Arrange
            JsonStreamModifier jsonBuilder = new();
            LongJsonContentMiddleware sut = new(jsonBuilder, null);
            string content = "{\"1\":1.1,\"object\":[\"1\",{\"2\":null}]}";
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            MemoryStream output = new();

            // Act
            sut.Modify(new MemoryStream(bytes), output);

            // Assert
            new StreamReader(output).ReadToEnd().Should()
                .NotBeEmpty()
                .And.BeEquivalentTo("{\"1\":1.1,\"object\":[\"1\",{\"2\":null}]}");
        }

        [Fact, Trait("Category", "Unit")]
        public void LongJsonContentMiddleware_Modify_IgnoresNonJsonContentType()
        {
            // Arrange
            JsonStreamModifier jsonBuilder = new();
            LongJsonContentMiddleware sut = new(jsonBuilder, null);
            string content = "This message is not a JSON string";
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            MemoryStream output = new();

            // Act
            sut.Modify(new MemoryStream(bytes), output);

            // Assert
            new StreamReader(output).ReadToEnd().Should()
                .NotBeEmpty()
                .And.BeEquivalentTo("This message is not a JSON string");
        }

        [Fact, Trait("Category", "Unit")]
        public void LongJsonContentMiddleware_Modify_IgnoresNonJson()
        {
            // Arrange
            JsonStreamModifier jsonBuilder = new();
            LongJsonContentMiddleware sut = new(jsonBuilder, null);
            string content = "This message is not a JSON string";
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            MemoryStream output = new();

            // Act
            sut.Modify(new MemoryStream(bytes), output);

            // Assert
            new StreamReader(output).ReadToEnd().Should()
                .Be("This message is not a JSON string");
        }

        [Fact, Trait("Category", "Unit")]
        public void LongJsonContentMiddleware_Modify_DoesNotModifySmallJson()
        {
            // Arrange
            JsonStreamModifier jsonBuilder = new();
            LongJsonContentMiddleware sut = new(jsonBuilder, 15);
            string content = @"{""key1"":1,""key2"":""some content""}";
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            MemoryStream output = new();

            // Act
            sut.Modify(new MemoryStream(bytes), output);

            // Assert
            new StreamReader(output).ReadToEnd().Should()
                .NotBeEmpty()
                .And.BeEquivalentTo(@"{""key1"":1,""key2"":""some content""}");
        }

        [Fact, Trait("Category", "Unit")]
        public void LongJsonContentMiddleware_Modify_TrimsValue()
        {
            // Arrange
            var jsonBuilder = new JsonStreamModifier();
            LongJsonContentMiddleware sut = new(jsonBuilder, 10);
            string content = @"{""key1"":""short"",""key2"":""some long content""}";
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            MemoryStream output = new();

            // Act
            sut.Modify(new MemoryStream(bytes), output);

            // Assert
            new StreamReader(output).ReadToEnd().Should()
                .NotBeEmpty()
                .And.BeEquivalentTo(@"{""key1"":""short"",""key2"":""some long ...""}");
        }

        [Fact, Trait("Category", "Unit")]
        public void LongJsonContentMiddleware_Modify_TrimsCustomLengthValue()
        {
            // Arrange
            JsonStreamModifier jsonBuilder = new();
            LongJsonContentMiddleware sut = new(jsonBuilder, 10)
            {
                LeaveOnTrim = 12,
            };

            string content = @"{""key1"":""short"",""key2"":""some long content""}";
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            MemoryStream output = new();

            // Act
            sut.Modify(new MemoryStream(bytes), output);

            // Assert
            new StreamReader(output).ReadToEnd().Should()
                .NotBeEmpty()
                .And.BeEquivalentTo(@"{""key1"":""short"",""key2"":""some long co...""}");
        }

        [Fact, Trait("Category", "Unit")]
        public void LongJsonContentMiddleware_Modify_TrimsValueOnMultiDimension()
        {
            // Arrange
            JsonStreamModifier jsonBuilder = new();
            LongJsonContentMiddleware sut = new(jsonBuilder, 10);
            string content = @"{""key1"":""short"",""key2"":{""key1"":""some long content""}}";
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            MemoryStream output = new();

            // Act
            sut.Modify(new MemoryStream(bytes), output);

            // Assert
            new StreamReader(output).ReadToEnd().Should()
                .NotBeEmpty()
                .And.BeEquivalentTo(@"{""key1"":""short"",""key2"":{""key1"":""some long ...""}}");
        }

        [Fact, Trait("Category", "Unit")]
        public void LongJsonContentMiddleware_Modify_ProperlyFailsOnInvalidTrimValue()
        {
            // Arrange
            JsonStreamModifier jsonBuilder = new();

            // Act
            Func<IRequestContentLogMiddleware> act = () => new LongJsonContentMiddleware(jsonBuilder, 0);

            // Assert
            act.Should()
                .ThrowExactly<ArgumentOutOfRangeException>()
                .WithMessage("Specified argument was out of the range of valid values. (Parameter 'maxCharCountInField')");
        }
    }
}