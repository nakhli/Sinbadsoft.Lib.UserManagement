// <copyright file="AuthenticationToken.cs" company="Sinbadsoft">
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
// <date>2012/07/28</date>
using System.Globalization;

namespace Sinbadsoft.Lib.UserManagement.Authentication
{
    internal static class AuthenticationToken
    {
        /// <summary>
        /// Minimum email length = 3, min user id length = 1, separator length = 1
        /// ticket template: "email;id;data" where data is optional.
        /// </summary>
        public const int MinTicketLength = 5;
        public const char Separator = ';';

        public static AuthenticatedUserInfo Parse(string ticket)
        {
            if (string.IsNullOrWhiteSpace(ticket) || ticket.Length < MinTicketLength)
            {
                return null;
            }

            int firstSeparator = ticket.IndexOf(Separator);
            if (firstSeparator < 3 || firstSeparator >= ticket.Length - 1)
            {
                return null;
            }

            string email = ticket.Substring(0, firstSeparator);

            int secondSepartor = ticket.IndexOf(Separator, firstSeparator + 1);

            string data = string.Empty;
            string idString;
            if (secondSepartor < 0)
            {
                idString = ticket.Substring(firstSeparator + 1);
            }
            else
            {
                idString = ticket.Substring(firstSeparator + 1, secondSepartor - firstSeparator - 1);
                data = ticket.Substring(secondSepartor + 1);
            }

            int id;
            if (!int.TryParse(idString, out id))
            {
                return null;
            }

            return new AuthenticatedUserInfo(id, email, data);
        }

        public static string Generate(string email, int id, string data = null)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0};{1};{2}", email, id, data ?? string.Empty);
        }
    }
}