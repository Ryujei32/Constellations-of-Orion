using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Ammo
{
	public class ConstelliteBullet : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 99;
		}

		public override void SetDefaults()
		{
			Item.width = 14;
			Item.height = 14;
			Item.ResearchUnlockCount = 99;

			Item.damage = 17; 
			Item.DamageType = DamageClass.Ranged;

			Item.knockBack = 2f;

			Item.consumable = true;
			Item.maxStack = 9999;

			Item.ammo = AmmoID.Bullet;

			Item.shoot = ModContent.ProjectileType<Content.Projectiles.ConstelliteBulletProj>();
			Item.shootSpeed = 4f;

			Item.value = Item.buyPrice(copper: 60);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void AddRecipes()
		{
			CreateRecipe(50)
				.AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 1)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
