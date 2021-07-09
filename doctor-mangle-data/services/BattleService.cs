﻿using doctor_mangle.constants;
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
            PlayerData winner;

            MonsterData bm = blue.Monster;
            MonsterData gm = green.Monster;

            while (bm.CanFight && gm.CanFight)
            {
                MonsterData attck;
                MonsterData reply;
                if (gm.MonsterStats[Stat.Alacrity] > bm.MonsterStats[Stat.Alacrity])
                {
                    attck = gm;
                    reply = bm;
                }
                else
                {
                    attck = bm;
                    reply = gm;
                }


                float strke = ((float)_rng.NextDouble() * attck.MonsterStats[Stat.Strength]);
                float parry = ((float)_rng.NextDouble() * reply.MonsterStats[Stat.Alacrity]);
                float repst = ((float)_rng.NextDouble() * reply.MonsterStats[Stat.Alacrity]);
                float block = ((float)_rng.NextDouble() * attck.MonsterStats[Stat.Strength]);

                BodyPart attackTarget;
                BodyPart replyTarget;

                //add technical to strike to hit head or torso
                attackTarget = GetTarget(reply, attck.Name, (_rng.Next(1, 101)) < (attck.MonsterStats[Stat.Technique] / 10000));

                //add technical to repost to hit head or torso
                replyTarget = GetTarget(attck, reply.Name, (_rng.Next(1, 101)) < (reply.MonsterStats[Stat.Technique] / 10000));

                //strike vs parry, result decreases random part damage
                Console.WriteLine(attck.Name + " swings at " + reply.Name + "'s " + attackTarget.PartName + "!");
                Console.WriteLine($"Strike: {strke}, Parry: {parry}.");
                if (strke > parry)
                {
                    decimal reduction = (decimal)(strke / (parry * attackTarget.PartStats[Stat.Endurance]));
                    Console.WriteLine($"{attackTarget.PartName} goes from {attackTarget.PartDurability} to {attackTarget.PartDurability-reduction}");
                    attackTarget.PartDurability -= reduction;
                }
                else
                {
                    Console.WriteLine($"{attck.Name} 's strike is blocked!");
                }

                if (attackTarget.PartDurability <= 0)
                {
                    Console.WriteLine($"{reply}'s {attackTarget.PartName} has been destroyed!");
                    attackTarget = null;
                }

                if (reply.CanFight)
                {
                    //repost vs block, result decreases random part damage
                    Console.WriteLine(reply.Name + " counters at " + attck.Name + "'s " + replyTarget.PartName + "!");
                    Console.WriteLine($"Repost: {repst}, Block: {block}.");
                    Console.WriteLine(repst > block 
                        ? (attackTarget + " goes from " + replyTarget.PartDurability + " to " + (replyTarget.PartDurability - (decimal)((repst - block)/5))) 
                        : reply.Name + "'s counter is blocked!");
                    replyTarget.PartDurability = repst > block ? replyTarget.PartDurability - (decimal)((repst - block)/5) : replyTarget.PartDurability;
                    if (replyTarget.PartDurability <= 0)
                    {
                        Console.WriteLine($"{attck}'s {replyTarget.PartName} has been destroyed!");
                        attackTarget = null;
                    }
                }
            }

            // only one mostner is left standing, determine who it is
            if (bm.CanFight)
            {
                winner = blue;
                blue.WinsCount ++;
                blue.Monster.Wins ++;
            }
            else
            {
                winner = green;
                green.WinsCount ++;
                green.Monster.Wins ++;
            }
            Console.WriteLine($"{winner}'s opponent can no longer put a fight.\r\n{winner} is victorious!");
            blue.FightsCount ++;
            blue.Monster.Fights ++;
            green.FightsCount ++;
            green.Monster.Fights ++;

            return winner;
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
