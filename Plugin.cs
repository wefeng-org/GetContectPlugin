using MG.WeCode.WeClients;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeChat.Pb.Entites;

namespace GetContectPlugin
{
    internal class Plugin : IPlugin
    {
        public string OriginId { get; set; }

        public string Name => "获取通信录";

        public string Version => "v0.0.1";

        public string Author => "Byboy";

        public string Description => "获取通信录以文本格式保存,手动插件,需要点击设置运行";

        public void Initialize()
        {
            
        }

        public async void Setting()
        {
            List<string> Username = new();
            List<ModContact> modContact = new();
            int currentWxcontactSeq=0, currentChatRoomContactSeq = 0;
            do {
                var contact = await WeClient.Contact.InitContact(OriginId,currentWxcontactSeq,currentChatRoomContactSeq);
                if (contact.BaseResponse.Ret == 0) {
                    if (contact.ContactUsernameList !=null! && contact.ContactUsernameList.Length != 0) {
                        Username.AddRange(contact.ContactUsernameList);
                        currentWxcontactSeq = contact.CurrentWxcontactSeq;
                        currentChatRoomContactSeq = contact.CurrentChatRoomContactSeq;
                    } else {
                        break;
                    }
                }
            } while (true);
            int i = 0;
            //do while循环,当i的数量比Username的数量多时,停止循环
            do {
                //如果username的数量大于100,则每次循环取100个,并且i+100
                if (Username.Count - i > 100) {
                    var mcontact = await WeClient.Contact.BatchGetContact(OriginId,Username.ToArray()[i..(i+100)].ToList());
                    IEnumerable<ModContact> cts = mcontact.ContactList.Select(t=>t.Contact);
                    modContact.AddRange(cts);
                    i += 100;
                } else {
                    var mcontact = await WeClient.Contact.BatchGetContact(OriginId,Username.ToArray()[i..].ToList());
                    IEnumerable<ModContact> cts = mcontact.ContactList.Select(t => t.Contact);
                    if (cts.Any()) {
                        modContact.AddRange(cts);
                    }
                    i = Username.Count;
                }
            } while (i < Username.Count);
            //如果文件Plugins/联系人信息.inf存在,则删除他
            if (System.IO.File.Exists("Plugins/联系人信息.inf")) {
                System.IO.File.Delete("Plugins/联系人信息.inf");
            }
            modContact =  modContact.Where(t => t.VerifyFlag==0).ToList();
            File.WriteAllText("Plugins/联系人信息.inf",JsonConvert.SerializeObject(modContact));

        }

        public void Terminate()
        {
            
        }
    }
}
