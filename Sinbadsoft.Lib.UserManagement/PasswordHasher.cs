// <copyright file="PasswordHasher.cs" company="Sinbadsoft">
// Copyright (c) Chaker Nakhli 2010-2012
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at http://www.apache.org/licenses/LICENSE-2.0 Unless required by 
// applicable law or agreed to in writing, software distributed under the License
// is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// </copyright>
// <author>Chaker Nakhli</author>
// <email>chaker.nakhli@sinbadsoft.com</email>
// <date>2010/11/04</date>
using System.Security.Cryptography;
using System.Text;

namespace Sinbadsoft.Lib.UserManagement
{
    public class PasswordHasher : IPasswordHasher
    {
        public const int DefaultHashLength = 64;
        public const int DefaultIterationCount = 2978;
        public const int DefaultSaltSize = 8;
        public const string DefaultAdditionalOffDbSalt = "@à wT5lü[`C\txgQ9ç'$;PZyB*";

        public PasswordHasher(
            int hashLength = DefaultHashLength,
            int iterationCount = DefaultIterationCount,
            int saltSize = DefaultSaltSize,
            string additionalOffDbSalt = DefaultAdditionalOffDbSalt)
        {
            this.HashLength = hashLength;
            this.SaltSize = saltSize;
            this.IterationCount = iterationCount;
            this.AdditionalOffDbSalt = additionalOffDbSalt;
        }

        public string AdditionalOffDbSalt { get; set; }

        public int SaltSize { get; set; }

        public int IterationCount { get; set; }

        public int HashLength { get; set; }

        public byte[] Hash(string password, ref byte[] salt)
        {
            Rfc2898DeriveBytes hasher;
            var saltedPassword = this.AdditionalOffDbSalt + password;

            // If salt is not specified, generate it on the fly.
            if (salt != null && salt.Length == this.SaltSize)
            {
                hasher = new Rfc2898DeriveBytes(saltedPassword, salt, this.IterationCount);
            }
            else
            {
                hasher = new Rfc2898DeriveBytes(saltedPassword, this.SaltSize, this.IterationCount);
                salt = hasher.Salt;
            }

            return hasher.GetBytes(this.HashLength);
        }
    }
}