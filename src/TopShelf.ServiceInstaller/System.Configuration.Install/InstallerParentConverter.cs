using System.ComponentModel;

namespace System.Configuration.Install
{
    internal class InstallerParentConverter : ReferenceConverter
    {
        public InstallerParentConverter(Type type) : base(type)
        {
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var baseValues = base.GetStandardValues(context);

            var component = context.Instance;
            int sourceIndex = 0, targetIndex = 0;
            // we want to return the same list, but with the current component removed.
            // (You can't set an installer's parent to itself.)
            // optimization: assume the current component will always be in the list.
            var newValues = new object[baseValues.Count - 1];
            while (sourceIndex < baseValues.Count)
            {
                if (baseValues[sourceIndex] != component)
                {
                    newValues[targetIndex] = baseValues[sourceIndex];
                    targetIndex++;
                }
                sourceIndex++;
            }

            return new StandardValuesCollection(newValues);
        }
    }
}
