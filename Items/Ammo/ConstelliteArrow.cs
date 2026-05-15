using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Ammo
{
	public class ConstelliteArrow : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 99;
		}

		public override void SetDefaults()
		{
			Item.width = 14;
			Item.height = 32;
			Item.ResearchUnlockCount = 99;

			Item.damage = 17; 
			Item.DamageType = DamageClass.Ranged;

			Item.knockBack = 2f;

			Item.consumable = true;
			Item.maxStack = 9999;

			Item.ammo = AmmoID.Arrow;

			Item.shoot = ModContent.ProjectileType<Projectiles.ConstelliteArrowProj>();
			Item.shootSpeed = 4f;

			Item.value = Item.buyPrice(copper: 50);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void AddRecipes()
		{
			CreateRecipe(25)
				.AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>())
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
