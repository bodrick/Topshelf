using System.Collections.Generic;

namespace System.Configuration.Install
{
    public class InstallerCollection : Collections.ObjectModel.Collection<Installer>
    {
        private readonly Installer _owner;

        internal InstallerCollection(Installer owner) => _owner = owner;

        public void AddRange(IEnumerable<Installer> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            foreach (var item in value)
            {
                Add(item);
            }
        }

        protected override void InsertItem(int index, Installer item)
        {
            if (item == _owner)
            {
                throw new ArgumentException(Res.GetString(Res.CantAddSelf));
            }

            item.Parent = _owner;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this[index].Parent = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, Installer item)
        {
            if (item == _owner)
            {
                throw new ArgumentException(Res.GetString(Res.CantAddSelf));
            }

            this[index].Parent = null;
            item.Parent = _owner;
            base.SetItem(index, item);
        }
    }
}
