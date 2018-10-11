using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using isRock.LineBot;
using LineBotWebHook.Controllers;
namespace LineBotWebHook
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Bot bot = new isRock.LineBot.Bot("WJ10DIkt+Y8abCGbAb/OmK/35F3iZ3h4xiB3CE8/B6SzkfTg4mLGonn/9nVI/P33o6+SnGwSBtKYjfa65YJSdfskV7Lf5RtWUAEmZajgARKdJva3ufd3cn7i/H4F/FXA685e75j18IsFjwDgSmugcQdB04t89/1O/w1cDnyilFU=");
            var actions = new List<TemplateActionBase>();
            actions.Add(new DateTimePickerAction()
            {
                label = "選取時間",
                mode = "time"
            });
            var ButtonTemplate = new ButtonsTemplate()
            {
                text = "選取時間來設定自動推送訊息喔~~",
                title = "設定推送任務時間",
                thumbnailImageUrl = new Uri("http://www.sayjb.com/wp-content/uploads/2017/02/unnamed-file-9.jpg"),
                actions = actions
            };
            bot.PushMessage("Ub7f9aa656d9493aa12bc911993678a4e", ButtonTemplate);
        }
    }
}