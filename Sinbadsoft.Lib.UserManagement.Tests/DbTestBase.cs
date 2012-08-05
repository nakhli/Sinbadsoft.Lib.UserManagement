// <copyright file="DbTestBase.cs" company="Sinbadsoft">
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
using System;

using MonkeyOrm;

using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace Sinbadsoft.Lib.UserManagement.Tests
{
    public class DbTestBase
    {
        private const string ConnectionStringTemplate = "server=localhost;user id=developer;password=etOile03;port=3306;";
        private readonly bool createUsersTable;
        private string connectionString;

        protected DbTestBase(bool createUsersTable = true)
        {
            this.createUsersTable = createUsersTable;
        }

        protected string DatabaseName { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            this.DatabaseName = this.GetType().Name + DateTime.UtcNow.ToString("yyyy_MM_dd__HH_mm_ss");

            this.ConnectionFactory(ConnectionStringTemplate)
                .Execute("CREATE DATABASE IF NOT EXISTS " + this.DatabaseName);

            var connectionStringBuilder = new MySqlConnectionStringBuilder(ConnectionStringTemplate)
                {
                    Database = this.DatabaseName
                };
            this.connectionString = connectionStringBuilder.ConnectionString;

            if (this.createUsersTable)
            {
                using (var connection = this.ConnectionFactory().Create())
                {
                    connection.Open();
                    UsersTable.Create(connection);
                }
            }
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            this.ConnectionFactory().Execute("DROP DATABASE IF EXISTS " + this.DatabaseName);
        }

        protected IConnectionFactory ConnectionFactory(string connStr = null)
        {
            return new ConnectionFactory<MySqlConnection>(connStr ?? this.connectionString);
        }
    }
}
