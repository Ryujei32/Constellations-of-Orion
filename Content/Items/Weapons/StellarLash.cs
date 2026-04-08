using ConstellationsOfOrion.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Weapons
{
	public class StellarLash : ModItem
	{

		public override void SetDefaults() {
			// This method quickly sets the whip's properties.
			// Mouse over to see its parameters.
            Item.DefaultToWhip(ModContent.ProjectileType<StellarLashProj>(), 96, 2f, 4f, 30);
			Item.rare = ItemRarityID.Pink;
			Item.channel = true;
		}

		// Makes the whip receive melee prefixes
		public override bool MeleePrefix() {
			return true;
		}
	}
}