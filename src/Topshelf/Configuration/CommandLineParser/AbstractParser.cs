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
using System.Linq;

namespace Topshelf.Configuration.CommandLineParser
{
    internal abstract class AbstractParser<TInput>
    {
        public Parser<TInput, TValue[]> Rep<TValue>(Parser<TInput, TValue> parser) =>
            Rep1(parser).Or(Succeed(System.Array.Empty<TValue>()));

        public Parser<TInput, TValue[]> Rep1<TValue>(Parser<TInput, TValue> parser) =>
            from x in parser
            from xs in Rep(parser)
            select new[] { x }.Concat(xs).ToArray();

        public static Parser<TInput, TValue> Succeed<TValue>(TValue value) => input => new Result<TInput, TValue>(value, input);
    }
}
