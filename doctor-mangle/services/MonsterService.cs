using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace doctor_mangle.services
{
    public class MonsterService : IMonsterService
    {
        private readonly ILogger<IMonsterService> _logger;
        public MonsterService(ILogger<IMonsterService> logger)
        {
            this._logger = logger;
        }
        public void MovePartsForSerilaization<T>(T objectWithCollection)
        {
            if (objectWithCollection.GetType() != typeof(MonsterData))
            {
                throw new ArgumentException("Must use monster in MonsterService serialization implementation");
            }

            var monster = objectWithCollection as MonsterData;

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

        public void MovePartsAfterDeserilaization<T>(T objectWithCollection)
        {
            if (objectWithCollection.GetType() != typeof(MonsterData))
            {
                throw new ArgumentException("Must use monster in MonsterService serialization implementation");
            }

            var monster = objectWithCollection as MonsterData;

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

        public string GetMonsterDetails(MonsterData monster)
        {
            if (monster == null)
            {
                _logger.LogWarning("Cannot print details when null argument is provided");
                return string.Empty;
            }
            _logger.LogInformation($"Print details for monster {monster.Name}");
            var result = string.IsNullOrEmpty(monster.Name)
                ? "Unnamed monster"
                : monster.Name;
            result += $"\r\nAlacrity:  {monster.MonsterStats[Stat.Alacrity]}" +
                $"\r\nStrenght:  {monster.MonsterStats[Stat.Strength]}" +
                $"\r\nEndurance: {monster.MonsterStats[Stat.Endurance]}" +
                $"\r\nTechnique: {monster.MonsterStats[Stat.Technique]}";
            result += monster.Fights == 0
                ? "\r\nUntested in battle"
                : $"\r\n{monster.Wins} wins in {monster.Fights} fights";
            return result;
        }

        public string GetMonsterGhostDetails(MonsterGhost monster)
        {
            if (monster == null)
            {
                _logger.LogWarning("Cannot print details when null argument is provided");
                return string.Empty;
            }
            _logger.LogInformation($"Print details for monster ghost {monster.Name}");
            var result = $"{monster.Name}" +
                $"\r\nWins:  {monster.Wins}" +
                $"\r\nFights:  {monster.Fights}" +
                $"\r\nDate of Death: {monster.DeathDay}";
            return result;
        }
    }
}
