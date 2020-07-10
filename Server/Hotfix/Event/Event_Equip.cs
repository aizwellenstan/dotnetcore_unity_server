using ETModel;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Linq;

namespace ETHotfix
{
    [Event(EventIdType.CreateEquipment)]
    public class Event_CreateEquipment : AEvent<long, long, int>
    {
        public override async void Run(long uid, long configId, int count)
        {
            var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();
            if(lobbyComponent == null)
            {
                Console.WriteLine($"The command needs to use lobbyComponent");
                return;
            }
            if(uid <= 0)
            {
                Console.WriteLine($"uid:{uid} <= 0 is invalid");
                return;
            }
            if (!EquipmentDataHelper.TryGetEquipmentConfig(configId, out CharacterConfig characterConfig))
            {
                Console.WriteLine($"configId:{configId} is invalid");
                return;
            }
            if (count <= 0)
            {
                Console.WriteLine($"count:{count} <= 0 is invalid");
                return;
            }

            var result = await EquipmentDataHelper.CreateEquipment(uid,
                new List<EquipmentInfo> 
                {
                    new EquipmentInfo 
                    {
                        Id = 0,
                        ConfigId = configId,
                        Count = count
                    } 
                },
                EquipmentDataHelper.EquipmentFrom.System,
                (int)EquipmentDataHelper.SystemUid.Console);

            if(result.error == ErrorCode.ERR_Success)
            {
                GateMessageHelper.BroadcastTarget(new L2C_OnEquipmentsCreated
                {
                    FromUid = (int)EquipmentDataHelper.SystemUid.Console,
                    EquipmentInfoList = result.equipmentInfos,
                    UserBagInfo = result.userBagInfo,
                }, uid);

                Console.WriteLine($"ok");
            }
            else
            {
                Console.WriteLine($"error code: {result.error}");
            }
        }
    }

    [Event(EventIdType.DeleteEquipment)]
    public class Event_DeleteEquipment : AEvent<long, long, int>
    {
        public override async void Run(long uid, long configId, int count)
        {
            var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();
            if (lobbyComponent == null)
            {
                Console.WriteLine($"The command needs to use lobbyComponent");
                return;
            }
            if (uid <= 0)
            {
                Console.WriteLine($"uid:{uid} <= 0 is invalid");
                return;
            }
            if (!EquipmentDataHelper.TryGetEquipmentConfig(configId, out CharacterConfig characterConfig))
            {
                Console.WriteLine($"configId:{configId} is invalid");
                return;
            }
            if (count <= 0)
            {
                Console.WriteLine($"count:{count} <= 0 is invalid");
                return;
            }

            var result = await EquipmentDataHelper.DeleteEquipment(uid,
                new List<EquipmentInfo>
                {
                    new EquipmentInfo
                    {
                        Id = 0,
                        ConfigId = configId,
                        Count = count
                    }
                },
                EquipmentDataHelper.EquipmentFrom.System,
                (int)EquipmentDataHelper.SystemUid.Console, true);

            if (result.error == ErrorCode.ERR_Success)
            {
                GateMessageHelper.BroadcastTarget(new L2C_OnEquipmentsDeleted
                {
                    FromUid = (int)EquipmentDataHelper.SystemUid.Console,
                    EquipmentInfoList = result.equipmentInfos,
                    UserBagInfo = result.userBagInfo,
                }, uid);

                Console.WriteLine($"ok");
            }
            else
            {
                Console.WriteLine($"error code: {result.error}");
            }
        }
    }
}
