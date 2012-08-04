// <copyright file="PasswordHasherPerformanceTest.cs" company="Sinbadsoft">
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
using System.Diagnostics;

using NUnit.Framework;

namespace Sinbadsoft.Lib.UserManagement.Tests
{
    [TestFixture]
    public class PasswordHasherPerformanceTest
    {
        [Test, Ignore]
        public void PerformanceHashAndGenerateSalt()
        {
            byte[] salt = null;
            RunHashLoop(ref salt, 100);
        }

        [Test, Ignore]
        public void PerformanceHashOnly()
        {
            var salt = new byte[] { 140, 143, 171, 199, 47, 63, 229, 21 };
            RunHashLoop(ref salt, 100);
        }

        private static void RunHashLoop(ref byte[] salt, int max)
        {
            var hasher = new PasswordHasher();
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < max; i++)
            {
                hasher.Hash("random password :!/", ref salt);
            }

            watch.Stop();
            Console.WriteLine("{0} s/hash", watch.Elapsed.TotalSeconds / max);
        }
    }
}