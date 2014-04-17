
using System;
using System.IO;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Security.DataProtection;
using System.Text;

namespace Microsoft.AspNet.Mvc
{
    internal sealed class AntiForgeryTokenSerializer : IAntiForgeryTokenSerializer
    {
        private readonly IDataProtector _cryptoSystem;
        private const byte TokenVersion = 0x01;

        internal AntiForgeryTokenSerializer(IDataProtector cryptoSystem)
        {
            _cryptoSystem = cryptoSystem;
        }

        public AntiForgeryToken Deserialize(string serializedToken)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(UrlTokenDecode(serializedToken)))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        AntiForgeryToken token = DeserializeImpl(reader);
                        if (token != null)
                        {
                            return token;
                        }
                    }
                }
            }
            catch
            {
                // swallow all exceptions - homogenize error if something went wrong
            }

            // TODO: Return proper exception here.
            // if we reached this point, something went wrong deserializing
            //  throw HttpAntiForgeryException.CreateDeserializationFailedException();
            throw new InvalidOperationException(Resources.AntiForgeryToken_DeserializationFailed);
        }

        /* The serialized format of the anti-XSRF token is as follows:
         * Version: 1 byte integer
         * SecurityToken: 16 byte binary blob
         * IsSessionToken: 1 byte Boolean
         * [if IsSessionToken = true]
         *   +- IsClaimsBased: 1 byte Boolean
         *   |  [if IsClaimsBased = true]
         *   |    `- ClaimUid: 32 byte binary blob
         *   |  [if IsClaimsBased = false]
         *   |    `- Username: UTF-8 string with 7-bit integer length prefix
         *   `- AdditionalData: UTF-8 string with 7-bit integer length prefix
         */
        private static AntiForgeryToken DeserializeImpl(BinaryReader reader)
        {
            // we can only consume tokens of the same serialized version that we generate
            byte embeddedVersion = reader.ReadByte();
            if (embeddedVersion != TokenVersion)
            {
                return null;
            }

            AntiForgeryToken deserializedToken = new AntiForgeryToken();
            byte[] securityTokenBytes = reader.ReadBytes(AntiForgeryToken.SecurityTokenBitLength / 8);
            deserializedToken.SecurityToken = new BinaryBlob(AntiForgeryToken.SecurityTokenBitLength, securityTokenBytes);
            deserializedToken.IsSessionToken = reader.ReadBoolean();

            if (!deserializedToken.IsSessionToken)
            {
                bool isClaimsBased = reader.ReadBoolean();
                if (isClaimsBased)
                {
                    byte[] claimUidBytes = reader.ReadBytes(AntiForgeryToken.ClaimUidBitLength / 8);
                    deserializedToken.ClaimUid = new BinaryBlob(AntiForgeryToken.ClaimUidBitLength, claimUidBytes);
                }
                else
                {
                    deserializedToken.Username = reader.ReadString();
                }

                deserializedToken.AdditionalData = reader.ReadString();
            }

            // if there's still unconsumed data in the stream, fail
            if (reader.BaseStream.ReadByte() != -1)
            {
                return null;
            }

            // success
            return deserializedToken;
        }

        public string Serialize([NotNull] AntiForgeryToken token)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(TokenVersion);
                    writer.Write(token.SecurityToken.GetData());
                    writer.Write(token.IsSessionToken);

                    if (!token.IsSessionToken)
                    {
                        if (token.ClaimUid != null)
                        {
                            writer.Write(true /* isClaimsBased */);
                            writer.Write(token.ClaimUid.GetData());
                        }
                        else
                        {
                            writer.Write(false /* isClaimsBased */);
                            writer.Write(token.Username);
                        }

                        writer.Write(token.AdditionalData);
                    }

                    writer.Flush();
                    return UrlTokenEncode(_cryptoSystem.Protect(stream.ToArray()));
                }
            }
        }

        // TODO: This is temporary replacement for HttpServerUtility.UrlTokenEncode.
        // This will be removed when webutils has this.
        private string UrlTokenEncode(byte[] input)
        {
            var base64String = Convert.ToBase64String(input);
            if (string.IsNullOrEmpty(base64String))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < base64String.Length; i++)
            {
                switch (base64String[i])
                {
                    case '+':
                        sb.Append('-');
                        break;
                    case '/':
                        sb.Append('_');
                        break;
                    case '=':
                        sb.Append('.');
                        break;
                    default:
                        sb.Append(base64String[i]);
                        break;
                }
            }

            return sb.ToString();
        }

        // TODO: This is temporary replacement for HttpServerUtility.UrlTokenDecode.
        // This will be removed when webutils has this.
        private byte[] UrlTokenDecode(string input)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case '-':
                        sb.Append('+');
                        break;
                    case '_':
                        sb.Append('/');
                        break;
                    case '.':
                        sb.Append('=');
                        break;
                    default:
                        sb.Append(input[i]);
                        break;
                }
            }

            return Convert.FromBase64String(sb.ToString());
        }
    }
}