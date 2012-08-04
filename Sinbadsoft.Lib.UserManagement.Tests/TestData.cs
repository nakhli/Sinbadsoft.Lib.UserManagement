// <copyright file="TestData.cs" company="Sinbadsoft">
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

using MonkeyOrm;

namespace Sinbadsoft.Lib.UserManagement.Tests
{
    public static class TestData
    {
        public static readonly User JoeEmailNotVerifiedPasswordSet = new User
        {
            Email = "joe@example.com",
            EmailVerified = false,
            VerificationToken = VerificationToken.Generate().Data,
        };

        public static readonly User RobertEmailVerifiedButBlocked = new User
            {
                Email = "robert@example.com",
                EmailVerified = true,
                UserBlocked = true
            };

        public static readonly User SamRegularUser = new User
        {
            Email = "sam@example.com",
            EmailVerified = true,
        };

        public static readonly User AnneEmailNotVerifiedPasswordNotSet = new User
        {
            Email = "anne@example.com",
            EmailVerified = false,
            VerificationToken = VerificationToken.Generate().Data,
        };

        static TestData()
        {
            SetPasswordData(JoeEmailNotVerifiedPasswordSet);
            SetPasswordData(RobertEmailVerifiedButBlocked);
            SetPasswordData(SamRegularUser);
        }

        public static void InsertData(IConnectionFactory factory, bool truncate = false)
        {
            if (truncate)
            {
                factory.Execute("TRUNCATE Users");
            }

            SaveUser(factory, JoeEmailNotVerifiedPasswordSet);
            SaveUser(factory, RobertEmailVerifiedButBlocked);
            SaveUser(factory, SamRegularUser);
            SaveUser(factory, AnneEmailNotVerifiedPasswordNotSet);
        }

        private static void SaveUser(IConnectionFactory connection, User user)
        {
            int id;
            connection.Save("Users", user, out id, blacklist: new[] { "Id", "StringPassword" });
            user.Id = id;
        }

        private static void SetPasswordData(User user)
        {
            byte[] salt = null;
            byte[] hash = new PasswordHasher().Hash(user.StringPassword, ref salt);
            user.Password = hash;
            user.Salt = salt;
        }

        public class User
        {
            public int Id { get; set; }

            public string Email { get; set; }

            public byte[] Password { get; set; }

            public byte[] Salt { get; set; }

            public bool UserBlocked { get; set; }

            public bool EmailVerified { get; set; }

            public byte[] VerificationToken { get; set; }

            public string StringPassword
            {
                get { return this.Email + " - password"; }
            }
        }
    }
}