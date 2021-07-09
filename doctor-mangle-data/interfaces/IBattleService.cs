using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;

namespace doctor_mangle.interfaces
{
    public interface IBattleService
    {
        PlayerData MonsterFight(PlayerData blue, PlayerData green);

        BodyPart GetTarget(MonsterData targetMonster, string attackerName, bool criticalHit);

        void GrantCash(PlayerData playerData, int wins);
    }
}
