// <copyright file="IAuthenticationTokenManager.cs" company="Sinbadsoft">
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
// <date>2012/07/28</date>
namespace Sinbadsoft.Lib.UserManagement.Authentication
{
    public interface IAuthenticationTokenManager
    {
        /// <summary>
        /// Sets a cookie with the crytpted authentication token. The token embeds the provided
        /// <paramref name="email"/>, <paramref name="id"/> and optional additional data <paramref name="data"/>.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <param name="id">User identifier.</param>
        /// <param name="persistent"><see langword="true"/> to create a persistent cookie, <see langword="false"/> otherwie.</param>
        /// <param name="data">Extra data to embedd in the authentication cookie. Should be short.</param>
        void Set(string email, int id, bool persistent = false, string data = null);

        /// <summary>
        /// Removes the authentication token cookie.
        /// </summary>
        void Remove();

        /// <summary>
        /// Decrypts and extracts the user information embedded in the authentication ticket.
        /// If the ticket is invalid or has bad format, the authentication is revoked
        /// and the token is removed using <see cref="Remove"/>.
        /// </summary>
        /// <returns>The user information embedded in the authentication ticket.</returns>
        AuthenticatedUserInfo Verify();
    }
}