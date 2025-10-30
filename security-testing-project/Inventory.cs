using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace security_testing_project
{
    internal class Inventory
    {
        private readonly Dictionary<string, Item> _items = new();

        public void Add(Item item)
        {
            _items[item.Id] = item;
        }

        public bool Has(string id)
        {
            return _items.ContainsKey(id);
        }
    }
}
