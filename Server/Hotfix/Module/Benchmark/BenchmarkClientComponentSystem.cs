using System;
using System.Net;
using System.Threading.Tasks;
using ETModel;
using MongoDB.Bson;

namespace ETHotfix
{
	[ObjectSystem]
	public class BenchmarkClientAwakeSystem : AwakeSystem<BenchmarkClientComponent, BenchmarkClientComponent.ClientSetting>
	{
		public override void Awake(BenchmarkClientComponent self, BenchmarkClientComponent.ClientSetting clientSetting)
		{
            self.Awake(clientSetting);
		}
	}

    [ObjectSystem]
    public class BenchmarkClientDestroySystem : DestroySystem<BenchmarkClientComponent>
    {
        public override void Destroy(BenchmarkClientComponent self)
        {
            self.Destroy();
        }
    }

    //[ObjectSystem]
    //public class BenchmarkClientUpdateSystem : UpdateSystem<BenchmarkClientComponent>
    //{
    //    public override void Update(BenchmarkClientComponent self)
    //    {
    //        self.Update();
    //    }
    //}

    public class TestPlayerDataComponent : Component
    {
        public TestPlayerSetting testPlayerSetting;
    }

    public static class BenchmarkClientComponentHelper
    {
		public static async void Awake(this BenchmarkClientComponent self, BenchmarkClientComponent.ClientSetting clientSetting)
		{
            try
            {
                self.networkComponent = clientSetting.networkComponent;
                self.ipEndPoint = clientSetting.ipEndPoint;
                self.testPlayerSetting = clientSetting.testPlayerSetting;
                self.stateMachine = new StateMachine<BenchmarkClientComponent>();
                self.robotMode = clientSetting.robotMode;
                self.roadSettingId = clientSetting.roadSettingId;
                self.IsFromPool = false;

                //self.InitializeStateMachine();

                /****************/

                await self.SignIn(self.testPlayerSetting.DeviceUniqueIdentifier);

                //設定玩家資料
                TestPlayerDataComponent playerDataComponent = self.session.AddComponent<TestPlayerDataComponent>();
                playerDataComponent.testPlayerSetting = clientSetting.testPlayerSetting;
                self.Init();
            }
            catch(Exception e)
            {
                Console.WriteLine($"Msg:{e.Message}, Stack:{e.StackTrace}");
            }
        }

        public static void PrintMessage(this BenchmarkClientComponent self)
        {
            if (self.components.Count != 0)
            {
                for (int i = 0; i < self.components.Count; i++)
                {
                    string msg = self.components[i].GetMessage();
                    self.UserLog($"{msg}, Ping:{self.currentPing}, Name:{self.userName}");
                }
            }
        }

        public static void Update(this BenchmarkClientComponent self)
        {
            //self.stateMachine.UpdateState();
            
            if(self.components.Count != 0)
            {
                for(int i = 0; i < self.components.Count; i++)
                {
                    self.components[i].Update();
                }
            }
        }

        public static void Destroy(this BenchmarkClientComponent self)
        {
            if(self.session != null)
            {
                self.session.Dispose();
                self.session = null;
            }
        }

        private static void InitializeStateMachine(this BenchmarkClientComponent self)
        {
            self.stateMachine.SetTarget(self);
            //create states.
            //self.login = self.stateMachine.CreateState<LoginState>();
            //self.roaming = self.stateMachine.CreateState<LoginState>();
            //change to defaule state.
            self.stateMachine.ChangeState(self.login);
        }

        private static void ConnectRealm(this BenchmarkClientComponent self)
        {
            self.session = self.networkComponent.Create(self.ipEndPoint);
            AttachPingComponent(self);
        }

        private static void ConnectGate(this BenchmarkClientComponent self, string address)
        {
            self.session = self.networkComponent.Create(address);
            AttachPingComponent(self);
        }

        private static void AttachPingComponent(BenchmarkClientComponent self)
        {
            var pingComponent = self.session.AddComponent<ETModel.Share.PingComponent, long, Action<Exception>>(BenchmarkClientComponent.pingWaitMilisec, ex => { HandleException(self, ex); });
            pingComponent.onPingChanged += ping => 
            {
                self.currentPing = ping;
                //self.UserLog($"Ping : {ping}");
            };
            self.session.ErrorCallback += (channel, error) => { AChannelError(self, channel, error); };
        }

        private static void HandleException(BenchmarkClientComponent self, Exception ex)
        {
            if (ex.InnerException is RpcException rpcEx)
            {
                ShowErrorMessage(self, rpcEx.Error);
            }
            else
            {
                string err = $"Message:{ex.InnerException.Message}\r\n{ex.InnerException.StackTrace}";
                self.UserLog(err);
                Log.Error(err);
            }
        }

        private static void AChannelError(BenchmarkClientComponent self, AChannel aChannel, int errorCode)
        {
            ShowErrorMessage(self, errorCode);
        }

        private static void ShowErrorMessage(BenchmarkClientComponent self, int errorCode)
        {
            string err = $"Session ErrorCode : {errorCode}";
            self.UserLog(err);
            Log.Error(err);
        }

        public static void UserLog(this BenchmarkClientComponent self, string message)
        {
            Console.WriteLine($"User[{self.testPlayerSetting.DeviceUniqueIdentifier}]> {message}");
        }

        /****************/

        private static async ETTask SignIn(this BenchmarkClientComponent self, string deviceUniqueIdentifier)
        {
            string secret = CryptographyHelper.AESEncrypt(deviceUniqueIdentifier);

            var signInResult = await UserUtility.SignIn(() =>
            {
                //連接Realm
                self.ConnectRealm();
                return self.session;
            }, address =>
            {
                //斷開Realm
                self.session.Dispose();
                //連接Gate
                self.ConnectGate(address);
                return self.session;
            }, new C2R_Authentication()
            {
                Info = new AuthenticationInfo
                {
                    Language = 10,
                    Secret = secret,
                    FirebaseDeviceToken = string.Empty,
                    Type = AuthenticationType.Guest,
                }
            });
            self.userName = signInResult.playerBaseInfo.Name;
            //self.UserLog(signInResult.playerBaseInfo.Name);
        }

        public static void Init(this BenchmarkClientComponent self)
        {
            self.components.Clear();
             switch (self.robotMode)
            {
                case BenchmarkClientComponent.RobotMode.Roaming:
                    self.RemoveComponent<RoamingBotModule>();
                    self.RemoveComponent<MapUnitBotModule>();
                    self.AddComponent<MapUnitBotModule>();
                    self.components.Add(self.AddComponent<RoamingBotModule>());
                    break;
                case BenchmarkClientComponent.RobotMode.Party:
                    self.RemoveComponent<PartyBotModule>();
                    self.RemoveComponent<MapUnitBotModule>();
                    self.AddComponent<MapUnitBotModule>();
                    self.components.Add(self.AddComponent<PartyBotModule>());
                    break;
                default:
                    throw new Exception($"clientSetting.robotMode:{self.robotMode} is not defined!");
            }
            self.UserLog("Reset client");
        }
    }
}