﻿using security_testing_project;

namespace SecurityProject.MSTests
{
    [TestClass]
    public class PlayerInteractionTests
    {
        [TestMethod]
        public void Look_ShouldReturnRoomDescription()
        {
            var world = new World();
            var room = new Room("Start", "You are in the start room.");
            world.AddRoom(room);
            world.SetStart("Start");

            string result = world.Look();

            Assert.IsTrue(result.Contains("Start"));
        }

        [TestMethod]
        public void Take_ShouldAddItemToInventory()
        {
            var world = new World();
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
        public void Go_ShouldChangeRoom_WhenExitExists()
        {
            var world = new World();
            var start = new Room("Start", "Start room");
            var next = new Room("Next", "Next room");
            world.AddRoom(start);
            world.AddRoom(next);
            world.Connect("Start", Direction.Up, "Next");
            world.SetStart("Start");

            string result = world.Go(Direction.Up);
            
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
            var world = new World();
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
    }
}