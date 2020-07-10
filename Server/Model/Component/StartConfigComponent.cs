using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ETModel
{
	[ObjectSystem]
	public class StartConfigComponentSystem : AwakeSystem<StartConfigComponent, string, int>
	{
		public override void Awake(StartConfigComponent self, string a, int b)
		{
			self.Awake(a, b);
		}
	}
	
	public class StartConfigComponent: Component
	{
		public static StartConfigComponent Instance { get; private set; }
		
		private Dictionary<int, StartConfig> configDict;
		
		private Dictionary<int, IPEndPoint> innerAddressDict = new Dictionary<int, IPEndPoint>();
		
		public StartConfig StartConfig { get; private set; }

		public StartConfig DBConfig { get; private set; }

		public StartConfig RealmConfig { get; private set; }

		public StartConfig LocationConfig { get; private set; }

		public List<StartConfig> MapConfigs { get; private set; }

		public List<StartConfig> GateConfigs { get; private set; }

        public List<StartConfig> LobbyConfigs { get; private set; }

        public StartConfig MasterConfig { get; private set; }

        public void Awake(string path, int appId)
		{
			Instance = this;
			
			this.configDict = new Dictionary<int, StartConfig>();
			this.MapConfigs = new List<StartConfig>();
			this.GateConfigs = new List<StartConfig>();
            this.LobbyConfigs = new List<StartConfig>();

            string[] ss = File.ReadAllText(path).Split('\n');
			foreach (string s in ss)
			{
				string s2 = s.Trim();
				if (s2 == "")
				{
					continue;
				}
				try
				{
					StartConfig startConfig = MongoHelper.FromJson<StartConfig>(s2);
                    AddConfig(startConfig);
                }
				catch (Exception e)
				{
					Log.Error($"config错误: {s2} {e}");
				}
			}

			this.StartConfig = this.Get(appId);
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();
			
			Instance = null;
		}

		public StartConfig Get(int id)
		{
			try
			{
				return this.configDict[id];
			}
			catch (Exception e)
			{
				throw new Exception($"not found startconfig: {id}", e);
			}
		}
		
		public IPEndPoint GetInnerAddress(int id)
		{
			try
			{
				return this.innerAddressDict[id];
			}
			catch (Exception e)
			{
				throw new Exception($"not found innerAddress: {id}", e);
			}
		}

		public StartConfig[] GetAll()
		{
			return this.configDict.Values.ToArray();
		}

		public int Count
		{
			get
			{
				return this.configDict.Count;
			}
		}

        public bool AddConfig(StartConfig startConfig)
        {
            if (this.configDict.ContainsKey(startConfig.AppId))
                return false;

            this.configDict.Add(startConfig.AppId, startConfig);

            InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
            if (innerConfig != null)
            {
                this.innerAddressDict.Add(startConfig.AppId, innerConfig.IPEndPoint);
            }

            if (startConfig.AppType.Is(AppType.Realm))
            {
                this.RealmConfig = startConfig;
            }

            if (startConfig.AppType.Is(AppType.Location))
            {
                this.LocationConfig = startConfig;
            }

            if (startConfig.AppType.Is(AppType.DB))
            {
                this.DBConfig = startConfig;
            }

            if (startConfig.AppType.Is(AppType.Map))
            {
                this.MapConfigs.Add(startConfig);
            }

            if (startConfig.AppType.Is(AppType.Gate))
            {
                this.GateConfigs.Add(startConfig);
            }

            if (startConfig.AppType.Is(AppType.Lobby))
            {
                this.LobbyConfigs.Add(startConfig);
            }

            if (startConfig.AppType.Is(AppType.Master))
            {
                MasterConfig = startConfig;
            }

            return true;
        }

		/// <summary>
		/// 向Master註冊服務
		/// </summary>
		/// <returns></returns>
        public async ETTask RegisterService()
        {
            // 延遲5秒再跟Master註冊
            await Game.Scene.GetComponent<TimerComponent>().WaitForSecondAsync(5f);

            var startComponent = Game.Scene.GetComponent<StartConfigComponent>();
            IPEndPoint instanceAddress = startComponent.MasterConfig.GetComponent<InnerConfig>().IPEndPoint;
            var session = Game.Scene.GetComponent<NetInnerComponent>().Get(instanceAddress);
            M2S_RegisterService m2S_RegisterService = (M2S_RegisterService)await session.Call(new S2M_RegisterService
            {
                Component = StartConfig,
            });

            if(m2S_RegisterService.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"to register service is failed, error code: {m2S_RegisterService.Error}.");
                return;
            }
			Log.Info($"to register service is successful.");

			foreach (var v in m2S_RegisterService.Components)
            {
				StartConfig startConfig = (StartConfig)v;
				if(startConfig.AppId == StartConfig.AppId)
				{
					continue;
				}

				AddConfig(startConfig);
				bool result = await TryConnectService(startConfig);
				if (!result)
				{
					Log.Error($"to connect service[{startConfig.AppType}:{startConfig.AppId}] is failed");
				}
				else
				{
					Log.Info($"to connect service[{startConfig.AppType}:{startConfig.AppId}] is successful");
				}
			}
		}

        /// <summary>
        /// 嘗試連接服務
        /// </summary>
        /// <param name="startConfig"></param>
        /// <returns></returns>
		private async ETTask<bool> TryConnectService(StartConfig startConfig)
		{
			IPEndPoint instanceAddress = startConfig.GetComponent<InnerConfig>().IPEndPoint;
			var session = Game.Scene.GetComponent<NetInnerComponent>().Get(instanceAddress);

            // 嘗試連接5次，每次間隔5秒
            for (int i = 0; i < 5; i++)
			{
                await Game.Scene.GetComponent<TimerComponent>().WaitForSecondAsync(5f);
                try
                {
                    A2S_ConnectService response = (A2S_ConnectService)await session.Call(new S2A_ConnectService());
                    if (response.Error == ErrorCode.ERR_Success)
                    {
                        return true;
                    }
                }
                catch(Exception)
                {
                    // 不處理例外
                    Log.Info($"to try connecting service[{startConfig.AppType}:{startConfig.AppId}] again on count:{i + 1}.");
                }
			}
			return false;
		}
	}
}
