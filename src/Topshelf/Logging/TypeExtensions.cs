using System;

namespace Topshelf.Logging
{
    internal static class TypeExtensions
    {
        private static readonly TypeNameFormatter TypeNameFormatter = new();

        public static string GetTypeName(this Type type) => TypeNameFormatter.GetTypeName(type);
    }
}
