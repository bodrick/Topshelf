using System.ComponentModel;

namespace System.Configuration.Install
{
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class ResDescriptionAttribute : DescriptionAttribute
    {
        private bool _replaced;

        public ResDescriptionAttribute(string description) : base(description)
        {
        }

        public override string Description
        {
            get
            {
                if (!_replaced)
                {
                    _replaced = true;
                    DescriptionValue = Res.GetString(base.Description) ?? string.Empty;
                }
                return base.Description;
            }
        }
    }
}
