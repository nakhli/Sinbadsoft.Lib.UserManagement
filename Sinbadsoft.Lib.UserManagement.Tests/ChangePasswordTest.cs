// <copyright file="ChangePasswordTests.cs" company="Sinbadsoft">
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
    public class ChangePasswordTest : DbTestBase
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
            Assert.IsFalse(this.membership.ChangePassword(1000, "foobar", "newpass"));
        }

        [Test]
        public void EmailNotVerified()
        {
            Assert.IsFalse(this.membership.ChangePassword(1, "foobar", "newpass"));
        }

        [Test]
        public void BlockedUser()
        {
            Assert.IsFalse(this.membership.ChangePassword(2, "foobar", "newpass"));
        }

        [Test]
        public void Success()
        {   
            var email = TestData.SamRegularUser.Email;
            var password = TestData.SamRegularUser.StringPassword;

            int id;
            Assert.AreEqual(LoginResult.WrongPassword, this.membership.Login(email, "newPass", out id));
            Assert.AreEqual(LoginResult.Success, this.membership.Login(email, password));
            Assert.IsTrue(this.membership.ChangePassword(id, password, "newPass"));
            Assert.AreEqual(LoginResult.Success, this.membership.Login(email, "newPass"));
            Assert.AreEqual(LoginResult.WrongPassword, this.membership.Login(email, password));
        }
    }
}