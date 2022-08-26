namespace Crip.AspNetCore.Logging.Tests
{
    public class StringExtensionsTests
    {
        [Theory, Trait("Category", "Unit")]
        [InlineData("", true)]
        [InlineData((string?)null, true)]
        [InlineData(" ", false)]
        [InlineData("a", false)]
        [InlineData("1", false)]
        [InlineData("\t", false)]
        public void StringExtensions_IsNullOrEmpty(string input, bool isNullOrEmpty)
        {
            input.IsNullOrEmpty().Should().Be(isNullOrEmpty);
        }

        [Theory, Trait("Category", "Unit")]
        [InlineData("", false)]
        [InlineData((string?)null, false)]
        [InlineData(" ", true)]
        [InlineData("a", true)]
        [InlineData("1", true)]
        [InlineData("\t", true)]
        public void StringExtensions_NotNullAndNotEmpty(string input, bool notNullAndNotEmpty)
        {
            input.NotNullAndNotEmpty().Should().Be(notNullAndNotEmpty);
        }

        [Theory, Trait("Category", "Unit")]
        [InlineData("", true)]
        [InlineData((string?)null, true)]
        [InlineData(" ", true)]
        [InlineData("a", false)]
        [InlineData("1", false)]
        [InlineData("\t", true)]
        public void StringExtensions_IsNullOrWhiteSpace(string input, bool isNullOrWhiteSpace)
        {
            input.IsNullOrWhiteSpace().Should().Be(isNullOrWhiteSpace);
        }

        [Theory, Trait("Category", "Unit")]
        [InlineData("", false)]
        [InlineData((string?)null, false)]
        [InlineData(" ", false)]
        [InlineData("a", true)]
        [InlineData("1", true)]
        [InlineData("\t", false)]
        public void StringExtensions_NotNullAndNotWhiteSpace(string input, bool notNullAndNotWhiteSpace)
        {
            input.NotNullAndNotWhiteSpace().Should().Be(notNullAndNotWhiteSpace);
        }
    }
}