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
namespace Topshelf.Configuration.Constants
{
    /// <summary>
    /// A selection of commonly-used Windows services.
    /// </summary>
    public static class KnownServiceNames
    {
        /// <summary>
        /// The Event Log service.
        /// </summary>
        public static string EventLog => "Eventlog";

        /// <summary>
        /// The Internet Information Server service.
        /// </summary>
        public static string IIS => "W3SVC";

        /// <summary>
        /// The Microsoft Message Queue service.
        /// </summary>
        public static string Msmq => "MSMQ";

        /// <summary>
        /// The Microsoft SQL Server service.
        /// </summary>
        public static string SqlServer => "MSSQLSERVER";
    }
}
