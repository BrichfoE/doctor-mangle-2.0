using doctor_mangle.models.monsters;

namespace doctor_mangle.interfaces
{
    public interface IMonsterService: IPartsCollectionSerializer
    {
        string GetMonsterDetails(MonsterData monster);
        string GetMonsterGhostDetails(MonsterGhost monster);
    }
}
