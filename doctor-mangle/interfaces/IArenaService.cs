using doctor_mangle.models;
using doctor_mangle.models.battles;

namespace doctor_mangle.interfaces
{
    public interface IArenaService
    {
        ArenaResult ManageBattlePhase(PlayerData[] players);
    }
}
