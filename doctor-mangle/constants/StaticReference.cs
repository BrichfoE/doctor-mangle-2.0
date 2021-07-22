using System.Collections.Generic;

namespace doctor_mangle.constants
{
    // not using zero based lists here so unit tests will be able to assert whether they're still on default setting
    public enum Part
    {
        head = 1,
        torso = 2,
        arm = 3,
        leg = 4,
        // tail = 7
    }

    public enum Structure
    {
        Magical = 1,
        Animal = 2,
        Human = 3,
        Mechanical = 4,
        Rock = 5
    }

    public enum Component
    {
        Ether = 1,
        Meat = 2,
        Biomatter = 3,
        Components = 4,
        Rocks = 5
    }

    public enum Rarity
    {
        Common = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Mythic = 5,
        Unicorn = 6,
    }

    public enum Stat
    {
        Alacrity = 1,
        Strength = 2,
        Endurance = 3,
        Technique = 4,
    }

    public static class StaticReference
    {

        public static Dictionary<Rarity, int> RarityMultiplier = new Dictionary<Rarity, int>()
        {
            {Rarity.Unicorn, 500},
            {Rarity.Mythic, 200 },
            {Rarity.Legendary, 100 },
            {Rarity.Epic, 30},
            {Rarity.Rare, 15 },
            {Rarity.Common, 5 }
        };

        public static Dictionary<Rarity, int> PartsPerRarity = new Dictionary<Rarity, int>()
        {
            { Rarity.Unicorn, 1000 },
            { Rarity.Mythic, 500 },
            { Rarity.Legendary, 200},
            { Rarity.Epic, 100 },
            { Rarity.Rare, 50 },
            { Rarity.Common, 10 }
        };

        public static Dictionary<Structure, Stat> StructureAffinity = new Dictionary<Structure, Stat>()
        {
            {Structure.Animal, Stat.Alacrity},
            {Structure.Mechanical, Stat.Strength},
            {Structure.Rock, Stat.Endurance},
            {Structure.Human, Stat.Technique}
        };

        public static string[] adjectives = new string[] { "Cool", "Nice", "Mad", "Helpful", "Thin", "Dirty", "Slick", "Ugly", "Super", "Octogenarian", "Beefy" };
        public static string[] names = new string[] { "Luke", "Matilda", "Martha", "Hannah", "Pete", "Harry", "Rick", "Veronica", "Susan", "Maynard", "Bobby" };
    }
}

