using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ConstellationsOfOrion.Content.Items.Tools
{
	public class ConstelliteAxe : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Constellite Axe");
		}

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.ResearchUnlockCount = 1;

			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 6; 
			Item.useAnimation = 12;

			Item.DamageType = DamageClass.Melee;
			Item.damage = 34;
			Item.knockBack = 5f;

			Item.axe = 30; // 150%

			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.useTurn = true;

			Item.value = Item.buyPrice(gold: 6);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "PowerInfo", "Chopping trees with stellar energy"));
		}

		public override void MeleeEffects(Player player, Rectangle hitbox)
		{
			if (Main.rand.NextBool(3))
			{
				int dustType = Main.rand.NextBool(2) ? DustID.GemAmethyst : DustID.GemTopaz;

				Dust dust = Dust.NewDustDirect(hitbox.TopLeft(), hitbox.Width, hitbox.Height, dustType);
				dust.noGravity = true;
				dust.scale = 1.1f;
				dust.velocity *= 0.3f;
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 14)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
