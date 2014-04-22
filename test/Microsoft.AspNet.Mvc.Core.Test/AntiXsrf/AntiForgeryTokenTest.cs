using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class AntiForgeryTokenTest
    {
        [Fact]
        public void AdditionalDataProperty()
        {
            // Arrange
            AntiForgeryToken token = new AntiForgeryToken();

            // Act & assert - 1
            Assert.Equal("", token.AdditionalData);

            // Act & assert - 2
            token.AdditionalData = "additional data";
            Assert.Equal("additional data", token.AdditionalData);

            // Act & assert - 3
            token.AdditionalData = null;
            Assert.Equal("", token.AdditionalData);
        }

        [Fact]
        public void ClaimUidProperty()
        {
            // Arrange
            AntiForgeryToken token = new AntiForgeryToken();

            // Act & assert - 1
            Assert.Null(token.ClaimUid);

            // Act & assert - 2
            BinaryBlob blob = new BinaryBlob(32);
            token.ClaimUid = blob;
            Assert.Equal(blob, token.ClaimUid);

            // Act & assert - 3
            token.ClaimUid = null;
            Assert.Null(token.ClaimUid);
        }

        [Fact]
        public void IsSessionTokenProperty()
        {
            // Arrange
            AntiForgeryToken token = new AntiForgeryToken();

            // Act & assert - 1
            Assert.False(token.IsSessionToken);

            // Act & assert - 2
            token.IsSessionToken = true;
            Assert.True(token.IsSessionToken);

            // Act & assert - 3
            token.IsSessionToken = false;
            Assert.False(token.IsSessionToken);
        }

        [Fact]
        public void SecurityTokenProperty()
        {
            // Arrange
            AntiForgeryToken token = new AntiForgeryToken();

            // Act & assert - 1
            BinaryBlob securityToken = token.SecurityToken;
            Assert.NotNull(securityToken);
            Assert.Equal(AntiForgeryToken.SecurityTokenBitLength, securityToken.BitLength);
            Assert.Equal(securityToken, token.SecurityToken); // check that we're not making a new one each property call

            // Act & assert - 2
            securityToken = new BinaryBlob(64);
            token.SecurityToken = securityToken;
            Assert.Equal(securityToken, token.SecurityToken);

            // Act & assert - 3
            token.SecurityToken = null;
            securityToken = token.SecurityToken;
            Assert.NotNull(securityToken);
            Assert.Equal(AntiForgeryToken.SecurityTokenBitLength, securityToken.BitLength);
            Assert.Equal(securityToken, token.SecurityToken); // check that we're not making a new one each property call
        }

        [Fact]
        public void UsernameProperty()
        {
            // Arrange
            AntiForgeryToken token = new AntiForgeryToken();

            // Act & assert - 1
            Assert.Equal("", token.Username);

            // Act & assert - 2
            token.Username = "my username";
            Assert.Equal("my username", token.Username);

            // Act & assert - 3
            token.Username = null;
            Assert.Equal("", token.Username);
        }
    }
}