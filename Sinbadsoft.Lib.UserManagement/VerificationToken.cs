// <copyright file="VerificationToken.cs" company="Sinbadsoft">
// Copyright (c) Chaker Nakhli 2010
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
// <date>2012/06/30</date>
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Sinbadsoft.Lib.UserManagement
{
    public class VerificationToken
    {
        /// <summary>
        /// 8 bytes to store a <see cref="DateTime"/> and 16 bytes for the random part.
        /// </summary>
        private const int DataLength = 16 + sizeof(double);

        private VerificationToken(byte[] token)
        {
            this.Data = token;
            this.Timestamp = DateTime.FromBinary(BitConverter.ToInt64(token, 0));
        }

        public DateTime Timestamp { get; private set; }

        public byte[] Data { get; private set; }

        public static VerificationToken Generate()
        {
            return Generate(DateTime.UtcNow);
        }

        public static VerificationToken Generate(DateTime timestamp)
        {
            var data = new byte[DataLength];
            new RNGCryptoServiceProvider().GetBytes(data);
            Array.Copy(BitConverter.GetBytes(timestamp.ToBinary()), data, sizeof(long));
            return new VerificationToken(data);
        }

        public static VerificationToken Parse(string tokenString)
        {
            var bytes = Convert.FromBase64String(tokenString);
            if (bytes.Length != DataLength)
            {
                throw new ArgumentOutOfRangeException("tokenString");
            }

            var token = bytes.Take(DataLength).ToArray();            
            return new VerificationToken(token);
        }

        public override string ToString()
        {
            return Convert.ToBase64String(this.Data);
        }

        public bool IsFresh(TimeSpan span, DateTime now)
        {
            return now.Subtract(span) <= this.Timestamp;
        }

        public bool IsFresh(int days)
        {
            return this.IsFresh(new TimeSpan(days, 0, 0, 0), DateTime.UtcNow);
        }
    }
}
