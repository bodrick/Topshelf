using System.Collections.Generic;

namespace System.Configuration.Install
{
    public class InstallerCollection : Collections.ObjectModel.Collection<Installer>
    {
        private readonly Installer owner;

        internal InstallerCollection(Installer owner) => this.owner = owner;

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
            if (item == owner)
            {
                throw new ArgumentException(Res.GetString("CantAddSelf"));
            }

            item.parent = owner;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this[index].parent = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, Installer item)
        {
            if (item == owner)
            {
                throw new ArgumentException(Res.GetString("CantAddSelf"));
            }

            this[index].parent = null;
            item.parent = owner;
            base.SetItem(index, item);
        }
    }
}
