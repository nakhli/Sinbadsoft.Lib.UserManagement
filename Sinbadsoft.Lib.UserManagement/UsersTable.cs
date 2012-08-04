// <copyright file="UsersTable.cs" company="Sinbadsoft">
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
using System.Data;
using MonkeyOrm;

namespace Sinbadsoft.Lib.UserManagement
{
    public class UsersTable
    {
        public static void Create(IDbConnection connection)
        {
            // NOTE(cnakhli) email length max is 320 chars see http://tools.ietf.org/html/rfc3696#section-3 
            connection.Execute(@"
CREATE TABLE `Users` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Email` varchar(254) NOT NULL,
  `Password` binary(64) DEFAULT NULL,
  `Salt` binary(8) DEFAULT NULL,
  `EmailVerified` tinyint(1) NOT NULL DEFAULT '0',
  `UserBlocked` tinyint(1) NOT NULL DEFAULT '0',
  `VerificationToken` varbinary(24) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Email_uq` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8");
        }

        public static void Drop(IDbConnection connection)
        {
            connection.Execute(@"DROP TABLE IF EXISTS `Users`");
        }

        public static bool Exists(IDbConnection connection)
        {
            const string Query = @"SELECT EXISTS(SELECT 1 
                      FROM information_schema.tables 
                      WHERE table_schema=@Database AND table_name='Users') AS HasTable";
            return connection.ReadOne(Query, new { connection.Database }).HasTable != 0;
        }

        public static bool CreateIfMissing(IDbConnection connection)
        {
            if (!Exists(connection))
            {
                Create(connection);
                return true;
            }

            return false;
        }
    }
}