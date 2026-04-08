using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ConstellationsOfOrion.Content.Items.Weapons
{
	public class Starang : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Starang");
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;

			Item.damage = 68;
			Item.DamageType = DamageClass.Melee;

			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.useStyle = ItemUseStyleID.Swing;

			Item.noUseGraphic = true;
			Item.noMelee = true;

			Item.knockBack = 5f;

			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;

			Item.UseSound = SoundID.Item1;

			Item.autoReuse = true;

			Item.shoot = ModContent.ProjectileType<Content.Projectiles.StarangProj>();
			Item.shootSpeed = 14f;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "Effect",
				"Can have up to 6 starangs thrown at once"));
		}

		public override bool CanUseItem(Player player)
		{
			// ⭐ LIGHT DISC STYLE LIMIT
			return player.ownedProjectileCounts[Item.shoot] < 6;
		}

		public override bool Shoot(Player player,
			EntitySource_ItemUse_WithAmmo source,
			Vector2 position,
			Vector2 velocity,
			int type,
			int damage,
			float knockback)
		{
			Projectile.NewProjectile(
				source,
				position,
				velocity,
				type,
				damage,
				knockback,
				player.whoAmI
			);

			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 5)
				.AddIngredient(ItemID.FallenStar, 2)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
