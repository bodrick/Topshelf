using System;
using System.Text;
using Topshelf.Caching;

namespace Topshelf.Logging
{
    internal class TypeNameFormatter
    {
        private readonly Cache<Type, string> _cache;
        private readonly string _genericArgumentSeparator;
        private readonly string _genericOpen;
        private readonly string _genericClose;
        private readonly string _namespaceSeparator;
        private readonly string _nestedTypeSeparator;

        public TypeNameFormatter()
            : this(",", "<", ">", ".", "+")
        {
        }

        public TypeNameFormatter(string genericArgumentSeparator, string genericOpen, string genericClose,
            string namespaceSeparator, string nestedTypeSeparator)
        {
            _genericArgumentSeparator = genericArgumentSeparator;
            _genericOpen = genericOpen;
            _genericClose = genericClose;
            _namespaceSeparator = namespaceSeparator;
            _nestedTypeSeparator = nestedTypeSeparator;

            _cache = new ConcurrentCache<Type, string>(FormatTypeName);
        }

        public string GetTypeName(Type type) => _cache[type];

        private string FormatTypeName(Type type)
        {
#if !NETFX_CORE
            if (type.IsGenericTypeDefinition)
#else
            if (type.GetTypeInfo().IsGenericTypeDefinition)
#endif
                throw new ArgumentException("An open generic type cannot be used as a message name");

            var sb = new StringBuilder("");

            return FormatTypeName(sb, type, null);
        }

        private string FormatTypeName(StringBuilder sb, Type type, string scope)
        {
            if (type.IsGenericParameter)
            {
                return "";
            }

            if (type.Namespace != null)
            {
                var ns = type.Namespace;
                if (!ns.Equals(scope))
                {
                    sb.Append(ns);
                    sb.Append(_namespaceSeparator);
                }
            }

            if (type.IsNested)
            {
                FormatTypeName(sb, type.DeclaringType, type.Namespace);
                sb.Append(_nestedTypeSeparator);
            }
#if !NETFX_CORE
            if (type.IsGenericType)
#else
            if (type.GetTypeInfo().IsGenericType)
#endif
            {
                var name = type.GetGenericTypeDefinition().Name;

                //remove `1
                var index = name.IndexOf('`');
                if (index > 0)
                {
                    name = name.Remove(index);
                }

                sb.Append(name);
                sb.Append(_genericOpen);
#if !NETFX_CORE
                var arguments = type.GetGenericArguments();
#else
                Type[] arguments = type.GetTypeInfo().GenericTypeArguments;
#endif
                for (var i = 0; i < arguments.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(_genericArgumentSeparator);
                    }

                    FormatTypeName(sb, arguments[i], type.Namespace);
                }

                sb.Append(_genericClose);
            }
            else
            {
                sb.Append(type.Name);
            }

            return sb.ToString();
        }
    }
}
