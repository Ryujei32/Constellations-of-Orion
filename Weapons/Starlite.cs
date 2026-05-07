using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.Collections.Generic;

namespace ConstellationsOfOrion.Content.Items.Weapons
{
	public class Starlite : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Starlite");

			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 34;

			Item.damage = 64;
			Item.DamageType = DamageClass.Magic;

			Item.mana = 12;

			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.useStyle = ItemUseStyleID.Shoot;

			Item.noMelee = true;
			Item.autoReuse = true;

			Item.knockBack = 3f;

			Item.value = Item.buyPrice(gold: 6);
			Item.rare = ItemRarityID.Pink;

			Item.UseSound = SoundID.Item20;

			// ⭐ Base projectile (won’t really matter, we override Shoot)
			Item.shoot = ProjectileID.SuperStar;
			Item.shootSpeed = 0f;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "Effect",
				"Calls down a barrage of celestial stars"));
		}

		public override bool Shoot(Player player,
			EntitySource_ItemUse_WithAmmo source,
			Vector2 position,
			Vector2 velocity,
			int type,
			int damage,
			float knockback)
		{
			Vector2 mouse = Main.MouseWorld;

			int starCount = Main.rand.Next(4, 7); // ⭐ barrage amount

			for (int i = 0; i < starCount; i++)
			{
				Vector2 spawnPos = new Vector2(
					mouse.X + Main.rand.Next(-80, 80),
					mouse.Y - 600 + Main.rand.Next(-50, 50)
				);

				Vector2 starVelocity = new Vector2(
					Main.rand.NextFloat(-1.5f, 1.5f),
					Main.rand.NextFloat(14f, 18f)
				);

				Projectile proj = Projectile.NewProjectileDirect(
					source,
					spawnPos,
					starVelocity,
					ProjectileID.SuperStar, // ⭐ Super Star Shooter projectile
					damage,
					knockback,
					player.whoAmI
				);

				// ⭐ slight delay between stars (feels better)
				proj.timeLeft += i * 2;
			}

			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 6)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
