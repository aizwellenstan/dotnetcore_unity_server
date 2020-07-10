using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ETHotfix;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ETModel
{
    [ObjectSystem]
    public class ProfileAwakeSystem : AwakeSystem<ProfileComponent>
    {
        public override void Awake(ProfileComponent self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
    public class ProfileUpdateSystem : UpdateSystem<ProfileComponent>
    {
        public override void Update(ProfileComponent self)
        {
            self.Update();
        }
    }
}