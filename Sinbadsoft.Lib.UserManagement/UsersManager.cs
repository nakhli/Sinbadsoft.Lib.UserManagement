// <copyright file="UsersManager.cs" company="Sinbadsoft">
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
using System.Collections.Generic;
using System.Data.Common;
using MonkeyOrm;

namespace Sinbadsoft.Lib.UserManagement
{
    public class UsersManager : IUsersManager
    {
        public const int DefaultMinPasswordLength = 5;

        public UsersManager(IConnectionFactory connectionFactory, IPasswordHasher hasher)
        {
            this.ConnectionFactory = connectionFactory;
            this.PasswordHasher = hasher;
            this.MinPasswordLength = DefaultMinPasswordLength;
        }

        public UsersManager(IConnectionFactory connectionFactory)
            : this(connectionFactory, new PasswordHasher())
        {
        }

        public int MinPasswordLength { get; set; }

        public IConnectionFactory ConnectionFactory { get; private set; }

        public IPasswordHasher PasswordHasher { get; set; }

        public LoginResult Login(string email, string password)
        {
            int id;
            return this.Login(email, password, out id);
        }

        public LoginResult Login(string email, string password, out int id)
        {
            // NOTE(cnakhli) don't call ValidatePassword logic here as if it changes old user entries will not be accessible anymore
            if (string.IsNullOrWhiteSpace(password))
            {
                id = 0;
                return LoginResult.WrongPassword;
            }

            if (!ValidateAndNormalizeEmail(ref email))
            {
                id = 0;
                return LoginResult.UnknownUser;
            }

            var userInfo = this.ConnectionFactory.ReadOne(
                "SELECT Id, Email, Password, Salt, EmailVerified, UserBlocked FROM Users WHERE Email=@email",
                new { email });

            return this.ValidateUserInfo(userInfo, password, out id);
        }

        public RegisterResult Register(string email, string password, out VerificationToken verificationToken)
        {
            int id;
            return this.Register(email, password, out verificationToken, out id);
        }

        public RegisterResult Register(string email, string password, out VerificationToken verificationToken, out int id)
        {
            if (!ValidateAndNormalizeEmail(ref email))
            {
                id = 0;
                verificationToken = null;
                return RegisterResult.InvalidEmail;
            }

            if (password != null && !this.ValidatePassword(password))
            {
                id = 0;
                verificationToken = null;
                return RegisterResult.InvalidPassword;
            }

            byte[] salt = null;
            byte[] hash = null;
            if (password != null)
            {
                hash = this.PasswordHasher.Hash(password, ref salt);
            }

            verificationToken = VerificationToken.Generate();

            var user = new
                    {
                        Salt = salt,
                        Password = hash,
                        Email = email,
                        EmailVerified = false,
                        UserBlocked = false,
                        VerificationToken = verificationToken.Data
                    };

            try
            {
                this.ConnectionFactory.Save("Users", user, out id);
                return RegisterResult.Success;
            }
            catch (DbException exception)
            {
                const int MysqlDuplicateEntryServerErrorCode = 1062;
                object exceptionData = exception.Data["Server Error Code"];
                if (exceptionData is int && (int)exceptionData == MysqlDuplicateEntryServerErrorCode)
                {
                    id = 0;
                    verificationToken = null;
                    return RegisterResult.DuplicateEmail;
                }

                throw;
            }
        }

        public VerifyResult CheckAndClearVerificationToken(int id, VerificationToken token, string newPassword = null)
        {
            if (newPassword != null && !this.ValidatePassword(newPassword))
            {
                return VerifyResult.InvalidPassword;
            }

            string email;
            return this.CheckAndClearVerificationToken(id, token, out email, newPassword);
        }

        public VerifyResult CheckAndClearVerificationToken(int id, VerificationToken token, out string email, string newPassword = null)
        {
            string userEmail = null;
            VerifyResult result = this.ConnectionFactory.InTransaction(true).Do(
                t =>
                {
                    var data = t.ReadOne("SELECT Email, UserBlocked, VerificationToken FROM Users WHERE Id=@id", new { id });
                    
                    result = VerifyToken(data, token, out userEmail);

                    if (newPassword != null && !this.ValidatePassword(newPassword))
                    {
                        return VerifyResult.InvalidPassword;
                    }
                    
                    if (result != VerifyResult.Success)
                    {
                        return result;
                    }

                    var values = new Dictionary<string, object> { { "VerificationToken", null }, { "EmailVerified", true } };
                    if (newPassword != null)
                    {
                        byte[] salt = null;
                        var hashedPassword = this.PasswordHasher.Hash(newPassword, ref salt);
                        values.Add("Password", hashedPassword);
                        values.Add("Salt", salt);
                    }

                    t.Update("Users", values, "Id=@id", new { id });

                    return result;
                });
            email = userEmail;
            return result;
        }

        public VerifyResult CheckVerificationToken(int id, VerificationToken token)
        {
            if (token == null)
            {
                return VerifyResult.InvalidToken;
            }
            
            string email;
            return this.CheckVerificationToken(id, token, out email);
        }

        public VerifyResult CheckVerificationToken(int id, VerificationToken token, out string email)
        {
            string userEmail = null;
            var result = this.ConnectionFactory.InTransaction(true).Do(t =>
                {
                    var data = t.ReadOne("SELECT Email, UserBlocked, VerificationToken FROM Users WHERE Id=@id", new { id });
                    return VerifyToken(data, token, out userEmail);
                });
            email = userEmail;
            return result;
        }

        public VerifyResult ResetVerificationToken(string email, out VerificationToken token)
        {
            if (!ValidateAndNormalizeEmail(ref email))
            {
                token = null;
                return VerifyResult.UnknownUser;
            }

            VerificationToken localToken = null;
            var result = this.ConnectionFactory.InTransaction(true).Do(
                t =>
                {
                    var data = t.ReadOne("SELECT Id, EmailVerified, UserBlocked FROM Users WHERE Email=@email", new { email });

                    if (data == null)
                    {
                        return VerifyResult.UnknownUser;
                    }

                    if (data.UserBlocked)
                    {
                        return VerifyResult.UserBlocked;
                    }

                    int id = data.Id;
                    localToken = VerificationToken.Generate();
                    t.Update("Users", new { VerificationToken = localToken.Data }, "@Id=id", new { id });
                    return VerifyResult.Success;
                });
            token = localToken;
            return result;
        }

        public bool ChangePassword(int id, string oldPassword, string newPassword)
        {
            if (oldPassword == newPassword || !this.ValidatePassword(newPassword))
            {
                return false;
            }

            dynamic userInfo = this.ConnectionFactory.ReadOne(
                "SELECT Id, Email, Password, Salt, EmailVerified, UserBlocked FROM Users WHERE Id=@id",
                new { id });

            if (this.ValidateUserInfo(userInfo, oldPassword, out id) != LoginResult.Success)
            {
                return false;
            }

            byte[] salt = null;
            var password = this.PasswordHasher.Hash(newPassword, ref salt);
            this.ConnectionFactory.Update("Users", new { password, salt }, "Id=@id", new { id });
            return true;
        }

        public void SetBlocked(int id, bool blocked)
        {
            this.ConnectionFactory.Update("Users", new { UserBlocked = blocked }, "Id=@id", new { id });
        }

        public bool IsPasswordSet(int id)
        {
            var data = this.ConnectionFactory.ReadOne("SELECT Password FROM Users WHERE Id=@id", new { id });
            return data != null && data.Password != null;
        }

        private static bool AreNotNullAndEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null || (a.Length != b.Length))
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateAndNormalizeEmail(ref string email)
        {
            return !string.IsNullOrWhiteSpace(email)
                && (email = email.Trim().ToLowerInvariant()).Length > 3
                && email.IndexOf('@') > -1;
        }

        private static VerifyResult VerifyToken(dynamic data, VerificationToken token, out string email)
        {
            if (data == null)
            {
                email = null;
                return VerifyResult.UnknownUser;
            }

            email = data.Email;

            if (data.UserBlocked)
            {
                return VerifyResult.UserBlocked;
            }

            byte[] storedToken = data.VerificationToken;
            if (token == null || !AreNotNullAndEqual(storedToken, token.Data))
            {
                return VerifyResult.InvalidToken;
            }

            return VerifyResult.Success;
        }

        private bool ValidatePassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password)
                && password.Length >= this.MinPasswordLength;
        }

        private LoginResult ValidateUserInfo(dynamic userInfo, string password, out int id)
        {
            if (userInfo == null)
            {
                id = 0;
                return LoginResult.UnknownUser;
            }

            id = userInfo.Id;
            if (userInfo.UserBlocked)
            {
                return LoginResult.UserBlocked;
            }

            if (!userInfo.EmailVerified)
            {
                return LoginResult.EmailNotVerified;
            }

            if (userInfo.Password == null)
            {
                return LoginResult.PasswordNotSet;
            }

            byte[] salt = userInfo.Salt;
            byte[] attemptedPasswordHash = this.PasswordHasher.Hash(password, ref salt);
            return AreNotNullAndEqual(attemptedPasswordHash, userInfo.Password) ? LoginResult.Success : LoginResult.WrongPassword;
        }
    }
}