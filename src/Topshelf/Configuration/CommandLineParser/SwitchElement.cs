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
    internal class SwitchElement : ISwitchElement, IEquatable<SwitchElement>
    {
        public SwitchElement(string key, bool value = true)
        {
            Key = key;
            Value = value;
        }

        private SwitchElement(char key) : this(key.ToString())
        {
        }

        public string Key { get; }
        public bool Value { get; }

        public static ICommandLineElement New(char key) => new SwitchElement(key);

        public static ICommandLineElement New(string key) => new SwitchElement(key);

        public static ICommandLineElement New(char key, bool value) => new SwitchElement(key.ToString(), value);

        public bool Equals(SwitchElement? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.Key, Key) && other.Value.Equals(Value);
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

            return obj.GetType() == typeof(SwitchElement) && Equals((SwitchElement)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Key.GetHashCode() * 397) ^ Value.GetHashCode();
            }
        }

        public override string ToString() => "SWITCH: " + Key + " (" + Value + ")";
    }
}
