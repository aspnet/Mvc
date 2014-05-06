﻿
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNet.Security.DataProtection;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class AntiForgeryTokenSerializerTest
    {
        private static readonly Mock<IDataProtector> _dataProtector = GetDataProtector();
        private static readonly BinaryBlob _claimUid = new BinaryBlob(256, new byte[] { 0x6F, 0x16, 0x48, 0xE9, 0x72, 0x49, 0xAA, 0x58, 0x75, 0x40, 0x36, 0xA6, 0x7E, 0x24, 0x8C, 0xF0, 0x44, 0xF0, 0x7E, 0xCF, 0xB0, 0xED, 0x38, 0x75, 0x56, 0xCE, 0x02, 0x9A, 0x4F, 0x9A, 0x40, 0xE0 });
        private static readonly BinaryBlob _securityToken = new BinaryBlob(128, new byte[] { 0x70, 0x5E, 0xED, 0xCC, 0x7D, 0x42, 0xF1, 0xD6, 0xB3, 0xB9, 0x8A, 0x59, 0x36, 0x25, 0xBB, 0x4C });
        private const byte _salt = 0x05;

        [Theory]
        [InlineData(
            "01" // Version
            + "705EEDCC7D42F1D6B3B9" // SecurityToken
                                     // (WRONG!) Stream ends too early
            )]
        [InlineData(
            "01" // Version
            + "705EEDCC7D42F1D6B3B98A593625BB4C" // SecurityToken
            + "01" // IsSessionToken
            + "00" // (WRONG!) Too much data in stream
            )]
        [InlineData(
            "02" // (WRONG! - must be 0x01) Version
            + "705EEDCC7D42F1D6B3B98A593625BB4C" // SecurityToken
            + "01" // IsSessionToken
            )]
        [InlineData(
            "01" // Version
            + "705EEDCC7D42F1D6B3B98A593625BB4C" // SecurityToken
            + "00" // IsSessionToken
            + "00" // IsClaimsBased
            + "05" // Username length header
            + "0000" // (WRONG!) Too little data in stream
            )]
        public void Deserialize_BadToken_Throws(string serializedToken)
        {
            // Arrange 
            var _testSerializer = new AntiForgeryTokenSerializer(_dataProtector.Object);

            // Act & assert
            var ex = Assert.Throws<InvalidOperationException>(() => _testSerializer.Deserialize(serializedToken));
            Assert.Equal(@"The anti-forgery token could not be decrypted.", ex.Message);
            _dataProtector.Verify();
        }

        [Fact]
        public void Serialize_FieldToken_WithClaimUid_TokenRoundTripSuccessful()
        {
            // Arrange
            //"01" // Version
            //+ "705EEDCC7D42F1D6B3B98A593625BB4C" // SecurityToken
            //+ "00" // IsSessionToken
            //+ "01" // IsClaimsBased
            //+ "6F1648E97249AA58754036A67E248CF044F07ECFB0ED387556CE029A4F9A40E0" // ClaimUid
            //+ "05" // AdditionalData length header
            //+ "E282AC3437"; // AdditionalData ("€47") as UTF8
            var _testSerializer = new AntiForgeryTokenSerializer(_dataProtector.Object);
            var token = new AntiForgeryToken()
            {
                SecurityToken = _securityToken,
                IsSessionToken = false,
                ClaimUid = _claimUid,
                AdditionalData = "€47"
            };

            // Act
            var actualSerializedData = _testSerializer.Serialize(token);
            var deserializedToken = _testSerializer.Deserialize(actualSerializedData);

            // Assert
            AssertTokensEqual(token, deserializedToken);
            _dataProtector.Verify();
        }

        [Fact]
        public void Serialize_FieldToken_WithUsername_TokenRoundTripSuccessful()
        {
            // Arrange
            //"01" // Version
            //+ "705EEDCC7D42F1D6B3B98A593625BB4C" // SecurityToken
            //+ "00" // IsSessionToken
            //+ "00" // IsClaimsBased
            //+ "08" // Username length header
            //+ "4AC3A972C3B46D65" // Username ("Jérôme") as UTF8
            //+ "05" // AdditionalData length header
            //+ "E282AC3437"; // AdditionalData ("€47") as UTF8
            var _testSerializer = new AntiForgeryTokenSerializer(_dataProtector.Object);
            var token = new AntiForgeryToken()
            {
                SecurityToken = _securityToken,
                IsSessionToken = false,
                Username = "Jérôme",
                AdditionalData = "€47"
            };

            // Act
            var actualSerializedData = _testSerializer.Serialize(token);
            var deserializedToken = _testSerializer.Deserialize(actualSerializedData);
            
            // Assert
            AssertTokensEqual(token, deserializedToken);
            _dataProtector.Verify();
        }

        [Fact]
        public void Serialize_SessionToken_TokenRoundTripSuccessful()
        {
            // Arrange
            //"01" // Version
            //+ "705EEDCC7D42F1D6B3B98A593625BB4C" // SecurityToken
            //+ "01"; // IsSessionToken
            var _testSerializer = new AntiForgeryTokenSerializer(_dataProtector.Object);
            var token = new AntiForgeryToken()
            {
                SecurityToken = _securityToken,
                IsSessionToken = true
            };

            // Act
            string actualSerializedData = _testSerializer.Serialize(token);
            var deserializedToken = _testSerializer.Deserialize(actualSerializedData);

            // Assert
            AssertTokensEqual(token, deserializedToken);
            _dataProtector.Verify();
        }

        private static Mock<IDataProtector> GetDataProtector()
        {
            var mockCryptoSystem = new Mock<IDataProtector>();
            mockCryptoSystem.Setup(o => o.Protect(It.IsAny<byte[]>()))
                            .Returns<byte[]>(Protect)
                            .Verifiable();
            mockCryptoSystem.Setup(o => o.Unprotect(It.IsAny<byte[]>()))
                            .Returns<byte[]>(UnProtect)
                            .Verifiable();
            return mockCryptoSystem;
        }

        private static byte[] Protect(byte[] data)
        {
            var input = new List<byte>(data);
            input.Add(_salt);
            return input.ToArray();
        }

        private static byte[] UnProtect(byte[] data)
        {
            var salt = data[data.Length - 1]; 
            if (salt != _salt)
            {
                throw new ArgumentException("Invalid salt value in data");
            }

            return data.Take(data.Length - 1).ToArray();
        }

        private static void AssertTokensEqual(AntiForgeryToken expected, AntiForgeryToken actual)
        {
            Assert.NotNull(expected);
            Assert.NotNull(actual);
            Assert.Equal(expected.AdditionalData, actual.AdditionalData);
            Assert.Equal(expected.ClaimUid, actual.ClaimUid);
            Assert.Equal(expected.IsSessionToken, actual.IsSessionToken);
            Assert.Equal(expected.SecurityToken, actual.SecurityToken);
            Assert.Equal(expected.Username, actual.Username);
        }
    }
}