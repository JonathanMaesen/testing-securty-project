using security_testing_project;

namespace SecurityProject.MSTests
{
    [TestClass]
    public class RoomTests
    {
        [TestMethod]
        public void DescribeContainsNameAndDescription()
        {
            var room = new Room("Hall", "A large echoing chamber");

            var text = room.Describe();

            StringAssert.Contains(text, "== Hall ==");
            StringAssert.Contains(text, "A large echoing chamber");
        }

        [TestMethod]
        public void DescribeContainsDeadly()
        {
            var room = new Room("Pit", "A dark hole") { IsDeadly = true };

            var text = room.Describe();

            StringAssert.Contains(text, "(A deadly presence lingers here.)");
        }

        [TestMethod]
        public void DescribeLocked()
        {
            var room = new Room("Vault", "A heavy door") { RequiresKey = true, IsUnlocked = false };

            var text = room.Describe();

            StringAssert.Contains(text, "The door is locked. You need a key.");
        }

        [TestMethod]
        public void DescribeFind()
        {
            var room = new Room("Closet", "Dusty shelves");
            room.Items.Add(new Item("Key", "A small brass key."));
            room.Items.Add(new Item("Note", "Scrawled handwriting."));

            var text = room.Describe();

            StringAssert.Contains(text, "You see: Key, Note");
        }

        [TestMethod]
        public void DescribeMonsters()
        {
            var room = new Room("Lair", "Bones everywhere")
            {
                IsDeadly = false,
                RequiresKey = false,
                IsUnlocked = false,
                Monster = null
            };
            room.Monster = new Monster("Goblin", true);

            var text = room.Describe();

            StringAssert.Contains(text, "You sense danger: a Goblin is here.");
        }

        [TestMethod]
        public void DescribeExit()
        {
            var room1 = new Room("Hall", "A chamber");
            var room2 = new Room("Kitchen", "Smells like soup");

            room1.Exits[Direction.Up] = room2;

            var text = room1.Describe();

            StringAssert.Contains(text, "Exits: Up");
        }
    }
    
}
