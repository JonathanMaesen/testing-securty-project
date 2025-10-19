using security_testing_project;

namespace SecurityProject.MSTests
{
    [TestClass]
    public sealed class WorldTests
    {
        [TestMethod]
        public void WorldGenerates()
        {
            var world = new World();
            Assert.IsTrue(world is IGameworld);
        }

        [TestMethod]
        public void AddRoomWorks()
        {
            var world = new World();
            var room1 = new Room("Entrance", "Start here");
            var room2 = new Room("Hall", "Corridor");

            world.AddRoom(room1);
            world.AddRoom(room2);
            world.Connect("Entrance", Direction.Up, "Hall");

            Assert.IsTrue(room1.Exits.ContainsKey(Direction.Up));
            Assert.AreSame(room2, room1.Exits[Direction.Up]);
        }

        [TestMethod]
        public void SetStart()
        {
            var world = new World();
            var start = new Room("Start", "The starting room");
            world.AddRoom(start);

            world.SetStart("Start");
            var text = world.Look();

            StringAssert.Contains(text, "== Start ==");
            StringAssert.Contains(text, "The starting room");
        }

        [TestMethod]
        public void ErrorWhenRoomsNotConnectedRight()
        {
            var world = new World();
            var room = new Room("Hall", "Corridor");
            world.AddRoom(room);

            Assert.ThrowsException<KeyNotFoundException>(() =>
            {
                world.Connect("Missing", Direction.Up, "Hall");
            });
        }

        [TestMethod]
        public void ErrorWhenRoomsNotConnected()
        {
            var world = new World();
            var room = new Room("Entrance", "Start");
            world.AddRoom(room);

            Assert.ThrowsException<KeyNotFoundException>(() =>
            {
                world.Connect("Entrance", Direction.Up, "Missing");
            });
        }
    }
}
