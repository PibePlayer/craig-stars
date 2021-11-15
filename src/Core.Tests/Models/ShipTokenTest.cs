using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class ShipTokenTest
    {
        FleetAggregator fleetAggregator = TestUtils.TestContainer.GetInstance<FleetAggregator>();

        [Test]
        public void TestApplyMineDamage()
        {
            var player = new Player();
            ShipToken token = new ShipToken(ShipDesigns.LongRangeScount, 1);
            token.Design.PlayerNum = player.Num;

            var mass = 100;
            var armor = 100;
            token.Design.Aggregate.Mass = mass;
            token.Design.Aggregate.Armor = armor;

            // we take 50 damage to 1 token
            Assert.AreEqual(new TokenDamage(50, 0), token.ApplyMineDamage(50));
            Assert.AreEqual(1, token.Quantity);
            Assert.AreEqual(1, token.QuantityDamaged);
            Assert.AreEqual(50, token.Damage);

            // we have 1 token out of 2 damaged at 50
            // if we take 75 more damage we should lose one token and have one token left with 25 dp
            token.Quantity = 2;
            token.Damage = 50;
            token.QuantityDamaged = 1;
            Assert.AreEqual(new TokenDamage(75, 1), token.ApplyMineDamage(75));
            Assert.AreEqual(1, token.Quantity);
            Assert.AreEqual(1, token.QuantityDamaged);
            Assert.AreEqual(25, token.Damage);
        }

        [Test]
        public void TestApplyMineDamageShields()
        {
            var player = new Player();
            ShipToken token = new ShipToken(ShipDesigns.LongRangeScount, 1);
            token.Design.PlayerNum = player.Num;

            var mass = 100;
            var armor = 150;
            var shield = 50;
            token.Design.Aggregate.Mass = mass;
            token.Design.Aggregate.Armor = armor;
            token.Design.Aggregate.Shield = shield;

            // we take 50 damage to 1 token
            // we should end up with 25 damage to the token because the shields absorb half
            Assert.AreEqual(new TokenDamage(25, 0), token.ApplyMineDamage(50));
            Assert.AreEqual(1, token.Quantity);
            Assert.AreEqual(1, token.QuantityDamaged);
            Assert.AreEqual(25, token.Damage);

            // if we take 150 mine damage, our shields absorb 50 and our token 
            // takes the rest
            token.Damage = 0;
            token.QuantityDamaged = 0;
            Assert.AreEqual(new TokenDamage(100, 0), token.ApplyMineDamage(150));
            Assert.AreEqual(1, token.Quantity);
            Assert.AreEqual(1, token.QuantityDamaged);
            Assert.AreEqual(100, token.Damage);
        }

        [Test]
        public void TestApplyOvergateRangeDamage()
        {
            var player = new Player();
            ShipToken token = new ShipToken(ShipDesigns.LongRangeScount, 1);
            token.Design.PlayerNum = player.Num;

            var mass = 100;
            var armor = 100;
            token.Design.Aggregate.Mass = mass;
            token.Design.Aggregate.Armor = armor;

            Assert.AreEqual(TokenDamage.None, token.ApplyOvergateDamage(dist: 100, safeRange: 100, safeSourceMass: mass, safeDestMass: mass));

            // 100% damage is 4x over safe range
            // so 2x over safe range (200 ly extra) should be 50% damage
            Assert.AreEqual(new TokenDamage(50), token.ApplyOvergateDamage(dist: 300, safeRange: 100, safeSourceMass: mass, safeDestMass: mass));

            // 100% damage is 4x over safe range (maxes out at 98%)
            token.Damage = 0;
            token.QuantityDamaged = 0;
            Assert.AreEqual(new TokenDamage(98, 0), token.ApplyOvergateDamage(dist: 500, safeRange: 100, safeSourceMass: mass, safeDestMass: mass));
        }

        [Test]
        public void TestApplyOvergateMassDamage()
        {
            var player = new Player();
            ShipToken token = new ShipToken(ShipDesigns.LongRangeScount, 1);
            token.Design.PlayerNum = player.Num;

            // set the mass and armor
            var mass = 200;
            var armor = 100;
            token.Design.Aggregate.Mass = mass;
            token.Design.Aggregate.Armor = armor;

            // 1/4 damage for doubling allowed mass
            // i.e. sending a 200kT ship through a 100kT gate source gate with infinite dest gate
            Assert.AreEqual(new TokenDamage(armor / 4), token.ApplyOvergateDamage(
                dist: 100,
                safeRange: 100,
                safeSourceMass: 100,
                safeDestMass: TechHullComponent.InfinteGate)
            );

            token.Damage = token.QuantityDamaged = 0;

            // 1/4 damage on each side for sending a ship through two gates with double mass limits
            // i.e. sending a 200kT ship through a 100kT gate source and dest gate
            Assert.AreEqual(new TokenDamage((int)(Math.Round(armor * (1 - .75 * .75)))), token.ApplyOvergateDamage(
                dist: 100,
                safeRange: 100,
                safeSourceMass: 100,
                safeDestMass: 100)
            );

        }

        [Test]
        public void TestApplyOvergateBothDamage()
        {
            var player = new Player();
            ShipToken token = new ShipToken(ShipDesigns.LongRangeScount, 1);
            token.Design.PlayerNum = player.Num;

            // set the mass and armor
            var mass = 200;
            var armor = 100;
            token.Design.Aggregate.Mass = mass;
            token.Design.Aggregate.Armor = armor;

            // 1/4 damage for doubling allowed mass
            // 50% damage for range
            // i.e. sending a 200kT ship through a 100kT gate source gate with infinite dest gate
            Assert.AreEqual(new TokenDamage(44), token.ApplyOvergateDamage(
                dist: 200,
                safeRange: 100,
                safeSourceMass: 100,
                safeDestMass: TechHullComponent.InfinteGate)
            );

        }

        [Test]
        public void TestApplyOvergateWithExistingDamage()
        {
            var player = new Player();
            ShipToken token = new ShipToken(ShipDesigns.LongRangeScount, 2);
            token.Design.PlayerNum = player.Num;
            fleetAggregator.ComputeDesignAggregate(player, token.Design);

            var mass = 100;
            var armor = 100;
            token.Design.Aggregate.Mass = mass;
            token.Design.Aggregate.Armor = armor;

            // one token at half damage
            token.Damage = 50;
            token.QuantityDamaged = 1;

            // going over range by 2x should give 100 total damage, destroying the damaged token
            // and leaving one behind with 50 damage
            Assert.AreEqual(new TokenDamage(100, 1), token.ApplyOvergateDamage(dist: 300, safeRange: 100, safeSourceMass: mass, safeDestMass: mass));
            Assert.AreEqual(1, token.Quantity);
            Assert.AreEqual(1, token.QuantityDamaged);
            Assert.AreEqual(50, token.Damage);
        }

        [Test]
        public void TestGetStargateMassVanishingChance()
        {
            var player = new Player();
            ShipToken token = new ShipToken(ShipDesigns.LongRangeScount, 1);
            token.Design.PlayerNum = player.Num;

            // set the mass and armor
            token.Design.Aggregate.Mass = 200;

            // no vanishing chance
            Assert.AreEqual(0, token.GetStargateMassVanishingChance(
                safeSourceMass: 200)
            );

            // no vanishing chance for 318kT on a 300/500 gate, due to rounding
            token.Design.Aggregate.Mass = 318;
            Assert.AreEqual(0, token.GetStargateMassVanishingChance(
                safeSourceMass: 300)
            );

            // 200kT ship in a 100kt gate has a 14% chance of vanishing
            token.Design.Aggregate.Mass = 600;
            Assert.AreEqual(.14, token.GetStargateMassVanishingChance(
                safeSourceMass: 300),
            .01);

        }

        [Test]
        public void TestGetStargateRangeVanishingChance()
        {
            var player = new Player();
            ShipToken token = new ShipToken(ShipDesigns.LongRangeScount, 1);
            token.Design.PlayerNum = player.Num;

            // no vanishing chance
            Assert.AreEqual(0, token.GetStargateRangeVanishingChance(
                dist: 100,
                safeRange: 100)
            );

            // 20% vanishing chance for 60% damage
            Assert.AreEqual(.2, token.GetStargateRangeVanishingChance(
                dist: 340,
                safeRange: 100)
            );

        }
    }

}