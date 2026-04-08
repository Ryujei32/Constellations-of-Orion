using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content
{
	public class OrionPlayer : ModPlayer
	{
		public bool orionSet;

		public override void ResetEffects()
		{
			orionSet = false;
		}

		public override void PostUpdate()
		{
			if (orionSet)
			{
				float pulse = (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.2f + 0.3f;
				Lighting.AddLight(Player.Center, 0.6f * pulse, 0.1f * pulse, 0.8f * pulse);
			}
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (orionSet && item.DamageType == DamageClass.Melee)
			{
				target.AddBuff(BuffID.Poisoned, 300);
				target.AddBuff(BuffID.Venom, 300);
			}
		}
	}
}
