using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace ETModel
{
	[ObjectSystem]
	public class MasterComponentAwakeSystem : AwakeSystem<MasterComponent>
	{
		public override void Awake(MasterComponent self)
		{
			self.Awake();
		}
	}

    public class MasterComponent : Component
    {
        /// <summary>
        /// 在線的Server
        /// </summary>
        private Dictionary<int, StartConfig> configDict;

        public void Awake()
		{
            this.configDict = new Dictionary<int, StartConfig>();
        }

        public StartConfig[] GetAll()
        {
            return this.configDict.Values.ToArray();
        }

        public bool AddConfig(StartConfig startConfig)
        {
            if (this.configDict.ContainsKey(startConfig.AppId))
                return false;

            this.configDict.Add(startConfig.AppId, startConfig);

            return true;
        }
    }
}