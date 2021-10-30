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
using System.Linq;

namespace Topshelf.Configuration.CommandLineParser
{
    internal class StringCommandLineParser : AbstractParser<string>
    {
        public StringCommandLineParser()
        {
            Whitespace = Rep(Char(' ').Or(Char('\t').Or(Char('\n')).Or(Char('\r'))));
            NewLine = Rep(Char('\r').Or(Char('\n')));

            EscChar = (from bs in Char('\\')
                       from ch in Char('\\').Or(Char('\"')).Or(Char('-')).Or(Char('/')).Or(Char('\''))
                       select ch)
                .Or(from ch in Char(x => x != '"') select ch);

            Id = from w in Whitespace
                 from c in Char(char.IsLetter)
                 from cs in Rep(Char(char.IsLetterOrDigit))
                 select cs.Aggregate(c.ToString(), (s, ch) => s + ch);

            Key = from w in Whitespace
                  from c in Char(char.IsLetter)
                  from cs in Rep(Char(char.IsLetterOrDigit).Or(Char('.')))
                  select cs.Aggregate(c.ToString(), (s, ch) => s + ch);

            Value = (from symbol in Rep(Char(char.IsLetterOrDigit).Or(Char(char.IsPunctuation)).Or(Char(char.IsSymbol)))
                     select symbol.Aggregate("", (s, ch) => s + ch));

            ValueInQuotes = from oq in Char('"')
                            from value in Rep(EscChar)
                            from cq in Char('"')
                            select value.Aggregate("", (s, ch) => s + ch);

            Definition = (from w in Whitespace
                          from c in Char('-').Or(Char('/'))
                          from key in Id
                          from eq in Char(':').Or(Char('='))
                          from value in Value
                          select DefinitionElement.New(key, value))
                .Or(from w in Whitespace
                    from c in Char('-').Or(Char('/'))
                    from key in Id
                    from ws in Whitespace
                    from value in ValueInQuotes.Or(Value)
                    select DefinitionElement.New(key, value));

            EmptyDefinition = (from w in Whitespace
                               from c in Char('-').Or(Char('/'))
                               from key in Id
                               from ws in Whitespace
                               select DefinitionElement.New(key, ""));

            Argument = from w in Whitespace
                       from c in Char(char.IsLetterOrDigit).Or(Char(char.IsPunctuation))
                       from cs in Rep(Char(char.IsLetterOrDigit).Or(Char(char.IsPunctuation)))
                       select ArgumentElement.New(cs.Aggregate(c.ToString(), (s, ch) => s + ch));

            Switch = (from w in Whitespace
                      from c in Char('-').Or(Char('/'))
                      from arg in Char(char.IsLetterOrDigit)
                      from non in Rep(Char(char.IsLetterOrDigit))
                      where non.Length == 0
                      select SwitchElement.New(arg))
                .Or(from w in Whitespace
                    from c in Char('-').Or(Char('/'))
                    from arg in Char(char.IsLetterOrDigit)
                    from n in Char('-')
                    select SwitchElement.New(arg, false))
                .Or(from w in Whitespace
                    from c1 in Char('-')
                    from c2 in Char('-')
                    from arg in Id
                    select SwitchElement.New(arg));

            Token = from w in Whitespace
                    from o in Char('[')
                    from t in Key
                    from c in Char(']')
                    select TokenElement.New(t);

            All =
                (from element in Definition select element)
                .Or(from element in Switch select element)
                .Or(from element in EmptyDefinition select element)
                .Or(from element in Token select element)
                .Or(from element in Argument select element);
        }

        public Parser<string, ICommandLineElement> All { get; }

        private static Parser<string, char> AnyChar => input => input.Length > 0
            ? new Result<string, char>(input[0], input[1..])
            : null;

        private Parser<string, ICommandLineElement> Argument { get; }
        private Parser<string, ICommandLineElement> Definition { get; }
        private Parser<string, ICommandLineElement> EmptyDefinition { get; }
        private Parser<string, char> EscChar { get; }
        private Parser<string, string> Id { get; }
        private Parser<string, string> Key { get; }
        private Parser<string, char[]> NewLine { get; }
        private Parser<string, ICommandLineElement> Switch { get; }
        private Parser<string, ICommandLineElement> Token { get; }
        private Parser<string, string> Value { get; }
        private Parser<string, string> ValueInQuotes { get; }
        private Parser<string, char[]> Whitespace { get; }

        private static Parser<string, char> Char(char ch) => from c in AnyChar where c == ch select c;

        private static Parser<string, char> Char(Predicate<char> predicate) => from c in AnyChar where predicate(c) select c;
    }
}
