using ETModel;
using System;
using System.Net;
using System.Threading;

namespace ETHotfix
{
    public static class CacheExHelper
    {
        public static T GetFromCache<T>(long id) where T : ComponentWithId
        {
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            var memSync = proxy.GetMemorySyncSolver<T>();
            return memSync.Get<T>(id);
        }

        public static async ETTask<T> UpdateInCache<T>(T entity) where T : ComponentWithId
        {
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            var memSync = proxy.GetMemorySyncSolver<T>();
            return await memSync.Update(entity);
        }

        /// <summary>
        /// 寫進快取，僅能用在:不等候快取回調，直接從現有IResponse傳回的Component直接先用
        /// 目的是在馬上要用該同步物件的時候不等快取Refresh，直接先塞值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public static void WriteInCache<T>(T entity, out T res) where T : ComponentWithId
        {
            var proxy = Game.Scene.GetComponent<CacheProxyComponent>();
            var memSync = proxy.GetMemorySyncSolver<T>();
            memSync.Write(entity.Id, entity);
            // 該回傳對象保證Reference指向Redis同步用的資料結構，也就保證能同步該物件
            res = memSync.Get<T>(entity.Id);
        }
    }
}
