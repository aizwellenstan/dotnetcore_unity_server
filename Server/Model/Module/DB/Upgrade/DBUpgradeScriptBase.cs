using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ETModel
{
    /// <summary>
    /// 執行腳本使用的類別
    /// 請參考MongoDB Command的網址
    /// https://docs.mongodb.com/manual/reference/command/
    /// </summary>
	public abstract class DBUpgradeScriptBase
	{
        public string failedReason { protected set; get; } = string.Empty;

		public abstract int step { get; }

		public virtual string scriptName => GetType().Name.ToLower();

		protected bool isChecked { set; get; }

		public DBComponent db => Game.Scene.GetComponent<DBComponent>();

		public async ETTask<bool> IsValid()
        {
            try
            {
                return await _IsValid();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        protected abstract ETTask<bool> _IsValid();

        public async ETTask Run()
        {
            try
            {
                await _Run();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        protected abstract ETTask _Run();
	}
}
