using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;

namespace doctor_mangle.models.battles
{
    public class BattleRound
    {
        public MonsterData Attacker { get; set; }
        public MonsterData Defender { get; set; }
        public BodyPart AttackTarget { get; set; }
        public BodyPart ReplyTarget { get; set; }
        public float Strike { get; set; }
        public float Parry { get; set; }
        public float Repost { get; set; }
        public float Block { get; set; }
        public decimal AttackTargetStartDurability { get; set; }
        public decimal ReplyTargetStartDurability { get; set; }
        public decimal AttackDamage { get; set; }
        public decimal ReplyDamage { get; set; }
        public bool CounterAttempted { get; set; }
        public bool AttackBlocked { get => this.Parry >= this.Strike; }
        public bool RepostBlocked { get => this.Block >= this.Repost; }
        public bool AttackTargetDestroyed { get => this.AttackTargetStartDurability - this.AttackDamage <= 0; }
        public bool ReplyTargetDestroyed { get => this.ReplyTargetStartDurability - this.ReplyDamage <= 0; }
    }
}
