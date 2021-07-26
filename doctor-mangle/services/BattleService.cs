using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.battles;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using System;

namespace doctor_mangle.services
{
    public class BattleService : IBattleService
    {
        private readonly Random _rng;

        public BattleService(Random rng)
        {
            this._rng = rng;
        }
        public BattleResult MonsterFight(PlayerData blue, PlayerData green)
        {
            var script = new BattleResult()
            {
                BlueCorner = blue,
                GreenCorner = green
            };

            script.Text.Add("Draw your eyes to the arena!");
            script.Text.Add($"In the blue corner, {script.BlueCorner.Name} presents {script.Blue.Name}");
            script.Text.Add($"{script.Blue.Name} boasts {script.Blue.Wins} wins!");
            script.Text.Add($"In the green corner {script.GreenCorner.Name} presents {script.Green.Name}");
            script.Text.Add($"{script.Green.Name} boasts {script.Green.Wins} wins!");


            while (script.Blue.CanFight && script.Green.CanFight)
            {
                var round = new BattleRound();
                script.Rounds.Add(round);

                if (script.Green.MonsterStats[Stat.Alacrity] * (float)_rng.NextDouble() > script.Blue.MonsterStats[Stat.Alacrity] * (float)_rng.NextDouble())
                {
                    round.Attacker = script.Green;
                    round.Defender = script.Blue;
                }
                else
                {
                    round.Attacker = script.Blue;
                    round.Defender = script.Green;
                }

                var attckStat = round.Attacker.MonsterStats[Stat.Strength] + round.Attacker.MonsterStats[Stat.Alacrity];
                var replyStat = round.Defender.MonsterStats[Stat.Strength] + round.Defender.MonsterStats[Stat.Alacrity];

                round.Strike = (float)_rng.NextDouble() * attckStat;
                round.Parry = 1 / ((float)_rng.NextDouble() * replyStat);
                round.Repost = ((float)_rng.NextDouble() * replyStat);
                round.Block = 1 / ((float)_rng.NextDouble() * replyStat);

                //add technical to strike to hit head or torso
                round.AttackTarget = GetTarget(round.Defender, round.Attacker.Name, (_rng.Next(1, 101)) < (round.Attacker.MonsterStats[Stat.Technique] / 10000));

                //add technical to repost to hit head or torso
                round.ReplyTarget = GetTarget(round.Attacker, round.Defender.Name, (_rng.Next(1, 101)) < (round.Defender.MonsterStats[Stat.Technique] / 10000));

                //strike vs round.Parry, result decreases random part damage
                script.Text.Add($"{round.Attacker.Name} swings at {round.Defender.Name}'s {round.AttackTarget.PartName}!");
                if (!round.AttackBlocked)
                {
                    round.AttackDamage = (decimal)(round.Strike  / ((round.Parry + round.AttackTarget.PartStats[Stat.Endurance]) * 20 * _rng.NextDouble()));
                    script.Text.Add($"{round.AttackTarget.PartName} goes from {round.AttackTarget.PartDurability} to {round.AttackTarget.PartDurability - round.AttackDamage }");
                    round.AttackTarget.PartDurability -= round.AttackDamage;
                }
                else
                {
                    script.Text.Add($"{round.Attacker.Name} 's strike is blocked!");
                }

                if (round.AttackTarget.PartDurability <= 0)
                {
                    script.Text.Add($"{round.Defender.Name}'s {round.AttackTarget.PartName} has been destroyed!");
                }

                if (round.Defender.CanFight)
                {
                    //repost vs block, result decreases random part damage
                    round.CounterAttempted = true;
                    script.Text.Add($"{ round.Defender.Name} counters at {round.Attacker.Name}'s {round.ReplyTarget.PartName}!");
                    if (!round.RepostBlocked)
                    {
                        round.ReplyDamage = (decimal)(round.Repost / ((round.Block + round.ReplyTarget.PartStats[Stat.Endurance]) * 20 * _rng.NextDouble()));
                        script.Text.Add($"{round.ReplyTarget.PartName} goes from {round.ReplyTarget.PartDurability} to {round.ReplyTarget.PartDurability - round.ReplyDamage}");
                        round.ReplyTarget.PartDurability -= round.ReplyDamage;
                    }
                    else
                    {
                        script.Text.Add($"{round.Defender.Name} 's counter is blocked!");
                    }

                    if (round.ReplyTarget.PartDurability <= 0)
                    {
                        script.Text.Add($"{round.Attacker.Name}'s {round.ReplyTarget.PartName} has been destroyed!");
                    }
                }
            }

            // only one mostner is left standing, determine who it is
            if (script.Blue.CanFight)
            {
                script.WinnerIsBlue = true;
                blue.WinsCount ++;
                blue.Monster.Wins ++;
            }
            else
            {
                script.WinnerIsBlue = false;
                green.WinsCount ++;
                green.Monster.Wins ++;
            }
            script.Text.Add($"{script.Winner.Name}'s opponent can no longer put a fight.\r\n{script.Winner.Name} is victorious!");
            blue.FightsCount ++;
            blue.Monster.Fights ++;
            green.FightsCount ++;
            green.Monster.Fights ++;

            return script;
        }

        public BodyPart GetTarget(MonsterData targetMonster, string attackerName, bool criticalHit)
        {
            BodyPart target = null;
            while (target == null)
            {
                if (criticalHit)
                {
                    Console.WriteLine(attackerName + " prepares a technical strike!");
                    for (int i = 0; i < 2; i++)
                    {
                        if (targetMonster.Parts[i] != null && (_rng.Next(i, 1)) <= i && targetMonster.Parts[i].PartDurability > 0)
                        {
                            target = targetMonster.Parts[i];
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < targetMonster.Parts.Count; i++)
                    {
                        if (targetMonster.Parts[i] != null && (_rng.Next(i, targetMonster.Parts.Count-1)) <= i && targetMonster.Parts[i].PartDurability > 0)
                        {
                            target = targetMonster.Parts[i];
                            break;
                        }
                    }
                }
            }
            return target;
        }

        public void GrantCash(PlayerData playerData, int wins)
        {
            Console.WriteLine("I'll add gold here for equipment eventually!");
        }
    }
}
