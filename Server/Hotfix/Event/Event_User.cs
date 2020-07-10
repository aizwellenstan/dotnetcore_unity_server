using ETModel;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Linq;

namespace ETHotfix
{
    [Event(EventIdType.CreateUser)]
    public class Event_CreateUser : AEvent<CreateUserMode, int>
    {
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private readonly Random random = new Random();

        public override async void Run(CreateUserMode mode, int count)
        {
            var db = Game.Scene.GetComponent<DBComponent>();
            if(db == null)
            {
                Console.WriteLine("To run the command only AppType == DB");
                return;
            }

            if(count <= 0)
            {
                Console.WriteLine("expect count > 0 but <= 0 that is invalid!");
                return;
            }
            switch (mode)
            {
                case CreateUserMode.Random:
                    Queue<string> emails = new Queue<string>(count);
                    List<string> existed = new List<string>();
                    string prefix = string.Empty;

                    for (int i = 0; i < count; i++)
                    {
                        do
                        {
                            prefix = new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
                        } while (emails.Contains(prefix));
                        emails.Enqueue(prefix);
                    }
                    for (int j = 0; j < count; j++)
                    {
                        prefix = emails.Dequeue();
                        var result = await AuthenticationHelper.AuthenticationByBot(prefix);
                        if(result == ErrorCode.ERR_Success)
                        {
                            Console.WriteLine($"to create user:{prefix} is successful!");
                        }
                        else
                        {
                            existed.Add(prefix);
                        }
                    }
                    Console.WriteLine("to create all users is finished!");
                    Console.WriteLine($"repeated user list:{existed.ToJson()}");
                    break;
                default:
                    Console.WriteLine("to create user failed! reason: no supported mode");
                    break;
            }
        }
    }

    [Event(EventIdType.DeleteUser)]
    public class Event_DeleteUser : AEvent<DeleteUserMode>
    {
        public override async void Run(DeleteUserMode mode)
        {
            var db = Game.Scene.GetComponent<DBComponent>();
            if (db == null)
            {
                Console.WriteLine("To run the command only AppType == DB");
                return;
            }

            switch (mode)
            {
                case DeleteUserMode.AllTestPlayer:
                    var proxy = Game.Scene.GetComponent<DBProxyComponent>();
                    var results = await proxy.Query<User>(entity => entity.identity == (int)User.Identity.TestPlayer);
                    var userIds = results.Select(e => e.Id);
                    await proxy.DeleteJson<User>(entity => userIds.Contains(entity.Id));
                    Console.WriteLine("to delete all test user is successful!");
                    break;
                default:
                    Console.WriteLine("to create user failed! reason: no supported mode");
                    break;
            }
        }
    }
}
