using ETModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ETHotfix
{
    public static class AuthenticationHelper
    {

        public class ThirdPartyInfo
        {
            public string id { set; get; }
            public string name { set; get; } = "";
            public string email { set; get; } = "";
            public string gender { set; get; } = "";
            public string location { set; get; } = "";
            public string birthday { set; get; } = "";

            public int genderCode
            {
                get
                {
                    return (int)(gender == "male" ? User.Gender.Male : User.Gender.Female);
                }
            }

            public int locationCode
            {
                get
                {
                    return (int)User.Location.Usa;
                }
            }

            public int birthdayCode
            {
                get
                {
                    try
                    {
                        string[] split = this.birthday.Split('/');
                        int year, month, day;
                        int.TryParse(split[2], out year);
                        int.TryParse(split[0], out month);
                        int.TryParse(split[1], out day);
                        return year * 10000 + month * 100 + day;
                    }
                    catch (Exception)
                    {
                        return 0;
                    }
                }
            }

            public static ThirdPartyInfo Deserialize(BsonDocument doc)
            {
                return new ThirdPartyInfo
                {
                    id = doc.Contains("id") ? doc["id"]?.ToString() : string.Empty,
                    name = doc.Contains("name") ? doc["name"]?.ToString() : string.Empty,
                    email = doc.Contains("email") ? doc["email"]?.ToString() : string.Empty,
                    gender = doc.Contains("gender") ? doc["gender"]?.ToString() : string.Empty,
                    location = doc.Contains("location") ? doc["location"]?.ToString() : string.Empty,
                    birthday = doc.Contains("birthday") ? doc["birthday"]?.ToString() : string.Empty,
                };
            }
        }

        private async static ETTask RealmToGate(Session session, User user, R2C_Authentication response, bool isRefreshToken)
        {
            // 隨機分配GateServer
            StartConfig config = Game.Scene.GetComponent<RealmGateAddressComponent>().GetAddress();

            // Log.Debug($"gate address: {MongoHelper.ToJson(config)}");
            IPEndPoint innerAddress = config.GetComponent<InnerConfig>().IPEndPoint;
            Session gateSession = Game.Scene.GetComponent<NetInnerComponent>().Get(innerAddress);
            //Game.Scene.GetComponent<PingComponent>().RemoveSession(session.Id);

            // 向Gate請求一個Key,Client可以拿這個Key連接Gate
            G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey)await gateSession.Call(new R2G_GetLoginKey() { Uid = user.Id });

            string outerAddress = config.GetComponent<OuterConfig>().Address2;

            // 創造權杖
            if (isRefreshToken)
            {
                SignInCryptographyHelper.Token tok = new SignInCryptographyHelper.Token
                {
                    uid = user.Id,
                    lastCreateTokenAt = user.lastCreateTokenAt,
                    salt = user.salt,
                };

                string token = SignInCryptographyHelper.EncodeToken(tok);
                response.Token = token;
            }

            PlayerRideTotalInfo playerRideTotalInfo = await UserDataHelper.QueryUserRideAllRecord(user);

            response.Error = ErrorCode.ERR_Success;
            response.Address = outerAddress;
            response.Key = g2RGetLoginKey.Key;
            response.Data = new PlayerBaseInfo
            {
                Uid = user.Id,
                Name = user.name,
                Sex = user.gender,
                Location = user.location,
                Height = user.height,
                Weight = user.weight,
                Birthday = user.birthday,
                CreateAt = user.createAt,
                // 校時用
                LastOnlineAt = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                CharSetting = user.playerCharSetting,
                TotalInfo = playerRideTotalInfo,
                Language = user.language,
            };
            response.LinkTypes.Clear();
            response.LinkTypes.AddRange(await GetAllLinkType(user.Id));
        }

        private async static ETTask SignInByUid(Session session, User user, R2C_Authentication response, string firebaseDeviceToken, bool isRefreshToken, string signInMethod)
        {
            BsonDocument log = new BsonDocument();
            // 更新user登入資訊
            user.lastOnlineAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            log["lastOnlineAt"] = user.lastOnlineAt;  // 最後登入時間
            log["signInMethod"] = signInMethod; // 登入方式
            if (user.firebaseDeviceToken != firebaseDeviceToken)
            {
                user.firebaseDeviceToken = firebaseDeviceToken;
                log["firebaseDeviceToken"] = user.firebaseDeviceToken; // 最後更新的FirebaseToken
            }
            if (isRefreshToken)
            {
                user.lastCreateTokenAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                log["lastCreateTokenAt"] = user.lastCreateTokenAt; // 最後更新Token的時間
            }
            log["ip"] = session.RemoteAddress.ToString(); // 登入位址
            await UserDataHelper.UpsertUser(user, DBLog.LogType.SignUserIn, log);

            // 從Realm轉登到Gate並傳給Client用戶資料
            await RealmToGate(session, user, response, isRefreshToken);
        }

        private static AuthenticationType PartyToAuthenticationType(string party)
        {
            switch (party)
            {
                case UserDataHelper.tagFB:
                    return AuthenticationType.FaceBook;
                case UserDataHelper.tagGuest:
                    return AuthenticationType.Guest;
                case UserDataHelper.tagAppleId:
                    return AuthenticationType.AppleId;
            }
            return AuthenticationType.Token;
        }

        public async static ETTask<List<AuthenticationType>> GetAllLinkType(long uid)
        {
            List<AuthenticationType> linkTypes = new List<AuthenticationType>();
            var thirdPartyUsers = await UserDataHelper.FindAllThirdPartyUser(uid);
            for (int i = 0; i < thirdPartyUsers?.Count; i++)
            {
                linkTypes.Add(PartyToAuthenticationType(thirdPartyUsers[i].party));
            }
            return linkTypes;
        }

        public async static ETTask LinkByFaceBook(Player player, LinkInfo info, L2C_Link response)
        {
            string fbToken = info.Secret;
            bool isValidToken = await FacebookHelper.ValidateFacebookToken(fbToken);
            if (!isValidToken)
            {
                response.Error = ErrorCode.ERR_LinkFailed;
                return;
            }
            ThirdPartyInfo fbInfo = await FacebookHelper.GetFacebookUserInfo(fbToken);
            if (fbInfo == null)
            {
                response.Error = ErrorCode.ERR_LinkFailed;
                return;
            }
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ThirdPartyUser thirdPartyUser = await UserDataHelper.FindOneThirdPartyUser(fbInfo.id, UserDataHelper.tagFB);
            if (thirdPartyUser == null)
            {
                long uid = player.uid;
                User user = await UserDataHelper.FindOneUser(uid);
                if (user == null)
                {
                    response.Error = ErrorCode.ERR_LinkFailed;
                    return;
                }

                //綁定第三方-FB
                thirdPartyUser = ComponentFactory.CreateWithId<ThirdPartyUser>(IdGenerater.GenerateId());
                thirdPartyUser.uid = user.Id;
                thirdPartyUser.party = UserDataHelper.tagFB;
                thirdPartyUser.userId = fbInfo.id;
                thirdPartyUser.gender = fbInfo.gender;
                thirdPartyUser.location = fbInfo.location;
                thirdPartyUser.email = fbInfo.email;
                thirdPartyUser.name = fbInfo.name;
                thirdPartyUser.birthday = fbInfo.birthday;
                thirdPartyUser.createAt = now;
                await UserDataHelper.UpsertThirdPartyUser(thirdPartyUser);

                //取得新的第三方列表
                response.LinkTypes.Clear();
                response.LinkTypes.AddRange(await GetAllLinkType(user.Id));
            }
            else
            {
                response.Error = ErrorCode.ERR_LinkIsExist;
            }
        }

        public async static ETTask LinkByAppleId(Player player, LinkInfo info, L2C_Link response)
        {
            string appleId = CryptographyHelper.AESDecrypt(info.Secret);
            ThirdPartyInfo appleInfo = new ThirdPartyInfo
            {
                id = appleId,
            };
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ThirdPartyUser thirdPartyUser = await UserDataHelper.FindOneThirdPartyUser(appleInfo.id, UserDataHelper.tagAppleId);
            if (thirdPartyUser == null)
            {
                long uid = player.uid;
                User user = await UserDataHelper.FindOneUser(uid);
                if (user == null)
                {
                    response.Error = ErrorCode.ERR_LinkFailed;
                    return;
                }

                // 綁定第三方-Apple
                thirdPartyUser = ComponentFactory.CreateWithId<ThirdPartyUser>(IdGenerater.GenerateId());
                thirdPartyUser.uid = user.Id;
                thirdPartyUser.party = UserDataHelper.tagAppleId;
                thirdPartyUser.userId = appleInfo.id;
                thirdPartyUser.gender = appleInfo.gender;
                thirdPartyUser.location = appleInfo.location;
                thirdPartyUser.email = appleInfo.email;
                thirdPartyUser.name = appleInfo.name;
                thirdPartyUser.birthday = appleInfo.birthday;
                thirdPartyUser.createAt = now;
                await UserDataHelper.UpsertThirdPartyUser(thirdPartyUser);

                // 取得新的第三方列表
                response.LinkTypes.Clear();
                response.LinkTypes.AddRange(await GetAllLinkType(user.Id));
            }
            else
            {
                response.Error = ErrorCode.ERR_LinkIsExist;
            }
        }

        public async static ETTask AuthenticationByToken(Session session, AuthenticationInfo info, R2C_Authentication response)
        {
            SignInCryptographyHelper.Token tok = null;
            try
            {
                tok = SignInCryptographyHelper.DecodeToken(info.Secret);
                if(tok == null)
                {
                    response.Error = ErrorCode.ERR_InvalidToken;
                    return;
                }
            }
            catch (Exception e)
            {
                response.Error = ErrorCode.ERR_InvalidToken;
                return;
            }

            User user = await UserDataHelper.FindOneUser(tok.uid);
            if (user != null)
            {
                if (user.salt != tok.salt || user.lastCreateTokenAt != tok.lastCreateTokenAt)
                {
                    response.Error = ErrorCode.ERR_InvalidToken;
                }
                else
                {
                    await SignInByUid(session, user, response, info.FirebaseDeviceToken, false, UserDataHelper.tagToken);
                }
            }
            else
            {
                response.Error = ErrorCode.ERR_AccountDoesntExist;
            }
        }

        public async static ETTask<int> AuthenticationByBot(string deviceUniqueIdentifier)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ThirdPartyUser thirdPartyUser = await UserDataHelper.FindOneThirdPartyUser(deviceUniqueIdentifier, UserDataHelper.tagGuest);
            User user = null;
            if (thirdPartyUser == null)
            {
                user = ComponentFactory.CreateWithId<User>(IdGenerater.GenerateId());
                string salt = CryptographyHelper.GenerateRandomId();
                string password = CryptographyHelper.GenerateRandomId(16);
                string hashPassword = CryptographyHelper.MD5Encoding(password, salt);
                user.salt = salt;
                user.hashPassword = hashPassword;
                user.createAt = now;
                user.name = $"{user.Id}";
                user.playerCharSetting = new PlayerCharSetting();
                user.playerCharSetting.CharacterId = 1L;
                user.playerRideTotalInfo = new RideTotalInfo();
                user.language = 10;
                user.identity = (int)User.Identity.TestPlayer;
                user.userBagCapacity = EquipmentDataHelper.GetDefaultUserBag();
                await UserDataHelper.SinUserUp(user);

                //註冊第三方-Guest
                thirdPartyUser = ComponentFactory.CreateWithId<ThirdPartyUser>(IdGenerater.GenerateId());
                thirdPartyUser.uid = user.Id;
                thirdPartyUser.party = UserDataHelper.tagGuest;
                thirdPartyUser.userId = deviceUniqueIdentifier;
                thirdPartyUser.name = "";
                thirdPartyUser.gender = "";
                thirdPartyUser.location = "";
                thirdPartyUser.email = "";
                thirdPartyUser.birthday = "";
                thirdPartyUser.createAt = now;
                await UserDataHelper.UpsertThirdPartyUser(thirdPartyUser);
                return ErrorCode.ERR_Success;
            }
            else
            {
                return ErrorCode.ERR_DeviceUniqueIdentifierIsExist;
            }
        }

        public async static ETTask AuthenticationByGuest(Session session, AuthenticationInfo info, R2C_Authentication response)
        {
            string deviceUniqueIdentifier = string.Empty;
            try
            {
                deviceUniqueIdentifier = CryptographyHelper.AESDecrypt(info.Secret);
            }
            catch (Exception)
            {
                response.Error = ErrorCode.ERR_InvalidDeviceUniqueIdentifier;
                return;
            }

            if (string.IsNullOrEmpty(deviceUniqueIdentifier))
            {
                response.Error = ErrorCode.ERR_DeviceUniqueIdentifierIsNull;
                return;
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ThirdPartyUser thirdPartyUser = await UserDataHelper.FindOneThirdPartyUser(deviceUniqueIdentifier, UserDataHelper.tagGuest);
            User user = null;
            if (thirdPartyUser == null)
            {
                user = ComponentFactory.CreateWithId<User>(IdGenerater.GenerateId());
                string salt = CryptographyHelper.GenerateRandomId();
                string password = CryptographyHelper.GenerateRandomId(16);
                string hashPassword = CryptographyHelper.MD5Encoding(password, salt);
                user.salt = salt;
                user.hashPassword = hashPassword;
                user.createAt = now;
                user.name = $"{user.Id}";
                user.email = "";
                user.playerCharSetting = new PlayerCharSetting();
                user.playerCharSetting.CharacterId = 1L;
                user.playerRideTotalInfo = new RideTotalInfo();
                user.language = info.Language;
                user.identity = (int)User.Identity.Player;
                user.userBagCapacity = EquipmentDataHelper.GetDefaultUserBag();
                await UserDataHelper.SinUserUp(user);

                //註冊第三方-Guest
                thirdPartyUser = ComponentFactory.CreateWithId<ThirdPartyUser>(IdGenerater.GenerateId());
                thirdPartyUser.uid = user.Id;
                thirdPartyUser.party = UserDataHelper.tagGuest;
                thirdPartyUser.userId = deviceUniqueIdentifier;
                thirdPartyUser.name = "";
                thirdPartyUser.gender = "";
                thirdPartyUser.location = "";
                thirdPartyUser.email = "";
                thirdPartyUser.birthday = "";
                thirdPartyUser.createAt = now;
                await UserDataHelper.UpsertThirdPartyUser(thirdPartyUser);
            }
            else
            {
                user = await UserDataHelper.FindOneUser(thirdPartyUser.uid);
            }

            await SignInByUid(session, user, response, info.FirebaseDeviceToken, true, UserDataHelper.tagGuest);
        }

        public async static ETTask AuthenticationByFaceBook(Session session, AuthenticationInfo info, R2C_Authentication response)
        {
            string fbToken = info.Secret;
            bool isValidToken = await FacebookHelper.ValidateFacebookToken(fbToken);
            if (!isValidToken)
            {
                response.Error = ErrorCode.ERR_FBSignInFailed;
                return;
            }
            ThirdPartyInfo fbInfo = await FacebookHelper.GetFacebookUserInfo(fbToken);
            if(fbInfo == null)
            {
                response.Error = ErrorCode.ERR_FBSignInFailed;
                return;
            }
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ThirdPartyUser thirdPartyUser = await UserDataHelper.FindOneThirdPartyUser(fbInfo.id, UserDataHelper.tagFB);
            User user = null;
            if (thirdPartyUser == null)
            {
                // 用FB註冊帳號
                user = ComponentFactory.CreateWithId<User>(IdGenerater.GenerateId());
                string salt = CryptographyHelper.GenerateRandomId();
                string password = CryptographyHelper.GenerateRandomId(16);
                string hashPassword = CryptographyHelper.MD5Encoding(password, salt);
                user.salt = salt;
                user.hashPassword = hashPassword;
                user.email = fbInfo.email;
                user.name = fbInfo.name;
                user.gender = fbInfo.genderCode;
                user.location = fbInfo.locationCode;
                user.birthday = fbInfo.birthdayCode;
                user.createAt = now;
                user.playerCharSetting = new PlayerCharSetting();
                user.playerCharSetting.CharacterId = 1L;
                user.playerRideTotalInfo = new RideTotalInfo();
                user.language = info.Language;
                user.identity = (int)User.Identity.Player;
                user.userBagCapacity = EquipmentDataHelper.GetDefaultUserBag();
                await UserDataHelper.SinUserUp(user);

                //註冊第三方-FB
                thirdPartyUser = ComponentFactory.CreateWithId<ThirdPartyUser>(IdGenerater.GenerateId());
                thirdPartyUser.uid = user.Id;
                thirdPartyUser.party = UserDataHelper.tagFB;
                thirdPartyUser.userId = fbInfo.id;
                thirdPartyUser.gender = fbInfo.gender;
                thirdPartyUser.location = fbInfo.location;
                thirdPartyUser.email = fbInfo.email;
                thirdPartyUser.name = fbInfo.name;
                thirdPartyUser.birthday = fbInfo.birthday;
                thirdPartyUser.createAt = now;
                await UserDataHelper.UpsertThirdPartyUser(thirdPartyUser);

                //註冊第三方-Guest
                thirdPartyUser = ComponentFactory.CreateWithId<ThirdPartyUser>(IdGenerater.GenerateId());
                thirdPartyUser.uid = user.Id;
                thirdPartyUser.party = UserDataHelper.tagGuest;
                thirdPartyUser.userId = info.DeviceId;
                thirdPartyUser.name = "";
                thirdPartyUser.gender = "";
                thirdPartyUser.location = "";
                thirdPartyUser.email = "";
                thirdPartyUser.birthday = "";
                thirdPartyUser.createAt = now;
                await UserDataHelper.UpsertThirdPartyUser(thirdPartyUser);
            }
            else
            {
                user = await UserDataHelper.FindOneUser(thirdPartyUser.uid);
            }

            await SignInByUid(session, user, response, info.FirebaseDeviceToken, true, UserDataHelper.tagFB);
        }

        /// <summary>
        /// 使用AppleID登入
        /// </summary>
        /// <param name="session"></param>
        /// <param name="info"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public async static ETTask AuthenticationByAppleId(Session session, AuthenticationInfo info, R2C_Authentication response)
        {
            string appleId = CryptographyHelper.AESDecrypt(info.Secret);
            ThirdPartyInfo appleInfo = new ThirdPartyInfo
            {
                id = appleId,
            };
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ThirdPartyUser thirdPartyUser = await UserDataHelper.FindOneThirdPartyUser(appleInfo.id, UserDataHelper.tagAppleId);
            User user = null;
            if (thirdPartyUser == null)
            {
                // 用AppleId註冊帳號
                user = ComponentFactory.CreateWithId<User>(IdGenerater.GenerateId());
                string salt = CryptographyHelper.GenerateRandomId();
                string password = CryptographyHelper.GenerateRandomId(16);
                string hashPassword = CryptographyHelper.MD5Encoding(password, salt);
                user.salt = salt;
                user.hashPassword = hashPassword;
                user.email = appleInfo.email;
                user.name = user.Id.ToString();
                user.gender = appleInfo.genderCode;
                user.location = appleInfo.locationCode;
                user.birthday = appleInfo.birthdayCode;
                user.createAt = now;
                user.playerCharSetting = new PlayerCharSetting();
                user.playerCharSetting.CharacterId = 1L;
                user.playerRideTotalInfo = new RideTotalInfo();
                user.language = info.Language;
                user.identity = (int)User.Identity.Player;
                user.userBagCapacity = EquipmentDataHelper.GetDefaultUserBag();
                await UserDataHelper.SinUserUp(user);

                // 註冊第三方-AppleId
                thirdPartyUser = ComponentFactory.CreateWithId<ThirdPartyUser>(IdGenerater.GenerateId());
                thirdPartyUser.uid = user.Id;
                thirdPartyUser.party = UserDataHelper.tagAppleId;
                thirdPartyUser.userId = appleInfo.id;
                thirdPartyUser.gender = appleInfo.gender;
                thirdPartyUser.location = appleInfo.location;
                thirdPartyUser.email = appleInfo.email;
                thirdPartyUser.name = appleInfo.name;
                thirdPartyUser.birthday = appleInfo.birthday;
                thirdPartyUser.createAt = now;
                await UserDataHelper.UpsertThirdPartyUser(thirdPartyUser);

                // 註冊第三方-Guest
                thirdPartyUser = ComponentFactory.CreateWithId<ThirdPartyUser>(IdGenerater.GenerateId());
                thirdPartyUser.uid = user.Id;
                thirdPartyUser.party = UserDataHelper.tagGuest;
                thirdPartyUser.userId = info.DeviceId;
                thirdPartyUser.name = "";
                thirdPartyUser.gender = "";
                thirdPartyUser.location = "";
                thirdPartyUser.email = "";
                thirdPartyUser.birthday = "";
                thirdPartyUser.createAt = now;
                await UserDataHelper.UpsertThirdPartyUser(thirdPartyUser);
            }
            else
            {
                user = await UserDataHelper.FindOneUser(thirdPartyUser.uid);
            }

            await SignInByUid(session, user, response, info.FirebaseDeviceToken, true, UserDataHelper.tagAppleId);
        }
    }

    public class FacebookHelper
    {
        private const string tokenToInspect = "{0}";

        private const string appToken = "{1}";

        public const string clientId = "840397989678099";

        public const string clientSecret = "329d3c0cb840d1742dc8889ec101eb02";

        public static string requestAppToken => $"https://graph.facebook.com/oauth/access_token?client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials";

        public static string validateClientTokenApi => $"https://graph.facebook.com/debug_token?input_token={tokenToInspect}&access_token={appToken}";

        public static string getUserInfoApi => $"https://graph.facebook.com/me?fields={string.Join(",", myInfoFields)}&access_token={tokenToInspect}";

        private static readonly string[] myInfoFields = new string[]
        {
            "id", "name", "gender", "location", "picture", "email", "birthday"
        };

        public static async ETTask<bool> ValidateFacebookToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            // 請求應用程式的Token，以驗證Unity client Facebook SDK給的Token
            var appDoc = await Get(requestAppToken);
            if (appDoc == null || !appDoc.Contains("access_token"))
            {
                return false;
            }
            // 驗證Unity client給的FB Token
            var appToken = appDoc["access_token"].AsString;
            var validUrl = string.Format(validateClientTokenApi, token, appToken);
            var validDoc = await Get(validUrl);
            if (validDoc != null)
            {
                if (validDoc.Contains("data"))
                {
                    var data = validDoc["data"].AsBsonDocument;
                    if (!data.Contains("is_valid"))
                    {
                        return false;
                    }
                    else
                    {
                        var isValid = data["is_valid"].AsBoolean;
                        if (!isValid)
                        {
                            return false;
                        }
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static async ETTask<AuthenticationHelper.ThirdPartyInfo> GetFacebookUserInfo(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var userUrl = string.Format(getUserInfoApi, token);
            var userDoc = await Get(userUrl);
            if (userDoc == null)
            {
                return null;
            }
            AuthenticationHelper.ThirdPartyInfo fbInfo = AuthenticationHelper.ThirdPartyInfo.Deserialize(userDoc);
            return fbInfo;
        }

        private static async ETTask<BsonDocument> Get(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "application/json";

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return null;
                    }
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        string responseStr = responseStr = reader.ReadToEnd();
                        var result = BsonSerializer.Deserialize<BsonDocument>(responseStr);
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }
    }
}
