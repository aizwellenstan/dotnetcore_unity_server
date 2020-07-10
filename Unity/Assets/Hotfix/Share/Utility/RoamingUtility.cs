using ETModel;
using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ETHotfix
{
    public static class RoamingUtility
    {
        public static async ETTask<L2C_RoamingGetList> GetMapList(Session session)
        {
            return await session.Call(new C2L_RoamingGetList()) as L2C_RoamingGetList;
        }

        public static async ETTask<L2C_RoamingEnter> EnterRoamingRoom(Session session, long roomId)
        {
            C2L_RoamingEnter c2G_EnterRoaming = new C2L_RoamingEnter() { RoamingRoomId = roomId };
            return await session.Call(c2G_EnterRoaming) as L2C_RoamingEnter;
        }
    }
}