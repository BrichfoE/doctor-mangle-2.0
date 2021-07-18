using doctor_mangle.interfaces;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace doctor_mangle.services
{
    public class MonsterService : IMonsterService
    {
        public void MovePartsForSerilaization(MonsterData monster)
        {
            monster._heads = monster.Parts
                .Where(x => x.GetType() == typeof(Head))
                .Select(y => new Head(y))
                .ToList();

            monster._torsos = monster.Parts
                .Where(x => x.GetType() == typeof(Torso))
                .Select(y => new Torso(y))
                .ToList();

            monster._arms = monster.Parts
                .Where(x => x.GetType() == typeof(Arm))
                .Select(y => new Arm(y))
                .ToList();

            monster._legs = monster.Parts
                .Where(x => x.GetType() == typeof(Leg))
                .Select(y => new Leg(y))
                .ToList();

            monster.Parts.Clear();
        }

        public void MovePartsAfterDeserilaization(MonsterData monster)
        {
            foreach (var item in monster._heads)
            {
                monster.Parts.Add(item);
            }
            monster._heads.Clear();
            foreach (var item in monster._torsos)
            {
                monster.Parts.Add(item);
            }
            monster._torsos.Clear();
            foreach (var item in monster._arms)
            {
                monster.Parts.Add(item);
            }
            monster._arms.Clear();
            foreach (var item in monster._legs)
            {
                monster.Parts.Add(item);
            }
            monster._legs.Clear();
        }
    }
}
