using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public class AntiForgeryToken
    {
        internal const int SecurityTokenBitLength = 128;
        internal const int ClaimUidBitLength = 256;

        private string _additionalData;
        private BinaryBlob _securityToken;
        private string _username;

        public string AdditionalData
        {
            get
            {
                return _additionalData ?? String.Empty;
            }
            set
            {
                _additionalData = value;
            }
        }

        public BinaryBlob ClaimUid { get; set; }

        public bool IsSessionToken { get; set; }

        public BinaryBlob SecurityToken
        {
            get
            {
                if (_securityToken == null)
                {
                    _securityToken = new BinaryBlob(SecurityTokenBitLength);
                }
                return _securityToken;
            }
            set
            {
                _securityToken = value;
            }
        }

        public string Username
        {
            get
            {
                return _username ?? String.Empty;
            }
            set
            {
                _username = value;
            }
        }
    }
}
