using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlayerTest
    {
        [Test]
        public void TestRelations()
        {
            var player1 = new Player() { Num = 0 };
            var player2 = new Player() { Num = 1 };
            var player3 = new Player() { Num = 2 };
            var player4 = new Player() { Num = 3 };

            player1.PlayerRelations.Add(new PlayerRelationship(PlayerRelation.Friend));
            player1.PlayerRelations.Add(new PlayerRelationship(PlayerRelation.Friend));
            player1.PlayerRelations.Add(new PlayerRelationship(PlayerRelation.Neutral));
            player1.PlayerRelations.Add(new PlayerRelationship(PlayerRelation.Enemy));

            // not enemies or neutral with self, friends
            Assert.IsTrue(player1.IsFriend(player1.Num));
            Assert.IsFalse(player1.IsEnemy(player1.Num));
            Assert.IsFalse(player1.IsNeutral(player1.Num));
            
            // friends with player2
            Assert.IsTrue(player1.IsFriend(player2.Num));

            // nuetral with player3
            Assert.IsTrue(player1.IsNeutral(player3.Num));

            // enemies with player4
            Assert.IsTrue(player1.IsEnemy(player4.Num));
        }
    }

}