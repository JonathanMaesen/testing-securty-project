﻿using security_testing_project;

namespace SecurityProject.MSTests
{
    [TestClass]
    public class WorldFightTests
    {
        [TestMethod]
        public void Fight_NoMonster_ReturnsExpectedMessage()
        {
            var room = new Room("MonsterRoom", "A scary place");
            var world = new World();
            world.AddRoom(room);
            world.SetStart("MonsterRoom");

            var result = world.Fight();

            Assert.AreEqual("There is nothing to fight here.", result);
        }

        [TestMethod]
        public void Fight_WithoutWeapon_PlayerDies()
        {
            var monster = new Monster("Goblin", true);
            var room = new Room("MonsterRoom", "A scary place") { Monster = monster };
            var world = new World();
            world.AddRoom(room);
            world.SetStart("MonsterRoom");

            var result = world.Fight();

            StringAssert.Contains(result, "die");
        }

        [TestMethod]
        public void Fight_WithWeapon_MonsterDies()
        {
            var monster = new Monster("Goblin", true);
            var room = new Room("MonsterRoom", "A scary place") { Monster = monster };
            var world = new World();
            world.AddRoom(room);
            world.SetStart("MonsterRoom");

            var sword = new Item("Sword", "A sharp sword", ItemType.Weapon);
            room.Items.Add(sword);
            world.Take("Sword"); 

            var result = world.Fight();

            Assert.IsFalse(monster.IsAlive);
            StringAssert.Contains(result.ToLower(), "defeat");
        }
    }
}
