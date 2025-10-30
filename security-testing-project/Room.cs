using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace security_testing_project
{
    internal class Room
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public bool HasMonster { get; set; }

        public Room(string id, string name, string description, bool hasMonster = false)
        {
            Id = id;
            Name = name;
            Description = description;
            HasMonster = hasMonster;
        }
    }
}
