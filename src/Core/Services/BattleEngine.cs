using System.Collections.Generic;
using System.Linq;
using Godot;
using log4net;

namespace CraigStars
{

    /// <summary>
    /// From: https://wiki.starsautohost.org/wiki/Guts_of_the_Battle_Engine
    /// ===================================================================
    /// Here are the guts of the battle engine as I understand it from both experience, observation and the help file 
    /// (please pull me up on any points I get wrong)
    /// 
    /// For a battle to take place 2 or more fleets (or a fleet and a starbase) must be at the same location and at 
    /// least one of the fleets must be armed and have orders to attack ships of the others race (the type of ships 
    /// involved doesn't matter). If are race has a fleet present at a location where there is a battle, but doesn't 
    /// have orders to attack any of the other races there and none of the other races present has orders to attack it 
    /// then it will not take part in the battle (and can not benefit from potential tech gain -- actually you can benefit 
    /// from tech gain, a fact I learned from trying not to get the tech gain in a wolf/lamb tech exchange - LEit).
    /// Each ship present at the battle will form part of a token (AKA a stack), it is possible to have a token comprised 
    /// of just a single ship. Tokens are always of ships of the same design. Each ship design in each fleet will create 
    /// a token, splitting a few ships off to form a second fleet before the battle will create a second token on the 
    /// battle board.
    /// 
    /// The battle grid is made up of 10 squares by 10 squares. Each token is in a single square, there can be more 
    /// than one token in the same square.There is an limit of 256 tokens per battle event for all players involved, 
    /// if this limit is exceeded, then excess tokens will be left out (those created from fleets with the highest 
    /// fleet numbers), in such a case each player will have an equal number of tokens, each player will be guaranteed 
    /// to get their "share" of the available token slots (ie in a 4 race battle 256 / 4 = 64 token slots), if a race
    /// doesn't use up all their "slots" then they are shared equally between the other players.
    /// 
    /// Each battle is made up of rounds. There are a maximum of 16 rounds in each battle. Each round has two parts, 
    /// movement and shooting. Each token has a speed rating, and will be able to move between 0 and 3 squares in a 
    /// single turn. If a token has a fractional speed rating then they will get a bonus square of movement every set 
    /// number of turns. a 1/4 bonus means an extra square of movement on the first round and then on every fourth round 
    /// after that starting with the fifth. A 1/2 speed bonus gets a bonus square of movement every other turn starting 
    /// with the first, and a 3/4 speed bonus gets a bonus square of movement for the first three rounds of every 4 round 
    /// cycle. The order of movement is this, each token with 3 movement squares moves a single square, then each token 
    /// with 2+ movement moves a single square (if it had speed 3 then it would move for its second square) and then all 
    /// ships with at least one square of movement move again. At each stage the ships with the most weight will move first 
    /// though there is less than a 15% difference in weight then there is a chance that the lighter ship will go first.
    /// The smaller the weight % difference the greater the chance of the lighter ship going first.
    /// 
    /// Each token has an attractiveness rating. This is used in both working out where ships move to and which ships are 
    /// shot at first. The essence of the formula is cost / defence. A ship will have different attractiveness ratings 
    /// verses different types of weapons (beams, sappers, torpedoes and capital missiles). Cost is calculated by summing 
    /// the resource and boranium costs of the ship design used (iron and germ costs don't affect the attractiveness rating). 
    /// Defence is calculated by the shield and armour dp modified by the enemies torpedo accuracy (after base accuracy, 
    /// comps and jammers are worked out) if defending vs torps or capital missiles, the effects of double damage for unshielded 
    /// targets vs capital missiles and the effects of deflectors against beam weapons. The attractiveness rating can be 
    /// change during the course of the battle as shields and armour deplete. Attractiveness doesn't take into account the 
    /// one missile one kill rule, thus chaff has become a fairly effective tactic.
    /// 
    /// Battle orders are comprised of 4 parts. A primary and secondary target type, legitimate races to attack and the tactic 
    /// to use in battle. Ships will only attack tokens belonging to legitimate target races, however if another race present 
    /// has any ships (including unarmed ships) with battles orders to attack your race then that race will also be considered 
    /// a legitimate target. When attacking ships will try and shoot the most attractive ship of a type listed as a primary 
    /// target and if no ships are available which are primary targets then the most attractive ship of a type listed as a 
    /// secondary target will be targeted. Ships which are not listed as primary or secondary targets will not get shot at, 
    /// even if they are shooting back.
    /// 
    /// There are 6 different battle orders which determine the movement AI of the ships in battle, the movement AI is applied 
    /// each time a ship wants to move a square on the battle board.:
    /// * Disengage - If there is any enemy ship in firing range then move to any square further away than your current square. 
    ///               If you are in range of an enemy weapon but cannot move further away then try move to a square that is of 
    ///               the same distance away. If you are in range of the enemies weapons and cannot move away or maintain distance 
    ///               then move to a random square. If you are not in range of the enemies weapons then move randomly. Also you 
    ///               will try and disengage which will require 7 squares of movement to be clocked up before you can leave from 
    ///               the battle board.
    /// * Disengage if Challenged - Behaves like Maximise Damage until token takes damage and then behaves like Disengage.
    /// * Minimise Damage to Self - (Not 100% sure on this one) If within range of an enemy weapon then move away from the 
    ///                             enemy (just like Disengage). If out of range of the enemies weapons or cannot move away from 
    ///                             the enemy then try and get in range of the best available target without moving towards the enemy.
    /// * Maximise Net Damage - Locate most attractive primary target (or secondary if no primary targets are left). If out 
    ///                         of range with ANY weapon then move towards target. If in range with all weapons them move as to 
    ///                         maximise damage_done/damage_taken. The effect of this is if your weapons are longer range then 
    ///                         try to stay at maximum range. If your weapons range is the same then do random movement while 
    ///                         staying in range. If your weapons are shorter range and also beam weapons then attempt to close 
    ///                         in to zero range.
    /// * Maximise Damage Ratio - As Maximise Net Damage but only considers the longest range weapon.
    /// * Maximise Damage - Locate most attractive primary target (or secondary if no primary targets are left). If any of 
    ///                     your weapons are out of range of that token then keep moving to squares that are closer to it until 
    ///                     in range with all weapons. If using any beam weapons (as they have range dissipation) then attempt 
    ///                     to close to 0 range. If just using missiles or torps and in range then move randomly to a squares 
    ///                     still in range.
    ///
    /// Note that there is a bug when fighting starbases, the battle AI doesn't count the +1 range bonus when calculating movement. 
    /// This mainly applies when your ships are attempting to get out of range of the enemy, so vs starbase with range 6 missiles, 
    /// your ships will move to distance 7, the movement AI won't calculate that they are still in range even when they keep getting
    /// shot at.
    /// After the movement phase all ships will shoot their weapons, a token will fire all weapons from the same slot in a single 
    /// shot. The weapon slot with the highest initiative will fire first. If there are two ships with slots of the same init, 
    /// then the ships will be randomly given a priority over who can fire first (which will stick for the entire battle). The 
    /// rest of the weapon slots are then fired in init order. Damage is worked out in between each shot and applied to the ships.
    /// If ships or tokens are destroyed before their turn to shoot then they won't be able to fire back. The movement AI will
    /// go after the most attractive primary target on the board, but if this token is not in range, then the ship will fire on 
    /// the most attractive primary target within range (or secondary if none available). Starbases have a +1 range bonus to all 
    /// their weapons (this also gets applied to minefield sweeping rates), though cannot move. The movement AI doesn't take this
    /// bonus into account when moving ships to close in on an enemy starbase.
    /// 
    /// Damage for each shot is calculated by multiplying the number of weapons in the slot by the number of ships in the token
    /// by the amount of dp the weapon does. For beam weapons, this damage will dissipate by 10% over the range of the beam
    /// (for a range 2 beam - no dissipation at range 0, 5% dissipation at range 1 and 10% dissipation at range 2). Also 
    /// capacitors and deflectors will modify the damage actually done to the enemy ship. Damage will be applied first to the 
    /// tokens shield stack and then to armour only when the entire shield stack of the token is down. For missile ships, 
    /// each missile fired will be tested to see if it will hit, the chance to hit is based on the base accuracy, the computers 
    /// on the ship and the enemy jammers. Missiles that miss will do 1/8 of their damage to the shields and won't affect armour.
    /// For missiles that hit, upto half will be taken by the shields, the rest will go to the armour. For capital missiles
    /// any damage done after the shields are taken down will do double damage to the armour. Whole ship kills are worked out 
    /// by adding up all the damage done to the armour by a single salvo (from a token's slot) and dividing this by the amount 
    /// of armour each single ship in the token has left (total armour x token damage %). The number of complete ships the shot
    /// could kill will be removed from the enemy token, the rest of the damage will divided equally among the rest of the ships 
    /// in the token and applied as damage. As token armour is stored in 1/512ths (about 0.2%s) of total armour and not as an 
    /// exact dp figure (shields are stored as an exact figure), there may be some rounding of the damage after each salvo 
    /// (AFAIK its always rounds up). This fact can be abused by creating lots of small fleet tokens with weak missiles 
    /// and many slots, where each slot that hits will do 0.2% damage to the enemy token even if each individual missile 
    /// would do less damage normally (especially the case with a beta torp shooting a large nub stack).
    /// 
    /// After all the weapons that are in range have fired, the next round begins, starting with ship movement.
    /// The battle is ended when either the 16 round timer runs out, there is only one race left present on the battle board 
    /// or if there are two or more races which have no hostile intentions towards each other.
    /// 
    /// After a battle, salvage is created. This is equal to 1/3 of the current mineral costs of all the ships that where 
    /// destroyed during the battle. This is left at the location of the battle and will decay over time, or if the battle 
    /// happened over a planet, then the minerals will get deposited there.
    /// 
    /// Any races that took part in a battle and had at least one ship that managed to survive (either through surviving 
    /// till the end or retreating beforehand) has a potential to gain tech levels from ships that where destroyed during
    /// the battle. For the exact details of the formulas and chances involved see the Guts of Tech Trading.
    /// 
    /// Movement speed and moves per round.
    /// 3/4 is      1110
    /// 1 is        1111
    /// 1 1/4 is    2111
    /// 1 1/2 is    2121
    /// 1 3/4 is    2212
    /// 2 is        2222
    /// 2 1/4 is    3222
    /// 2 1/2 is    3232
    /// </summary>
    public class BattleEngine
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BattleEngine));

        // TODO: make this algorithmic
        readonly Vector2[] PositionsByPlayer = new Vector2[] {
            new Vector2(1, 4),
            new Vector2(8, 5),
            new Vector2(4, 1),
            new Vector2(5, 8),
            new Vector2(1, 1),
            new Vector2(8, 1),
            new Vector2(8, 8),
            new Vector2(1, 8),
        };

        static readonly int[,] MovementByRound = new int[,]
        {
            {1, 0, 1, 0},
            {1, 1, 0, 1},
            {1, 1, 1, 1},
            {2, 1, 1, 1},
            {2, 1, 2, 1},
            {2, 2, 1, 2},
            {2, 2, 2, 2},
            {3, 2, 2, 2},
            {3, 2, 3, 2},
        };

        public Rules Rules { get; set; } = new Rules();

        /// <summary>
        /// For a list of fleets that contains more than one player, build a battle recording
        /// with all the battle tokens. We'll use this to determine if 
        /// </summary>
        /// <param name="fleets"></param>
        /// <returns></returns>
        public Battle BuildBattle(List<Fleet> fleets)
        {
            Battle recording = new Battle();

            // add each fleet's token to the battle
            fleets.ForEach(fleet =>
                fleet.Tokens.ForEach(token =>
                    recording.Tokens.Add(new BattleToken()
                    {
                        Player = fleet.Player,
                        Fleet = fleet,
                        Token = token,
                        Attributes = GetTokenAttributes(token)
                    })
                )
            );

            return recording;
        }

        /// <summary>
        /// Determine what attributes this token has, i.e. is it armed? a  starbase?
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public BattleTokenAttribute GetTokenAttributes(ShipToken token)
        {
            BattleTokenAttribute attributes = BattleTokenAttribute.Unarmed;
            var hullType = token.Design.Hull.Type;

            if (hullType == TechHullType.Starbase)
            {
                attributes |= BattleTokenAttribute.Starbase;
            }

            if (token.Design.Aggregate.HasWeapons)
            {
                attributes |= BattleTokenAttribute.Armed;
            }

            if (hullType == TechHullType.Freighter)
            {
                attributes |= BattleTokenAttribute.Freighter;
            }

            if (hullType == TechHullType.FuelTransport)
            {
                attributes |= BattleTokenAttribute.FuelTransport;
            }

            if (hullType == TechHullType.Bomber)
            {
                attributes |= BattleTokenAttribute.Bomber;
            }

            return attributes;
        }

        /// <summary>
        /// For a battle, allocate targets for each token
        /// </summary>
        /// <param name="battle"></param>
        public void FindTargets(Battle battle)
        {
            battle.HasTargets = false;
            battle.Tokens.ForEach(token =>
            {
                token.Target = GetTarget(token, battle.Tokens);
                if (token.Target != null)
                {
                    token.Target.TargetedBy.Add(token);
                }
                battle.HasTargets = token.Target != null ? true : battle.HasTargets;
            });

        }

        /// <summary>
        /// Run a battle!
        /// </summary>
        /// <param name="battle"></param>
        public void RunBattle(Battle battle)
        {
            PlaceTokensOnBoard(battle);
            BuildMovementOrder(battle);
            for (int round = 0; round < Rules.BattleRounds; round++)
            {
                FindTargets(battle);
                if (battle.HasTargets)
                {
                    // if we still have targets, process the round

                    // movement is a repeating pattern of 4 movement blocks
                    // which we figured out in BuildMovement
                    int roundBlock = round % 4;
                    foreach (BattleToken token in battle.MoveOrder[roundBlock])
                    {
                        MoveToken(token);
                    }

                    // iterate over 
                    foreach (var weaponSlot in battle.SortedWeaponSlots)
                    {
                        if (weaponSlot.IsInRange())
                        {
                            FireWeaponSlot(weaponSlot);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Place tokens on the board in their starting locations
        /// </summary>
        /// <param name="battle"></param>
        internal void PlaceTokensOnBoard(Battle battle)
        {
            var tokensByPlayer = battle.Tokens.ToLookup(token => token.Player).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray());
            int playerIndex = 0;
            foreach (var entry in tokensByPlayer)
            {
                foreach (var token in entry.Value)
                {
                    token.Position = PositionsByPlayer[playerIndex];
                }
                playerIndex++;
                if (playerIndex >= PositionsByPlayer.Length)
                {
                    log.Warn($"Oh noes! We have a battle with more players than we have positions for... {battle.Guid}");
                    playerIndex = 0;
                }
            }
        }

        /// <summary>
        /// Build a list of Movers in this battle
        /// Each ship moves in order of mass with heavier ships moving first.
        /// Ships that can move 3 times in a round move first, then ships that move 2 times, then 1
        /// </summary>
        /// <param name="battle"></param>
        internal void BuildMovementOrder(Battle battle)
        {
            // our tokens are moved by mass
            var tokensByMass = battle.Tokens.OrderByDescending(token => token.Token.Design.Aggregate.Mass).ToList();

            // each token can move up to 3 times in a round
            // ships that can move 3 times go first, so we loop through the moveNum backwards
            // so that our Movers list has ships that move 3 times first
            for (int moveNum = 2; moveNum >= 0; moveNum--)
            {
                // for each block of 4 rounds, add each ship to the movement list if it's supposed to move that round
                for (int roundBlock = 0; roundBlock < 4; roundBlock++)
                {
                    // add each battle token to the movement for this roundBlock
                    foreach (BattleToken token in tokensByMass)
                    {
                        // movement is between 2 and 10, so we offset it to fit in our MovementByRound table
                        var movement = token.Token.Design.Aggregate.Movement;

                        // see if this token can move on this moveNum (i.e. move 1, 2, or 3)
                        if (MovementByRound[movement - 2, roundBlock] > moveNum)
                        {
                            battle.MoveOrder[roundBlock].Add(token);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Fire the weapon slot towards its target
        /// </summary>
        /// <param name="weaponSlot"></param>
        internal void FireWeaponSlot(BattleWeaponSlot weaponSlot)
        {

        }

        /// <summary>
        /// Move a token towards or away from its target
        /// TODO: figure out moving away/random
        /// </summary>
        /// <param name="token"></param>
        internal void MoveToken(BattleToken token)
        {
            switch (token.Fleet.BattleOrders.Tactic)
            {
                case BattleTactic.Disengage:

                    break;
                case BattleTactic.DisengageIfChallenged:
                case BattleTactic.MinimizeDamageToSelf:
                case BattleTactic.MaximizeNetDamage:
                case BattleTactic.MaximizeDamageRatio:
                case BattleTactic.MaximizeDamage:
                    if (token.Target != null)
                    {
                        var dist = token.Position - token.Target.Position;
                        if (dist.x >= dist.y)
                        {
                            // move on x

                        }
                        else
                        {
                            // move on y
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// For an attacker, 
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public BattleToken GetTarget(BattleToken attacker, List<BattleToken> defenders)
        {
            BattleToken primaryTarget = null;
            BattleToken secondaryTarget = null;
            float primaryTargetAttractiveness = 0;
            float secondaryTargetAttractiveness = 0;
            BattleTactic tactic = attacker.Fleet.BattleOrders.Tactic;
            BattleTargetType primaryTargetOrder = attacker.Fleet.BattleOrders.PrimaryTarget;
            BattleTargetType secondaryTargetOrder = attacker.Fleet.BattleOrders.SecondaryTarget;

            // TODO: We need to account for the fact that if a fleet targets us, we will target them back
            foreach (var defender in defenders.Where(defender => attacker.Fleet.WillAttack(defender.Player)))
            {
                // if we would target this defender with our primary target and it's more attactive than our current primaryTarget, pick it
                if (WillTarget(primaryTargetOrder, defender))
                {
                    var attractiveness = GetAttractiveness(attacker, defender);
                    if (attractiveness >= primaryTargetAttractiveness)
                    {
                        primaryTarget = defender;
                        primaryTargetAttractiveness = attractiveness;
                    }
                }

                // if we would target this defender with our secondary target, pick it
                if (WillTarget(secondaryTargetOrder, defender))
                {
                    var attractiveness = GetAttractiveness(attacker, defender);
                    if (attractiveness >= secondaryTargetAttractiveness)
                    {
                        secondaryTarget = defender;
                        secondaryTargetAttractiveness = attractiveness;
                    }
                }
            }

            return primaryTarget != null ? primaryTarget : secondaryTarget;
        }

        /// <summary>
        /// Returns true if the BattleOrder Target type would target this token
        /// </summary>
        /// <param name="target">The target a fleet's battle order is using for primary or secondary</param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal bool WillTarget(BattleTargetType target, BattleToken token)
        {
            switch (target)
            {
                case BattleTargetType.Any:
                    return true;
                case BattleTargetType.None:
                    return false;
                case BattleTargetType.Starbase:
                    return (token.Attributes & BattleTokenAttribute.Starbase) > 0;
                case BattleTargetType.ArmedShips:
                    return (token.Attributes & BattleTokenAttribute.Armed) > 0;
                case BattleTargetType.BombersFreighters:
                    return (token.Attributes & BattleTokenAttribute.Bomber) > 0 || (token.Attributes & BattleTokenAttribute.Freighter) > 0;
                case BattleTargetType.UnarmedShips:
                    return (token.Attributes & BattleTokenAttribute.Armed) == 0;
                case BattleTargetType.FuelTransports:
                    return (token.Attributes & BattleTokenAttribute.FuelTransport) > 0;
                case BattleTargetType.Freighters:
                    return (token.Attributes & BattleTokenAttribute.Freighter) > 0;
            }

            return false;
        }
        /// <summary>
        /// Get the attractiveness of a defender token vs an attacker token
        /// This assumes the defender is owned by a player we want to attack
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        float GetAttractiveness(BattleToken attacker, BattleToken defender)
        {
            var cost = defender.Token.Design.Aggregate.Cost * defender.Token.Quantity;
            var defense = (defender.Token.Design.Aggregate.Armor + defender.Token.Design.Aggregate.Shield) * defender.Token.Quantity;

            // TODO: change defense based on attacker weapons

            return (cost.Germanium + cost.Resources) / defense;
        }

    }
}