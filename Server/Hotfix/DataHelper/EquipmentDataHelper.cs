using Google.Protobuf;
using Google.Protobuf.Collections;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using ETModel;

namespace ETHotfix
{
    public static class EquipmentDataHelper
    {
        public const string UsedCount = "usedCount_Type_{0}";

        public const string TotalCount = "totalCount_Type_{0}";

        public enum SystemUid
        {
            Console = -1,
        }

        public enum EquipmentFrom
        {
            System,

            Player,
        }

        private class EquipmentLog : Entity
        {
            public EquipmentFrom fromType;

            public int from;

            public int count;
        }

        public class EquipmentResult
        {
            public int error;

            public RepeatedField<EquipmentInfo> equipmentInfos;

            public UserBagCapacity userBagInfo;
        }

        private static DBProxyComponent dbProxy
        {
            get
            {
                return Game.Scene.GetComponent<DBProxyComponent>();
            }
        }

        private static ConfigComponent configComponent => Game.Scene.GetComponent<ConfigComponent>();

        public static string GetUsedCountString(Equipment.EquipmentType equipmentType)
        {
            return string.Format(UsedCount, (int)equipmentType);
        }

        public static string GetTotalCountString(Equipment.EquipmentType equipmentType)
        {
            return string.Format(TotalCount, (int)equipmentType);
        }

        /// <summary>
        /// 取得使用者所有道具資料
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async ETTask<RepeatedField<EquipmentInfo>> GetUserAllEquipmentInfo(long uid)
        {
            var list = new RepeatedField<EquipmentInfo>();
            var listEq = new List<Equipment>();
            var result = await dbProxy.Query<Equipment>(entity => entity.uid == uid);
            for(int i = 0; i < result.Count; i++)
            {
                var equipment = (Equipment)result[i];
                if(TryGetEquipmentConfig(equipment.configId, out CharacterConfig config))
                {
                    if (config.IsStackEquipment())
                    {
                        var obj = OtherHelper.Search(listEq, entity => entity.configId == equipment.configId);
                        if (obj != null)
                        {
                            await _MergeEquipment(uid, obj, equipment);
                            continue;
                        }
                    }
                    list.Add(equipment.ToEquipmentInfo());
                    listEq.Add(equipment);
                }
            }
            return list;
        }

        public static async ETTask<List<Equipment>> GetUserEquipments(long uid, int type, long equipmentId)
        {
            var result = await dbProxy.Query<Equipment>(entity => entity.uid == uid && entity.configType == type && entity.configId == equipmentId);
            return result.OfType<Equipment>().ToList();
        }

        public static async ETTask<List<Equipment>> GetUserEquipments(long uid, int type)
        {
            var result = await dbProxy.Query<Equipment>(entity => entity.uid == uid && entity.configType == type);
            return result.OfType<Equipment>().ToList();
        }

        /// <summary>
        /// 得到"新增|異動"一筆道具紀錄的Log物件
        /// </summary>
        /// <param name="uid">異動到哪位使用者的資料</param>
        /// <param name="equipment">要寫入的道具</param>
        /// <param name="equipmentLog">需要詳細記錄的Log</param>
        /// <returns></returns>
        private static DBLog _GetSaveEquipmentDBLog(long uid, Equipment equipment, EquipmentLog equipmentLog, DBLog.LogType logType)
        {
            DBLog dBLog = ComponentFactory.CreateWithId<DBLog>(IdGenerater.GenerateId());
            dBLog.uid = uid;
            dBLog.logType = (int)logType;
            dBLog.document = new BsonDocument
            {
                { "id", equipment.Id }, // 對應的道具紀錄id
                { "configType", equipment.configType },
                { "configId" , equipment.configId },
                { "count", equipmentLog.count }, // 交易數量
                { "fromType", (int)equipmentLog.fromType }, // 從哪邊獲得|移除的
                { "from", equipmentLog.from } // 從哪邊獲得|移除的(玩家=uid, 系統=SystemUid)
            };
            dBLog.createAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return dBLog;
        }

        /// <summary>
        /// 得到"新增|異動"多筆同類型的道具紀錄的Log物件串列
        /// </summary>
        /// <param name="uid">異動到哪位使用者的資料</param>
        /// <param name="equipment">要寫入的道具</param>
        /// <param name="equipmentLog">需要詳細記錄的Log</param>
        /// <returns></returns>
        private static DBLog _GetSaveBatchEquipmentDBLog(long uid, List<Equipment> equipments, EquipmentLog equipmentLog)
        {
            DBLog dBLog = ComponentFactory.CreateWithId<DBLog>(IdGenerater.GenerateId());
            dBLog.uid = uid;
            dBLog.logType = (int)DBLog.LogType.AddEquipment;
            dBLog.document = new BsonDocument
            {
                { "ids", new BsonArray(equipments.Select(e => e.Id)) }, // 對應的道具紀錄id list
                { "configType", equipments[0].configType },
                { "configId" , equipments[0].configId },
                { "count", equipments.Count }, // 交易數量
                { "fromType", (int)equipmentLog.fromType }, // 從哪邊獲得的
                { "from", equipmentLog.from } // 從哪邊獲得的(玩家=uid, 系統=SystemUid)
            };
            dBLog.createAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return dBLog;
        }

        /// <summary>
        /// 刪除不可疊多筆同類型的道具紀錄
        /// </summary>
        /// <param name="uid">異動到哪位使用者的資料</param>
        /// <param name="equipment">要寫入的道具</param>
        /// <param name="equipmentLog">需要詳細記錄的Log</param>
        /// <returns></returns>
        private static DBLog _GetDeleteBatchNoStackEquipmentDBLog(long uid, List<Equipment> equipments, EquipmentLog equipmentLog)
        {
            var ids = equipments.Select(e => e.Id).ToList();
            DBLog dBLog = ComponentFactory.CreateWithId<DBLog>(IdGenerater.GenerateId());
            dBLog.uid = uid;
            dBLog.logType = (int)DBLog.LogType.SubtractEquipment;
            dBLog.document = new BsonDocument
            {
                { "ids", new BsonArray(ids) }, // 對應的道具紀錄id list
                { "configType", equipments[0].configType },
                { "configId" , equipments[0].configId },
                { "count", equipments.Count }, // 交易數量
                { "fromType", (int)equipmentLog.fromType }, // 從哪邊移除的
                { "from", equipmentLog.from } // 從哪邊移除的(玩家=uid, 系統=SystemUid)
            };
            dBLog.createAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return dBLog;
        }

        /// <summary>
        /// 合併兩筆以上的可堆疊道具紀錄
        /// 出現Bug的時候才會執行
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="equipmentList"></param>
        /// <returns></returns>
        private static async ETTask _MergeEquipment(long uid, params Equipment[] equipmentList)
        {
            var first = equipmentList[0];
            var set = new HashSet<long>(equipmentList.Length);
            for(int i = 1; i < equipmentList.Length; i++)
            {
                var equipmentInfo = equipmentList[i];
                if(first.configId == equipmentInfo.configId)
                {
                    first.count += equipmentInfo.count;
                    set.Add(equipmentInfo.Id);
                }
            }
            await dbProxy.Save(first);
            await dbProxy.DeleteJson<Equipment>(entity => set.Contains(entity.Id));
            CharacterConfig characterConfig = (CharacterConfig)configComponent.Get(typeof(CharacterConfig), first.configId);
            DBLog dBLog = ComponentFactory.CreateWithId<DBLog>(IdGenerater.GenerateId());
            dBLog.uid = uid;
            dBLog.logType = (int)DBLog.LogType.MergeEquipment;
            dBLog.document = new BsonDocument
            {
                { "mergedId", new BsonArray(equipmentList.Select(e => e.Id)) }, // 紀錄參與合併的道具紀錄Ids
                { "configType",  characterConfig.Type },
                { "configId" , first.configId },
                { "mergeCount", first.count }, // 合併後的數量
            };
            dBLog.createAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await dbProxy.Save(dBLog);
        }

        public static async ETTask<EquipmentResult> CreateEquipment(long uid, List<EquipmentInfo> items, EquipmentFrom fromType, int from)
        {
            // 回傳結構
            var equipmentResult = new EquipmentResult();

            // 抓取使用者背包限制資料
            User user = await UserDataHelper.FindOneUser(uid);

            // 無使用者?
            if (user == null)
            {
                equipmentResult.error = ErrorCode.ERR_AccountDoesntExist;
                return equipmentResult;
            }

            // 先把相關資料搜尋出來
            var eids = items.Select(e => e.ConfigId).ToList();
            var ret = await dbProxy.Query<Equipment>(entity => entity.uid == uid && eids.Contains(entity.configId));
            var results = ret.OfType<Equipment>().ToList();

            // 用企劃表id進行分組
            var dictConfigId = OtherHelper.Group(results, entity => entity.configId);
            // 用企劃表type進行分組(TODO:記得擋背包類型下最大存放數)
            //var dictConfigType = OtherHelper.Group(results, entity => entity.configType);

            // 批處理
            var saveBatch = new List<ComponentWithId>();
            var logBatch = new List<ComponentWithId>();
            var refreshBagType = new List<int>();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.Count <= 0)
                {
                    equipmentResult.error = ErrorCode.ERR_EquipmentInvalidCount;
                    return equipmentResult;
                }

                if (TryGetEquipmentConfig(item.ConfigId, out CharacterConfig equipmentConfig))
                {
                    // 道具是否可用?
                    if (!equipmentConfig.IsAvailable())
                    {
                        equipmentResult.error = ErrorCode.ERR_EquipmentUnavailable;
                        return equipmentResult;
                    }

                    if (!dictConfigId.TryGetValue(equipmentConfig.Id, out var userEqs))
                    {
                        userEqs = new List<Equipment>(0);
                    }

                    // 道具是否可疊加?
                    if (equipmentConfig.IsStackEquipment())
                    {
                        var maxCount = equipmentConfig.MaxCountOnSlot;

                        // 可疊不該有兩筆以上的記錄
                        if (userEqs.Count > 1)
                        {
                            // 記在ErrorLog裡
                            Log.Error(new Exception($"ERR_EquipmentRecordError> Uid:{uid}, Type:{equipmentConfig.Type}, ConfigId:{equipmentConfig.Id}"));
                            await _MergeEquipment(uid, userEqs.ToArray());

                            // 合併資料後用遞迴再跑一次
                            return await CreateEquipment(uid, items.Skip(i).ToList(), fromType, from);
                        }
                        // 判斷有無記錄?
                        else if (userEqs.Count == 0)
                        {
                            // 判斷背包容量
                            if (user.IsOverOrEqualBagMaxSlotCount((Equipment.EquipmentType)equipmentConfig.Type))
                            {
                                equipmentResult.error = ErrorCode.ERR_EquipmentBagOverLimit;
                                return equipmentResult;
                            }
                            else
                            {
                                if(equipmentConfig.IsOverOwnedEquipmentLimit(item.Count))
                                {
                                    equipmentResult.error = ErrorCode.ERR_EquipmentOverOwnedLimit;
                                    return equipmentResult;
                                }
                                var userEq = ComponentFactory.CreateWithId<Equipment>(IdGenerater.GenerateId());
                                userEq.uid = uid;
                                userEq.configType = equipmentConfig.Type;
                                userEq.configId = item.ConfigId;
                                userEq.count = item.Count;
                                saveBatch.Add(userEq);
                                var log = ComponentFactory.Create<EquipmentLog>();
                                log.from = from;
                                log.fromType = fromType;
                                log.count = item.Count;
                                var dbLog = _GetSaveEquipmentDBLog(uid, userEq, log, DBLog.LogType.AddEquipment);
                                logBatch.Add(dbLog);
                            }
                        }
                        // 存在一筆紀錄了
                        else
                        {
                            var userEq = userEqs[0];
                            userEq.count += item.Count;
                            if (equipmentConfig.IsOverOwnedEquipmentLimit(userEq.count))
                            {
                                equipmentResult.error = ErrorCode.ERR_EquipmentOverOwnedLimit;
                                return equipmentResult;
                            }
                            saveBatch.Add(userEq);
                            var log = ComponentFactory.Create<EquipmentLog>();
                            log.from = from;
                            log.fromType = fromType;
                            log.count = item.Count;
                            var dbLog = _GetSaveEquipmentDBLog(uid, userEq, log, DBLog.LogType.AddEquipment);
                            logBatch.Add(dbLog);
                        }
                    }
                    else
                    {
                        // 不可疊加就直接創，Log額外再紀錄
                        if (equipmentConfig.IsOverOwnedEquipmentLimit(userEqs.Count + item.Count))
                        {
                            equipmentResult.error = ErrorCode.ERR_EquipmentOverOwnedLimit;
                            return equipmentResult;
                        }
                        var equipments = new List<Equipment>();
                        for (int j = 0; j < item.Count; j++)
                        {
                            var userEq = ComponentFactory.CreateWithId<Equipment>(IdGenerater.GenerateId());
                            userEq.uid = uid;
                            userEq.configType = equipmentConfig.Type;
                            userEq.configId = equipmentConfig.Id;
                            userEq.count = 1;
                            equipments.Add(userEq);
                        }
                        saveBatch.AddRange(equipments);
                        var log = ComponentFactory.Create<EquipmentLog>();
                        log.from = from;
                        log.fromType = fromType;
                        var dbLog = _GetSaveBatchEquipmentDBLog(uid, equipments, log);
                        logBatch.Add(dbLog);
                    }

                    if (!refreshBagType.Contains(equipmentConfig.Type))
                        refreshBagType.Add(equipmentConfig.Type);
                }
                else
                {
                    equipmentResult.error = ErrorCode.ERR_EquipmentNotDefined;
                    return equipmentResult;
                }
            }

            // 批儲存
            if (saveBatch.Count != 0)
            {
                await dbProxy.SaveBatch(saveBatch);
                await dbProxy.SaveBatch(logBatch);
            }

            if(refreshBagType.Count != 0)
            {
                await user.RecalculateBagCount(refreshBagType);
            }

            equipmentResult.error = ErrorCode.ERR_Success;
            equipmentResult.userBagInfo = ToUserBagCapacity(user.userBagCapacity);
            equipmentResult.equipmentInfos = saveBatch.Aggregate(new RepeatedField<EquipmentInfo>(), (list, item) => 
            {
                list.Add(((Equipment)item).ToEquipmentInfo());
                return list;
            });
            return equipmentResult;
        }

        /// <summary>
        /// 可疊數量如果歸0->不刪除紀錄，直接更新成0(背包計算上已處理這塊)
        /// 不可疊->直接砍掉該筆紀錄
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="items"></param>
        /// <param name="fromType"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static async ETTask<EquipmentResult> DeleteEquipment(long uid, List<EquipmentInfo> items, EquipmentFrom fromType, int from, bool deleteAnyhow = false)
        {
            // 回傳結構
            var equipmentResult = new EquipmentResult();

            // 抓取使用者背包限制資料
            User user = await UserDataHelper.FindOneUser(uid);

            // 無使用者?
            if (user == null)
            {
                equipmentResult.error = ErrorCode.ERR_AccountDoesntExist;
                return equipmentResult;
            }

            // 先把相關資料搜尋出來
            var eids = items.Select(e => e.ConfigId).ToList();
            var results = await dbProxy.Query<Equipment>(entity => entity.uid == uid && eids.Contains(entity.configId));
            var equipments = results.OfType<Equipment>().ToList();

            // 用企劃表id進行分組
            var dictConfigId = OtherHelper.Group(equipments, entity => entity.configId);
            //// 用企劃表type進行分組(TODO:記得擋背包類型下最大存放數)
            //var dictConfigType = OtherHelper.Group(equipments, entity => entity.configType);

            // 批處理
            var saveBatch = new List<ComponentWithId>();
            var logBatch = new List<ComponentWithId>();
            var deleteBatch = new List<long>();
            var refreshBagType = new List<int>();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.Count <= 0)
                {
                    equipmentResult.error = ErrorCode.ERR_EquipmentInvalidCount;
                    return equipmentResult;
                }

                if (TryGetEquipmentConfig(item.ConfigId, out CharacterConfig equipmentConfig))
                {
                    // 道具是否可用?
                    if (!equipmentConfig.IsAvailable())
                    {
                        equipmentResult.error = ErrorCode.ERR_EquipmentUnavailable;
                        return equipmentResult;
                    }

                    if (!dictConfigId.TryGetValue(equipmentConfig.Id, out var userEqs))
                    {
                        userEqs = new List<Equipment>(0);
                    }

                    // 道具是否可疊加?
                    if (equipmentConfig.IsStackEquipment())
                    {
                        var maxCount = equipmentConfig.MaxCountOnSlot;
     
                        // 可疊不該有兩筆以上的記錄
                        if (userEqs.Count > 1)
                        {
                            // 記在ErrorLog裡
                            Log.Error(new Exception($"ERR_EquipmentRecordError> Uid:{uid}, Type:{equipmentConfig.Type}, ConfigId:{equipmentConfig.Id}"));
                            await _MergeEquipment(uid, userEqs.ToArray());

                            // 合併資料後用遞迴再跑一次
                            return await DeleteEquipment(uid, items.Skip(i).ToList(), fromType, from);
                        }
                        // 判斷有無記錄?
                        else if (userEqs.Count == 0)
                        {
                            equipmentResult.error = ErrorCode.ERR_EquipmentBagIsEmpty;
                            return equipmentResult;
                        }
                        // 存在一筆紀錄了
                        else
                        {
                            var userEq = userEqs[0];
                            if (userEq.count < item.Count)
                            {
                                equipmentResult.error = ErrorCode.ERR_EquipmentNotEnough;
                                return equipmentResult;
                            }
                            userEq.count -= item.Count;
                            saveBatch.Add(userEq);
                            var log = ComponentFactory.Create<EquipmentLog>();
                            log.from = from;
                            log.fromType = fromType;
                            log.count = item.Count;
                            var dbLog = _GetSaveEquipmentDBLog(uid, userEq, log, DBLog.LogType.SubtractEquipment);
                            logBatch.Add(dbLog);
                        }
                    }
                    else
                    {
                        if (deleteAnyhow)
                        {
                            var configs = userEqs;
                            if(configs.Count < item.Count)
                            {
                                equipmentResult.error = ErrorCode.ERR_EquipmentNotEnough;
                                return equipmentResult;
                            }
                            for (int k = 0; k < item.Count; k++)
                            {
                                var config = configs[k];
                                if (!deleteBatch.Contains(config.Id))
                                    deleteBatch.Add(config.Id);
                            }
                        }
                        else
                        {
                            if (!deleteBatch.Contains(item.Id))
                                deleteBatch.Add(item.Id);
                        }
                    }

                    if (!refreshBagType.Contains(equipmentConfig.Type))
                        refreshBagType.Add(equipmentConfig.Type);
                }
                else
                {
                    equipmentResult.error = ErrorCode.ERR_EquipmentNotDefined;
                    return equipmentResult;
                }
            }

            var result = new RepeatedField<EquipmentInfo>();

            // 批刪除
            // 不可疊加就根據Id直接砍，Log額外再紀錄
            if (deleteBatch.Count != 0)
            {
                foreach (var v in dictConfigId)
                {
                    var dels = OtherHelper.SearchAll(v.Value, entity => deleteBatch.Contains(entity.Id));
                    var delLog = ComponentFactory.Create<EquipmentLog>();
                    delLog.from = from;
                    delLog.fromType = fromType;
                    var dbLog = _GetDeleteBatchNoStackEquipmentDBLog(uid, dels, delLog);
                    logBatch.Add(dbLog);
                }
                await dbProxy.DeleteJson<Equipment>(entity => deleteBatch.Contains(entity.Id));

                foreach(var id in deleteBatch)
                {
                    result.Add(new EquipmentInfo
                    {
                        Id = id,
                        Count = 0,
                    });
                }
            }

            // 批儲存資料
            if (saveBatch.Count != 0)
            {
                await dbProxy.SaveBatch(saveBatch);

                foreach(var item in saveBatch)
                {
                    result.Add(((Equipment)item).ToEquipmentInfo());
                }
            }

            // 批儲存Log
            if (logBatch.Count != 0)
            {
                await dbProxy.SaveBatch(logBatch);
            }

            if (refreshBagType.Count != 0)
            {
                await user.RecalculateBagCount(refreshBagType);
            }

            equipmentResult.error = ErrorCode.ERR_Success;
            equipmentResult.equipmentInfos = result;
            equipmentResult.userBagInfo = ToUserBagCapacity(user.userBagCapacity);
            return equipmentResult;
        }

        public static bool TryGetEquipmentConfig(long configId, out CharacterConfig config)
        {
            var configComponent = Game.Scene.GetComponent<ConfigComponent>();
            config = (CharacterConfig)configComponent.Get(typeof(CharacterConfig), configId);
            if(config == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool IsOverOrEqualBagMaxSlotCount(this User user, Equipment.EquipmentType equipmentType)
        {
            var usedCountString = GetUsedCountString(equipmentType);
            var totalCountString = GetTotalCountString(equipmentType);
            var userBagCapacity = user.userBagCapacity;
            return userBagCapacity[usedCountString].AsInt32 >= userBagCapacity[totalCountString].AsInt32;
        }

        public static BsonDocument GetDefaultUserBag()
        {
            var configComponent = Game.Scene.GetComponent<ConfigComponent>();
            var bagLimitSettings = configComponent.GetAll(typeof(BagLimitSetting)).OfType<BagLimitSetting>().ToList();
            var userBagCapacity = new BsonDocument();
            for (int i = 0; i < bagLimitSettings.Count; i++)
            {
                var bagLimitSetting = bagLimitSettings[i];
                Equipment.EquipmentType equipmentType = (Equipment.EquipmentType)bagLimitSetting.Id;
                var usedCountString = GetUsedCountString(equipmentType);
                var totalCountString = GetTotalCountString(equipmentType);
                userBagCapacity[usedCountString] = 0;
                userBagCapacity[totalCountString] = bagLimitSetting.MaxSlotCount;
            }
            return userBagCapacity;
        }

        public static UserBagCapacity ToUserBagCapacity(BsonDocument doc)
        {
            var usedDecorationCountString = GetUsedCountString(Equipment.EquipmentType.Decoration);
            var totalDecorationCountString = GetTotalCountString(Equipment.EquipmentType.Decoration);
            return new UserBagCapacity
            {
                UsedDecorationCount = doc.Contains(usedDecorationCountString) ? doc[usedDecorationCountString].AsInt32 : 0,
                TotalDecorationCount = doc.Contains(totalDecorationCountString) ? doc[totalDecorationCountString].AsInt32 : 0,
            };
        }

        #region Extension method

        public static EquipmentInfo ToEquipmentInfo(this Equipment equipment)
        {
            return new EquipmentInfo
            {
                Id = equipment.Id,
                ConfigId = equipment.configId,
                Count = equipment.count,
            };
        }

        /// <summary>
        /// 是否可疊加
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        private static bool IsStackEquipment(this CharacterConfig equipment)
        {
            return equipment.IsStack == 1;
        }

        /// <summary>
        /// 是否超過個人擁有該道具的數量上限?
        /// </summary>
        /// <param name="equipment"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static bool IsOverOwnedEquipmentLimit(this CharacterConfig equipment, int count)
        {
            return equipment.MaxCountOnBag != -1 && count > equipment.MaxCountOnBag;
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        private static bool IsAvailable(this CharacterConfig equipment)
        {
            bool IsExpired()
            {
                if (equipment.UseExpireAt == -1)
                    return false;
                var now = DateTimeOffset.UtcNow;
                DateTime expire = DateHelper.TimestampSecondToDateTimeUTC(equipment.UseExpireAt);
                return now.Ticks >= expire.Ticks;
            }
            return (equipment.MaxCountOnSlot != 0) && !IsExpired();
        }

        /// <summary>
        /// 重新計算背包數
        /// </summary>
        /// <param name="user"></param>
        /// <param name="equipmentTypes"></param>
        /// <returns></returns>
        private static async ETTask RecalculateBagCount(this User user, List<int> equipmentTypes)
        {
            var userBagCapacity = user.userBagCapacity;

            var results = await dbProxy.Query<Equipment>(entity => entity.uid == user.Id && 
                equipmentTypes.Contains(entity.configType));

            var dict = OtherHelper.Group(results.OfType<Equipment>().ToList(), entity => entity.configType);

            var logBag = new BsonDocument();
            var logCharSetting = new BsonDocument();
            var isDecorationExisted = false;

            for (int a = 0; a < equipmentTypes.Count; a++)
            {
                var type = equipmentTypes[a];
                var equipmentType = (Equipment.EquipmentType)type;
                if(!dict.TryGetValue(type, out var equipments))
                {
                    equipments = new List<Equipment>(0);
                }

                // 初始化
                int usedCount = 0;
                var usedCountString = GetUsedCountString(equipmentType);

                for (int i = 0; i < equipments.Count; i++)
                {
                    var equipment = equipments[i];
                    if (TryGetEquipmentConfig(equipment.configId, out CharacterConfig characterConfig))
                    {
                        // 不可疊直接++
                        if (!characterConfig.IsStackEquipment())
                        {
                            // 現在的不可疊裝備至少有一件符合(TODO:能裝備的裝備一定要不可疊)
                            // TODO:腳色裝備的記錄改用裝備的_id
                            if (equipment.configId == user.playerCharSetting.DecorationId)
                            {
                                isDecorationExisted = true;
                            }

                            usedCount++;
                        }
                        else
                        {
                            if (characterConfig.MaxCountOnSlot < 0)
                            {
                                usedCount++;
                            }
                            else
                            {
                                var remainder = equipment.count % characterConfig.MaxCountOnSlot;
                                var slotCount = equipment.count / characterConfig.MaxCountOnSlot + (remainder == 0 ? 0 : 1);
                                usedCount += slotCount;
                            }
                        }
                    }
                    else
                    {
                        Log.Error($"Equipment config id:{equipment.configId} is not defined!");
                        continue;
                    }
                }

                if (userBagCapacity[usedCountString] != usedCount)
                {
                    logBag[usedCountString] = usedCount;
                    userBagCapacity[usedCountString] = usedCount;
                }
            }

            if (!isDecorationExisted)
            {
                user.playerCharSetting.DecorationId = 0;
            }

            await UserDataHelper.UpsertUser(user, DBLog.LogType.UpdateUserBagSlotCount, logBag);
        }

        #endregion
    }
}
