using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doctor_mangle.services
{
    public class BattleService : IBattleService
    {
        private readonly Random _rng;

        public BattleService() 
        {
            _rng = new Random(); 
        }

        public BattleService(Random rng)
        {
            _rng = rng;
        }

        public PlayerData MonsterFight(PlayerData blue, PlayerData green)
        {
            var script = new BattleScript()
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
                MonsterData attck;
                MonsterData reply;
                if (script.Green.MonsterStats[Stat.Alacrity] > script.Blue.MonsterStats[Stat.Alacrity])
                {
                    attck = script.Green;
                    reply = script.Blue;
                }
                else
                {
                    attck = script.Blue;
                    reply = script.Green;
                }
                script.Debug.Add($"Attack: {attck.Name}, Reply: {reply.Name}");


                float strke = ((float)_rng.NextDouble() * attck.MonsterStats[Stat.Strength]);
                float parry = ((float)_rng.NextDouble() * reply.MonsterStats[Stat.Alacrity]);
                float repst = ((float)_rng.NextDouble() * reply.MonsterStats[Stat.Alacrity]);
                float block = ((float)_rng.NextDouble() * attck.MonsterStats[Stat.Strength]);
                script.Debug[script.Debug.Count-1] += $" Strike: { strke}, Parry: { parry}, Repost: {repst}, Block: {block},";

                BodyPart attackTarget;
                BodyPart replyTarget;

                //add technical to strike to hit head or torso
                attackTarget = GetTarget(reply, attck.Name, (_rng.Next(1, 101)) < (attck.MonsterStats[Stat.Technique] / 10000));

                //add technical to repost to hit head or torso
                replyTarget = GetTarget(attck, reply.Name, (_rng.Next(1, 101)) < (reply.MonsterStats[Stat.Technique] / 10000));

                script.Debug[script.Debug.Count - 1] += $" AttackTarget: {attackTarget.PartType}, ReplyTarget: {replyTarget.PartType},";

                //strike vs parry, result decreases random part damage
                script.Text.Add($"{attck.Name} swings at {reply.Name}'s {attackTarget.PartName}!");
                if (strke > parry)
                {
                    decimal reduction = (decimal)(strke / (parry * attackTarget.PartStats[Stat.Endurance]));
                    script.Text.Add($"{attackTarget.PartName} goes from {attackTarget.PartDurability} to {attackTarget.PartDurability-reduction}");
                    attackTarget.PartDurability -= reduction;
                    script.Debug[script.Debug.Count - 1] += $" AttackDamage: {reduction},";
                }
                else
                {
                    script.Text.Add($"{attck.Name} 's strike is blocked!");
                    script.Debug[script.Debug.Count - 1] += $" AttackDamage: 0,";
                }

                if (attackTarget.PartDurability <= 0)
                {
                    script.Text.Add($"{reply}'s {attackTarget.PartName} has been destroyed!");
                }
                script.Debug[script.Debug.Count - 1] += $" AttackTargetDestroyed: {attackTarget.PartDurability <= 0}";

                if (reply.CanFight)
                {
                    //repost vs block, result decreases random part damage
                    script.Debug[script.Debug.Count - 1] += $" Counter: True,";
                    script.Text.Add($"{ reply.Name} counters at {attck.Name}'s {replyTarget.PartName}!");
                    if (strke > parry)
                    {
                        decimal reduction = (decimal)((repst - block) / 5);
                        script.Text.Add($"{replyTarget.PartName} goes from {replyTarget.PartDurability} to {replyTarget.PartDurability - reduction}");
                        replyTarget.PartDurability -= reduction;
                        script.Debug[script.Debug.Count - 1] += $" ReplyDamage: {reduction},";
                    }
                    else
                    {
                        script.Text.Add($"{reply.Name} 's counter is blocked!");
                        script.Debug[script.Debug.Count - 1] += $" ReplyDamage: 0,";
                    }
                    if (replyTarget.PartDurability <= 0)
                    {
                        script.Text.Add($"{attck}'s {replyTarget.PartName} has been destroyed!");
                    }
                    script.Debug[script.Debug.Count - 1] += $" ReplyTargetDestroyed: {replyTarget.PartDurability <= 0}";
                }
                else
                {
                    script.Debug[script.Debug.Count - 1] += $" Counter: false, ReplyDamage: 0, ReplyTargetDestroyed: False";
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
            script.Text.Add($"{script.Winner}'s opponent can no longer put a fight.\r\n{script.Winner} is victorious!");
            blue.FightsCount ++;
            blue.Monster.Fights ++;
            green.FightsCount ++;
            green.Monster.Fights ++;

            return script.Winner;
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
