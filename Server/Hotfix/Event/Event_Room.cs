using ETModel;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Linq;

namespace ETHotfix
{
    [Event(EventIdType.ShowRoom)]
    public class Event_Room : AEvent<long>
    {
        public override void Run(long roomId)
        {
            var roomComponent = Game.Scene.GetComponent<RoomComponent>();
            if (roomComponent == null)
            {
                Console.WriteLine("roomComponent is null");
                return;
            }
            var room = roomComponent.Get(roomId);
            if(room == null)
            {
                Console.WriteLine("room is null");
                return;
            }
            Console.WriteLine($"Room[{roomId}]: {room.ToJson()}");
        }
    }
}
