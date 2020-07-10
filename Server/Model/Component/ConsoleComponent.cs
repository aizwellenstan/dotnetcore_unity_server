using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
    [ObjectSystem]
    public class ConsoleComponentAwakeSystem : StartSystem<ConsoleComponent>
    {
        public override void Start(ConsoleComponent self)
        {
            self.Start().Coroutine();
        }
    }

    public static class ConsoleMode
    {
        public const string None = "";
        public const string Repl = "repl";
    }

    public enum CreateUserMode
    {
        Random,
    }

    public enum DeleteUserMode
    {
        AllTestPlayer,
    }

    public class ConsoleComponent: Entity
    {
        public CancellationTokenSource CancellationTokenSource;
        public string Mode = "";

        public async ETVoid Start()
        {
            this.CancellationTokenSource = new CancellationTokenSource();

            while (true)
            {
                string line = string.Empty;
                try
                {
                    line = await Task.Factory.StartNew(() =>
                    {
                        Console.Write($"{this.Mode}> ");
                        return Console.In.ReadLine();
                    }, this.CancellationTokenSource.Token);
                    
                    line = line.Trim();

                    if (this.Mode != "")
                    {
                        bool isExited = true;
                        switch (this.Mode)
                        {
                            case ConsoleMode.Repl:
                            {
                                ReplComponent replComponent = this.GetComponent<ReplComponent>();
                                if (replComponent == null)
                                {
                                    Console.WriteLine($"no command: {line}!");
                                    break;
                                }
                            
                                try
                                {
                                    isExited = await replComponent.Run(line, this.CancellationTokenSource.Token);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }

                                break;
                            }
                        }

                        if (isExited)
                        {
                            this.Mode = "";
                        }

                        continue;
                    }

                    string[] argv = line.Split(' ').Where(e => !string.IsNullOrEmpty(e)).ToArray();
                    string mainCmd = argv[0];
                    switch (mainCmd)
                    {
                        case "create": 
                            {
                                switch (argv[1])
                                {
                                    case "equip":
                                        if(long.TryParse(argv[2], out long uid))
                                        {
                                            long.TryParse(argv[3], out long configId);
                                            int.TryParse(argv[4], out int count);
                                            Game.EventSystem.Run(EventIdType.CreateEquipment, uid, configId, count);
                                        }
                                        else
                                        {
                                            throw new NoSuchCommandException(line);
                                        }
                                        break;
                                    case "user":
                                        switch (argv[2])
                                        {
                                            case "-r": //隨機創造腳色
                                                if (string.IsNullOrEmpty(argv[3]))
                                                {
                                                    string countStr = await Task.Factory.StartNew(() =>
                                                    {
                                                        Console.Write($"How much do you want?:");
                                                        return Console.In.ReadLine();
                                                    }, this.CancellationTokenSource.Token);
                                                    argv[3] = countStr;
                                                    goto case "-r";
                                                }
                                                else
                                                {
                                                    if (int.TryParse(argv[3], out int count))
                                                    {
                                                        if (count > 50000)
                                                        {
                                                            Console.WriteLine("over limit count of bulk creating");
                                                            return;
                                                        }
                                                        Game.EventSystem.Run(EventIdType.CreateUser, CreateUserMode.Random, count);
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("invalid integer string");
                                                    }
                                                }
                                                break;
                                            default:
                                                throw new NoSuchCommandException(line);
                                        }
                                        break;
                                    default:
                                        throw new NoSuchCommandException(line);
                                }
                                break;
                            }
                        //TODO:記得設權限
                        case "delete": //刪除全部測試用的腳色(高風險指令)
                            {
                                switch (argv[1])
                                {
                                    case "equip":
                                        if (long.TryParse(argv[2], out long uid))
                                        {
                                            long.TryParse(argv[3], out long configId);
                                            int.TryParse(argv[4], out int count);
                                            Game.EventSystem.Run(EventIdType.DeleteEquipment, uid, configId, count);
                                        }
                                        else
                                        {
                                            throw new NoSuchCommandException(line);
                                        }
                                        break;
                                    case "user":
                                        if(argv.Contains("-t"))
                                        {
                                            if (argv.Contains("-a"))
                                            {
                                                Game.EventSystem.Run(EventIdType.DeleteUser, DeleteUserMode.AllTestPlayer);
                                            }
                                            else
                                            {
                                                throw new NoSuchCommandException(line);
                                            }
                                        }
                                        else
                                        {
                                            throw new NoSuchCommandException(line);
                                        }
                                        break;
                                    default:
                                        throw new NoSuchCommandException(line);
                                }
                                break;
                            }
                        case "mapunit": 
                            {
                                switch (argv[1])
                                {
                                    case "remove": // 移除所有不動的MapUnit
                                        if (argv.Length > 2)
                                        {
                                            throw new NoSuchCommandException(line);
                                        }
                                        else
                                        {
                                            Game.EventSystem.Run(EventIdType.RemoveMapunit);
                                        }
                                        break;
                                    case "-s": // 顯示MapUnit Data
                                        if (long.TryParse(argv[2], out long mapUnitId))
                                        {
                                            Game.EventSystem.Run(EventIdType.ShowMapUnit, mapUnitId);
                                        }
                                        else
                                        {
                                            Console.WriteLine("invalid int64 string");
                                        }
                                        break;
                                    default:
                                        throw new NoSuchCommandException(line);
                                }
                                break;
                            }
                        case "player":
                            switch (argv[1])
                            {
                                case "-s": // 顯示Player Data
                                    if (long.TryParse(argv[2], out long playerUid))
                                    {
                                        Game.EventSystem.Run(EventIdType.ShowPlayer, playerUid);
                                    }
                                    else
                                    {
                                        Console.WriteLine("invalid int64 string");
                                    }
                                    break;
                                default:
                                    throw new NoSuchCommandException(line);
                            }
                            break;
                        case "profiler":
                            {
                                switch (argv[1])
                                {
                                    case "-s": //顯示Server Profiler
                                        if(int.TryParse(argv[2], out int milisec))
                                        {
                                            if(milisec > 0)
                                            {
                                                Game.EventSystem.Run(EventIdType.ShowProfiler, milisec);
                                            }
                                            else
                                            {
                                                Console.WriteLine("invalid milisecond string");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("invalid integer string");
                                        }
                                        break;
                                    case "-h": //隱藏Server Profiler
                                        Game.EventSystem.Run(EventIdType.HideProfiler);
                                        break;
                                    default:
                                        throw new NoSuchCommandException(line);
                                }
                                break;
                            }
                        case "reload": 
                            try
                            {
                                switch (argv[1])
                                {
                                    case "bulletin":
                                        if (Game.Scene.GetComponent<LobbyComponent>() == null)
                                        {
                                            Console.WriteLine("to run command is failed! \r\nthe command is only running on AppType.Lobby");
                                        }
                                        else
                                        {
                                            Game.EventSystem.Run(EventIdType.ConfigRoload, new Type[] { typeof(AnnouncementSettingCategory) });
                                        }
                                        break;
                                    case "config":
                                        Game.EventSystem.Run(EventIdType.ConfigRoload);
                                        break;
                                    case "":
                                        Game.EventSystem.Add(DLLType.Hotfix, DllHelper.GetHotfixAssembly());
                                        break;
                                    default:
                                        throw new NoSuchCommandException(line);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                            break;
                        case "repl":
                            try
                            {
                                this.Mode = ConsoleMode.Repl;
                                this.AddComponent<ReplComponent>();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                            break;
                        case "room":
                            {
                                switch (argv[1])
                                {
                                    case "-s": // 顯示Room Data
                                        if (long.TryParse(argv[2], out long roomId))
                                        {
                                            Game.EventSystem.Run(EventIdType.ShowRoom, roomId);
                                        }
                                        else
                                        {
                                            Console.WriteLine("invalid int64 string");
                                        }
                                        break;
                                    default:
                                        throw new NoSuchCommandException(line);
                                }
                                break;
                            }
                        case "statistic":
                            switch (argv[1])
                            {
                                case "-t": // 顯示總統計數據
                                    if (argv.Length > 2)
                                    {
                                        throw new NoSuchCommandException(line);
                                    }
                                    else
                                    {
                                        Game.EventSystem.Run(EventIdType.PrintFullStatistic);
                                    }
                                    break;
                                default:
                                    throw new NoSuchCommandException(line);
                            }
                            break;
                        case "watcher":
                            switch (argv[1])
                            {
                                case "-s":
                                    List<int> targetList = new List<int>();
                                    foreach(var arg in argv.Skip(2))
                                    {
                                        if (int.TryParse(arg, out int count))
                                        {
                                            targetList.Add(count);
                                        }
                                    }
                                    if(targetList.Count == 0)
                                    {
                                        Console.WriteLine($"no any target needs to watch!");
                                    }
                                    else
                                    {
                                        Game.EventSystem.Run(EventIdType.ShowWatcher, targetList);
                                    }
                                    break;
                                case "-h":
                                    Game.EventSystem.Run(EventIdType.HideWatcher);
                                    break;
                            }
                            break;
                        case "npc":
                            switch (argv[1])
                            {
                                case "refresh":
                                    {
                                        if (argv.Length == 3)
                                        {
                                            if (long.TryParse(argv[2], out var targetId))
                                            {
                                                Game.EventSystem.Run(EventIdType.RefreshNPC, targetId);
                                            }
                                        }
                                        else if (argv.Length == 4)
                                        {
                                            if (long.TryParse(argv[2], out var minId))
                                            {
                                                long.TryParse(argv[3], out var maxId);
                                                if (maxId == 0)
                                                    maxId = minId;
                                                Game.EventSystem.Run(EventIdType.RefreshNPC, minId, maxId);
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine($"npc refresh [minId] [maxId] | npc refresh [Id]");
                                        }
                                    }
                                    break;
                                default:
                                    Console.WriteLine($"npc refresh [minId] [maxId] | npc refresh [Id]");
                                    break;
                            }
                            break;
                        case "tip":
                            {
                                if (argv.Length >= 2)
                                {
                                    if (long.TryParse(argv[1], out var messageTipId))
                                    {
                                        Game.EventSystem.Run(EventIdType.MessageTip, messageTipId);
                                    }
                                    else
                                    {
                                        Console.WriteLine($"tip [MessageTipSetting Id]");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"tip [MessageTipSetting Id]");
                                }
                            }
                            break;
                        default:
                            Console.WriteLine($"no such command: {line}");
                            break;
                    }
                }
                catch (Exception e)
                {
                    if(e is NoSuchCommandException noSuchCommandException)
                    {
                        Console.WriteLine($"no such command: {noSuchCommandException.line}");
                    }
                    else if(e is IndexOutOfRangeException indexOutOfRangeException) 
                    {
                        Console.WriteLine($"no such command: {line}");
                    }
                    else
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        private class NoSuchCommandException : Exception
        {
            public string line { private set; get; }

            public NoSuchCommandException(string line)
            {
                this.line = line;
            }
        }
    }
}