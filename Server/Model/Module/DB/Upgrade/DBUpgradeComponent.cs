using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ETModel
{
	[ObjectSystem]
	public class DBUpgradeAwakeComponentSystem : AwakeSystem<DBUpgradeComponent>
	{
		public override void Awake(DBUpgradeComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class DBUpgradeDestroyComponentSystem : DestroySystem<DBUpgradeComponent>
	{
		public override void Destroy(DBUpgradeComponent self)
		{
			self.Destroy();
		}
	}

	public class DBUpgradeComponent : Component
	{
		public DBComponent db => Game.Scene.GetComponent<DBComponent>();

		public const string allFindCondition = "{ }";

		public int totalScriptCount => allScriptTable.Count;

		public int newScriptCount { private set; get; } = 0;

		private List<DBUpgradeScript> allUpgradingInfo = new List<DBUpgradeScript>();

		private Dictionary<string, DBUpgradeScriptBase> allScriptTable = new Dictionary<string, DBUpgradeScriptBase>();

		public async void Awake()
		{
			if(db == null)
			{
				throw new Exception($"Component:{typeof(DBUpgradeComponent).Name} must depend on {typeof(DBComponent).Name}!");
			}
			await DetectAndCreateNewScript();
			await RunUpgradingScript();
		}

		public void Destroy()
		{
			allUpgradingInfo.Clear();
			allScriptTable.Clear();
			newScriptCount = 0;
			allUpgradingInfo = null;
			allScriptTable = null;
		}

		private async ETTask DetectAndCreateNewScript()
		{
			List<ComponentWithId> components = await db.GetJson(typeof(DBUpgradeScript).Name, allFindCondition);
            List<int> steps = new List<int>();
			allUpgradingInfo = components.OfType<DBUpgradeScript>().ToList();
			foreach (Type type in Game.EventSystem.GetTypes(typeof(DBUpgradeScriptAttribute))
				.Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(DBUpgradeScriptBase))))
			{
				DBUpgradeScriptBase script = (DBUpgradeScriptBase)Activator.CreateInstance(type);
                if (!steps.Contains(script.step))
                {
                    steps.Add(script.step);
                }
                else
                {
                    throw new Exception($"To create upgrading script is failed! Reason: repeated step: {script.step}");
                }
				string scriptName = script.scriptName.ToLower();
				if (!allUpgradingInfo.Any(e => e.step == script.step))
				{
					DBUpgradeScript newScript = ComponentFactory.CreateWithId<DBUpgradeScript>(IdGenerater.GenerateId());
					newScript.step = script.step;
					newScript.scriptName = scriptName;
					newScript.isChecked = false;
					newScript.createAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
					allUpgradingInfo.Add(newScript);
					newScriptCount++;
				}
				allScriptTable.Add(scriptName, script);
			}
			if(allUpgradingInfo.Count != totalScriptCount)
			{
				throw new Exception($"Database upgrading scripts count is not consistent with table {typeof(DBUpgradeScript).Name}");
			}
			await db.AddBatch(allUpgradingInfo.OfType<ComponentWithId>().ToList(), typeof(DBUpgradeScript).Name);
			Console.WriteLine($"to create database upgrading script is finished");
			Console.WriteLine($"new script count : {newScriptCount}, total script count : {totalScriptCount}");
		}

		private async ETTask RunUpgradingScript()
		{
			if(allUpgradingInfo.Count == 0)
			{
				Console.WriteLine($"no any upgrading script available");
				return;
			}
			foreach(DBUpgradeScript script in allUpgradingInfo.OrderBy(e => e.step))
			{
				string scriptName = script.scriptName.ToLower();
				if (!script.isChecked)
				{
					allScriptTable.TryGetValue(scriptName, out DBUpgradeScriptBase dBUpgradeScriptBase);
					await dBUpgradeScriptBase.Run();
					if(await dBUpgradeScriptBase.IsValid())
					{
						script.isChecked = true;
						script.checkAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
						await db.Add(script);
						Console.WriteLine($"to upgrade database with step:{script.step}, script:{scriptName} is successful");
					}
					else
					{
                        string reason = dBUpgradeScriptBase.failedReason;
                        if (string.IsNullOrEmpty(reason))
                        {
                            reason = "Unknown error(database upgrading script is not defined)";
                        }
						throw new Exception($"to upgrade database failed!\r\nreason: {reason}\r\nplease try again!(reboot server)");
					}
				}
			}
			Console.WriteLine($"all upgrading scripts is finished!");
		}
	}
}
