using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace ItemMagnetPro
{
    internal class MagItem : GlobalItem
    {
        public Player Target;
        public int ReserveCD;
        public static int MaxSpeed = 36;
        public static int MinSpeed = 18;
        public override bool InstancePerEntity => true;

        private Vector2 CalcPosChange(Item item)
        {
            if (!Target.TryGetModPlayer(out MagPlayer mp))
            {
                return Vector2.Zero;
            }
            if (mp.Approach == Approach.Teleport)
            {
                return Target.Center - item.position;
            }
            float speed = MathHelper.Lerp(MinSpeed, MaxSpeed, Target.Center.DistanceSQ(item.position) * (1f / MagPlayer.RangeSQ));
            Vector2 dir = (Target.Center - item.position).SafeNormalize(Vector2.Zero);
            return dir * speed;
        }

        public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
        {
            ReserveCD--;
            if (Target != null)
            {
                ReserveCD = 0;
                gravity = 0;
                maxFallSpeed = 0;
                item.position = item.position + CalcPosChange(item);
                if (MagSystem.Timely)
                {
                    Dust d = Dust.NewDustPerfect(item.position, DustID.AncientLight);
                    d.noGravity = true;
                }
            }
        }
        public override void UpdateInventory(Item item, Player player)
        {
            ReserveCD = MagPlayer.Delay;
            Target = null;
        }
        public override bool CanPickup(Item item, Player player)
        {
            if (Target != player && player.GetModPlayer<MagPlayer>().ItemAction == ItemAction.Encumber)
            {
                return false;
            }
            return base.CanPickup(item, player);
        }
        public override bool OnPickup(Item item, Player player)
        {
            if (Target == player && player.GetModPlayer<MagPlayer>().ItemAction == ItemAction.Exhaust)
            {
                item.TurnToAir();
                return false;
            }
            return base.OnPickup(item, player);
        }
    }
}
