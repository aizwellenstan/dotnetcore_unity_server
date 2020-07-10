using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using ETModel;
using System;
using Google.Protobuf.Collections;
using MongoDB.Bson.Serialization;

namespace ETHotfix
{
    [ObjectSystem]
    public class LobbyComponentStartSystem : StartSystem<LobbyComponent>
    {
        public override void Start(LobbyComponent self)
        {
            self.Start();
        }
    }

    public static class LobbyComponentSystem
    {
        public static async ETTask<Room> CreateTeamRoom(this LobbyComponent self, int mapAppId, RoomInfo roomInfo, TeamRoomData teamRoomData)
        {
            // 等候房間創建完成，並且也同步Room到了Lobby
            Session mapSession = SessionHelper.GetMapSession(mapAppId);
            M2L_TeamCreate m2L_TeamCreate = (M2L_TeamCreate)await mapSession.Call(new L2M_TeamCreate 
            {
                Info = roomInfo,
                Data = teamRoomData,
            });

            if(m2L_TeamCreate.Error != ErrorCode.ERR_Success)
            {
                return null;
            }

            Room room = BsonSerializer.Deserialize<Room>(m2L_TeamCreate.Json);
            CacheExHelper.WriteInCache(room, out room);

            return room;
        }

        public static async ETTask<Tuple<TeamRoomData, RepeatedField<TeamMemberData>, RepeatedField<ReservationMemberData>>> GetTeamInfo(this LobbyComponent self, long roomId)
        {
            int mapAppId = IdGenerater.GetAppId(roomId);
            Session mapSession = SessionHelper.GetMapSession(mapAppId);
            M2L_GetTeamData m2L_GetTeamData = (M2L_GetTeamData)await mapSession.Call(new L2M_GetTeamData
            {
                RoomId = roomId,
            });
            if(m2L_GetTeamData.Error != ErrorCode.ERR_Success)
            {
                return null;
            }
            var room = BsonSerializer.Deserialize<Room>(m2L_GetTeamData.RoomJson);
            CacheExHelper.WriteInCache(room, out _);
            return new Tuple<TeamRoomData, RepeatedField<TeamMemberData>, RepeatedField<ReservationMemberData>>
                (m2L_GetTeamData.TeamData, m2L_GetTeamData.TeamMember, m2L_GetTeamData.ReservationTeamData);
        }

        public static async ETTask<RepeatedField<MapUnitInfo>> GetAllMapUnitInfoOnRoom(this LobbyComponent self, long roomId)
        {
            int mapAppId = IdGenerater.GetAppId(roomId);
            Session mapSession = SessionHelper.GetMapSession(mapAppId);
            M2L_GetAllMapUnitInfoOnRoom m2L_GetAllMapUnitInfoOnRoom = (M2L_GetAllMapUnitInfoOnRoom)await mapSession.Call(new L2M_GetAllMapUnitInfoOnRoom
            {
                RoomId = roomId,
            });
            if (m2L_GetAllMapUnitInfoOnRoom.Error != ErrorCode.ERR_Success)
            {
                return new RepeatedField<MapUnitInfo>();
            }
            return m2L_GetAllMapUnitInfoOnRoom.Data;
        }

        public static async ETTask<bool> DestroyRoom(this LobbyComponent self, long roomId)
        {
            int mapAppId = IdGenerater.GetAppId(roomId);
            Session mapSession = SessionHelper.GetMapSession(mapAppId);
            M2L_DestroyRoom m2L_DestroyRoom = (M2L_DestroyRoom)await mapSession.Call(new L2M_DestroyRoom
            {
                RoomId = roomId,
            });
            if (m2L_DestroyRoom.Error != ErrorCode.ERR_Success)
            {
                return false;
            }
            return true;
        }

        public static async ETTask<bool> SetReservationMember(this LobbyComponent self, long roomId, RepeatedField<ReservationMemberData> reservationMemberDatas)
        {
            int mapAppId = IdGenerater.GetAppId(roomId);
            Session mapSession = SessionHelper.GetMapSession(mapAppId);
            M2L_SetReservationMember m2L_SetReservationMember = (M2L_SetReservationMember)await mapSession.Call(new L2M_SetReservationMember
            {
                RoomId = roomId,
                Data = reservationMemberDatas,
            });
            if (m2L_SetReservationMember.Error != ErrorCode.ERR_Success)
            {
                return false;
            }
            return true;
        }

        public static async ETTask<bool> RunRoomOnTeam(this LobbyComponent self, long roomId)
        {
            int mapAppId = IdGenerater.GetAppId(roomId);
            Session mapSession = SessionHelper.GetMapSession(mapAppId);
            M2L_RunRoomOnTeam m2L_RunRoomOnTeam = (M2L_RunRoomOnTeam)await mapSession.Call(new L2M_RunRoomOnTeam
            {
                RoomId = roomId,
            });
            if (m2L_RunRoomOnTeam.Error != ErrorCode.ERR_Success)
            {
                return false;
            }
            return true;
        }

        public static async ETTask<RepeatedField<MapUnitInfo_Global>> GetAllMapUnitGlobalInfoOnRoom(this LobbyComponent self, long roomId)
        {
            int mapAppId = IdGenerater.GetAppId(roomId);
            Session mapSession = SessionHelper.GetMapSession(mapAppId);
            M2L_GetAllMapUnitGlobalInfoOnRoom m2L_GetAllMapUnitGlobalInfoOnRoom = (M2L_GetAllMapUnitGlobalInfoOnRoom)await mapSession.Call(new L2M_GetAllMapUnitGlobalInfoOnRoom
            {
                RoomId = roomId,
            });
            if (m2L_GetAllMapUnitGlobalInfoOnRoom.Error != ErrorCode.ERR_Success)
            {
                return new RepeatedField<MapUnitInfo_Global>();
            }
            return m2L_GetAllMapUnitGlobalInfoOnRoom.Data;
        }

        public static async ETTask<TeamMemberData> GetTeamMember(this LobbyComponent self, long uid, long roomId)
        {
            int mapAppId = IdGenerater.GetAppId(roomId);
            Session mapSession = SessionHelper.GetMapSession(mapAppId);
            M2L_GetTeamMember m2L_GetTeamMember = (M2L_GetTeamMember)await mapSession.Call(new L2M_GetTeamMember
            {
                Uid = uid,
                RoomId = roomId,
            });
            if (m2L_GetTeamMember.Error != ErrorCode.ERR_Success)
            {
                return null;
            }
            return m2L_GetTeamMember.MemberData;
        }

        public static async ETTask<bool> BroadcastTeamModifyMember(this LobbyComponent self, long uid, long roomId)
        {
            L2M_TeamModifyMember l2M_TeamModifyMember = new L2M_TeamModifyMember
            {
                Uid = uid,
                RoomId = roomId,
            };
            int mapAppId = IdGenerater.GetAppId(roomId);
            Session mapSession = SessionHelper.GetMapSession(mapAppId);
            M2L_TeamModifyMember m2L_TeamModifyMember = (M2L_TeamModifyMember)await mapSession.Call(l2M_TeamModifyMember);
            if (m2L_TeamModifyMember.Error != ErrorCode.ERR_Success)
            {
                return false;
            }
            return true;
        }

        public static async ETTask<bool> BroadcastTeamLose(this LobbyComponent self, long roomId)
        {
            L2M_TeamLose l2M_TeamLose = new L2M_TeamLose
            {
                RoomId = roomId,
            };
            int mapAppId = IdGenerater.GetAppId(roomId);
            Session mapSession = SessionHelper.GetMapSession(mapAppId);
            M2L_TeamLose m2L_TeamLose = (M2L_TeamLose)await mapSession.Call(l2M_TeamLose);
            if (m2L_TeamLose.Error != ErrorCode.ERR_Success)
            {
                return false;
            }
            return true;
        }

        public static async ETTask<bool> DestroyMapUnit(this LobbyComponent self, long mapUnitId)
        {
            if(mapUnitId == 0)
            {
                return false;
            }
            L2M_DestroyMapUnit l2M_DestroyMapUnit = new L2M_DestroyMapUnit
            {
                MapUnitId = mapUnitId,
            };
            int mapAppId = IdGenerater.GetAppId(mapUnitId);
            Session mapSession = SessionHelper.GetMapSession(mapAppId);
            M2L_DestroyMapUnit m2L_TeamLose = (M2L_DestroyMapUnit)await mapSession.Call(l2M_DestroyMapUnit);
            if (m2L_TeamLose.Error != ErrorCode.ERR_Success)
            {
                return false;
            }
            return true;
        }

        public static async ETTask<int> LeaveRoom(this LobbyComponent self, long roomId, long uid)
        {
            Room room = self.GetRoom(roomId);
            if(room == null)
            {
                return ErrorCode.ERR_RoomIdNotFound;
            }

            var roomType = room.Type;

            switch (roomType)
            {
                case RoomType.Roaming:
                    return await self.LeaveRoaming(roomId, uid);
                case RoomType.Team:
                    return await self.LeaveTeam(roomId, uid);
                default:
                    return ErrorCode.ERR_RoomIdNotFound;
            }
        }

        public static async ETTask<int> LeaveRoaming(this LobbyComponent self, long roomId, long uid)
        {
            L2M_RoamingLeave l2M_RoamingLeave = new L2M_RoamingLeave()
            {
                Uid = uid,
            };
            int mapAppId = IdGenerater.GetAppId(roomId);
            Session mapSession = SessionHelper.GetMapSession(mapAppId);
            M2L_RoamingLeave m2L_RoamingLeave = (M2L_RoamingLeave)await mapSession.Call(l2M_RoamingLeave);
            return m2L_RoamingLeave.Error;
        }

        public static async ETTask<int> LeaveTeam(this LobbyComponent self, long roomId, long uid)
        {
            L2M_TeamLeave l2M_TeamLeave = new L2M_TeamLeave()
            {
                Uid = uid,
            };
            int mapAppId = IdGenerater.GetAppId(roomId);
            Session mapSession = SessionHelper.GetMapSession(mapAppId);
            M2L_TeamLeave m2L_TeamLeave = (M2L_TeamLeave)await mapSession.Call(l2M_TeamLeave);
            return m2L_TeamLeave.Error;
        }

        public static int GetTeamRoomCount(this LobbyComponent self)
        {
            return self.TeamList.Count;
        }

        public static int GetMaxTeamRoomLimitCount(this LobbyComponent self)
        {
            var configComponent = Game.Scene.GetComponent<ConfigComponent>();
            var mapLimitSetting = (MapLimitSetting)configComponent.Get(typeof(MapLimitSetting), 0);
            return mapLimitSetting != null ? mapLimitSetting.MaxPartyCount : 150;
        }
    }
}