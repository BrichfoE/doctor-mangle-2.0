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

namespace doctor_mangle_console_app
{
    public class GameController
    {
        private readonly IGameService _gameService;
        private readonly IPlayerService _playerService;
        private readonly IParkService _parkService;
        private readonly IBattleService _battleService;
        private readonly IComparer<BodyPart> _partComparer;
        private readonly GameRepo _gameRepo;
        private readonly string currentFile = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();

        public GameData Data { get; set; }
        private PlayerData[] AllPlayers;

        public GameController(
            IGameService gameService,
            IPlayerService playerService,
            IParkService parkService,
            IBattleService battleService,
            IComparer<BodyPart> partComparer,
            GameRepo gameRepo)
        {
            this._gameService = gameService;
            this._playerService = playerService;
            this._parkService = parkService;
            this._battleService = battleService;
            this._partComparer = partComparer;
            this._gameRepo = gameRepo;
        }

        public void Init()
        {
            _gameRepo.FileSetup();
            StaticConsoleHelper.TalkPause("Welcome to the Isle of Dr. Mangle.");
            if (_gameRepo.CanLoadGames())
            {
                Data = _gameRepo.LoadGameDialogue();
            }
            if (Data != null)
            {
                var players = new List<PlayerData>() { Data.CurrentPlayer };
                players.AddRange(Data.AiPlayers);
                this.AllPlayers = players.ToArray();
            }
            else
            {
                string textInput = "default";
                bool halt = true;
                while (halt)
                {
                    Console.WriteLine("Please enter a name for your game data:");
                    textInput = Console.ReadLine();
                    if (_gameRepo.GetGameIdFromName(textInput) != -1)
                    {
                        Console.WriteLine("A game by that name already exists.");
                    }
                    else
                    {
                        halt = false;
                    }
                }
                Console.WriteLine("And how many contestants will you be competing against?");
                var intInput = StaticConsoleHelper.CheckInput(1, 7);
                Data = _gameService.GetNewGameData(textInput, intInput, _gameRepo.GetNextGameID());
                AllPlayers = new PlayerData[Data.AiPlayers.Length + 1];
                AllPlayers[0] = Data.CurrentPlayer;
                var i = 1;
                foreach (var player in Data.AiPlayers)
                {
                    AllPlayers[i] = player;
                    i++;
                }
                _gameRepo.SaveGame(Data);
            }
        }

        public bool RunGame()
        {
            var intInput = 0;
            bool gameStatus = true;

            #region search
            StaticConsoleHelper.TalkPause("A new day has dawned!");
            StaticConsoleHelper.TalkPause("The parks will be open for 5 hours...");
            StaticConsoleHelper.TalkPause("You will then have one more hour in your labs before the evening's entertainment.");

            for (int i = 1; i < 6; i++)
            {
                try
                {
                    StaticConsoleHelper.TalkPause("It is currently " + i + " o'clock. The parks close at 6.");
                    Console.WriteLine($"You are currently in the {Data.RegionText}, \r\n what will you do next?");
                    Console.WriteLine(_gameService.PrintRegionOptions(Data));
                    intInput = StaticConsoleHelper.CheckInput(1, 4);
                    Data.CurrentRegion = intInput;

                    gameStatus = PlayerSearchTurn(i - 1);
                    _gameService.AISearchTurn(Data, i);
                    if (!gameStatus)
                    {
                        return gameStatus;
                    }
                }
                catch (System.Exception ex)
                {
                    int currentLine = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber();
                    _gameRepo.LogException(Data, $"Search Phase exception {currentFile} line {currentLine}", ex, false);
                }
            }
            #endregion

            #region build
            try
            {
                StaticConsoleHelper.TalkPause("It is now 6 o'clock. Return to your lab and prepare for the floorshow at 7.");
                Data.CurrentRegion = 0;
                foreach (var player in AllPlayers)
                {
                    _playerService.DumpBagIntoWorkshop(player);
                }
                Console.WriteLine("Bag contents added to workshop inventory.");
                gameStatus = PlayerLabTurn();
                if (!gameStatus)
                {
                    return gameStatus;
                }
            }
            catch (System.Exception ex)
            {
                int currentLine = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber();
                _gameRepo.LogException(Data, $"Player Build Phase exception {currentFile} line {currentLine}\n", ex, false);
            }

            try
            {
                _gameService.AIBuildTurn(Data);
            }
            catch (Exception ex)
            {
                int currentLine = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber();
                _gameRepo.LogException(Data, $"AI Build Phase exception {currentFile} line {currentLine}\n", ex, false);
            }


            #endregion

            #region fight
            try
            {
                StaticConsoleHelper.TalkPause("Welcome to the evening's entertainment!");
                if (Data.CurrentPlayer.Monster != null && Data.CurrentPlayer.Monster.CanFight)
                {
                    Console.WriteLine("Would you like to particpate tonight?");
                    StaticConsoleHelper.TalkPause("1 - Yes, 2 - No");
                    intInput = StaticConsoleHelper.CheckInput(1, 2);
                    if (intInput != 1)
                    {
                        StaticConsoleHelper.TalkPause("Well, maybe tomorrow then...");
                        Console.WriteLine("Let's find you a comfortable seat.");

                    }
                    else
                    {
                        StaticConsoleHelper.TalkPause("Let the games begin!");
                    }
                }
                else
                {
                    StaticConsoleHelper.TalkPause("Seeing as you do not have a living, able bodied contestant...");
                    Console.WriteLine("Let's find you a comfortable seat.");
                }
                ManageBattlePhase();
            }
            catch (Exception ex)
            {
                int currentLine = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber();
                _gameRepo.LogException(Data, $"Fighting Phase exception {currentFile} line {currentLine}\n", ex, false);
            }
            #endregion

            #region dayEnd
            try
            {
                AllPlayers = _playerService.SortPlayersByWins(AllPlayers);
                Data.Parks = _parkService.AddParts(Data.Parks, AllPlayers.Length);
                Data.Parks = _parkService.HalveParts(Data.Parks);
                Data.GameDayNumber++;
                _gameRepo.SaveGame(Data);
            }
            catch (Exception ex)
            {
                int currentLine = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber();
                _gameRepo.LogException(Data, $"End of Day Phaseexception {currentFile} line {currentLine}\n", ex, false);
            }
            return gameStatus;
            #endregion
        }
        private bool PlayerSearchTurn(int bagSlot)
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

                intInput = StaticConsoleHelper.CheckInput(0, 4);

                switch (intInput)
                {
                    case 0:
                        status = RunMenu();
                        searching = status;
                        break;
                    case 1:
                        if (_parkService.SearchForPart(Data.Parks[Data.CurrentRegion], out BodyPart part))
                        {
                            Data.CurrentPlayer.Bag[bagSlot] = part;
                            Console.WriteLine("You found: " + part.PartName);
                        }
                        else
                        {
                            Console.WriteLine("There are no more parts in this region");
                        }
                        searching = false;
                        break;
                    case 2:
                        Console.WriteLine(_parkService.PrintPartCounts(Data.Parks));
                        searching = false;
                        break;
                    case 3:
                        Console.WriteLine(_playerService.CheckBag(Data.CurrentPlayer));
                        break;
                    case 4:
                        Console.WriteLine($"You are currently in the {Data.RegionText}, \r\n what will you do next?");
                        Console.WriteLine(_gameService.PrintRegionOptions(Data));
                        intInput = StaticConsoleHelper.CheckInput(1, 4);
                        Data.CurrentRegion = intInput;
                        break;
                    default:
                        throw new Exception("Bad Input in GameController.ShowSearchOptions");
                }
            }
            return status;
        }
        private MonsterData PlayerBuildMonster(bool isNew)
        {
            var table = new List<BodyPart>();
            List<Head> heads = Data.CurrentPlayer.WorkshopCuppoard
                .Where(x => x.GetType() == typeof(Head))
                .Select(y => (Head)y).ToList();

            List<Torso> torsos = Data.CurrentPlayer.WorkshopCuppoard
                .Where(x => x.GetType() == typeof(Torso))
                .Select(y => (Torso)y).ToList();

            List<BodyPart> limbs = Data.CurrentPlayer.WorkshopCuppoard
                .Where(x => x.GetType() != typeof(Head)
                    && x.GetType() == typeof(Torso))
                .ToList();

            MonsterData currentMonster = Data.CurrentPlayer.Monster;

            if (isNew)
            {
                Console.WriteLine("You aproach the empty table...");
            }
            else
            {
                Console.WriteLine(currentMonster.Name + " slides onto the table...");
                table.AddRange(currentMonster.Parts);
            }

            // ensure a viable monster is possible
            if (heads.Count == 0 && !table.Where(x => x.PartType.GetType() == typeof(Head)).Any())
            {
                Console.WriteLine($"You do not have a head in your workshop.\r\nA monster without one is no moster at all, better luck tomorrow...");
                return currentMonster;
            }
            if (torsos.Count == 0 && !table.Where(x => x.PartType.GetType() == typeof(Torso)).Any())
            {
                Console.WriteLine($"You do not have a torso in your workshop.\r\nA monster without one is no moster at all, better luck tomorrow...");
                return currentMonster;
            }
            if (limbs.Count == 0 && !table.Where(x => x.PartType.GetType() != typeof(Head) || x.PartType.GetType() != typeof(Torso)).Any())
            {
                Console.WriteLine($"You do not have any limbs in your workshop.\r\nBetter luck tomorrow...");
                return currentMonster;
            }

            _playerService.TryBuildMonster(Data.CurrentPlayer);
            return Data.CurrentPlayer.Monster;

            //int intInput;
            //bool leave = false;
            //bool halt = true;

            //while (halt)
            //{
            //    Console.WriteLine("Workshop Items:");
            //    Console.WriteLine("0 - Leave Table");
            //    int count = 0;
            //    foreach (var item in workshopCopy)
            //    {
            //        count++;
            //        Console.WriteLine(count + " - " + item.PartName);
            //    }
            

            //        Console.WriteLine("Please choose a " + type + ":");
            //        intInput = StaticConsoleHelper.CheckInput(0, StaticUtility.NonNullCount(workshopCopy));

            //        if (intInput == 0)
            //        {
            //            halt = false;
            //            leave = true;
            //            break;
            //        }

            //        BodyPart chosenPart = workshopCopy[intInput - 1];

            //        Console.WriteLine(chosenPart.PartName);
            //        if (chosenPart.PartType != type)
            //        {
            //            Console.WriteLine("That is not a " + type + "!");
            //        }
            //        else
            //        {
            //            Console.WriteLine("Durability: " + chosenPart.PartDurability);
            //            Console.WriteLine("Alacrity: " + chosenPart.PartStats[Stat.Alacrity]);
            //            Console.WriteLine("Strenght: " + chosenPart.PartStats[Stat.Strength]);
            //            Console.WriteLine("Endurance: " + chosenPart.PartStats[Stat.Endurance]);
            //            StaticConsoleHelper.TalkPause("Technique: " + chosenPart.PartStats[Stat.Technique]);
            //            Console.WriteLine("Use this part?");
            //            Console.WriteLine("1 - Yes");
            //            Console.WriteLine("2 - No");
            //            Console.WriteLine("3 - Skip part");
            //            Console.WriteLine("4 - Leave Table");
            //            int intInput2 = StaticConsoleHelper.CheckInput(1, 4);

            //            switch (intInput2)
            //            {
            //                case 1:
            //                    table.Add(chosenPart);
            //                    workshopCopy.RemoveAt(intInput - 1);
            //                    break;
            //                case 2:
            //                    break;
            //                case 3:
            //                    halt = false;
            //                    break;
            //                case 4:
            //                    leave = true;
            //                    halt = false;
            //                    break;
            //                default:
            //                    break;
            //            }

            //        }

            //    }
            //    //leave table
            //    if (leave)
            //    {
            //        break;
            //    }
            

            //if (table[0] != null && table[1] != null)
            //{
            //    MonsterData newMonster = new MonsterData(null, table);

            //    StaticConsoleHelper.TalkPause("This is your monster...");
            //    foreach (var part in table)
            //    {
            //        if (part != null)
            //        {
            //            Console.WriteLine(part.PartName);
            //        }
            //    }
            //    int count = 0;
            //    foreach (var stat in newMonster.MonsterStats)
            //    {
            //        // Console.WriteLine(StaticReference.statList[count] + ": " + stat);
            //        count++;
            //    }
            //    Console.WriteLine("Would you like to keep this monster?");
            //    Console.WriteLine("1 - Yes, 2 - No");
            //    intInput = StaticConsoleHelper.CheckInput(1, 2);
            //    if (intInput == 1)
            //    {
            //        if (isNew)
            //        {
            //            Console.WriteLine("What is its name?");
            //            currentMonster = newMonster;
            //            currentMonster.Name = Console.ReadLine();

            //        }
            //        else
            //        {
            //            foreach (var part in table)
            //            {
            //                currentMonster.Parts.Add(part);
            //            }
            //        }
            //        Data.CurrentPlayer.WorkshopCuppoard = workshopCopy.Select(x => x).ToList();
            //    }
            //    else
            //    {
            //        Console.WriteLine("Better luck building tomorrow...");
            //    }
            //}

            //Data.CurrentPlayer.WorkshopCuppoard = Data.CurrentPlayer.WorkshopCuppoard.Where(x => x != null).ToList();
            //return currentMonster;

        }

        private bool PlayerLabTurn()
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

                int intInput = StaticConsoleHelper.CheckInput(0, 4);
                int answer = 0;

                switch (intInput)
                {
                    case 0:
                        status = RunMenu();
                        halt = status;
                        break;
                    case 1:
                        Data.CurrentPlayer.Monster = PlayerBuildMonster(Data.CurrentPlayer.Monster == null);
                        break;
                    case 2:
                        Console.WriteLine("Which Item would you like to scrap?");
                        Console.WriteLine("0 - Exit");
                        Console.WriteLine(_playerService.GetWorkshopItemList(Data.CurrentPlayer));
                        answer = StaticConsoleHelper.CheckInput(0, Data.CurrentPlayer.WorkshopCuppoard.Count);
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
                            answer = StaticConsoleHelper.CheckInput(0, 7);
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
                                        intInput = StaticConsoleHelper.CheckInput(1, 2);
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

        // TODO: Move this to the BattleService, this should return a comprehensive script with each fight script nested inside
        private void ManageBattlePhase()
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
                StaticConsoleHelper.TalkPause("There will be no show tonight!  Better luck gathering tomorrow");
            }
            else if (fighters.Count == 1)
            {
                StaticConsoleHelper.TalkPause("Only one of you managed to scrape together a monster?  No shows tonight, but rewards for the one busy beaver.");
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
                        StaticConsoleHelper.TalkPause("And we have a winner!");
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
            int intInput = StaticConsoleHelper.CheckInput(1, 2);

            if (intInput == 1)
            {
                gameStatus = false;
            }

            return gameStatus;
        }
    }
}
