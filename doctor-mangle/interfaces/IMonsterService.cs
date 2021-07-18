using doctor_mangle.models.monsters;

namespace doctor_mangle.interfaces
{
    public interface IMonsterService
    {
        void MovePartsForSerilaization(MonsterData monster);

        void MovePartsAfterDeserilaization(MonsterData monster);
    }
}
