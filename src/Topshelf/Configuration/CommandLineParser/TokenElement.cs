// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.

using System;

namespace Topshelf.Configuration.CommandLineParser
{
    internal class TokenElement : ITokenElement, IEquatable<TokenElement>
    {
        public TokenElement(string token) => Token = token;

        public string Token { get; }

        public static ICommandLineElement New(string token) => new TokenElement(token);

        public bool Equals(TokenElement? other)
        {
            if (other is null)
            {
                return false;
            }

            return ReferenceEquals(this, other) || Equals(other.Token, Token);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == typeof(TokenElement) && Equals((TokenElement)obj);
        }

        public override int GetHashCode() => Token.GetHashCode();

        public override string ToString() => "TOKEN: " + Token;
    }
}
