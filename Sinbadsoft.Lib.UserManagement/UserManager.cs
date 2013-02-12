// <copyright file="UserManager.cs" company="Sinbadsoft">
// Copyright (c) Chaker Nakhli 2010-2013
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using MonkeyOrm;

namespace Sinbadsoft.Lib.UserManagement
{
    public class UserManager : IUserManager
    {
        public const int DefaultMinPasswordLength = 5;

        public UserManager(IConnectionFactory connectionFactory, IPasswordHasher hasher = null)
        {
            this.Connection = connectionFactory;
            this.PasswordHasher = hasher ?? new PasswordHasher();
            this.MinPasswordLength = DefaultMinPasswordLength;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UserManager"/> class with
        /// a database provider invariant name, a connection string and a password hasher.
        /// </summary>
        /// <param name="providerName">Invariant name of a provider. Used to get a used to get <see cref="DbProviderFactory"/>.</param>
        /// <param name="connectionString">Connection string to connect to the database.</param>
        /// <param name="hasher">Password hashing strategy.</param>
        public UserManager(string providerName, string connectionString, IPasswordHasher hasher = null)
            : this(new DbProviderBasedConnectionFactory(providerName, connectionString), hasher)
        {
        }

        public UserManager(Func<IDbConnection> connectionFactory, IPasswordHasher hasher = null)
            : this(new FunctionBasedConnectionFactory(connectionFactory), hasher)
        {
        }

        public int MinPasswordLength { get; set; }

        public IConnectionFactory Connection { get; private set; }

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

            var userInfo = this.Connection.ReadOne(
                "SELECT Id, Email, Password, Salt, EmailVerified, UserBlocked FROM Users WHERE Email=@email",
                new { email });

            return this.ValidateUserInfo(userInfo, password, out id);
        }

        public RegisterResult Register(string email, string password, out VerificationToken token)
        {
            int id;
            return this.Register(email, password, out token, out id);
        }

        public RegisterResult Register(string email, string password, out VerificationToken token, out int id)
        {
            if (!ValidateAndNormalizeEmail(ref email))
            {
                id = 0;
                token = null;
                return RegisterResult.InvalidEmail;
            }

            if (password != null && !this.ValidatePassword(password))
            {
                id = 0;
                token = null;
                return RegisterResult.InvalidPassword;
            }

            byte[] salt = null;
            byte[] hash = null;
            if (password != null)
            {
                hash = this.PasswordHasher.Hash(password, ref salt);
            }

            token = VerificationToken.Generate();

            var user = new
                    {
                        Salt = salt,
                        Password = hash,
                        Email = email,
                        EmailVerified = false,
                        UserBlocked = false,
                        VerificationToken = token.Data
                    };

            try
            {
                int userId = 0;
                var result = this.Connection.InTransaction(true).Do(
                t =>
                {
                    var userInfo = t.ReadOne("SELECT Id, UserBlocked FROM Users WHERE Email=@email", new { email });
                    if (userInfo != null)
                    {
                        userId = userInfo.Id;
                        return userInfo.UserBlocked 
                            ? RegisterResult.UserBlocked
                            : RegisterResult.DuplicateEmail;
                    }

                    t.Save("Users", user, out userId);
                    return RegisterResult.Success;
                });

                id = userId;
                return result;
            }
            catch (DbException exception)
            {
                const int MysqlDuplicateEntryServerErrorCode = 1062;
                object exceptionData = exception.Data["Server Error Code"];
                if (exceptionData is int && (int)exceptionData == MysqlDuplicateEntryServerErrorCode)
                {
                    id = 0;
                    token = null;
                    return RegisterResult.DuplicateEmail;
                }

                throw;
            }
        }

        public VerifyResult CheckAndClearVerificationToken(int id, VerificationToken token, string newPassword = null)
        {
            string email;
            return this.CheckAndClearVerificationToken(id, token, out email, newPassword);
        }

        public VerifyResult CheckAndClearVerificationToken(int id, VerificationToken token, out string email, string newPassword = null)
        {
            string userEmail = null;
            VerifyResult result = this.Connection.InTransaction(true).Do(
                t =>
                {
                    var data = t.ReadOne("SELECT Email, Password, UserBlocked, VerificationToken FROM Users WHERE Id=@id", new { id });

                    result = VerifyToken(data, token, out userEmail);

                    if ((newPassword != null && !this.ValidatePassword(newPassword))
                        || (newPassword == null && data.Password == null))
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
            var result = this.Connection.InTransaction(true).Do(t =>
                {
                    var data = t.ReadOne("SELECT Email, UserBlocked, VerificationToken FROM Users WHERE Id=@id", new { id });
                    return VerifyToken(data, token, out userEmail);
                });
            email = userEmail;
            return result;
        }

        public VerifyResult ResetVerificationToken(string email, out VerificationToken token)
        {
            int id;
            return this.ResetVerificationToken(email, out token, out id);
        }

        public VerifyResult ResetVerificationToken(string email, out VerificationToken token, out int id)
        {
            if (!ValidateAndNormalizeEmail(ref email))
            {
                token = null;
                id = 0;
                return VerifyResult.UnknownUser;
            }

            VerificationToken localToken = null;
            int localId = 0;
            var result = this.Connection.InTransaction(true).Do(
                t =>
                {
                    var data = t.ReadOne("SELECT Id, EmailVerified, UserBlocked FROM Users WHERE Email=@email", new { email });

                    if (data == null)
                    {
                        return VerifyResult.UnknownUser;
                    }

                    localId = data.Id;

                    if (data.UserBlocked)
                    {    
                        return VerifyResult.UserBlocked;
                    }
                    
                    localToken = VerificationToken.Generate();
                    t.Update("Users", new { VerificationToken = localToken.Data }, "Id=@id", new { id = localId });
                    return VerifyResult.Success;
                });
            token = localToken;
            id = localId;
            return result;
        }

        public bool ChangePassword(int id, string oldPassword, string newPassword)
        {
            if (oldPassword == newPassword || !this.ValidatePassword(newPassword))
            {
                return false;
            }

            dynamic userInfo = this.Connection.ReadOne(
                "SELECT Id, Email, Password, Salt, EmailVerified, UserBlocked FROM Users WHERE Id=@id",
                new { id });

            if (this.ValidateUserInfo(userInfo, oldPassword, out id) != LoginResult.Success)
            {
                return false;
            }

            byte[] salt = null;
            var password = this.PasswordHasher.Hash(newPassword, ref salt);
            this.Connection.Update("Users", new { password, salt }, "Id=@id", new { id });
            return true;
        }

        public void SetBlocked(int id, bool blocked)
        {
            this.Connection.Update("Users", new { UserBlocked = blocked }, "Id=@id", new { id });
        }

        public UserData LoadUserData(int id)
        {
            var userInfo = this.Connection.ReadOne(
                "SELECT Id, Email, Password IS NOT NULL AS PasswordSet, EmailVerified, UserBlocked, VerificationToken FROM Users WHERE Id=@id",
                new { id });
            return CreateUserData(userInfo);
        }

        public UserData LoadUserData(string email)
        {
            var userInfo = this.Connection.ReadOne(
                "SELECT Id, Email, Password IS NOT NULL AS PasswordSet, EmailVerified, UserBlocked, VerificationToken FROM Users WHERE Email=@email",
                new { email });
            return CreateUserData(userInfo);
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

        private static UserData CreateUserData(dynamic data)
        {
            return data == null
                       ? null
                       : new UserData
                           {
                               Id = data.Id,
                               Email = data.Email,
                               PasswordSet = data.PasswordSet != 0,
                               EmailVerified = data.EmailVerified,
                               UserBlocked = data.UserBlocked,
                               VerificationToken = data.VerificationToken == null
                                    ? null
                                    : new VerificationToken((byte[])data.VerificationToken)
                           };
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