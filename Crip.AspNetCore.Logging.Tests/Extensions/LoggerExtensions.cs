﻿using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace Crip.AspNetCore.Logging.Tests
{
    public static class LoggerExtensions
    {
        public static Mock<ILogger> VerifyLogging(
            this Mock<ILogger> loggerMock,
            string expectedMessage,
            LogLevel expectedLogLevel = LogLevel.Debug,
            Times? times = null)
        {
            times ??= Times.Once();

            Func<object, Type, bool> state = (v, t) =>
                string.Compare(v.ToString(), expectedMessage, StringComparison.Ordinal) == 0;

            loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), (Times)times);

            return loggerMock;
        }

        public static Mock<ILogger<T>> VerifyLogging<T>(
            this Mock<ILogger<T>> loggerMock,
            string expectedMessage,
            LogLevel expectedLogLevel = LogLevel.Debug,
            Times? times = null)
        {
            (loggerMock as Mock<ILogger> ?? throw new Exception("Shit happens"))
                .VerifyLogging(expectedMessage, expectedLogLevel, times);

            return loggerMock;
        }
    }
}
