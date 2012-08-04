// <copyright file="LoginTests.cs" company="Sinbadsoft">
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
    public class LoginTests : DbTestBase
    {
        private IUsersManager membership;

        [TestFixtureSetUp]
        public new void FixtureSetup()
        {
            TestData.InsertData(this.ConnectionFactory());
        }

        [SetUp]
        public void SetUp()
        {
            this.membership = new UsersManager(this.ConnectionFactory());
        }

        [Test]
        public void UnknownUser()
        {
            var result = this.membership.Login("unknown-user@example.com", "foobar");
            Assert.AreEqual(LoginResult.UnknownUser, result);
        }

        [Test]
        public void EmailNotVerified()
        {
            var joe = TestData.JoeEmailNotVerifiedPasswordSet;
            var result = this.membership.Login(joe.Email, joe.StringPassword);
            Assert.AreEqual(LoginResult.EmailNotVerified, result);
        }

        [Test]
        public void UserBlocked()
        {
            var robert = TestData.RobertEmailVerifiedButBlocked;
            var result = this.membership.Login(robert.Email, robert.StringPassword);
            Assert.AreEqual(LoginResult.UserBlocked, result);
        }

        [Test]
        public void WrongPassword()
        {
            var sam = TestData.SamRegularUser;
            var result = this.membership.Login(sam.Email, sam.StringPassword + "wrong!");
            Assert.AreEqual(LoginResult.WrongPassword, result);
        }

        [Test]
        public void Success()
        {
            var sam = TestData.SamRegularUser;
            var result = this.membership.Login(sam.Email, sam.StringPassword);
            Assert.AreEqual(LoginResult.Success, result);
        }
    }
}