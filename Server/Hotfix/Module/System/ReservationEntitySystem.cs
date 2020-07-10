using ETModel;
using Google.Protobuf.Collections;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using static ETModel.Reservation;

namespace ETHotfix
{
    [ObjectSystem]
    public class ReservationEntityAwakeSystem : AwakeSystem<Reservation>
    {
        public override void Awake(Reservation self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
    public class ReservationEntityUpdateSystem : UpdateSystem<Reservation>
    {
        public override void Update(Reservation self)
        {
            if (!self.IsInitialized)
                return;
            CheckState(self);
        }

        private async void CheckState(Reservation self)
        {
            if (self.allData == null)
                return;

            switch (self.State)
            {
                case ReservationState.Sleep:
                    {
                        if (DateTime.UtcNow.Ticks > self.allData.AwakeUTCTimeTick)
                        {
                            self.State = ReservationState.Show;
                            SwitchState(self, ReservationState.Show);
                        }
                    }
                    break;
                case ReservationState.Show:
                    {
                        if (self.room != null && DateTime.UtcNow.Ticks > self.allData.StartUTCTimeTick)
                        {
                            if (self.room.info.NowMemberCount > 0)
                            {
                                SwitchState(self, ReservationState.Run);
                            }
                            else
                            {
                                // 刪除房間實體
                                var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();
                                await lobbyComponent.DestroyRoom(self.room.Id);
                                SwitchState(self, ReservationState.Destroy);
                            }
                        }
                    }
                    break;
                case ReservationState.Run:
                    {
                        SwitchState(self, ReservationState.Destroy);
                    }
                    break;
                case ReservationState.Destroy:
                    break;
            }
        }

        private async void SwitchState(Reservation self, ReservationState state)
        {
            self.State = state;

            switch (self.State)
            {
                case ReservationState.Sleep:
                    break;
                case ReservationState.Show:
                    {
                        CreateRoomAsync(self);
                    }
                    break;
                case ReservationState.Run:
                    {
                        try
                        {
                            if (self.room == null)
                            {
                                return;
                            }

                            if (self.room.Type != RoomType.Team)
                            {
                                return;
                            }

                            if (self.room.State != RoomState.Start)
                            {
                                return;
                            }

                            var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();
                            await lobbyComponent.RunRoomOnTeam(self.room.Id);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                    break;
                case ReservationState.Destroy:
                    {
                        var reservationComponent = Game.Scene.GetComponent<ReservationComponent>();
                        await reservationComponent.DestroyReservation(self.Id);
                    }
                    break;
            }

            // 同步預約的狀態
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            var memsync = proxy.GetMemorySyncSolver<Reservation>();
            await memsync.Update(self);
        }

        private async void CreateRoomAsync(Reservation self)
        {
            var roomInfo = new RoomInfo()
            {
                RoomId = 0,
                Title = self.allData.SenderName,
                RoadSettingId = self.allData.RoadSettingId,
                MaxMemberCount = 8,
                NowMemberCount = 0,
            };

            var teamRoomData = new TeamRoomData()
            {
                LeaderUid = -1,
                LeaderName = self.allData.SenderName,
                StartUTCTimeTick = self.allData.StartUTCTimeTick,
                IsReservation = true,
            };

            var reservationMembers = new RepeatedField<ReservationMemberData>();
            for (int i = 0; i < self.allData.MemberUid?.count; i++)
            {
                var uid = self.allData.MemberUid[i];
                User user = await UserDataHelper.FindOneUser(uid);
                if (user == null)
                {
                    Log.Error($"Reservation CreateRoomAsync Failed, Can't find user, uid:{uid}");
                    continue;
                }
                var reservationMemberData = new ReservationMemberData()
                {
                    MemberIndex = i,
                    Uid = user.Id,
                    Name = user.name,
                    Location = user.location,
                };
                reservationMembers.Add(reservationMemberData);
            }

            var lobbyComponent = Game.Scene.GetComponent<LobbyComponent>();
            // 隨機一個Map給要預約的房間
            var startConfig = NetworkHelper.GetRandomMap();
            self.room = await lobbyComponent.CreateTeamRoom(startConfig.AppId, roomInfo, teamRoomData);
            var reservationComponent = Game.Scene.GetComponent<ReservationComponent>();
            await reservationComponent.UpdateReservation(self);
            await lobbyComponent.SetReservationMember(self.room.Id, reservationMembers);
        }
    }

    [ObjectSystem]
    public class ReservationEntityDestroySystem : DestroySystem<Reservation>
    {
        public override async void Destroy(Reservation self)
        {
            //刪除DB
            await ReservationDataHelper.Remove(self.allData.ReservationId);
        }
    }
}