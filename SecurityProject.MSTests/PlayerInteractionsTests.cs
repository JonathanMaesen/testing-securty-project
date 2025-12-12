using security_testing_project;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;

namespace SecurityProject.MSTests
{
    [TestClass]
    public class PlayerInteractionTests
    {
        private Mock<ApiService> _mockApiService = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockApiService = new Mock<ApiService>(new HttpClient());
        }

        [TestMethod]
        public void Look_ShouldReturnRoomDescription()
        {
            var world = new World(_mockApiService.Object);
            var room = new Room("Start", "You are in the start room.");
            world.AddRoom(room);
            world.SetStart("Start");

            string result = world.Look();

            Assert.IsTrue(result.Contains("Start"));
        }

        [TestMethod]
        public void Take_ShouldAddItemToInventory()
        {
            var world = new World(_mockApiService.Object);
            var room = new Room("Start", "You are in the start room.");
            var item = new Item("Sword", "A sharp blade.", ItemType.Weapon);
            room.Items.Add(item);
            world.AddRoom(room);
            world.SetStart("Start");

            var result = world.Take("Sword");

            Assert.IsFalse(room.Items.Any(i => i.Name == "Sword"));
            StringAssert.Contains(result, "took");
        }

        [TestMethod]
        public async Task Go_ShouldChangeRoom_WhenExitExists()
        {
            var world = new World(_mockApiService.Object);
            var start = new Room("Start", "Start room");
            var next = new Room("Next", "Next room");
            world.AddRoom(start);
            world.AddRoom(next);
            world.Connect("Start", Direction.Up, "Next");
            world.SetStart("Start");

            string result = await world.Go(Direction.Up);
            
            Assert.IsTrue(result.Contains("Next"));
        }

        [TestMethod]
        public void Inventory_ShouldConfirmItemPresence()
        {
            var inv = new PlayerInventory();
            var key = new Item("Key", "A small key", ItemType.Key);
            inv.AddItem(key);

            Assert.IsTrue(inv.HasType(ItemType.Key));
        }

        [TestMethod]
        public void Fight_ShouldKillMonster_WhenWeaponInInventory()
        {
            var world = new World(_mockApiService.Object);
            var room = new Room("MonsterRoom", "A scary place");
            
            var sword = new Item("Sword", "A sharp sword", ItemType.Weapon);
            var monster = new Monster("Goblin", true);
            
            room.Items.Add(sword);
            room.Monster = monster;
            
            world.AddRoom(room);
            world.SetStart("MonsterRoom");
            world.Take("Sword");
            
            var result = world.Fight();

            Assert.IsFalse(monster.IsAlive, "Monster moet dood zijn na Fight()");
            StringAssert.Contains(result.ToLower(), "defeat");
        }
        [TestMethod]
        public async Task GoBack_ShouldReturnToPreviousRoom()
        {
            var world = new World(_mockApiService.Object);
            var start = new Room("Start", "Start room");
            var next = new Room("Next", "Next room");
            world.AddRoom(start);
            world.AddRoom(next);
            world.Connect("Start", Direction.Up, "Next");
            world.SetStart("Start");

            await world.Go(Direction.Up);
            var result = world.GoBack();

            StringAssert.Contains(result, "Start");
        }

        [TestMethod]
        public void GoBack_ShouldFail_WhenNoPreviousRoom()
        {
            var world = new World(_mockApiService.Object);
            var start = new Room("Start", "Start room");
            world.AddRoom(start);
            world.SetStart("Start");

            var result = world.GoBack();

            StringAssert.Contains(result, "You can't go back.");
        }
        [TestMethod]
        public async Task GoBack_ShouldFail_WhenCalledTwice()
        {
            var world = new World(_mockApiService.Object);
            var start = new Room("Start", "Start room");
            var next = new Room("Next", "Next room");
            world.AddRoom(start);
            world.AddRoom(next);
            world.Connect("Start", Direction.Up, "Next");
            world.SetStart("Start");

            await world.Go(Direction.Up);
            world.GoBack();
            var result = world.GoBack();

            StringAssert.Contains(result, "You can't go back.");
        }

        [TestMethod]
        public async Task GoBackCommand_IsCaseInsensitive()
        {
            var world = new World(_mockApiService.Object);
            var start = new Room("Start", "Start room");
            var next = new Room("Next", "Next room");
            world.AddRoom(start);
            world.AddRoom(next);
            world.Connect("Start", Direction.Up, "Next");
            world.SetStart("Start");

            await world.Go(Direction.Up);

            var terminal = new CommandManager<string?>(() => { });
            terminal.AddCommand("go", arg => {
                if (arg != null && arg.Equals("back", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(world.GoBack());
                }
                return Task.FromResult(0);
            }, "Go back");

            using (var sw = new System.IO.StringWriter())
            {
                Console.SetOut(sw);
                await terminal.TryCommand("go", "BACK");
                var result = sw.ToString();
                StringAssert.Contains(result, "Start");
            }
        }
    }
}