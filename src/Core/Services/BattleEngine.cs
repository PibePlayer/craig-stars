using System;
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
    /// 
    /// ===================================================================
    /// TODO: 
    /// * Accuracy/Beam Defense modifications to attractiveness
    /// * Beam Bonus
    /// * Capital Ship missiles doing double damage to capital ships
    /// * Unit test FireTorpedo and FireBeamWeapon
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

        public Rules Rules { get; set; } = new Rules(0);
        public ShipDesignDiscoverer designDiscoverer = new ShipDesignDiscoverer();

        public BattleEngine(Rules rules)
        {
            Rules = rules;
        }

        /// <summary>
        /// For a list of fleets that contains more than one player, build a battle recording
        /// with all the battle tokens. We'll use this to determine if a battle should take
        /// place at this location. Also, any players that have a potential battle will discover
        /// each other's designs
        /// </summary>
        /// <param name="fleets">The fleets in this battle</param>
        /// /// <returns></returns>
        public Battle BuildBattle(List<Fleet> fleets)
        {
            if (fleets == null || fleets.Count == 0)
            {
                log.Error("Can't build battle with no fleets.");
                return null;
            }

            Battle battle = new Battle()
            {
                Planet = fleets[0].Orbiting,
                Position = fleets[0].Position
            };


            // add recordings for each player
            HashSet<Player> players = fleets.Select(fleet => fleet.Player).ToHashSet();
            foreach (var player in players)
            {
                battle.PlayerRecords[player] = new BattleRecord() { Player = player };

                // every player should discover all designs in a battle as if they were penscanned.
                foreach (var design in fleets.SelectMany(fleet => fleet.Tokens).Select(token => token.Design))
                {
                    designDiscoverer.Discover(player, design, true);
                }
            }

            // add each fleet's token to the battle
            fleets.ForEach(fleet =>
                fleet.Tokens.ForEach(token =>
                    battle.AddToken(new BattleToken()
                    {
                        Player = fleet.Player,
                        Fleet = fleet,
                        Token = token,
                        Shields = token.Quantity * token.Design.Aggregate.Shield,
                        Attributes = GetTokenAttributes(token)
                    })
                )
            );

            battle.SetupPlayerRecords(Rules.NumBattleRounds);

            return battle;
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
        /// Find all the targets for a weapon
        /// </summary>
        /// <param name="battle"></param>
        public void FindTargets(Battle battle, BattleWeaponSlot weapon)
        {
            weapon.Targets.Clear();
            var attacker = weapon.Token;
            var primaryTarget = attacker.Fleet.BattlePlan.PrimaryTarget;
            var secondaryTarget = attacker.Fleet.BattlePlan.SecondaryTarget;

            var primaryTargets = new List<BattleToken>();
            var secondaryTargets = new List<BattleToken>();

            // Find all enemy tokens
            foreach (var token in battle.RemainingTokens.Where(token => attacker.Fleet.WillAttack(token.Player)))
            {
                // if we will target this
                if (WillTarget(primaryTarget, token) && weapon.IsInRange(token))
                {
                    primaryTargets.Add(token);
                }
                else if (WillTarget(secondaryTarget, token) && weapon.IsInRange(token))
                {
                    secondaryTargets.Add(token);
                }
            };
            // our list of available targets is all primary and all secondary targets in range
            weapon.Targets.AddRange(primaryTargets.OrderBy(token => GetAttractiveness(weapon, token)));
            weapon.Targets.AddRange(secondaryTargets.OrderBy(token => GetAttractiveness(weapon, token)));
        }

        /// <summary>
        /// For a battle, allocate targets for each token
        /// </summary>
        /// <param name="battle"></param>
        public void FindMoveTargets(Battle battle)
        {
            battle.HasTargets = false;
            // ignore tokens that are no longer part of the battle
            foreach (var token in battle.RemainingTokens.Where(token => token.Token.Design.Aggregate.HasWeapons))
            {
                token.MoveTarget = GetTarget(token, battle.RemainingTokens.Where(target => target != token));
                if (token.MoveTarget != null)
                {
                    token.MoveTarget.TargetedBy.Add(token);
                }
                battle.HasTargets = token.MoveTarget != null ? true : battle.HasTargets;
            };
        }

        /// <summary>
        /// Run a battle!
        /// </summary>
        /// <param name="battle"></param>
        public void RunBattle(Battle battle)
        {
            var planet = battle.Planet;
            if (planet != null)
            {
                log.Info($"Running a battle at {battle.Planet.Name} involving {battle.PlayerRecords.Count} players and {battle.Tokens.Count} tokens.");
            }
            else
            {
                log.Info($"Running a battle at ({battle.Position.x:.##}, {battle.Position.y:.##}) involving {battle.PlayerRecords.Count} players and {battle.Tokens.Count} tokens.");
            }

            PlaceTokensOnBoard(battle);
            BuildMovementOrder(battle);
            for (battle.Round = 0; battle.Round < Rules.NumBattleRounds; battle.Round++)
            {
                battle.RecordNewRound();

                // each round we build the SortedWeaponSlots list
                // anew to account for ships that were destroyed
                battle.BuildSortedWeaponSlots();

                // find new targets
                FindMoveTargets(battle);
                if (battle.HasTargets)
                {
                    // if we still have targets, process the round

                    // movement is a repeating pattern of 4 movement blocks
                    // which we figured out in BuildMovement
                    int roundBlock = battle.Round % 4;
                    foreach (BattleToken token in battle.MoveOrder[roundBlock].Where(token => !token.Destroyed && !token.RanAway))
                    {
                        MoveToken(battle, token);
                    }


                    // iterate over each weapon and fire if they have a target
                    foreach (var weaponSlot in battle.SortedWeaponSlots)
                    {
                        // find all available targets for this weapon
                        FindTargets(battle, weaponSlot);
                        FireWeaponSlot(battle, weaponSlot);
                    }
                }
                else
                {
                    // no one has targets, we are done
                    break;
                }
            }

            // tell players about the battle
            foreach (var playerEntry in battle.PlayerRecords)
            {
                Message.Battle(playerEntry.Key, battle.Planet, battle.Position, playerEntry.Value);
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
                    battle.RecordStartingPosition(token, token.Position);
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
            var tokensByMass = battle.Tokens
                .Where(token => token.Token.Design.Aggregate.Movement > 0) // starbases don't move
                .OrderByDescending(token => token.Token.Design.Aggregate.Mass).ToList();

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
        /// <param name="weapon"></param>
        internal void FireWeaponSlot(Battle battle, BattleWeaponSlot weapon)
        {
            if (weapon.Targets.Count == 0 || weapon.Token.Destroyed || weapon.Token.RanAway)
            {
                // no targets, nothing to do
                return;
            }

            switch (weapon.WeaponType)
            {
                case BattleWeaponType.Beam:
                    FireBeamWeapon(battle, weapon);
                    break;
                case BattleWeaponType.Torpedo:
                    FireTorpedo(battle, weapon);
                    break;
            }
        }

        internal void FireBeamWeapon(Battle battle, BattleWeaponSlot weapon)
        {
            ShipToken attackerShipToken = weapon.Token.Token;
            // damage is power * number of weapons * number of attackers.
            var damage = weapon.Slot.HullComponent.Power * weapon.Slot.Quantity * attackerShipToken.Quantity;
            float remainingDamage = damage;

            log.Debug($"{weapon.Token} is firing at {weapon.Targets.Count} targets for a total of {damage} damage");

            foreach (var target in weapon.Targets)
            {
                if (target.Destroyed || target.RanAway)
                {
                    // this token isn't valid anymore, skip it
                    continue;
                }
                // no more damage to spread, break out
                if (remainingDamage == 0)
                {
                    break;
                }
                ShipToken targetShipToken = target.Token;

                // shields are shared among all tokens
                var shields = target.Shields;
                var armor = targetShipToken.Design.Aggregate.Armor;

                // beam weapons degrade 10% max over distance
                var distance = weapon.Token.GetDistanceAway(target.Position);

                // if a target of a beam weapon is 3 spaces away and the beam has a range of 4
                // we do 3/4 * 10% less damage
                var rangedDamage = (int)(remainingDamage * (1 - Rules.BeamRangeDropoff * distance / weapon.Range));

                log.Debug($"{weapon.Token} fired {weapon.Slot.Quantity} {weapon.Slot.HullComponent.Name}(s) at {target} (shields: {shields}, armor: {armor}, distance: {distance}, {targetShipToken.Quantity}@{targetShipToken.Damage} damage) for {remainingDamage} (range adjusted to {rangedDamage})");

                if (rangedDamage > shields)
                {
                    // wipe out the shields
                    remainingDamage = rangedDamage - shields;
                    target.Shields = 0;

                    // figure out how damaged the fleet currently is
                    float existingDamage = targetShipToken.Damage * targetShipToken.QuantityDamaged;
                    remainingDamage += existingDamage;

                    // figure out how many tokens were destroyed
                    int numDestroyed = (int)remainingDamage / armor;
                    if (numDestroyed >= targetShipToken.Quantity)
                    {
                        numDestroyed = targetShipToken.Quantity;
                        targetShipToken.Quantity = 0;
                        // token completely destroyed
                        remainingDamage -= armor * numDestroyed;

                        // record that we destroyed this token
                        target.Destroyed = true;
                        log.Debug($"{weapon.Token} {weapon.Slot.Quantity} {weapon.Slot.HullComponent.Name}(s) hit {target}, did {shields} shield damage and {rangedDamage - shields} armor damage damage and completely destroyed {target}");

                        battle.RecordBeamFire(weapon.Token, weapon.Token.Position, target.Position, weapon.Slot.HullSlotIndex, target, shields, rangedDamage - shields, numDestroyed);
                    }
                    else
                    {
                        if (numDestroyed > 0)
                        {
                            targetShipToken.Quantity -= (int)numDestroyed;
                        }

                        // reduce our remaining damage by however many we destroyed
                        remainingDamage -= armor * numDestroyed;

                        if (remainingDamage > 0)
                        {
                            // all remaining damage is applied equally to all ships
                            targetShipToken.Damage = remainingDamage / targetShipToken.Quantity;
                            targetShipToken.QuantityDamaged = targetShipToken.Quantity;
                            remainingDamage = 0;
                            log.Debug($"{weapon.Token} destroyed {numDestroyed} ships, leaving {target} damaged {targetShipToken.Quantity}@{targetShipToken.Damage} damage");
                        }


                        battle.RecordBeamFire(weapon.Token, weapon.Token.Position, target.Position, weapon.Slot.HullSlotIndex, target, shields, rangedDamage - shields, numDestroyed);
                    }

                }
                else
                {
                    // no ships were destroyed, deplete shields
                    target.Shields -= rangedDamage;
                    log.Debug($"{weapon.Token} firing {weapon.Slot.Quantity} {weapon.Slot.HullComponent.Name}(s) did {rangedDamage} damage to {target} shields, leaving {target.Shields} shields still operational.");
                    battle.RecordBeamFire(weapon.Token, weapon.Token.Position, target.Position, weapon.Slot.HullSlotIndex, target, rangedDamage, 0, 0);
                }

                log.Debug($"{weapon.Token} {weapon.Slot.Quantity} {weapon.Slot.HullComponent.Name}(s) has {remainingDamage} remaining dp to burn through {weapon.Targets.Count - 1} additional targets.");

                target.Damaged = true;
            }
        }

        /// <summary>
        /// Fire a torpedo slot from a ship. Torpedos are different than beam weapons
        /// A ship will fire each torpedo at it's target until the target is destroyed, then
        /// fire remaining torpedos at the next target.
        /// Each torpedo has an accuracy rating. That determines if it hits. A torpedo that
        /// misses still explodes and does 1/8th damage to shields
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="weapon"></param>
        internal void FireTorpedo(Battle battle, BattleWeaponSlot weapon)
        {
            ShipToken attackerShipToken = weapon.Token.Token;
            // damage is power * number of weapons * number of attackers.
            var damage = weapon.Slot.HullComponent.Power;
            var torpedoInaccuracyFactor = weapon.Token.Token.Design.Aggregate.TorpedoInaccuracyFactor;
            var accuracy = (100f - (100f - weapon.Slot.HullComponent.Accuracy) * torpedoInaccuracyFactor) / 100f;
            var numTorpedos = weapon.Slot.Quantity * attackerShipToken.Quantity;
            float remainingTorpedos = numTorpedos;

            log.Debug($"{weapon.Token} is firing at {weapon.Targets.Count} targets with {numTorpedos} torpedos at %{accuracy * 100f:.##} accuracy for {damage} damage each");

            int torpedoNum = 0;
            foreach (var target in weapon.Targets)
            {
                if (target.Destroyed || target.RanAway)
                {
                    // this token isn't valid anymore, skip it
                    continue;
                }

                // no more damage to spread, break out
                if (remainingTorpedos == 0)
                {
                    break;
                }
                ShipToken targetShipToken = target.Token;

                // shields are shared among all tokens
                var shields = target.Shields;
                var armor = targetShipToken.Design.Aggregate.Armor;

                int totalShieldDamage = 0;
                int totalArmorDamage = 0;
                int hits = 0;
                int misses = 0;
                int shipsDestroyed = 0;

                while (remainingTorpedos > 0 && !(target.Destroyed))
                {
                    // fire a torpedo
                    torpedoNum++;
                    remainingTorpedos--;
                    bool hit = accuracy > Rules.Random.NextDouble();

                    if (hit)
                    {
                        hits++;
                        var shieldDamage = .5f * damage;
                        var armorDamage = .5f * damage;

                        // apply up to half our damage to shields
                        // anything leftover goes to armor
                        var afterShieldsDamaged = shields - shieldDamage;
                        var actualShieldDamage = 0f;
                        if (afterShieldsDamaged < 0)
                        {
                            // We did more damage to shields than they had remaining
                            // apply the difference to armor
                            armorDamage += -afterShieldsDamaged;
                            actualShieldDamage = shieldDamage + afterShieldsDamaged;
                        }
                        else
                        {
                            actualShieldDamage = shieldDamage;
                        }
                        target.Shields -= (int)actualShieldDamage;

                        // this torpedo blew up a ship, hooray!
                        if ((armorDamage + targetShipToken.Damage / targetShipToken.QuantityDamaged) >= armor)
                        {
                            targetShipToken.Quantity--;
                            if (targetShipToken.Quantity <= 0)
                            {
                                // record that we destroyed this token
                                target.Destroyed = true;
                                log.Debug($"{weapon.Token} torpedo number {torpedoNum} hit {target}, did {actualShieldDamage} shield damage and {armorDamage} armor damage and completely destroyed {target}");

                                totalShieldDamage += (int)actualShieldDamage;
                                totalArmorDamage += (int)(armorDamage);
                                shipsDestroyed++;
                            }
                        }
                        else
                        {
                            // damage all remaining ships in this token
                            // leave at least 1dp per token. We can't destroy more than one token with a torpedo
                            // it's possible a torpedo blast into 100 ships will leave them all severely damaged
                            // but will only destroy one
                            var previousDamage = targetShipToken.Damage;
                            targetShipToken.Damage = Math.Min(armor * targetShipToken.Quantity - targetShipToken.Quantity, armorDamage + targetShipToken.Damage);
                            targetShipToken.QuantityDamaged = targetShipToken.Quantity;
                            var actualArmorDamage = (int)(targetShipToken.Damage - previousDamage);

                            log.Debug($"{weapon.Token} torpedo number {torpedoNum} hit {target}, did {actualShieldDamage} shield damage and {actualArmorDamage} armor damage leaving {targetShipToken.Quantity}@{targetShipToken.Damage} damage");

                            totalShieldDamage += (int)actualShieldDamage;
                            totalArmorDamage += (int)(targetShipToken.Damage - previousDamage);
                        }

                    }
                    else
                    {
                        misses++;
                        // damage shields by 1/8th
                        // round up, do a minimum of 1 damage
                        int shieldDamage = (int)Math.Min(1, Math.Round(Rules.TorpedoSplashDamage * damage));
                        int actualShieldDamage = shieldDamage;
                        if (shieldDamage > target.Shields)
                        {
                            actualShieldDamage = target.Shields;
                        }
                        target.Shields = Math.Max(0, target.Shields - shieldDamage);
                        log.Debug($"{weapon.Token} torpedo number {torpedoNum} missed {target}, did {shieldDamage} damage to shields leaving {target.Shields} shields");

                        totalShieldDamage += actualShieldDamage;
                    }
                }

                battle.RecordTorpedoFire(weapon.Token, weapon.Token.Position, target.Position, weapon.Slot.HullSlotIndex, target, totalShieldDamage, totalArmorDamage, shipsDestroyed, hits, misses);
            }

        }

        /// <summary>
        /// Move a token towards or away from its target
        /// TODO: figure out moving away/random
        /// </summary>
        /// <param name="token"></param>
        internal void MoveToken(Battle battle, BattleToken token)
        {
            // count this token's moves
            token.MovesMade++;
            if (token.MoveTarget == null || !token.Token.Design.Aggregate.HasWeapons)
            {
                // tokens with no weapons always run away
                RunAway(battle, token);
                return;
            }

            // we have weapons, figure out our tactic and targets
            switch (token.Fleet.BattlePlan.Tactic)
            {
                case BattleTactic.Disengage:
                    RunAway(battle, token);
                    if (token.MovesMade >= Rules.MovesToRunAway)
                    {
                        token.RanAway = true;
                        battle.RecordRunAway(token);
                    }
                    break;
                case BattleTactic.DisengageIfChallenged:
                    if (token.Damaged)
                    {
                        RunAway(battle, token);
                    }
                    else
                    {
                        MaximizeDamage(battle, token);
                    }
                    break;
                case BattleTactic.MinimizeDamageToSelf:
                case BattleTactic.MaximizeNetDamage:
                case BattleTactic.MaximizeDamageRatio:
                case BattleTactic.MaximizeDamage:
                    MaximizeDamage(battle, token);
                    break;
            }
        }

        /// <summary>
        /// Locate most attractive primary target (or secondary if no primary targets are left). If any of 
        /// your weapons are out of range of that token then keep moving to squares that are closer to it until 
        /// in range with all weapons. If using any beam weapons (as they have range dissipation) then attempt 
        /// to close to 0 range. If just using missiles or torps and in range then move randomly to a squares 
        /// still in range.
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="token"></param>
        internal void MaximizeDamage(Battle battle, BattleToken token)
        {
            if (token.MoveTarget != null)
            {
                Vector2 newPosition = token.Position;
                if (token.Position.y > token.MoveTarget.Position.y)
                {
                    newPosition.y--;
                }
                else
                {
                    newPosition.y++;
                }
                if (token.Position.x > token.MoveTarget.Position.x)
                {
                    newPosition.x--;
                }
                else
                {
                    newPosition.x++;
                }

                // we can't move off board
                newPosition = new Vector2(
                    Mathf.Clamp(newPosition.x, 0, 9),
                    Mathf.Clamp(newPosition.y, 0, 9)
                );

                // create a move record for the viewer and then move the token
                battle.RecordMove(token, token.Position, newPosition);
                token.Position = newPosition;
            }
        }

        /// <summary>
        /// Run away from weapons
        /// TODO: tokens might randomly move into range of a weapon
        /// TODO: tokens should probably weight their movement to move to a place with less powerful weapons?
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="token"></param>
        internal void RunAway(Battle battle, BattleToken token)
        {
            // if we are in range of a weapon, move away, otherwise move randomly
            var weaponsInRange = battle.SortedWeaponSlots.Where(weapon => weapon.IsInRange(token)).ToList();

            var possiblePositions = new Vector2[] {
                            new Vector2(token.Position + Vector2.Right),
                            new Vector2(token.Position + Vector2.Left),
                            new Vector2(token.Position + Vector2.Down),
                            new Vector2(token.Position + Vector2.Up),
                            new Vector2(token.Position + Vector2.Up + Vector2.Right),
                            new Vector2(token.Position + Vector2.Up + Vector2.Left),
                            new Vector2(token.Position + Vector2.Down + Vector2.Right),
                            new Vector2(token.Position + Vector2.Down + Vector2.Left),
                        }.Where(position => position.x >= 0 && position.x < 10 && position.y >= 0 && position.y < 10)
                        .ToArray();

            if (weaponsInRange.Count > 0)
            {
                // default to move to a random position
                var newPosition = possiblePositions[Rules.Random.Next(0, possiblePositions.Length)];

                // move to a position that is out of range, or to the greatest distance away we can get
                int maxNumWeaponsInRange = int.MinValue;
                foreach (var possiblePosition in possiblePositions)
                {
                    // can't move here
                    if (possiblePosition.x < 0 || possiblePosition.x > 9 || possiblePosition.y < 0 || possiblePosition.y > 9)
                    {
                        continue;
                    }
                    int numWeaponsInRange = 0;
                    foreach (var weapon in weaponsInRange)
                    {
                        int distanceAway = weapon.Token.GetDistanceAway(possiblePosition);
                        if (weapon.IsInRange(distanceAway))
                        {
                            numWeaponsInRange++;
                            if (distanceAway > maxNumWeaponsInRange)
                            {
                                maxNumWeaponsInRange = distanceAway;
                                newPosition = possiblePosition;
                            }
                        }
                    }

                    // no weapons in range of this position, move there
                    if (numWeaponsInRange == 0)
                    {
                        newPosition = possiblePosition;
                        break;
                    }
                }

                // we can't move off board (this should never be a problem)
                newPosition = new Vector2(
                    Mathf.Clamp(newPosition.x, 0, 9),
                    Mathf.Clamp(newPosition.y, 0, 9)
                );

                // move to our new position
                battle.RecordMove(token, token.Position, newPosition);
                token.Position = newPosition;
            }
            else
            {
                // move at random
                var newPosition = possiblePositions[Rules.Random.Next(0, possiblePositions.Length)];
                battle.RecordMove(token, token.Position, newPosition);
                token.Position = newPosition;
            }
        }

        /// <summary>
        /// For an attacker, 
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public BattleToken GetTarget(BattleToken attacker, IEnumerable<BattleToken> defenders)
        {
            BattleToken primaryTarget = null;
            BattleToken secondaryTarget = null;
            float primaryTargetAttractiveness = 0;
            float secondaryTargetAttractiveness = 0;
            BattleTactic tactic = attacker.Fleet.BattlePlan.Tactic;
            BattleTargetType primaryTargetOrder = attacker.Fleet.BattlePlan.PrimaryTarget;
            BattleTargetType secondaryTargetOrder = attacker.Fleet.BattlePlan.SecondaryTarget;

            // TODO: We need to account for the fact that if a fleet targets us, we will target them back
            foreach (var defender in defenders.Where(defender => !(defender.Destroyed || defender.RanAway) && attacker.Fleet.WillAttack(defender.Player)))
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

        /// <summary>
        /// Get the attractiveness of a defender token vs an attacker weapon
        /// This assumes the defender is owned by a player we want to attack
        /// TODO: work in weapon attractiveness
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        float GetAttractiveness(BattleWeaponSlot weapon, BattleToken target)
        {
            var cost = target.Token.Design.Aggregate.Cost * target.Token.Quantity;
            var defense = (target.Token.Design.Aggregate.Armor + target.Token.Design.Aggregate.Shield) * target.Token.Quantity;

            // TODO: change defense based on attacker weapons

            return (cost.Germanium + cost.Resources) / defense;
        }
    }
}