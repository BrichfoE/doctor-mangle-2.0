using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doctor_mangle
{
    public class ArenaBattleCalculator
    {
        private Random RNG = new Random();

        public PlayerData MonsterFight(PlayerData blue, PlayerData green)
        {
            PlayerData winner;

            MonsterData bm = blue.Monster;
            MonsterData gm = green.Monster;

            Console.WriteLine("In the blue corner, " + blue.Name + " presents " + bm.Name);
            Console.WriteLine(bm.Name + " boasts " + bm.Wins + " wins!");
            Console.WriteLine("In the green corner, " + green.Name + " presents " + gm.Name);
            Console.WriteLine(gm.Name + " boasts " + gm.Wins + " wins!");

            while (bm.Parts[0].PartDurability > 0 && bm.Parts[1].PartDurability > 0 && gm.Parts[0].PartDurability > 0 && gm.Parts[1].PartDurability > 0)
            {
                MonsterData attack;
                MonsterData reply;
                if (gm.MonsterStats[0] > bm.MonsterStats[0])
                {
                    attack = gm;
                    reply = bm;
                }
                else
                {
                    attack = bm;
                    reply = gm;
                }

                float strike = (RNG.Next(1, 101) * attack.MonsterStats[constants.Stat.Strength]) / 100000;
                float parry = (RNG.Next(1, 101)  * reply.MonsterStats[constants.Stat.Alacrity]) / 100000;
                float repost = (RNG.Next(1, 101) * reply.MonsterStats[constants.Stat.Alacrity]) / 100000;
                float block = (RNG.Next(1, 101)  * attack.MonsterStats[constants.Stat.Strength]) / 100000;
                BodyPart attackTarget;
                BodyPart replyTarget;

                //add technical to strike to hit head or torso
                attackTarget = GetTarget(reply, attack, (RNG.Next(1, 101)) < (attack.MonsterStats[constants.Stat.Technique] / 10000));

                //add technical to repost to hit head or torso
                replyTarget = GetTarget(attack, reply, (RNG.Next(1, 101)) < (reply.MonsterStats[constants.Stat.Technique] / 10000));

                //strike vs parry, result decreases random part damage
                Console.WriteLine(attack.Name + " swings at " + reply.Name + "'s " + attackTarget.PartName + "!");
                Console.WriteLine((strike > parry ? attackTarget.PartName + " goes from " + attackTarget.PartDurability + " to " + (attackTarget.PartDurability - (decimal)((strike - parry)/5)) : attack.Name + " is blocked!"));
                attackTarget.PartDurability = strike > parry ? attackTarget.PartDurability - (decimal)((strike - parry)/5) : attackTarget.PartDurability;
                if (attackTarget.PartDurability <= 0)
                {
                    Console.WriteLine(attackTarget.PartName + " has been destroyed!");
                    attackTarget = null;
                }

                if (reply.Parts[0].PartDurability > 0 && reply.Parts[1].PartDurability > 0)
                {
                    //repost vs block, result decreases random part damage
                    Console.WriteLine(reply.Name + " counters at " + attack.Name + "'s " + replyTarget.PartName + "!");
                    Console.WriteLine(repost > block ? (attackTarget + " goes from " + replyTarget.PartDurability + " to " + (replyTarget.PartDurability - (decimal)((repost - block)/5))) : reply.Name + " is blocked!");
                    replyTarget.PartDurability = repost > block ? replyTarget.PartDurability - (decimal)((repost - block)/5) : replyTarget.PartDurability;
                    if (replyTarget.PartDurability <= 0)
                    {
                        Console.WriteLine(replyTarget.PartName + " has been destroyed!");
                        attackTarget = null;
                    }
                }
            }

            if (bm.Parts[0].PartDurability > 0 && bm.Parts[1].PartDurability > 0)
            {
                winner = blue;
                blue.WinsCount = blue.WinsCount + 1;
                blue.Monster.Wins = blue.Monster.Wins + 1;
            }
            else
            {
                winner = green;
                green.WinsCount = green.WinsCount + 1;
                green.Monster.Wins = green.Monster.Wins + 1;
            }
            blue.FightsCount = blue.FightsCount + 1;
            blue.Monster.Fights = blue.Monster.Fights + 1;
            green.FightsCount = green.FightsCount + 1;
            green.Monster.Fights = green.Monster.Fights + 1;

            return winner;
        }

        public BodyPart GetTarget(MonsterData targetMonster, MonsterData attackingMonster, bool criticalHit)
        {
            BodyPart target = null;
            while (target == null)
            {
                if (criticalHit)
                {
                    Console.WriteLine(attackingMonster.Name + " prepares a technical strike!");
                    for (int i = 0; i < 2; i++)
                    {
                        if (targetMonster.Parts[i] != null && (RNG.Next(i, 1)) == i && targetMonster.Parts[i].PartDurability > 0)
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
                        if (targetMonster.Parts[i] != null && (RNG.Next(i, targetMonster.Parts.Count-1)) == i && targetMonster.Parts[i].PartDurability > 0)
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
