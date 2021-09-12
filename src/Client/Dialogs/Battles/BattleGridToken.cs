using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class BattleGridToken : TextureRect
    {
        public BattleRecordToken Token
        {
            get => token;
            set
            {
                token = value;
                ResetTokenState();
                UpdateControls();
            }
        }
        BattleRecordToken token;

        public BattleGridToken Target
        {
            get => target;
            set
            {
                target = value;
                if (target != null && FiringBeam)
                {
                    // remove the target endpoint
                    laserLine2D1.Points = new Vector2[] {
                        Vector2.Zero,
                        target.RectGlobalPosition - laserLine2D1.GlobalPosition + target.RectSize / 2
                    };
                    laserLine2D2.Points = new Vector2[] {
                        Vector2.Zero,
                        target.RectGlobalPosition - laserLine2D2.GlobalPosition + target.RectSize / 2
                    };
                }
                else
                {
                    // remove the target endpoint
                    laserLine2D1.Points = new Vector2[] {
                        Vector2.Zero
                    };
                    laserLine2D2.Points = new Vector2[] {
                        Vector2.Zero
                    };
                }
            }
        }
        BattleGridToken target;


        public int Quantity { get; set; }
        public int Shields { get; set; }
        public int Armor { get; set; }
        public float DamageShields { get; set; }
        public float DamageArmor { get; set; }
        public int QuantityDamaged { get; set; }
        public bool Destroyed { get; set; }
        public bool RanAway { get; set; }
        public bool FiringBeam { get; set; }
        public bool FiringTorpedo { get; set; }
        public bool TakingDamage { get; set; }
        public bool Damaged { get; set; }

        Line2D laserLine2D1;
        Line2D laserLine2D2;

        public override void _Ready()
        {
            laserLine2D1 = GetNode<Line2D>("LaserLine2D1");
            laserLine2D2 = GetNode<Line2D>("LaserLine2D2");
        }

        /// <summary>
        /// Reset the token state, like quantity, armor, shields, etc.
        /// </summary>
        public void ResetTokenState()
        {
            if (token != null)
            {
                Quantity = token.Token.Quantity;
                Armor = token.Token.Design.Armor;
                Shields = token.Token.Design.Shields;
                DamageArmor = token.Token.Damage;
                QuantityDamaged = token.Token.QuantityDamaged;
            }
        }

        void UpdateControls()
        {
            if (Token != null)
            {
                Texture = TextureLoader.Instance.FindTexture(Token.Token.Design);
            }
            else
            {
                Texture = TextureLoader.Instance.FindTexture(Token.Token.Design);
            }
        }
    }
}