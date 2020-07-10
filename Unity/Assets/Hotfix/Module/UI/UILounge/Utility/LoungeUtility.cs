using ETModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Google.Protobuf.Collections;


namespace ETHotfix
{
    public static class LoungeUtility
    {
        /// <summary>
        /// 不合法的Long ID
        /// </summary>
        public const long INVALID = -1;
        public const long CHARRACTER_REMAINED_START_ID = 9000;

        public enum ConfigType
        {
            Character = 1,
            Bicycle,
            Body,
            LightWheel,
            Medal,
            //Decoration
        }

        public static bool IsCharacterType(long id)
        {
            return id >= 1 && id <= 5;
        }

        public static bool IsBicycleType(long id)
        {
            return id >= 100 && id <= 199;
        }

        public static bool IsBodyType(long id)
        {
            return id >= 200 && id <= 499;
        }

        public static bool IsLightWheelType(long id)
        {
            return id >= 500 && id <= 999;
        }

        public static bool IsMedalType(long id)
        {
            return id >= 1000 && id <= 1499;
        }
        //public static bool IsDecorationType(long id)
        //{
        //    return id >= 500 && id <= 999;
        //}
        public static bool IsCross(CharacterConfig config)
        {
            return config.Icon == "cross";
        }

#if !SERVER

        private static Transform _character_remained_parent = null;
        private static Dictionary<long, GameObject> _character_remained_cache = new Dictionary<long, GameObject>();
        private static Dictionary<long, List<ETTaskCompletionSource<GameObject>>> _character_remained_loading = new Dictionary<long, List<ETTaskCompletionSource<GameObject>>>();

        #region AssetBundle相關

        /// <summary>
        /// ImageLoungeIcon的Bundle物件
        /// </summary>
        private static GameObject imageLoungeIconBundle = null;

        private const string imageIconBundleName = "imageloungeicon";

        #endregion

        public static void UnloadCharacterBundle(string resName)
        {
            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            resourcesComponent.UnloadBundle(ResourcesPathType.Avatar, resName);
        }

        /// <summary>
        /// 根據資源名稱創造腳色模型
        /// </summary>
        /// <param name="resName"></param>
        /// <returns></returns>
        public static async ETTask<GameObject> CreateCharacterModel(string resName)
        {
            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            //不用先LoadBundleAsync，底層會自動做
            GameObject bundleGameObject = await resourcesComponent.LoadAssetAsync(ResourcesPathType.Avatar, resName) as GameObject;
            GameObject go = UnityEngine.Object.Instantiate(bundleGameObject);
            go.name = resName;
            return go;
        }

        public static async ETTask<GameObject> CreateCharacterModel(long Id)
        {
            CharacterConfig config = GetCharacterConfig(Id);
            if (config == null || config.Type != (int)ConfigType.Character)
            {
                return null;
            }
            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            //不用先LoadBundleAsync，底層會自動做
            GameObject bundleGameObject = await resourcesComponent.LoadAssetAsync(ResourcesPathType.Avatar, config.Name) as GameObject;
            GameObject go = UnityEngine.Object.Instantiate(bundleGameObject);
            go.name = config.Name;
            return go;
        }
        public static void SetLayer(GameObject obj, int layer)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].gameObject.layer = layer;
            }
        }

        public static async ETTask<GameObject> CreateCharacterModelBySetting(PlayerCharSetting setting, int layer)
        {
            GameObject go = await CreateCharacterModel(setting.CharacterId);
            if(go == null)
            {
                return null;
            }
            CharacterConfig config = null;

            //TODO:Wade 新角色
            if (go.GetComponent<AvatarModelCollector>() != null && Define.OpenNewModel)
            {
                config = GetCharacterConfig(setting.CharacterId);
                var initModel = go.GetComponent<AvatarModelCollector>();
                initModel.SetModel(config.Icon);
            }
            //腳踏車
            config = GetAppearanceConfig(setting.BicycleId, (int)ConfigType.Bicycle);
            ChangeCharAppearance(go, config, setting.CharacterId);
            //身體
            config = GetAppearanceConfig(setting.BodyId, (int)ConfigType.Body);
            ChangeCharAppearance(go, config, setting.CharacterId);
            //光輪
            config = GetAppearanceConfig(setting.DecorationId, (int)ConfigType.LightWheel);
            ChangeCharAppearance(go, config, setting.CharacterId);
            //獎牌
            config = GetAppearanceConfig(setting.MedalId, (int)ConfigType.Medal);
            ChangeCharAppearance(go, config, setting.CharacterId);
            //裝飾
            //config = GetAppearanceConfig(setting.DecorationId, (int)ConfigType.Decoration);
            //ChangeCharAppearance(go, config);
            SetLayer(go, layer);
            return go;
        }

        private static void CheckCharacterRemainedParent()
        {
            if (_character_remained_parent == null)
            {
                var character_remained_obj = new GameObject("_character_remained_parent");
                UnityEngine.Object.DontDestroyOnLoad(character_remained_obj);
                _character_remained_parent = character_remained_obj.transform;
            }
        }

        private static void AddCharacterRemainedCache(long id, GameObject obj)
        {
            CheckCharacterRemainedParent();
            obj.transform.SetParent(_character_remained_parent);
            obj.SetActive(false);
            _character_remained_cache.Add(id, obj);
        }

        private static async ETTask<GameObject> LoadCharacterRemainedCache(long id)
        {
            if (!_character_remained_loading.TryGetValue(id, out var loadingSources))
            {
                //Create TaskCompletionSource
                loadingSources = new List<ETTaskCompletionSource<GameObject>>();
                _character_remained_loading.Add(id, loadingSources);

                //Load asset
                var cacheModel = await CreateCharacterModel(id);
                AddCharacterRemainedCache(id, cacheModel);

                //SetResult TaskCompletionSource
                for (int i = 0; i < loadingSources.Count; i++)
                {
                    loadingSources[i].SetResult(cacheModel);
                }
                loadingSources.Clear();
                _character_remained_loading.Remove(id);
                return cacheModel;
            }
            else
            {
                var loadingSource = new ETTaskCompletionSource<GameObject>();
                loadingSources.Add(loadingSource);
                return await loadingSource.Task;
            }
        }

        private static async ETTask<GameObject> GetCharacterRemainedCache(long id, int layer)
        {
            if (!_character_remained_cache.TryGetValue(id, out GameObject cacheModel))
            {
                cacheModel = await LoadCharacterRemainedCache(id);
            }
            var newModel = GameObject.Instantiate(cacheModel);
            SetLayer(newModel, layer);
            newModel.SetActive(true);
            newModel.transform.SetParent(null);
            return newModel;
        }

        public static GameObject CreateMockCharacterModel(long id, int layer)
        {
            if (!_character_remained_cache.TryGetValue(id, out GameObject cacheModel))
            {
                return null;
            }
            var newModel = GameObject.Instantiate(cacheModel);
            SetLayer(newModel, layer);
            newModel.SetActive(true);
            newModel.transform.SetParent(null);
            return newModel;
        }

        /// <summary>
        /// 創造保留區腳色模型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async ETTask<GameObject> CreateMockCharacterModelAsync(long id, int layer)
        {
            if (id < CHARRACTER_REMAINED_START_ID)
            {
                Log.Error($"Creation mock character with id less than {CHARRACTER_REMAINED_START_ID} is invalid!");
                return null;
            }
            return await GetCharacterRemainedCache(id, layer);
        }

        public static CharacterConfig GetCharacterConfig(long id)
        {
            ConfigComponent configComponent = Game.Scene.GetComponent<ConfigComponent>();
            CharacterConfig data = configComponent.Get(typeof(CharacterConfig), id) as CharacterConfig;
            if(data == null)
            {
                //DB存檔資料預設值會跑到這裡來，可能要改善一下
                //Log.Info($"Character Config ID [{id}] doesnt exist!");
            }
            return data;
        }

        public static CharacterConfig[] GetAllCharacterConfigs()
        {
            ConfigComponent configComponent = Game.Scene.GetComponent<ConfigComponent>();
            CharacterConfig[] array = configComponent.GetAll(typeof(CharacterConfig)).OfType<CharacterConfig>()
                .Where(e => e.Type == (int)ConfigType.Character).ToArray();
            return array;
        }

        public static CharacterConfig GetAppearanceConfig(long id)
        {
            return GetCharacterConfig(id);
        }

        public static CharacterConfig GetAppearanceConfig(long id, int type)
        {
            var config = GetCharacterConfig(id);
            //if(config.Type != type)
            //{
            //    Debug.LogError($"Appearance type error! id:{id} doesn't match type:{(ConfigType)type}");
            //}
            return config;
        }

        public static CharacterConfig[] GetAllAppearanceConfigs()
        {
            ConfigComponent configComponent = Game.Scene.GetComponent<ConfigComponent>();
            CharacterConfig[] data = configComponent.GetAll(typeof(CharacterConfig))
                .OfType<CharacterConfig>()
                .Where(e => new List<ConfigType>
                {
                    ConfigType.Character, ConfigType.Bicycle, ConfigType.Body, ConfigType.LightWheel, ConfigType.Medal
                }.Contains((ConfigType)e.Type))
                .ToArray();
            return data;
        }

        public static async ETTask<GameObject> GetLoungeCharactorShowRoom()
        {
            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            GameObject bundleObj = await resourcesComponent.LoadAssetAsync(ResourcesPathType.DynamicObj, "LoungeCharactorShowRoom") as GameObject;
            GameObject instanceObj = UnityEngine.Object.Instantiate(bundleObj);
            return instanceObj;
        }
        public static void UnloadLoungeCharactorShowRoom()
        {
            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            resourcesComponent.UnloadBundle(ResourcesPathType.DynamicObj, "LoungeCharactorShowRoom");
        }

        public static void UnloadAllAppearanceBundle()
        {
            CharacterConfig[] data = GetAllAppearanceConfigs();
            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            for(int i = 0; i < data.Length; i++)
            {
                try
                {
                    resourcesComponent.UnloadBundle(ResourcesPathType.DynamicObj, data[i].Name);
                }
                catch(Exception ex)
                {
                    Log.Info($"Ignore exception: {ex.Message}");
                }
            }
        }

        public static async ETTask<GameObject> CreateAppearanceModel(string resName, Transform parent = null)
        {
            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            GameObject bundleObj = await resourcesComponent.LoadAssetAsync(ResourcesPathType.DynamicObj, resName) as GameObject;
            GameObject instanceObj = UnityEngine.Object.Instantiate(bundleObj, parent, false);
            instanceObj.transform.localPosition = Vector3.zero;
            instanceObj.transform.localEulerAngles = Vector3.zero;
            return instanceObj;
        }

        public static async ETTask<GameObject> LoadLoungeIconBundle()
        {
            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            imageLoungeIconBundle = await resourcesComponent.LoadAssetAsync(ResourcesPathType.Image, imageIconBundleName) as GameObject;
            return imageLoungeIconBundle;
        }

        public static void UnloadLoungeIconBundle()
        {
            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            resourcesComponent.UnloadBundle(ResourcesPathType.Image, imageIconBundleName);
            imageLoungeIconBundle = null;
        }

        public static Sprite GetLoungeIcon(string resName)
        {
            if(imageLoungeIconBundle == null)
            {
                throw new Exception("Before using, it needs to load bundle first!");
            }
            ReferenceCollector imageemoticonRC = imageLoungeIconBundle.GetComponent<ReferenceCollector>();
            return imageemoticonRC.Get<Sprite>(resName);
        }

        public static Color ConvertIntArray2Color(int[] colorArray)
        {
            Color color = Color.white;
            if (colorArray != null && colorArray.Length == 4)
            {
                color = new Color(colorArray[0] / 255f, colorArray[1] / 255f,
                    colorArray[2] / 255f, colorArray[3] / 255f);
            }
            return color;
        }

        public static async void ChangeCharAppearance(GameObject avatar, CharacterConfig config, long modelId)
        {
            if(avatar == null || config == null)
            {
                return;
            }
            ReferenceCollector referenceCollector = avatar.GetComponent<ReferenceCollector>();
            switch ((ConfigType)config.Type)
            {
                case ConfigType.Bicycle:
                case ConfigType.Body:
                {
                        var _config = GetAppearanceConfig(modelId, (int)ConfigType.Character);
                        AvatarDressCollector dress = referenceCollector.GetComponent<AvatarDressCollector>();
                        dress.Dress(config.Id, _config.Icon);
                        break;
                }
                case ConfigType.LightWheel:
                {
                        break;
                }
                case ConfigType.Medal:
                    {
                        Transform medalPoint = referenceCollector.Get<Transform>("MedalPoint");
                        for (int i = 0; i < medalPoint.childCount; i++)
                        {
                            var child = medalPoint.GetChild(i);
                            UnityEngine.Object.Destroy(child.gameObject);
                        }
                        if (config.Off != 1)
                        {
                            await CreateAppearanceModel(config.Name, medalPoint);
                        }
                        break;
                    }
                //case ConfigType.Decoration:
                //    {
                //        Transform decorationPoint = referenceCollector.Get<Transform>("DecorationPoint");
                //        for (int i = 0; i < decorationPoint.childCount; i++)
                //        {
                //            var child = decorationPoint.GetChild(i);
                //            UnityEngine.Object.Destroy(child.gameObject);
                //        }
                //        if (config.Off != 1)
                //        {
                //            await CreateAppearanceModel(config.Name, decorationPoint);
                //        }
                //        break;
                //    }
                default:
                    {
                        Log.Info($"Doesnt exist 'LoungeUtility.ConfigType' type:{config.Type}");
                        break;
                    }
            }
        }

        public static void ChangeLoungeSettingByConfigId(long id, ref PlayerCharSetting setting)
        {
            if(setting == null)
            {
                setting = new PlayerCharSetting();
            }
            CharacterConfig config = GetAppearanceConfig(id);
            switch ((ConfigType)config.Type)
            {
                case ConfigType.Bicycle:
                    {
                        setting.BicycleId = config.Id;
                        break;
                    }
                case ConfigType.Body:
                    {
                        setting.BodyId = config.Id;
                        break;
                    }
                case ConfigType.LightWheel:
                    {
                        setting.DecorationId = config.Id;
                        break;
                    }
                case ConfigType.Medal:
                    {
                        setting.MedalId = config.Id;
                        break;
                    }
                //case ConfigType.Decoration:
                //    {
                //        setting.DecorationId = config.Id;
                //        break;
                //    }
                default:
                    {
                        Log.Info($"Doesnt exist 'LoungeUtility.ConfigType' type");
                        break;
                    }
            }
        }

        #region Extension method

        public static Color ToColor(this CharacterConfig config)
        {
            if (config.Off == 1)
            {
                return Color.white;
            }
            return ConvertIntArray2Color(config.Color);
        }

        public static void Reset(this PlayerCharSetting setting)
        {
            setting.CharacterId = INVALID;
            setting.BicycleId = INVALID;
            setting.BodyId = INVALID;
            setting.DecorationId = INVALID;
            setting.MedalId = INVALID;
        }

#endregion

        #region 網路相關
        public static async ETTask<List<EquipmentInfo>> GetUserAllEquipment(long uid)
        {
            Session session = Game.Scene.GetComponent<SessionComponent>().Session;
            var result = await session.Call(new C2L_GetUserAllEquipment { Uid = uid }) as L2C_GetUserAllEquipment;
            return result.EquipmentInfoList.ToList();
        }

        [MessageHandler(AppType.Benchmark)]
        public class L2C_GiveMeCreatedEquipmentsHandler : AMHandler<L2C_OnEquipmentsCreated>
        {
            protected override void Run(ETModel.Session session, L2C_OnEquipmentsCreated message)
            {
                Log.Info($"message.FromUid:{message.FromUid}");
                Game.EventSystem.Run(EventIdType.GiveMeCreatedEquipments, message.FromUid, message.EquipmentInfoList);
            }
        }

        [MessageHandler(AppType.Benchmark)]
        public class L2C_GiveMeDeletedEquipmentsHandler : AMHandler<L2C_OnEquipmentsDeleted>
        {
            protected override void Run(ETModel.Session session, L2C_OnEquipmentsDeleted message)
            {
                Log.Info($"message.FromUid:{message.FromUid}");
                Game.EventSystem.Run(EventIdType.GiveMeDeletedEquipments, message.FromUid, message.EquipmentInfoList);
            }
        }
        #endregion

#endif
    }
}