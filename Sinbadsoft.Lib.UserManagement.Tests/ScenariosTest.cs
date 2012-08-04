// <copyright file="ScenariosTest.cs" company="Sinbadsoft">
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
// <date>2012/11/04</date>
using NUnit.Framework;

namespace Sinbadsoft.Lib.UserManagement.Tests
{
    [TestFixture]
    public class ScenariosTest : DbTestBase
    {
        private IUsersManager membership;

        [SetUp]
        public void SetUp()
        {
            TestData.InsertData(this.ConnectionFactory(), true);
            this.membership = new UsersManager(this.ConnectionFactory());
        }

        [Test]
        public void LoginRightAfterRegisterGivesNotVerifiedEmailResult()
        {
            const string Password = "dubdub*$£d";

            var loginResultBefore = this.membership.Login("new-user@example.com", Password);
            Assert.AreEqual(LoginResult.UnknownUser, loginResultBefore);

            VerificationToken token;
            Assert.AreEqual(RegisterResult.Success, this.membership.Register("new-user@example.com", Password, out token));

            var loginResultAfter = this.membership.Login("new-user@example.com", Password);
            Assert.AreEqual(LoginResult.EmailNotVerified, loginResultAfter);
        }

        [Test]
        public void VerifyUnverifiedUserUnlocksLogin()
        {
            const string Password = "dubdub*$£d";

            VerificationToken token;
            const string Email = "new-user@example.com";
            int userId;
            Assert.AreEqual(RegisterResult.Success, this.membership.Register(Email, Password, out token, out userId));            
            Assert.AreEqual(LoginResult.EmailNotVerified, this.membership.Login(Email, Password));
            
            // Check and clear token
            Assert.AreEqual(VerifyResult.Success, this.membership.CheckVerificationToken(userId, token));
            Assert.AreEqual(VerifyResult.Success, this.membership.CheckAndClearVerificationToken(userId, token));
            
            // Token is cleared
            Assert.AreEqual(VerifyResult.InvalidToken, this.membership.CheckAndClearVerificationToken(userId, token));
            Assert.AreEqual(VerifyResult.InvalidToken, this.membership.CheckVerificationToken(userId, token));

            // Login successfull
            Assert.AreEqual(LoginResult.Success, this.membership.Login(Email, Password));
        }

        [Test]
        public void VerifyBlockedUserDoesntUnlocksLogin()
        {
            const string Email = "new-user@example.com";
            const string Password = "dubdub*$£d";

            VerificationToken token;
            int userId;
            Assert.AreEqual(RegisterResult.Success, this.membership.Register(Email, Password, out token));
            Assert.AreEqual(LoginResult.EmailNotVerified, this.membership.Login(Email, Password, out userId));
            this.membership.SetBlocked(userId, true);

            Assert.AreEqual(VerifyResult.UserBlocked, this.membership.CheckAndClearVerificationToken(userId, token));
            Assert.AreEqual(LoginResult.UserBlocked, this.membership.Login(Email, Password));
        }

        [Test]
        public void ResetVerificationTokenDoesntBlockLoginOnVerifiedEmail()
        {
            var password = TestData.SamRegularUser.StringPassword;
            var email = TestData.SamRegularUser.Email;

            // Login works
            int id;
            Assert.AreEqual(LoginResult.Success, this.membership.Login(email, password, out id));

            // Reset verification token
            VerificationToken token;
            Assert.AreEqual(VerifyResult.Success, this.membership.ResetVerificationToken(email, out token));

            // Login still works
            Assert.AreEqual(LoginResult.Success, this.membership.Login(email, password));

            // Token is there though, we can check it and clear it successfully
            Assert.AreEqual(VerifyResult.Success, this.membership.CheckVerificationToken(id, token));
            Assert.AreEqual(VerifyResult.Success, this.membership.CheckAndClearVerificationToken(id, token));

            // And of course user can login after clearing the token
            Assert.AreEqual(LoginResult.Success, this.membership.Login(email, password));
        }

        [Test]
        public void ResetVerificationTokenThenClearVerificationAndSetNewPassword()
        {
            var password = TestData.SamRegularUser.StringPassword;
            var email = TestData.SamRegularUser.Email;

            // Login works
            int id;
            Assert.AreEqual(LoginResult.Success, this.membership.Login(email, password, out id));

            // Reset verification token
            VerificationToken token;
            Assert.AreEqual(VerifyResult.Success, this.membership.ResetVerificationToken(email, out token));

            // Check and clear verification with new password
            Assert.AreEqual(VerifyResult.Success, this.membership.CheckVerificationToken(id, token));
            const string NewPassword = "my new pass";
            Assert.AreEqual(VerifyResult.Success, this.membership.CheckAndClearVerificationToken(id, token, NewPassword));

            // Login with new password works
            Assert.AreEqual(LoginResult.Success, this.membership.Login(email, NewPassword));
        }
    }
}