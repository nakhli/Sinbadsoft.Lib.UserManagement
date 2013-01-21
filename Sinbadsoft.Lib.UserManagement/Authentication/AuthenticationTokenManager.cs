// <copyright file="AuthenticationTokenManager.cs" company="Sinbadsoft">
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
using System.Web;
using System.Web.Security;

namespace Sinbadsoft.Lib.UserManagement.Authentication
{
    /// <summary>
    /// An implementation of <see cref="IAuthenticationTokenManager"/> based on <see cref="FormsAuthentication"/>.
    /// </summary>
    public class AuthenticationTokenManager : IAuthenticationTokenManager
    {
        public void Set(string email, int id, bool persistent = false, string data = null)
        {
            string ticket = AuthenticationToken.Generate(email, id);
            FormsAuthentication.SetAuthCookie(ticket, persistent);
        }

        public void Remove()
        {
            FormsAuthentication.SignOut();
        }

        public AuthenticatedUserInfo Verify()
        {
            if (HttpContext.Current.Request.IsAuthenticated)
            {
                var userInfo = AuthenticationToken.Parse(HttpContext.Current.User.Identity.Name);
                if (userInfo != null)
                {
                    return userInfo;
                }
                
                this.Remove();
            }

            return null;
        }
    }
}