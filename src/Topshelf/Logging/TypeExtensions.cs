using System;

namespace Topshelf.Logging
{
    internal static class TypeExtensions
    {
        private static readonly TypeNameFormatter _typeNameFormatter = new TypeNameFormatter();

        public static string GetTypeName(this Type type) => _typeNameFormatter.GetTypeName(type);
    }
}
