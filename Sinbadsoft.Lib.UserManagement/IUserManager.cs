// <copyright file="IUserManager.cs" company="Sinbadsoft">
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

namespace Sinbadsoft.Lib.UserManagement
{
    public interface IUserManager
    {
        int MinPasswordLength { get; set; }

        /// <summary>
        /// Attempts to log user in with the provided email and password.
        /// </summary>
        /// <param name="email">User email used as unique identifier.</param>
        /// <param name="password">User password.</param>
        /// <returns><see cref="LoginResult.Success"/> if login is successful.</returns>
        LoginResult Login(string email, string password);

        LoginResult Login(string email, string password, out int id);

        /// <summary>
        /// Attempts to register a new user. If registration is successful, it creates a record with the provided
        /// <paramref name="email"/>, returns <see cref="RegisterResult.Success"/> and sets the verification
        /// <paramref name="token"/>. The newly created user is pending email verification and won't be able login before
        /// email verification. See <see cref="CheckAndClearVerificationToken(int,Sinbadsoft.Lib.UserManagement.VerificationToken,string)"/>.
        /// 
        /// Cases where registration is not successful:
        /// <list type="bullet">
        /// <item>If <paramref name="email"/> is marlformed, <see cref="RegisterResult.InvalidEmail"/> is returned. <paramref name="token"/>.</item>
        /// 
        /// <item>If <paramref name="password"/> is not null, it is checked and <see cref="RegisterResult.InvalidPassword"/> is
        /// returned for invalid passwords. <paramref name="token"/> is set to default value.</item>
        /// 
        /// <item>If a user with the given <paramref name="email"/> exist and the corresponding user is flagged as blocked (See <see cref="SetBlocked"/>),
        /// then <see cref="RegisterResult.UserBlocked"/> is returned. <paramref name="token"/> is set to default value.</item>
        /// <item></item>
        ///
        /// <item>If a user with the given <paramref name="email"/> exist and the corresponding user is not blocked,
        /// then <see cref="RegisterResult.DuplicateEmail"/> is returned.
        /// <paramref name="token"/> is set to default value.</item>
        /// </list>
        /// 
        /// </summary>
        /// <param name="email">User's email. Should be well formed and unique per user.</param>
        /// <param name="password">User's password. Can be null and set later using <see cref="CheckAndClearVerificationToken(int,Sinbadsoft.Lib.UserManagement.VerificationToken,string)"/></param>
        /// <param name="token">Set to the generated email verification token.</param>
        /// <returns>The status of the registration process.</returns>
        RegisterResult Register(string email, string password, out VerificationToken token);

        /// <summary>
        /// Attempts to register a new user. If registration is successful, it creates a record with the provided
        /// <paramref name="email"/>, returns <see cref="RegisterResult.Success"/> and sets the verification
        /// <paramref name="token"/> and the created user identifier <paramref name="id"/>. The newly created user
        /// is pending email verification and won't be able login before email verification. See <see cref="CheckAndClearVerificationToken(int,Sinbadsoft.Lib.UserManagement.VerificationToken,string)"/>.
        /// 
        /// Cases where registration is not successful:
        /// <list type="bullet">
        /// <item>If <paramref name="email"/> is marlformed, <see cref="RegisterResult.InvalidEmail"/> is returned. <paramref name="token"/>
        /// and <paramref name="id"/> are set to default values.</item>
        /// 
        /// <item>If <paramref name="password"/> is not null, it is checked and <see cref="RegisterResult.InvalidPassword"/> is
        /// returned for invalid passwords. <paramref name="token"/> and <paramref name="id"/> are set to default values.</item>
        /// 
        /// <item>If a user with the given <paramref name="email"/> exist and the corresponding user is flagged as blocked (See <see cref="SetBlocked"/>),
        /// then <see cref="RegisterResult.UserBlocked"/> is returned. <paramref name="token"/> is set to default value and <paramref name="id"/> is set to the existing user id.</item>
        /// <item></item>
        ///
        /// <item>If a user with the given <paramref name="email"/> exist and the corresponding user is not blocked,
        /// then <see cref="RegisterResult.DuplicateEmail"/> is returned.
        /// <paramref name="token"/> is set to default value and <paramref name="id"/> is set to the existing user id.</item>
        /// </list>
        /// 
        /// </summary>
        /// <param name="email">User's email. Should be well formed and unique per user.</param>
        /// <param name="password">User's password. Can be null and set later using <see cref="CheckAndClearVerificationToken(int,Sinbadsoft.Lib.UserManagement.VerificationToken,string)"/></param>
        /// <param name="token">Set to the generated email verification token.</param>
        /// <param name="id">Set to the created, if creation successful, or existing user identifier.</param>
        /// <returns>The status of the registration process.</returns>
        RegisterResult Register(string email, string password, out VerificationToken token, out int id);

        VerifyResult CheckVerificationToken(int id, VerificationToken token);

        VerifyResult CheckVerificationToken(int id, VerificationToken token, out string email);

        VerifyResult ResetVerificationToken(string email, out VerificationToken token);

        VerifyResult ResetVerificationToken(string email, out VerificationToken token, out int id);

        VerifyResult CheckAndClearVerificationToken(int id, VerificationToken token, string newPassword = null);

        VerifyResult CheckAndClearVerificationToken(int id, VerificationToken token, out string email, string newPassword = null);

        bool ChangePassword(int id, string oldPassword, string newPassword);

        void SetBlocked(int id, bool blocked);

        UserData LoadUserData(int id);

        UserData LoadUserData(string email);
    }
}
