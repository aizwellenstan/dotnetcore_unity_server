using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using ETHotfix;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ETModel
{
    /// <summary>
    /// 參考文件
    /// 1.https://firebase.google.com/docs/reference/admin/dotnet/class/firebase-admin/messaging/firebase-messaging#class_firebase_admin_1_1_messaging_1_1_firebase_messaging_1a15a60af9a46d9aba14564656ccfb4847
    /// </summary>
    public class FirebaseComponent : Component
	{
        public const string firebaseUrl = "https://fcm.googleapis.com/fcm/send";
        private const string oAuth = "key=AIzaSyBC6X87T-0c_8HH5VF7ArejF1LD5IiMv1o";

        private FirebaseApp firebaseApp = null;

        private FirebaseMessaging firebaseMessaging = null;

        public FirebaseAuth firebaseAuth { private set; get; } = null;

        // 推播訊息的Json格式範例
        //{
        //    "multicast_id": 7707128547180318330,
        //    "success": 0,
        //    "failure": 1,
        //    "canonical_ids": 0,
        //    "results": [
        //        {
        //            "error": "NotRegistered"
        //        }
        //    ]
        //}

        public void Awake()
        {
            const string credentialFileName = "serviceAccountKey.json";

            // 讀取憑證文件並產生憑證物件
            GoogleCredential googleCredential = null;
            string[] paths = new string[] 
            {
                Path.Combine("..", "Config", "Key", credentialFileName),
                Path.Combine("..", "..", "Config", "Key", credentialFileName)
            };
            string path = paths.FirstOrDefault(e => File.Exists(e));
            if (string.IsNullOrEmpty(path))
            {
                Log.Error($"GoogleCredential 's serviceAccountKey.json doesnt exist on the path!");
                return;
            }
            else
            {
                googleCredential = GoogleCredential.FromFile(path);
            }

            // 產生FirebaseApp實體
            firebaseApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = googleCredential,
            });

            // 產生FirebaseMessaging實體
            firebaseMessaging = FirebaseMessaging.GetMessaging(firebaseApp);

            firebaseAuth = FirebaseAuth.GetAuth(firebaseApp);
        }

        private async ETTask<bool> Post(string url, BsonDocument parameter = null)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(firebaseUrl);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", oAuth);
                byte[] byteArray = Encoding.UTF8.GetBytes(parameter.ToJson());
                using (Stream reqStream = await request.GetRequestStreamAsync())
                {
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return false;
                    }
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        string responseStr = responseStr = reader.ReadToEnd();
                        var result = BsonSerializer.Deserialize<BsonDocument>(responseStr);
                        if(result["success"].ToString() == "0")
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async ETTask<bool> SendOneNotification(string token, string title, string body)
        {
            // 使用WebApi的方式
            //if (string.IsNullOrEmpty(token))
            //{
            //    return false;
            //}
            //BsonDocument bson = new BsonDocument();
            //bson["to"] = token;
            //bson["notification"] = new BsonDocument();
            //bson["notification"]["title"] = title;
            //bson["notification"]["body"] = body;
            //bson["notification"]["icon"] = "big_icon";
            //return await Post(firebaseUrl, bson);

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            try
            {
                Message message = new Message();
                message.Token = token;
                message.Android = new AndroidConfig
                {
                    Notification = new AndroidNotification
                    {
                        Icon = "big_icon",
                    },
                };
                message.Notification = new Notification
                {
                    Title = title,
                    Body = body,
                };

                string messageId = await firebaseMessaging.SendAsync(message);
                if (string.IsNullOrEmpty(messageId))
                {
                    Log.Error($"To send firebase notification is failed!, Token: {token}");
                    return false;
                }
                else
                {
                    Log.Info($"To send firebase notification is successful! MessageID: {messageId}");
                    return true;
                }
            }
            catch(Exception ex)
            {
                Log.Error($"To send firebase notification is failed! Message: {ex.Message}, Stack: {ex.StackTrace}");
                return false;
            }
        }
    }
}