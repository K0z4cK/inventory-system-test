using System.Collections.Generic;

public interface ICraftingService
{
    List<ItemCraftStruct> GetAllCrafts();
    List<ItemCraftStruct> GetAvailableCrafts();
    List<InventoryItem> GetCraftItems(List<InventoryItem> craftRecipe);
    List<InventoryItem> GetMissingItems(List<InventoryItem> craftRecipe);
    bool CanCraft(ItemCraftStruct craft);
    CraftAvailability GetCraftAvailability(ItemCraftStruct craft);
    bool TryCraft(ItemCraftStruct craft);
}
