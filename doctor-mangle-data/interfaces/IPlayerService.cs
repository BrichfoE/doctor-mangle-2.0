using doctor_mangle.constants;
using doctor_mangle.models;
using doctor_mangle.models.parts;
using System.Collections.Generic;

namespace doctor_mangle.interfaces
{
    public interface IPlayerService
    {
        PlayerData GeneratePlayer(string playerName, bool isAI);
        string GenerateRandomName();
        string CheckBag(PlayerData player);
        string ScrapItem(Dictionary<Structure, int> spareParts, List<BodyPart> storage, int reference);
        int GetRepairCost(BodyPart part);
        int RepairPart(BodyPart part, int availableParts);
        string OrchestratePartRepair(PlayerData player, int reference);
        void DumpBagIntoWorkshop(PlayerData player);
        string GetWorkshopItemList(PlayerData player);
        int Compare(PlayerData x, PlayerData y);
    }
}
