using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging.Tests
{
    public class LoggerExtensionsTests
    {
        [Theory, Trait("Category", "Unit")]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.None)]
        public void LoggerExtensions_GetLogLevel_ReturnsCorrectLogLevel(LogLevel level)
        {
            // Arrange
            Mock<ILogger> loggerMock = new();

            // Mock
            loggerMock
                .Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>()))
                .Returns<LogLevel>(lvl => lvl == level);

            // Act
            var result = loggerMock.Object.GetLogLevel();

            // Assert
            result.Should().Be(level);
        }

        [Fact, Trait("Category", "Unit")]
        public void LoggerExtensions_GetLogLevel_ProperlyFailsOnNull()
        {
            // Arrange
            ILogger? logger = null;

            // Act
            Action act = () => logger!.GetLogLevel();

            // Assert
            act.Should()
                .ThrowExactly<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'logger')");
        }
    }
}
