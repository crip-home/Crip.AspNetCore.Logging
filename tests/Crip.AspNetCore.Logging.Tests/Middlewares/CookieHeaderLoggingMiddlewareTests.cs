using FluentAssertions;
using Xunit;

namespace Crip.AspNetCore.Logging.Tests
{
    public class CookieHeaderLoggingMiddlewareTests
    {
        [Fact, Trait("Category", "Unit")]
        public void CookieHeaderLoggingMiddleware_Modify_ChangesSetCookieHeaderValue()
        {
            // Arrange
            CookieHeaderLoggingMiddleware sut = new();

            // Act
            var result = sut.Modify("Set-Cookie",
                ".AspNetCore.Identity.Application=CfDJ8NfEhWmHsRVFinKuK3Skt70KTmbH8TOOQyNrYZVidVKRs_vkAZ_ZeL_ALgqjsTOB1IAOc; path=/; samesite=lax; httponly,refreshToken=96D750FC9EBAF990C7F7104B0B0AD95AC03266030CE0B2C8D2FA7AA4BFBEBD5928D3B25DACCACEC9; expires=Wed, 25 Nov 2020 10:15:55 GMT; path=/; httponly");

            // Assert
            var expected =
                ".AspNetCore.Identity.Application=CfDJ8NfEhW***; path=/; samesite=lax; httponly,refreshToken=96D750FC9E***; expires=Wed, 25 Nov 2020 10:15:55 GMT; path=/; httponly";
            result.Should().Be(expected);
        }

        [Fact, Trait("Category", "Unit")]
        public void CookieHeaderLoggingMiddleware_Modify_ChangesCookieHeaderValue()
        {
            // Arrange
            CookieHeaderLoggingMiddleware sut = new();

            // Act
            var result = sut.Modify("Cookie",
                ".AspNetCore.Identity.Application=OUsFSjRvqGjHWIdFr1hEGtQI8pT0YYa72qgKF3nm7iZWaLoeLNn3ZlXTcgADaWQhQ504DkSBWdlidepUKG2VHI2GuCj_wRikzbGa4Zn9fidVKRs_vkAZ_ZeL_ALgqjsTOB1IAOc; refreshToken=96D750FC9EBAF990C7F7104B0B0AD95AC03266030CE0B2C8D2FA7");

            // Assert
            var expected = ".AspNetCore.Identity.Application=OUsFSjRvqG***; refreshToken=96D750FC9E***";
            result.Should().Be(expected);
        }

        [Fact, Trait("Category", "Unit")]
        public void CookieHeaderLoggingMiddleware_Modify_IgnoresOtherHeader()
        {
            // Arrange
            CookieHeaderLoggingMiddleware sut = new();

            // Act
            var result = sut.Modify("Authorization", "toke-value");

            // Assert
            result.Should().Be("toke-value");
        }
    }
}
