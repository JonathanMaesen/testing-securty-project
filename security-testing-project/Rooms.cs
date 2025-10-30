using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace security_testing_project
{
    internal class Rooms
    {
        public Room CurrentRoom { get; private set; }
        public Inventory Inventory { get; }
        public string State { get; private set; } = "playing";

        public Rooms(Room startRoom, Inventory inventory)
        {
            CurrentRoom = startRoom;
            Inventory = inventory;
        }

        public string Fight()
        {
            if (!CurrentRoom.HasMonster)
            {
                return "Er is hier niets om tegen te vechten.";
            }

            if (!Inventory.Has("sword"))
            {
                State = "lost";
                return "Je probeert met je blote handen te vechten... het monster verslindt je!";
            }

            CurrentRoom.HasMonster = false;
            return "Je hebt het monster verslagen met je zwaard!";
        }
    }
}
