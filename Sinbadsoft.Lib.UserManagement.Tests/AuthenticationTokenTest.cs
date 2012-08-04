// <copyright file="AuthenticationTokenTest.cs" company="Sinbadsoft">
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
// <date>2012/07/28</date>
using NUnit.Framework;

using Sinbadsoft.Lib.UserManagement.Authentication;

namespace Sinbadsoft.Lib.UserManagement.Tests
{
    [TestFixture]
    public class AuthenticationTokenTest
    {
        [Test]
        public void ParseValidTokenWithExtraData()
        {
            AuthenticatedUserInfo info = AuthenticationToken.Parse("user@example.com;3421;some short data here");
            CheckUserInfoValues(info, 3421, "user@example.com", "some short data here");
        }

        [Test]
        public void ParseValidTokenWithNoExtraDataNoTrailingSeparator()
        {
            AuthenticatedUserInfo info = AuthenticationToken.Parse("user@example.com;3421");
            CheckUserInfoValues(info, 3421, "user@example.com", string.Empty);
        }

        [Test]
        public void ParseValidTokenWithNoExtraDataWithTrailingSeparator()
        {
            AuthenticatedUserInfo info = AuthenticationToken.Parse("user@example.com;3421;");
            CheckUserInfoValues(info, 3421, "user@example.com", string.Empty);
        }

        [Test]
        public void InvalidTokens()
        {
            Assert.IsNull(AuthenticationToken.Parse("user@example.com;34x21;"));
            Assert.IsNull(AuthenticationToken.Parse("ud;3421;"));
            Assert.IsNull(AuthenticationToken.Parse("user@example.com;;xxczd"));
            Assert.IsNull(AuthenticationToken.Parse("user@example.com;12345678912345;xxczd"));
        }

        private static void CheckUserInfoValues(AuthenticatedUserInfo info, int id, string email, string data)
        {
            Assert.IsNotNull(info);
            Assert.AreEqual(email, info.Email);
            Assert.AreEqual(id, info.Id);
            Assert.AreEqual(data, info.Data);
        }
    }
}