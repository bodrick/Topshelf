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
            var standardValues = base.GetStandardValues(context);
            var instance = context.Instance;
            var num = 0;
            var array = new object[standardValues.Count - 1];
            foreach (var standardValue in standardValues)
            {
                if (standardValue != instance)
                {
                    array[num] = standardValue;
                    num++;
                }
            }

            return new StandardValuesCollection(array);
        }
    }
}
