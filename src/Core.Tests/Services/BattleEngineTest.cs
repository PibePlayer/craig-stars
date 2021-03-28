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
        BattleEngine battleEngine = new BattleEngine();

        /// <summary>
        /// Build a simple battle between two players, one with a Stalwart Defender, one with a Long Range Scout
        /// </summary>
        /// <returns></returns>
        List<Fleet> GetFleetsForBattle()
        {
            var player1 = new Player()
            {
                Num = 0,
                Name = "Bob"
            };
            var player2 = new Player()
            {
                Num = 1,
                Name = "Ted"
            };

            var design1 = ShipDesigns.StalwartDefender.Clone();
            design1.Player = player1;
            player1.Designs.Add(design1);
            player1.Fleets.Add(new Fleet()
            {
                Player = player1,
                Name = "Attacker",
                Tokens = new List<ShipToken>() {
                        new ShipToken(design1, 1)
                    },
                BattleOrders = new BattleOrders()
                {
                    Tactic = BattleTactic.MaximizeDamageRatio,
                    PrimaryTarget = BattleTargetType.ArmedShips,
                    SecondaryTarget = BattleTargetType.UnarmedShips,
                    AttackWho = BattleAttackWho.Enemies,
                }
            });

            var design2 = ShipDesigns.LongRangeScount.Clone();
            design2.Player = player2;
            player2.Designs.Add(design2);
            player2.Fleets.Add(new Fleet()
            {
                Player = player2,
                Name = "Defender",
                Tokens = new List<ShipToken>() {
                        new ShipToken(design2, 1)
                    },
                BattleOrders = new BattleOrders()
                {
                    Tactic = BattleTactic.Disengage,
                    PrimaryTarget = BattleTargetType.None,
                    SecondaryTarget = BattleTargetType.None,
                    AttackWho = BattleAttackWho.Enemies,
                }
            });


            var fleets = new List<Fleet>()
            {
                player1.Fleets[0],
                player2.Fleets[0]
            };

            player1.ComputeAggregates();
            player2.ComputeAggregates();

            return fleets;
        }

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
            var battle = battleEngine.BuildBattle(GetFleetsForBattle());

            // find some targets!
            battleEngine.FindTargets(battle);

            Assert.IsTrue(battle.HasTargets);
            Assert.AreEqual(2, battle.Tokens.Count);
            var token1 = battle.Tokens[0];
            var token2 = battle.Tokens[1];
            Assert.AreEqual(token2, token1.Target);
            Assert.IsNull(token2.Target);
        }

        [Test]
        public void TestPlaceTokensOnBoard()
        {
            // create a new battle from two test fleets
            var battle = battleEngine.BuildBattle(GetFleetsForBattle());

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
            var battle = battleEngine.BuildBattle(GetFleetsForBattle());
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
            var battle = battleEngine.BuildBattle(GetFleetsForBattle());
            var attacker = battle.Tokens[0];
            var defender = battle.Tokens[1];

            battle.BuildSortedWeaponSlots();

            var weapon = battle.SortedWeaponSlots[0];

            attacker.Position = new Vector2(0, 0);
            defender.Position = attacker.Position + Vector2.Right * weapon.Range;

            battleEngine.RunAway(battle, defender);

            // we should move one space to the right to get away
            Assert.AreEqual(attacker.Position + (Vector2.Right * weapon.Range) + Vector2.Right, defender.Position);

            // make sure we recorded this move
            Assert.AreEqual(1, battle.Actions.Count);
            Assert.AreEqual(typeof(BattleRecordTokenMove), battle.Actions[0].GetType());
            Assert.AreEqual(defender, battle.Actions[0].Token);
        }

        [Test]
        public void TestMaximizeDamage()
        {
            // create a new battle from two test fleets
            var battle = battleEngine.BuildBattle(GetFleetsForBattle());
            var attacker = battle.Tokens[0];
            var defender = battle.Tokens[1];

            battleEngine.FindTargets(battle);
            battle.BuildSortedWeaponSlots();

            var weapon = battle.SortedWeaponSlots[0];

            // put our defender one position out of range
            attacker.Position = new Vector2(0, 0);
            defender.Position = attacker.Position + Vector2.Right * (weapon.Range + 1);

            battleEngine.MaximizeDamage(battle, attacker);

            // our attacker should move towards the defender, and also move down, because it zigzags
            Assert.AreEqual(new Vector2(1, 1), attacker.Position);

            // make sure we recorded this move
            Assert.AreEqual(1, battle.Actions.Count);
            Assert.AreEqual(typeof(BattleRecordTokenMove), battle.Actions[0].GetType());
            Assert.AreEqual(attacker, battle.Actions[0].Token);
        }

        [Test]
        public void TestFireWeaponSlot()
        {
            // create a new battle from two test fleets
            var battle = battleEngine.BuildBattle(GetFleetsForBattle());
            var attacker = battle.Tokens[0];
            var defender = battle.Tokens[1];

            battleEngine.FindTargets(battle);
            battle.BuildSortedWeaponSlots();

            var weapon = battle.SortedWeaponSlots[0];

            // put our defender one position out of range
            attacker.Position = new Vector2(0, 0);
            defender.Position = new Vector2(0, 0);

            // this weapon slot should destroy the token
            battleEngine.FireWeaponSlot(battle, weapon);

            // make sure we recorded this move
            Assert.AreEqual(2, battle.Actions.Count);
            Assert.AreEqual(typeof(BattleRecordTokenFire), battle.Actions[0].GetType());
            Assert.AreEqual(attacker, battle.Actions[0].Token);
            Assert.AreEqual(typeof(BattleRecordTokenDestroyed), battle.Actions[1].GetType());
            Assert.AreEqual(defender, battle.Actions[1].Token);
        }
    }

}