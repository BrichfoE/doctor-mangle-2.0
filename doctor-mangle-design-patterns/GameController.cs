﻿using doctor_mangle;
using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using doctor_mangle.utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace doctor_mangle_design_patterns
{
    public class GameController
    {
        // private readonly IGameService _gameService;
        private readonly IGameService _gameService;
        private readonly IPlayerService _playerService;
        private readonly IParkService _parkService;
        private readonly IBattleService _battleService;
        private readonly IComparer<BodyPart> _partComparer;
        private readonly string currentFile = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();

        public GameData Data { get; set; }
        public GameRepo Repo { get; set; }
        public PlayerData[] AllPlayers { get; set; }

        public GameController(
            IGameService gameService,
            IPlayerService playerService,
            IParkService parkService,
            IBattleService battleService,
            IComparer<BodyPart> partComparer)
        {
            this._gameService = gameService;
            this._playerService = playerService;
            this._parkService = parkService;
            this._battleService = battleService;
            this._partComparer = partComparer;
        }

        public bool RunGame()
        {
            string textInput = "default";
            int intInput = 3;

            #region FileSetup

            Repo = new GameRepo();

            Repo.FileSetup();
            StaticUtility.TalkPause("Welcome to the Isle of Dr. Mangle.");
            if (Repo.gameIndex.Count > 1)
            {
                Data = Repo.LoadGame();
            }
            if (Data == null)
            {
                bool halt = true;
                while (halt)
                {
                    Console.WriteLine("Please enter a name for your game data:");
                    textInput = Console.ReadLine();
                    if (Repo.gameIndex.ContainsKey(textInput))
                    {
                        Console.WriteLine("A game by that name already exists.");
                    }
                    else
                    {
                        halt = false;
                    }
                }
                Console.WriteLine("And how many contestants will you be competing against?");
                intInput = StaticUtility.CheckInput(1, 7);
                Data = _gameService.GetNewGameData(textInput, intInput, Repo.GetNextGameID());
                AllPlayers = new PlayerData[Data.AiPlayers.Length];
                AllPlayers[0] = Data.CurrentPlayer;
                var i = 1;
                foreach (var player in Data.AiPlayers)
                {
                    AllPlayers[i] = player;
                    i++;
                }
                Repo.SaveGame(Data);
            }
            #endregion FileSetup

            bool gameStatus = true;



            #region search
            StaticUtility.TalkPause("A new day has dawned!");
            StaticUtility.TalkPause("The parks will be open for 5 hours...");
            StaticUtility.TalkPause("You will then have one more hour in your labs before the evening's entertainment.");

            for (int i = 1; i < 6; i++)
            {
                try
                {
                    StaticUtility.TalkPause("It is currently " + i + " o'clock. The parks close at 6.");
                    Console.WriteLine($"You are currently in the {Data.RegionText}, \r\n what will you do next?");
                    Console.WriteLine(_gameService.PrintRegionOptions(Data));
                    intInput = StaticUtility.CheckInput(1, 4);
                    Data.CurrentRegion = intInput;

                    gameStatus = ShowSearchOptions(i - 1);
                    _gameService.AISearchTurn(Data, i);
                    if (!gameStatus)
                    {
                        return gameStatus;
                    }
                }
                catch (System.Exception ex)
                {
                    int currentLine = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber();
                    Repo.LogException(Data, $"Search Phase exception {currentFile} line {currentLine}", ex, false);
                }
            }
            #endregion

            #region build
            try
            {
                StaticUtility.TalkPause("It is now 6 o'clock. Return to your lab and prepare for the floorshow at 7.");
                Data.CurrentRegion = 0;
                foreach (var player in AllPlayers)
                {
                    _playerService.DumpBagIntoWorkshop(player);
                }
                Console.WriteLine("Bag contents added to workshop inventory.");
                gameStatus = ShowLabOptions();
                if (!gameStatus)
                {
                    return gameStatus;
                }
            }
            catch (System.Exception ex)
            {
                int currentLine = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber();
                Repo.LogException(Data, $"Player Build Phase exception {currentFile} line {currentLine}\n", ex, false);
            }

            try
            {
                _gameService.AIBuildTurn(Data);
            }
            catch (Exception ex)
            {
                int currentLine = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber();
                Repo.LogException(Data, $"AI Build Phase exception {currentFile} line {currentLine}\n", ex, false);
            }


            #endregion

            #region fight
            try
            {
                StaticUtility.TalkPause("Welcome to the evening's entertainment!");
                if (Data.CurrentPlayer.Monster != null && Data.CurrentPlayer.Monster.CanFight)
                {
                    Console.WriteLine("Would you like to particpate tonight?");
                    StaticUtility.TalkPause("1 - Yes, 2 - No");
                    intInput = StaticUtility.CheckInput(1, 2);
                    if (intInput != 1)
                    {
                        StaticUtility.TalkPause("Well, maybe tomorrow then...");
                        Console.WriteLine("Let's find you a comfortable seat.");

                    }
                    else
                    {
                        StaticUtility.TalkPause("Let the games begin!");
                    }
                }
                else
                {
                    StaticUtility.TalkPause("Seeing as you do not have a living, able bodied contestant...");
                    Console.WriteLine("Let's find you a comfortable seat.");
                }
                CalculateFights();
            }
            catch (Exception ex)
            {
                int currentLine = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber();
                Repo.LogException(Data, $"Fighting Phase exception {currentFile} line {currentLine}\n", ex, false);
            }
            #endregion

            #region dayEnd
            try
            {
                AllPlayers = _gameService.SortPlayersByWins(AllPlayers);
                Data.Parks = _parkService.AddParts(Data.Parks, AllPlayers.Length);
                Data.Parks = _parkService.HalveParts(Data.Parks);
                Data.GameDayNumber++;
                Repo.SaveGame(Data);
            }
            catch (Exception ex)
            {
                int currentLine = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber();
                Repo.LogException(Data, $"End of Day Phaseexception {currentFile} line {currentLine}\n", ex, false);
            }
            return gameStatus;
            #endregion
        }
        private bool ShowSearchOptions(int bagSlot)
        {
            bool status = true;
            bool searching = true;
            while (searching)
            {
                int intInput;
                Console.WriteLine("Welcome to the " + Data.RegionText + "! Here you can: ");

                Console.WriteLine("0 - Menu");
                Console.WriteLine("1 - Search for parts");
                Console.WriteLine("2 - Scan for parts");
                Console.WriteLine("3 - Look in bag");
                Console.WriteLine("4 - Go to another region");

                intInput = StaticUtility.CheckInput(0, 4);

                switch (intInput)
                {
                    case 0:
                        status = RunMenu();
                        searching = status;
                        break;
                    case 1:
                        if (Data.Parks[Data.CurrentRegion].PartsList.Count == 0)
                        {
                            Console.WriteLine("There are no more parts in this region");
                        }
                        else
                        {
                            Data.CurrentPlayer.Bag[bagSlot] = Data.Parks[Data.CurrentRegion].PartsList.Last();
                            Data.Parks[Data.CurrentRegion].PartsList.RemoveLast();
                            Console.WriteLine("You found: " + Data.CurrentPlayer.Bag[bagSlot].PartName);
                        }
                        searching = false;
                        break;
                    case 2:
                        foreach (var park in Data.Parks)
                            Console.WriteLine("There are " + park.PartsList.Count + " parts left in the " + park.ParkName + ".");
                        searching = false;
                        break;
                    case 3:
                        Console.WriteLine(_playerService.CheckBag(Data.CurrentPlayer));
                        break;
                    case 4:
                        Console.WriteLine($"You are currently in the {Data.RegionText}, \r\n what will you do next?");
                        Console.WriteLine(_gameService.PrintRegionOptions(Data));
                        intInput = StaticUtility.CheckInput(1, 4);
                        Data.CurrentRegion = intInput;
                        break;
                    default:
                        throw new Exception("Bad Input in GameController.ShowSearchOptions");
                }
            }
            return status;
        }
        private MonsterData BuildMonster(bool isNew)
        {
            int intInput;
            BodyPart[] table = new BodyPart[6];
            string type = "";
            BodyPart chosenPart;
            bool halt = false;
            bool leave = false;
            int loopStart = 0;
            MonsterData currentMonster = Data.CurrentPlayer.Monster;
            List<BodyPart> workshopCopy = new List<BodyPart>();

            if (isNew)
            {
                Console.WriteLine("You aproach the empty table...");
            }
            else
            {
                Console.WriteLine("Would you like to end " + currentMonster.Name + "'s career?  This is permanent...");
                Console.WriteLine("1 - Yes, kill " + currentMonster.Name);
                Console.WriteLine("2 - No, upgrade limbs");
                intInput = StaticUtility.CheckInput(1, 2);
                if (intInput == 2)
                {
                    loopStart = 2;
                    Console.WriteLine(currentMonster.Name + " slides onto the table...");
                    for (int i = 0; i < 6; i++)
                    {
                        table[i] = currentMonster.Parts[i];
                    }
                }
                else
                {
                    Data.Graveyard.Add(new MonsterGhost(currentMonster, Data.GameDayNumber));
                    loopStart = 0;
                    Console.WriteLine("You gently dismember " + currentMonster.Name + " and bury its head and torso in the communal graveyard.");
                    Console.WriteLine(currentMonster.Name + " will be missed.");
                    Console.WriteLine("Limbs have been added to your workshop inventory");
                    for (int i = 2; i < currentMonster.Parts.Count; i++)
                    {
                        if (currentMonster.Parts[i] != null)
                        {
                            Console.WriteLine(currentMonster.Parts[i].PartName + ", Durability: " + currentMonster.Parts[i].PartDurability);
                            Data.CurrentPlayer.WorkshopCuppoard.Add(currentMonster.Parts[i]);
                        }
                    }
                    Data.CurrentPlayer.Monster = null;
                    currentMonster = null;
                    Data.CurrentPlayer.WorkshopCuppoard.Sort(_partComparer);
                    isNew = true;
                }
            }

            workshopCopy = Data.CurrentPlayer.WorkshopCuppoard.Select(x => x).ToList();

            for (int i = loopStart; i < 5; i++)
            {
                switch (i)
                {
                    case 0:
                        type = "head";
                        break;
                    case 1:
                        type = "torso";
                        break;
                    case 2:
                        type = "left arm";
                        break;
                    case 3:
                        type = "right arm";
                        break;
                    case 4:
                        type = "left leg";
                        break;
                    case 5:
                        type = "right leg";
                        break;
                    default:
                        break;
                }

                halt = true;

                if (!workshopCopy.Any(x => x.PartType == (Part)i))
                {
                    Console.WriteLine("You do not have a " + type + " in your workshop.");
                    if (i == 1 || i == 2)
                    {
                        Console.WriteLine("A monster without a " + type + " is no moster at all, better luck tomorrow...");
                        table[0] = null; //this is in case they have a head but no torso
                        break;
                    }
                    halt = false;
                }

                while (halt)
                {
                    if (isNew == false && currentMonster.Parts[i] != null)
                    {
                        table[i] = currentMonster.Parts[i];
                        StaticUtility.TalkPause("Currently " + currentMonster.Name + " has the below " + type);
                        Console.WriteLine(currentMonster.Parts[i].PartName);
                        Console.WriteLine("Durability: " + currentMonster.Parts[i].PartDurability);
                        Console.WriteLine("Alacrity: " + currentMonster.Parts[i].PartStats[Stat.Alacrity]);
                        Console.WriteLine("Strenght: " + currentMonster.Parts[i].PartStats[Stat.Strength]);
                        Console.WriteLine("Endurance: " + currentMonster.Parts[i].PartStats[Stat.Endurance]);
                        StaticUtility.TalkPause("Technique: " + currentMonster.Parts[i].PartStats[Stat.Technique]);
                    }

                    Console.WriteLine("Workshop Items:");
                    Console.WriteLine("0 - Leave Table");
                    int count = 0;
                    foreach (var item in workshopCopy)
                    {
                        count++;
                        Console.WriteLine(count + " - " + item.PartName);
                    }

                    Console.WriteLine("Please choose a " + type + ":");
                    intInput = StaticUtility.CheckInput(0, StaticUtility.NonNullCount(workshopCopy));

                    if (intInput == 0)
                    {
                        halt = false;
                        leave = true;
                        break;
                    }
                    chosenPart = workshopCopy[intInput - 1];

                    Console.WriteLine(chosenPart.PartName);
                    if (chosenPart.PartType != (Part)i)
                    {
                        Console.WriteLine("That is not a " + type + "!");
                    }
                    else
                    {
                        Console.WriteLine("Durability: " + chosenPart.PartDurability);
                        Console.WriteLine("Alacrity: " + chosenPart.PartStats[Stat.Alacrity]);
                        Console.WriteLine("Strenght: " + chosenPart.PartStats[Stat.Strength]);
                        Console.WriteLine("Endurance: " + chosenPart.PartStats[Stat.Endurance]);
                        StaticUtility.TalkPause("Technique: " + chosenPart.PartStats[Stat.Technique]);
                        Console.WriteLine("Use this part?");
                        Console.WriteLine("1 - Yes");
                        Console.WriteLine("2 - No");
                        Console.WriteLine("3 - Skip part");
                        Console.WriteLine("4 - Leave Table");
                        int intInput2 = StaticUtility.CheckInput(1, 4);

                        switch (intInput2)
                        {
                            case 1:
                                if (table[i] != null) workshopCopy.Add(table[i]);
                                table[i] = chosenPart;
                                workshopCopy[intInput - 1] = null;
                                workshopCopy = workshopCopy.Where(x => x != null).ToList();
                                halt = false;
                                break;
                            case 2:
                                break;
                            case 3:
                                halt = false;
                                break;
                            case 4:
                                leave = true;
                                halt = false;
                                break;
                            default:
                                break;
                        }

                    }

                }
                //leave table
                if (leave)
                {
                    break;
                }
            }

            if (table[0] != null && table[1] != null)
            {
                MonsterData newMonster = new MonsterData(null, table);

                StaticUtility.TalkPause("This is your monster...");
                foreach (var part in table)
                {
                    if (part != null)
                    {
                        Console.WriteLine(part.PartName);
                    }
                }
                int count = 0;
                foreach (var stat in newMonster.MonsterStats)
                {
                    // Console.WriteLine(StaticReference.statList[count] + ": " + stat);
                    count++;
                }
                Console.WriteLine("Would you like to keep this monster?");
                Console.WriteLine("1 - Yes, 2 - No");
                intInput = StaticUtility.CheckInput(1, 2);
                if (intInput == 1)
                {
                    if (isNew)
                    {
                        Console.WriteLine("What is its name?");
                        currentMonster = newMonster;
                        currentMonster.Name = Console.ReadLine();

                    }
                    else
                    {
                        foreach (var part in table)
                        {
                            currentMonster.Parts.Add(part);
                        }
                    }
                    Data.CurrentPlayer.WorkshopCuppoard = workshopCopy.Select(x => x).ToList();
                }
                else
                {
                    Console.WriteLine("Better luck building tomorrow...");
                }
            }

            Data.CurrentPlayer.WorkshopCuppoard = Data.CurrentPlayer.WorkshopCuppoard.Where(x => x != null).ToList();
            return currentMonster;

        }

        private bool ShowLabOptions()
        {
            bool status = true;
            bool halt = true;
            while (halt)
            {
                Console.WriteLine("0 - Menu");
                Console.WriteLine("1 - Work on the monster");
                Console.WriteLine("2 - Scrap unwanted parts");
                Console.WriteLine("3 - Repair monster's parts");
                Console.WriteLine("4 - Head out to the floor show");

                int intInput = StaticUtility.CheckInput(0, 4);
                int answer = 0;

                switch (intInput)
                {
                    case 0:
                        status = RunMenu();
                        halt = status;
                        break;
                    case 1:
                        if (Data.CurrentPlayer.Monster == null)
                        {
                            Data.CurrentPlayer.Monster = BuildMonster(true);
                        }
                        else
                        {
                            Data.CurrentPlayer.Monster = BuildMonster(false);
                        }
                        break;
                    case 2:
                        Console.WriteLine("Which Item would you like to scrap?");
                        Console.WriteLine("0 - Exit");
                        Console.WriteLine(_playerService.GetWorkshopItemList(Data.CurrentPlayer));
                        answer = StaticUtility.CheckInput(0, Data.CurrentPlayer.WorkshopCuppoard.Count);
                        if (answer != 0)
                        {
                            Console.WriteLine(_playerService.ScrapItem(Data.CurrentPlayer.SpareParts, Data.CurrentPlayer.WorkshopCuppoard, answer - 1));
                        }
                        break;
                    case 3:
                        if (Data.CurrentPlayer.Monster != null)
                        {
                            Console.WriteLine("Which Item would you like to repair?");
                            Console.WriteLine("0 - Exit");
                            int count = 0;
                            foreach (var part in Data.CurrentPlayer.Monster.Parts)
                            {
                                count++;
                                if (part != null)
                                {
                                    Console.WriteLine(count + " - " + part.PartName + ": Durability " + part.PartDurability);
                                }
                            }
                            answer = StaticUtility.CheckInput(0, 7);
                            if (answer != 0)
                            {
                                var part = Data.CurrentPlayer.Monster.Parts[answer - 1];
                                if (part == null)
                                {
                                    Console.WriteLine("Please pick an existing part to repair that part.");
                                }
                                else
                                {
                                    var cost = _playerService.GetRepairCost(part);
                                    if (!Data.CurrentPlayer.IsAI)
                                    {
                                        Console.WriteLine($"Full repair will cost {cost} {part.PartStructure} parts, but partial repair is possible.\r\n You currently have {Data.CurrentPlayer.SpareParts[part.PartStructure]}.");
                                        Console.WriteLine("Confirm repair?");
                                        Console.WriteLine("1 - Yes");
                                        Console.WriteLine("2 - No");
                                        intInput = StaticUtility.CheckInput(1, 2);
                                        if (intInput == 1)
                                        {
                                            Console.WriteLine(_playerService.OrchestratePartRepair(Data.CurrentPlayer, answer - 1));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("You need a monster to repair a monster.");
                        }
                        break;
                    case 4:
                        halt = false;
                        break;
                    default:
                        throw new Exception("Bad Input in GameController.ShowLabOptions");
                }
            }
            return status;
        }
        private void CalculateFights()
        {
            Queue<PlayerData> fighters = new Queue<PlayerData>();

            //find all available competitors
            foreach (var player in AllPlayers)
            {
                if (player.Monster != null && player.Monster.CanFight)
                {
                    fighters.Enqueue(player);
                }
            }

            //pair off
            if (fighters.Count == 0)
            {
                StaticUtility.TalkPause("There will be no show tonight!  Better luck gathering tomorrow");
            }
            else if (fighters.Count == 1)
            {
                StaticUtility.TalkPause("Only one of you managed to scrape together a monster?  No shows tonight, but rewards for the one busy beaver.");
                _battleService.GrantCash(fighters.Dequeue(), 1);
            }
            else
            {
                decimal countTotal = fighters.Count;
                //fight in rounds
                while (fighters.Count != 0)
                {
                    int round = 0;
                    if (fighters.Count == 1)
                    {
                        StaticUtility.TalkPause("And we have a winner!");
                        _battleService.GrantCash(fighters.Dequeue(), round);
                    }
                    else
                    {
                        PlayerData left = fighters.Dequeue();
                        PlayerData right = fighters.Dequeue();

                        var result = _battleService.MonsterFight(left, right);
                        foreach (var line in result.Text)
                        {
                            Console.WriteLine(line);
                            Thread.Sleep(1000);
                        }

                        fighters.Enqueue(result.Winner);

                    }
                    if (fighters.Count <= Math.Ceiling(countTotal / 2))
                    {
                        round = round + 1;
                        countTotal = fighters.Count;
                    }

                }

            }

            //apply luck to losers
        }
        private bool RunMenu()
        {
            bool gameStatus = true;

            Console.WriteLine("Would you like to quit?  Today's progress will not be saved.");
            Console.WriteLine("1 - Yes");
            Console.WriteLine("2 - No");
            int intInput = StaticUtility.CheckInput(1, 2);

            if (intInput == 1)
            {
                gameStatus = false;
            }

            return gameStatus;
        }
    }
}