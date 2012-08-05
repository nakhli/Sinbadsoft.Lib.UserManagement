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

        RegisterResult Register(string email, string password, out VerificationToken token);

        RegisterResult Register(string email, string password, out VerificationToken token, out int id);

        VerifyResult CheckVerificationToken(int id, VerificationToken token);

        VerifyResult CheckVerificationToken(int id, VerificationToken token, out string email);

        VerifyResult ResetVerificationToken(string email, out VerificationToken token);

        VerifyResult CheckAndClearVerificationToken(int id, VerificationToken token, string newPassword = null);

        VerifyResult CheckAndClearVerificationToken(int id, VerificationToken token, out string email, string newPassword = null);

        bool ChangePassword(int id, string oldPassword, string newPassword);

        void SetBlocked(int id, bool blocked);

        bool IsPasswordSet(int id);
    }
}
