using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
    [ObjectSystem]
    public class BDCacheComponentSystem : AwakeSystem<BDCacheComponent, DBComponent, CacheComponent>
    {
        public override void Awake(BDCacheComponent self, DBComponent db, CacheComponent cache)
        {
            self.Awake(db, cache);
        }
    }

    public class BDCacheComponent : Component
    {
        private DBComponent dbComponent { get; set; }

        private CacheComponent cacheComponent { get; set; }

        public bool isCache { get; set; }

        public void Awake(DBComponent db, CacheComponent cache)
        {
            dbComponent = db;
            cacheComponent = cache;
        }

        #region HashSet

        /*Core-Begin*/

        //        public async Task<T> FindOne<T>(long id, string[] fields) where T : ComponentWithId
        //        {
        //            return cacheComponent.database.KeyExistsAsync() this.existId(id).then((isExist) => {
        //            //存在去抓Cache
        //            if (isExist)
        //                return this._findOneInCache(id, fields);
        //            else
        //            {
        //                //不存在資料庫讀取
        //                if (global.debug)
        //                {
        //                    console.log(`Missing cache ${ JSON.stringify(id)}`);
        //        }
        //                return this.model.findById(id).then((dbResult) => {
        //            if (dbResult != null)
        //            {
        //                return this._createCache(id, dbResult);
        //            }
        //            else
        //                return Promise.resolve();
        //        });
        //            }
        //});
        //        }

        /*Core-End*/

        /*HashSet Helper-Begin*/

        /// <summary>
        /// 檢查Redis Key是否存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        //public ETTask<bool> ExistKey<T>(string key) where T : ComponentWithId
        //{
        //    return isCache ? cacheComponent.database.KeyExistsAsync(GetKey<T>(key)) : Task.FromResult(false);
        //}

        /// <summary>
        /// 透過Key取得Redis Key
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetKey<T>(string key) where T : ComponentWithId
        {
            var collectionName = typeof(T).Name;
            return $"{collectionName}:{key}";
        }

        /*HashSet Helper-End*/

        #endregion

        public async Task<T> FindOne<T>(long id) where T : ComponentWithId
        {
            Type type = typeof(T);
            T entity = await cacheComponent.FindOne<T>(id);
            if (entity == null)
            {
                var collectionName = cacheComponent.GetCollectionName(type);
                entity = (T)await dbComponent.Get(collectionName, id);
                if(entity == null)
                {
                    return entity;
                }
                entity = await cacheComponent.Create(entity);
            }
            return entity;
        }

        public async Task<T> FindOneByUniqueIndex<T>(string indexName, T entity) where T : ComponentWithId
        {
            Type type = typeof(T);
            T res = await cacheComponent.FindOneByUniqueIndex(indexName, entity);
            if (res == null)
            {
                var collectionName = cacheComponent.GetCollectionName(type);
                res = await dbComponent.FindOneByUniqueIndex<T>(collectionName, indexName, entity);
                if (res == null)
                {
                    return res;
                }
                res = await cacheComponent.Create(res);
            }
            return res;
        }

        public async Task<List<T>> FindAllByIndex<T>(string indexName, T entity) where T : ComponentWithId
        {
            Type type = typeof(T);
            List<T> res = await cacheComponent.FindAllByIndex(indexName, entity);
            if (res.Count == 0)
            {
                var collectionName = cacheComponent.GetCollectionName(type);
                res = await dbComponent.FindAllByIndex(collectionName, indexName, entity);
                if (res.Count == 0)
                {
                    return res;
                }
                for(int i = 0; i < res.Count; i++)
                {
                    await cacheComponent.Create(res[i]);
                }
            }
            return res;
        }

        public async Task<T> Create<T>(T entity) where T : ComponentWithId
        {
            Type type = typeof(T);
            T res = await cacheComponent.Create(entity);
            if(res != null)
            {
                var collectionName = cacheComponent.GetCollectionName(type);
                res = await dbComponent.Create(collectionName, res);
            }
            return res;
        }

        public async Task<T> Update<T>(T entity) where T : ComponentWithId
        {
            Type type = typeof(T);
            T res = await cacheComponent.Update(entity);
            if(res != null)
            {
                var collectionName = cacheComponent.GetCollectionName(type);
                res = await dbComponent.Update(collectionName, res);
            }
            return res;
        }

        public async Task<T> Upsert<T>(T entity) where T : ComponentWithId
        {
            Type type = typeof(T);
            T res = await cacheComponent.Upsert(entity);
            if(res != null)
            {
                var collectionName = cacheComponent.GetCollectionName(type);
                res = await dbComponent.Upsert(collectionName, res);
            }
            return res;
        }

        public async Task<bool> Delete<T>(long id) where T : ComponentWithId
        {
            Type type = typeof(T);
            bool res = await cacheComponent.Delete<T>(id);
            if (res)
            {
                var collectionName = cacheComponent.GetCollectionName(type);
                res = await dbComponent.Delete<T>(collectionName, id);
            }
            return res;
        }
    }
}
