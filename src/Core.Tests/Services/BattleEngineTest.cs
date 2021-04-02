using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars;
using CraigStars.Singletons;

namespace CraigStars.Tests
{
    [TestFixture]
    public class BattleEngineTest
    {
        BattleEngine battleEngine = new BattleEngine(new Rules(0));

        [Test]
        public void TestWillTarget()
        {
            BattleToken token = new BattleToken()
            {
                Attributes = BattleTokenAttribute.Armed | BattleTokenAttribute.Starbase
            };

            // Armed starbases are targeted by Any, Starbase, and ArmedShip
            Assert.IsTrue(battleEngine.WillTarget(BattleTargetType.Any, token));
            Assert.IsTrue(battleEngine.WillTarget(BattleTargetType.Starbase, token));
            Assert.IsTrue(battleEngine.WillTarget(BattleTargetType.ArmedShips, token));

            // unarmed, none, freighters, and bomber targets won't work on this token
            Assert.IsFalse(battleEngine.WillTarget(BattleTargetType.None, token));
            Assert.IsFalse(battleEngine.WillTarget(BattleTargetType.UnarmedShips, token));
            Assert.IsFalse(battleEngine.WillTarget(BattleTargetType.Freighters, token));
            Assert.IsFalse(battleEngine.WillTarget(BattleTargetType.BombersFreighters, token));
            Assert.IsFalse(battleEngine.WillTarget(BattleTargetType.FuelTransports, token));
        }

        [Test]
        public void TestFindTargets()
        {
            // create a new battle from two test fleets
            var battle = battleEngine.BuildBattle(Battles.GetFleetsForSimpleBattle());

            // find some targets!
            battleEngine.FindMoveTargets(battle);

            Assert.IsTrue(battle.HasTargets);
            Assert.AreEqual(2, battle.Tokens.Count);
            var token1 = battle.Tokens[0];
            var token2 = battle.Tokens[1];
            Assert.AreEqual(token2, token1.MoveTarget);
            Assert.IsNull(token2.MoveTarget);
        }

        [Test]
        public void TestPlaceTokensOnBoard()
        {
            // create a new battle from two test fleets
            var battle = battleEngine.BuildBattle(Battles.GetFleetsForSimpleBattle());

            // find some targets!
            battleEngine.PlaceTokensOnBoard(battle);
            var token1 = battle.Tokens[0];
            var token2 = battle.Tokens[1];

            // tokens should be left middle, right middle
            Assert.AreEqual(new Vector2(1, 4), token1.Position);
            Assert.AreEqual(new Vector2(8, 5), token2.Position);
        }

        [Test]
        public void TestBuildMovement()
        {
            // create a new battle from two test fleets
            var battle = battleEngine.BuildBattle(Battles.GetFleetsForSimpleBattle());
            var token1 = battle.Tokens[0];
            var token2 = battle.Tokens[1];

            // build the movement orders
            battleEngine.BuildMovementOrder(battle);
            var round1Tokens = battle.MoveOrder[0];
            var round2Tokens = battle.MoveOrder[1];

            // both tokens should move the first round
            // the heavier ship moves first, so the attacker should move
            Assert.AreEqual(2, round1Tokens.Count);
            Assert.AreEqual(token1, round1Tokens[0]);
            Assert.AreEqual(token2, round1Tokens[1]);

            // only the scout should move the second
            Assert.AreEqual(1, round2Tokens.Count);
            Assert.AreEqual(token2, round2Tokens[0]);
        }

        [Test]
        public void TestRunAway()
        {
            // create a new battle from two test fleets
            var battle = battleEngine.BuildBattle(Battles.GetFleetsForSimpleBattle());
            var attacker = battle.Tokens[0];
            var defender = battle.Tokens[1];
            var player1 = attacker.Fleet.Player;

            battle.BuildSortedWeaponSlots();

            var weapon = battle.SortedWeaponSlots[0];

            attacker.Position = new Vector2(0, 0);
            defender.Position = attacker.Position + Vector2.Right * weapon.Range;

            battleEngine.RunAway(battle, defender);

            // we should move one space to the right to get away
            Assert.AreEqual(attacker.Position + (Vector2.Right * weapon.Range) + Vector2.Right, defender.Position);

            // make sure we recorded this move
            Assert.AreEqual(1, battle.PlayerRecords[player1].ActionsPerRound[0].Count);
            Assert.AreEqual(typeof(BattleRecordTokenMove), battle.PlayerRecords[player1].ActionsPerRound[0][0].GetType());
            Assert.AreEqual(defender.Guid, battle.PlayerRecords[player1].ActionsPerRound[0][0].Token.Guid);
        }

        [Test]
        public void TestMaximizeDamage()
        {
            // create a new battle from two test fleets
            var battle = battleEngine.BuildBattle(Battles.GetFleetsForSimpleBattle());
            var attacker = battle.Tokens[0];
            var defender = battle.Tokens[1];
            var player1 = attacker.Fleet.Player;

            battleEngine.FindMoveTargets(battle);
            battle.BuildSortedWeaponSlots();

            var weapon = battle.SortedWeaponSlots[0];

            // put our defender one position out of range
            attacker.Position = new Vector2(0, 0);
            defender.Position = attacker.Position + Vector2.Right * (weapon.Range + 1);

            battleEngine.MaximizeDamage(battle, attacker);

            // our attacker should move towards the defender, and also move down, because it zigzags
            Assert.AreEqual(new Vector2(1, 1), attacker.Position);

            // make sure we recorded this move
            Assert.AreEqual(1, battle.PlayerRecords[player1].ActionsPerRound[0].Count);
            Assert.AreEqual(typeof(BattleRecordTokenMove), battle.PlayerRecords[player1].ActionsPerRound[0][0].GetType());
            Assert.AreEqual(attacker.Guid, battle.PlayerRecords[player1].ActionsPerRound[0][0].Token.Guid);
        }

        [Test]
        public void TestFireWeaponSlot()
        {
            // create a new battle from two test fleets
            var battle = battleEngine.BuildBattle(Battles.GetFleetsForSimpleBattle());
            var attacker = battle.Tokens[0];
            var defender = battle.Tokens[1];
            var player1 = attacker.Fleet.Player;

            battleEngine.FindMoveTargets(battle);
            battle.BuildSortedWeaponSlots();

            var weapon = battle.SortedWeaponSlots[0];
            battleEngine.FindTargets(battle, weapon);

            // put our defender one position out of range
            attacker.Position = new Vector2(0, 0);
            defender.Position = new Vector2(0, 0);

            // this weapon slot should destroy the token
            battleEngine.FireWeaponSlot(battle, weapon);

            // make sure we recorded this move
            Assert.AreEqual(1, battle.PlayerRecords[player1].ActionsPerRound[0].Count);
            Assert.AreEqual(typeof(BattleRecordTokenTorpedoFire), battle.PlayerRecords[player1].ActionsPerRound[0][0].GetType());
            Assert.AreEqual(attacker.Guid, battle.PlayerRecords[player1].ActionsPerRound[0][0].Token.Guid);
        }

        [Test]
        public void TestRunBattle1()
        {
            // create a new battle from two test fleets
            var battle = battleEngine.BuildBattle(Battles.GetFleetsForSimpleBattle());
            var attacker = battle.Tokens[0];
            var defender = battle.Tokens[1];
            var player1 = attacker.Fleet.Player;

            battleEngine.RunBattle(battle);

            // the scout moves 7 times, and then runs away (for 8 actions)
            // the defender moves 4 times trying to get to the scout
            Assert.Greater(10, battle.PlayerRecords[player1].ActionsPerRound[0].Count);
        }

        [Test]
        public void TestRunBattle2()
        {
            // create a new battle from two test fleets
            var battle = battleEngine.BuildBattle(Battles.GetFleetsForSimpleBattle());
            var attacker = battle.Tokens[0];
            var defender = battle.Tokens[1];

            // make the attacker move faster
            attacker.Token.Design.Slots[0].HullComponent = Techs.TransStar10;
            attacker.Token.Design.ComputeAggregate(attacker.Fleet.Player, true);

            battleEngine.RunBattle(battle);
        }

        [Test]
        public void TestRunBattle3()
        {
            var player1 = new Player() { Num = 0, Name = "Bob" };
            // level up our players so they will have designs
            player1.TechLevels = new TechLevel(10, 10, 10, 10, 10, 10);

            // create a second weaker player
            var player2 = new Player() { Num = 1, Name = "Ted" };
            player2.TechLevels = new TechLevel(6, 6, 6, 6, 6, 6);

            var battle = battleEngine.BuildBattle(Battles.GetFleetsForDesignsBattle(
                player1,
                player2,
                new HashSet<string>() { "Destroyer", "Space Station" },
                new HashSet<string>() { "Destroyer", "Scout", "Fuel Transport" }
            ));
            battleEngine.RunBattle(battle);
        }
    }

}