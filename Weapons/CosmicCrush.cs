// Content/Items/Weapons/CosmicCrush.cs

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ConstellationsOfOrion.Content.Items.Weapons
{
	public class CosmicCrush : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 34;

			Item.damage = 92; // strong mid-hardmode melee
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.knockBack = 8f;

			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 32;
			Item.useTime = 32;

			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.channel = true;
			Item.autoReuse = false;

			Item.shoot = ModContent.ProjectileType<Content.Projectiles.CosmicCrushProj>();
			Item.shootSpeed = 14f;

			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 7);

			Item.UseSound = SoundID.Item1;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "Flavor",
				"Releases the head when swung"));
		}

		public override bool CanUseItem(Player player)
		{
			return player.ownedProjectileCounts[
				ModContent.ProjectileType<Content.Projectiles.CosmicCrushProj>()
			] <= 0;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 5)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}