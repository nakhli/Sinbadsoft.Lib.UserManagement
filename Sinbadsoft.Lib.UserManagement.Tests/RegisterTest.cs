// <copyright file="RegisterTest.cs" company="Sinbadsoft">
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
// <date>2010/11/04</date>
using NUnit.Framework;

namespace Sinbadsoft.Lib.UserManagement.Tests
{
    [TestFixture]
    public class RegisterTest : DbTestBase
    {
        private IUserManager membership;

        [TestFixtureSetUp]
        public new void FixtureSetup()
        {
            TestData.InsertData(this.ConnectionFactory());
        }

        [SetUp]
        public void SetUp()
        {
            this.membership = new UserManager(this.ConnectionFactory());
        }

        [Test]
        public void EmailAlreadyTaken()
        {
            VerificationToken token;
            var result = this.membership.Register("joe@example.com", "foobar", out token);
            Assert.AreEqual(RegisterResult.DuplicateEmail, result);
        }

        [Test]
        public void EmailWithoutAtChar()
        {
            const string Password = "dqzegxé*$";
            VerificationToken token;
            var result = this.membership.Register("  gde  ", Password, out token);
            Assert.AreEqual(RegisterResult.InvalidEmail, result);
        }

        [Test]
        public void EmailTooShort()
        {
            const string Password = "dqzegxé*$";
            VerificationToken token;
            var result = this.membership.Register(" g@ ", Password, out token);
            Assert.AreEqual(RegisterResult.InvalidEmail, result);
        }

        [Test]
        public void WhiteSpacesPassword()
        {
            const string Password = "      \t        \t\t      \r\n            ";
            VerificationToken token;
            var result = this.membership.Register("new-user@example.com", Password, out token);
            Assert.AreEqual(RegisterResult.InvalidPassword, result);
        }

        [Test]
        public void SpacesPassword()
        {
            const string Password = "                   ";
            VerificationToken token;
            var result = this.membership.Register("new-user@example.com", Password, out token);
            Assert.AreEqual(RegisterResult.InvalidPassword, result);
        }

        [Test]
        public void ShortPassword()
        {
            string shortPassword = "dubdub*$£d124RF".Substring(0, this.membership.MinPasswordLength - 1);
            VerificationToken token;
            var result = this.membership.Register("new-user@example.com", shortPassword, out token);
            Assert.AreEqual(RegisterResult.InvalidPassword, result);
        }
    }
}