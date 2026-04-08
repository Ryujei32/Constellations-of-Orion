using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ConstellationsOfOrion.Content.Items.Armor
{
	[AutoloadEquip(EquipType.Body)]
	public class OrionChestplate : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 22;
			Item.defense = 23;
			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			Player player = Main.LocalPlayer;

			bool inSocialSlot =
				player.armor[11].type == Type;

			if (inSocialSlot)
				return;

			tooltips.Add(new TooltipLine(Mod, "DamageBonus", "8% increased damage"));
			tooltips.Add(new TooltipLine(Mod, "CritBonus", "8% increased critical strike chance"));
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 0.08f;
			player.GetCritChance(DamageClass.Generic) += 8f;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
                .AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 20)
				.AddIngredient(ItemID.FallenStar, 15)
				.AddIngredient(ItemID.SoulofLight, 10)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
