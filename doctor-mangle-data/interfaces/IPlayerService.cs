using doctor_mangle.models;
namespace doctor_mangle.interfaces
{
    public interface IPlayerService
    {
        PlayerData GeneratePlayer(string playerName, bool isAI);
        string GenerateRandomName();
        string CheckBag(PlayerData player);
    }
}
