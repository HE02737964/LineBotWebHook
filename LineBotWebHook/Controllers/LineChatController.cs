using System;
using System.IO;
using System.Text;
using System.Web.Http;
using System.Threading.Tasks;
using System.Linq;
using isRock.LineBot;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace LineBotWebHook.Controllers
{
    public class Global
    {
        public static int id;
        public static string city;
        public static bool quiet = false;
    }

    public class LineChatController : ApiController
    {
        [HttpPost]
        public IHttpActionResult POST()
        {
            string ChannelAccessToken = "WJ10DIkt+Y8abCGbAb/OmK/35F3iZ3h4xiB3CE8/B6SzkfTg4mLGonn/9nVI/P33o6+SnGwSBtKYjfa65YJSdfskV7Lf5RtWUAEmZajgARKdJva3ufd3cn7i/H4F/FXA685e75j18IsFjwDgSmugcQdB04t89/1O/w1cDnyilFU=";

            Microsoft.Cognitive.LUIS.LuisClient lc = new Microsoft.Cognitive.LUIS.LuisClient("ac6c1ff9-b031-4eff-bf31-442a202b1038", "61c5bbfb4ac5435b8f839a9528d50a46", true, "westus");

            Bot LineBot = new Bot(ChannelAccessToken);
            string postData = Request.Content.ReadAsStringAsync().Result;
            //剖析JSON
            var ReceivedMessage = Utility.Parsing(postData);
            var ReplyToken = ReceivedMessage.events[0].replyToken;

            //取得用戶名稱
            //var UserName = LineBot.GetUserInfo(ReceivedMessage.events.FirstOrDefault().source.userId);

            //取得用戶ID
            var UserId = ReceivedMessage.events.FirstOrDefault().source.userId;
            //取得聊天室ID
            var RoomId = ReceivedMessage.events.FirstOrDefault().source.roomId;
            //取得群組ID
            var GroupId = ReceivedMessage.events.FirstOrDefault().source.groupId;
            var item = ReceivedMessage.events[0];

            var weather_api = "https://works.ioa.tw/weather/api/all.json";

            try
            {
                LineUserInfo UserInfo = null;
                if (ReceivedMessage.events[0].type == "message")
                {
                    if(ReceivedMessage.events[0].message.type == "text")
                    {
                        var UserSays = ReceivedMessage.events[0].message.text;
                        var LuisResult = lc.Predict(UserSays).Result;

                        if (UserSays == "安靜")
                        {
                            Global.quiet = true;
                            LineBot.ReplyMessage(ReplyToken, "找蟹安靜!!!");
                        }
                        else if (UserSays == "說話") Global.quiet = false;

                        if (UserSays == "狀態")
                        {
                            if (Global.quiet == true) LineBot.ReplyMessage(ReplyToken, "目前為安靜狀態");
                            else LineBot.ReplyMessage(ReplyToken, "目前為說話狀態");
                        }
                        if (LuisResult.TopScoringIntent.Name == "詢問天氣" && LuisResult.TopScoringIntent.Score > 0.8)
                        {
                            if (LuisResult.Entities.FirstOrDefault().Value.FirstOrDefault().Value == "淡水")
                            {
                                Global.id = 50;
                                Global.city = "淡水";
                            }
                            else if (LuisResult.Entities.FirstOrDefault().Value.FirstOrDefault().Value == "北投")
                            {
                                Global.id = 9;
                                Global.city = "北投";
                            }

                            var town_weather = "https://works.ioa.tw/weather/api/weathers/" + Global.id + ".json";

                            WebClient wc = new WebClient();
                            wc.Encoding = Encoding.UTF8;

                            string jsonStr = wc.DownloadString(town_weather);
                            JObject obj = JsonConvert.DeserializeObject<JObject>(jsonStr);

                            var desc = "";
                            var temperature = "";
                            var felt_air_temp = "";
                            var humidity = "";
                            var rainfall = "";
                            var sunrise = "";
                            var sunset = "";
                            var at = "";

                            desc = obj["desc"].ToString();
                            temperature = obj["temperature"].ToString();
                            felt_air_temp = obj["felt_air_temp"].ToString();
                            humidity = obj["humidity"].ToString();
                            rainfall = obj["rainfall"].ToString();
                            sunrise = obj["sunrise"].ToString();
                            sunset = obj["sunset"].ToString();
                            at = obj["at"].ToString();
                            LineBot.ReplyMessage(ReplyToken, Global.city + "\n" + "天氣:" + desc + "\n" + "溫度:" + temperature + "度\n" + "體感溫度:" + felt_air_temp + "度\n" + "雨量:" + rainfall + "\n" + "日出時間:" + sunrise + "\n" + "日落時間:" + sunset + "\n" + "最後更新時間:" + at + "\n" + "資料每30分鐘更新");
                        }
                        else if (ReceivedMessage.events[0].message.type == "text")
                        {
                            if (UserSays == "滾")
                            {
                                if (ReceivedMessage.events[0].source.type == "user")
                                {
                                    LineBot.PushMessage(UserId, "滾屁");
                                }
                                else if (ReceivedMessage.events[0].source.type == "room")
                                {
                                    UserInfo = Utility.GetRoomMemberProfile(RoomId, UserId, ChannelAccessToken);
                                    LineBot.PushMessage(RoomId, $"{UserInfo.displayName}，我記住你了\n掰掰");
                                    Utility.LeaveRoom(RoomId, ChannelAccessToken);
                                }
                                else if (ReceivedMessage.events[0].source.type == "group")
                                {
                                    UserInfo = Utility.GetGroupMemberProfile(GroupId, UserId, ChannelAccessToken);
                                    LineBot.PushMessage(GroupId, $"{UserInfo.displayName}，我記住你了\n掰掰");
                                    Utility.LeaveGroup(GroupId, ChannelAccessToken);
                                }
                            }

                            /*WebClient wc = new WebClient();
                            wc.Encoding = Encoding.UTF8;

                            string jsonStr = wc.DownloadString(weather_api);

                            JArray array = JsonConvert.DeserializeObject<JArray>(jsonStr);

                            int i=0, j=0;

                            JObject obj = (JObject)array[i];

                            var name = obj["name"].ToString();
                            var towns = obj["towns"][j]["name"].ToString();

                            LineBot.PushMessage(UserId, name + "\n" + towns);*/
                        }
                        if (Global.quiet == true) { }
                        else if (Global.quiet == false)
                        {
                            if (ReceivedMessage.events[0].message.type == "text")
                            {
                                if (LuisResult.TopScoringIntent.Name == "尋找葛格" && LuisResult.TopScoringIntent.Score > 0.8)
                                {
                                    LineBot.ReplyMessage(ReplyToken, "嘿嘿嘿！怎麼啦？\n需要葛格提供你什麼服務嗎");
                                    if (ReceivedMessage.events[0].source.type == "user")
                                    {
                                        LineBot.PushMessage(UserId, $"這是一個天氣預報機器人\n在群組或聊天室裡說'滾'我就會離開喔\n希望能幫到你");
                                        LineBot.PushMessage(UserId, 1, 2);
                                    }
                                    else if (ReceivedMessage.events[0].source.type == "room")
                                    {
                                        LineBot.PushMessage(RoomId, $"這是一個天氣預報機器人\n在群組或聊天室裡說'滾'我就會離開喔\n希望能幫到你");
                                        LineBot.PushMessage(RoomId, 1, 2);
                                    }
                                    else if (ReceivedMessage.events[0].source.type == "group")
                                    {
                                        LineBot.PushMessage(GroupId, $"這是一個天氣預報機器人\n在群組或聊天室裡說'滾'我就會離開喔\n希望能幫到你");
                                        LineBot.PushMessage(GroupId, 1, 2);
                                    }
                                }
                                else if (LuisResult.TopScoringIntent.Name == "None" || LuisResult.TopScoringIntent.Score < 0.8)
                                {
                                    if (ReceivedMessage.events[0].source.type == "user")
                                    {
                                        LineBot.ReplyMessage(ReplyToken, UserSays);
                                    }
                                    else if (ReceivedMessage.events[0].source.type == "room")
                                    {
                                        LineBot.ReplyMessage(ReplyToken, UserSays);
                                    }
                                    else if (ReceivedMessage.events[0].source.type == "group")
                                    {
                                        LineBot.ReplyMessage(ReplyToken, UserSays);
                                    }
                                }
                            }
                        }
                    }
                    else if (ReceivedMessage.events[0].message.type == "sticker")
                    {
                        if (Global.quiet == true) { }
                        else if (Global.quiet == false)
                        {
                            string botMessage = "機器人看不懂貼圖你還傳";
                            if (ReceivedMessage.events[0].source.type == "user")
                            {
                                LineBot.PushMessage(UserId, botMessage);
                                LineBot.PushMessage(UserId, 3, 185);
                            }
                            else if (ReceivedMessage.events[0].source.type == "room")
                            {
                                LineBot.PushMessage(RoomId, botMessage);
                                LineBot.PushMessage(RoomId, 3, 185);
                            }
                            else if (ReceivedMessage.events[0].source.type == "group")
                            {
                                LineBot.ReplyMessage(ReplyToken, botMessage);
                                LineBot.PushMessage(GroupId, 3, 185);
                            }
                        }
                    }
                }
                else if (ReceivedMessage.events[0].type == "follow")
                {
                    UserInfo = Utility.GetUserInfo(ReceivedMessage.events[0].source.userId, ChannelAccessToken);
                    LineBot.PushMessage(UserId, $"{UserInfo.displayName}，我來了!!!");
                }
                else if (ReceivedMessage.events[0].type == "join")
                {
                    if (ReceivedMessage.events[0].source.type == "room")
                    {
                        LineBot.PushMessage(RoomId, $"我來了!!!");
                    }
                    else if (ReceivedMessage.events[0].source.type == "group")
                    {
                        LineBot.PushMessage(GroupId, $"我來了!!!");
                    }
                }
                else
                {
                    string Message = ReceivedMessage.events[0].type;
                    LineBot.ReplyMessage(ReplyToken, postData);
                }
                //回覆API OK
                return Ok();
            }
            catch (Exception ex)
            {
                string Message = "我錯了，錯在\n" + ex.Message;
                //回覆用戶
                LineBot.ReplyMessage(ReplyToken, Message);
                return Ok();
            }

        }
    }
}