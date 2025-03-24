using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TetraCreations.Attributes;
using UnityEngine;
using UnityEngine.UI;
using static HGF.GalManager.Struct_PlotData;
namespace HGF
{
    public class GalManager : MonoBehaviour
    {
        public static string PlotID;

        [Title("当前对话")]
        ///
        public GalManager_Text Gal_Text;

        [Title("当前角色部分")]
        public GalManager_CharacterImg Gal_CharacterImg;

        [Title("控制选项")]
        public GalManager_Choice Gal_Choice;

        [Title("控制背景图片的组件")]
        public GalManager_BackImg Gal_BackImg;

        /// <summary>
        /// 当前场景角色数量
        /// </summary>
        [Title("当前场景角色数量")]
        public int CharacterNum;

        public class AudioSystemModel
        {
            public AudioSource Character_Voice;
            public AudioSource BackMix;
            public AudioSource TextMix;
            public class AudioInfo
            {
                public string name;
                public string path;
            }
            public List<AudioInfo> AudioList = new();
            /// <summary>
            /// 背景音乐Clip
            /// </summary>

        }
        public static AudioSystemModel AudioSystem = new();
        /// <summary>
        /// 存储整个剧本的XML文档
        /// </summary>
        private XDocument PlotxDoc;
        [Serializable]
        public class Struct_PlotData
        {
            public JObject ConfigCharacterInfo = new();
            public JObject ConfigDepartment = new();
            public string Title;
            public string Synopsis;
            public List<XElement> BranchPlot = new();
            public Queue<XElement> BranchPlotInfo = new();
            public Queue<XElement> MainPlot = new();
            public class Struct_Choice
            {
                public string Title;
                public string JumpID;
            }
            public class Struct_CharacterInfo
            {
                public string CharacterID;
                public GameObject CharacterGameObject;
                public string Name;
                public string Affiliation;
                public string FromID;
            }
            public List<Struct_CharacterInfo> CharacterInfo = new();
            public List<Struct_Choice> ChoiceText = new();
            /// <summary>
            /// 当前的剧情节点
            /// </summary>
            public XElement NowPlotDataNode;

            /// <summary>
            /// 当前是否为分支剧情节点
            /// </summary>
            public bool IsBranch = false;
            public string NowJumpID;

        }

        public static Struct_PlotData PlotData = new();
        public void StartGame (HGFStartModel model)
        {
            PlotData = new Struct_PlotData();
            PlotData.ConfigCharacterInfo = JsonConvert.DeserializeObject<JObject>(model.characterInfo);
            PlotData.ConfigDepartment = JsonConvert.DeserializeObject<JObject>(model.departmentInfo);
            StartBackAudio();
            StartCoroutine(LoadPlot(model.plotText));
            return;
        }
        /// <summary>
        /// 解析框架文本
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadPlot (string plotText)
        {
            yield return null;
            try
            {

                GameAPI.Print($"游戏剧本：{plotText}");
                PlotxDoc = XDocument.Parse(plotText);

                //-----开始读取数据

                foreach (var item in PlotxDoc.Root.Elements())
                {
                    switch (item.Name.ToString())
                    {
                        case "ID":
                        {
                            PlotID = item.Value;
                            break;
                        }
                        case "title":
                        {
                            PlotData.Title = item.Value;
                            break;
                        }
                        case "Synopsis":
                        {
                            PlotData.Synopsis = item.Value;
                            break;
                        }
                        case "BranchPlot":
                        {
                            foreach (var BranchItem in item.Elements())
                            {
                                PlotData.BranchPlot.Add(BranchItem);
                            }
                            break;
                        }
                        case "AudioList":
                        {
                            foreach (var item_name in item.Elements())
                            {

                                AudioSystem.AudioList.Add(new AudioSystemModel.AudioInfo
                                {
                                    name = item_name.Value,
                                    path = item_name.Attribute("Path").Value,
                                });
                            }
                            break;
                        }
                        case "MainPlot":
                        {
                            foreach (var MainPlotItem in item.Elements())
                            {
                                PlotData.MainPlot.Enqueue(MainPlotItem);
                            }
                            break;
                        }
                        default:
                        {
                            throw new Exception("无法识别的根标签");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "无法识别的根标签")
                {

                    GameAPI.Print(ex.Message, "error");
                }
            }
            GameAPI.Print(Newtonsoft.Json.JsonConvert.SerializeObject(PlotData));
            Button_Click_NextPlot();
        }
        /// <summary>
        /// 点击屏幕 下一句
        /// </summary>
        public void Button_Click_NextPlot ()
        {

            if (PlotData.MainPlot.Count == 0)
            {
                GameAPI.Print("游戏结束!");
                return;
            }

            //IsCanJump这里有问题，如果一直点击会为false，而不是说true，这是因为没有点击按钮 ，没有添加按钮
            if (GalManager_Text.IsSpeak || !GalManager_Text.IsCanJump) { return; }
            if (!PlotData.IsBranch)
            {
                PlotData.MainPlot.TryDequeue(out PlotData.NowPlotDataNode);//队列出队+内联 出一个temp节点
                PlotData.BranchPlotInfo.Clear();
            }
            else//当前为分支节点
            {
                //这块得妥善处理
                PlotData.NowPlotDataNode = GetBranchByID(PlotData.NowJumpID);
            }

            PlotData.ChoiceText.Clear();
            if (PlotData.NowPlotDataNode == null)
            {
                GameAPI.Print("无效的剧情结点", "error");
                return;
            }
            switch (PlotData.NowPlotDataNode.Name.ToString())
            {
                case "AddCharacter"://处理添加角色信息的东西
                {
                    var _ = new Struct_CharacterInfo();
                    var _From = PlotData.NowPlotDataNode.Attribute("From").Value;
                    var _CharacterId = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                    _.Name = PlotData.ConfigCharacterInfo[_From]["Name"].ToString();
                    _.CharacterID = _CharacterId;
                    _.Affiliation = PlotData.ConfigDepartment[(PlotData.ConfigCharacterInfo[_From]["Department"].ToString())]["Name"].ToString();
                    _.FromID = _From;
                    var _CameObj = Resources.Load<GameObject>("HGF/Img-Character");

                    //首次添加角色，默认添加Normall立绘
                    _CameObj.GetComponent<Image>().sprite = GetCharacterImg(_From, "Normall");

                    _.CharacterGameObject = Instantiate(_CameObj, Gal_CharacterImg.gameObject.transform);

                    if (PlotData.NowPlotDataNode.Attributes("SendMessage").Count() != 0)
                    {
                        _.CharacterGameObject.GetComponent<GalManager_CharacterAnimate>().Animate_StartOrOutside = PlotData.NowPlotDataNode.Attribute("SendMessage").Value;
                    }

                    PlotData.CharacterInfo.Add(_);

                    Button_Click_NextPlot();
                    break;
                }
                case "Speak":  //处理发言
                {
                    var _nodeinfo = GetCharacterObjectByName(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);
                    if (PlotData.NowPlotDataNode.Elements().Count() != 0) //有选项，因为他有子节点数目了
                    {
                        GalManager_Text.IsCanJump = false;
                        foreach (var ClildItem in PlotData.NowPlotDataNode.Elements())
                        {
                            if (ClildItem.Name.ToString() == "Choice")
                                PlotData.ChoiceText.Add(new Struct_Choice { Title = ClildItem.Value, JumpID = ClildItem.Attribute("JumpID").Value });

                        }
                        Gal_Text.StartTextContent(PlotData.NowPlotDataNode.Attribute("Content").Value, _nodeinfo.Name, _nodeinfo.Affiliation, () =>
                        {

                            foreach (var ClildItem in GalManager.PlotData.ChoiceText)
                            {
                                Gal_Choice.CreatNewChoice(ClildItem.JumpID, ClildItem.Title);
                            }
                        });
                    }
                    else Gal_Text.StartTextContent(PlotData.NowPlotDataNode.Attribute("Content").Value, _nodeinfo.Name, _nodeinfo.Affiliation);

                    //处理消息
                    if (PlotData.NowPlotDataNode.Attributes("SendMessage").Count() != 0)
                        SendCharMessage(_nodeinfo.CharacterID, PlotData.NowPlotDataNode.Attribute("SendMessage").Value);
                    if (PlotData.NowPlotDataNode.Attributes("AudioPath").Count() != 0)
                        StartCoroutine(PlayCharacterVoice(AudioSystem.Character_Voice, _nodeinfo.FromID, PlotData.NowPlotDataNode.Attribute("AudioPath").Value));
                    break;
                }
                case "ChangeBackImg"://更换背景图片
                {
                    var _Path = PlotData.NowPlotDataNode.Attribute("Path").Value;
                    Gal_BackImg.SetImage(GetBackImg(_Path));
                    Button_Click_NextPlot();
                    break;
                }
                case "DeleteCharacter":
                {
                    DestroyCharacterByID(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);
                    break;
                }
                case "ChangeCharacterImg":
                {
                    var _CharacterID = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                    var _obj = GetCharacterObjectByName(_CharacterID);

                    //Debug.Log(_obj.CharacterGameObject.GetComponent<Image>() is null);

                    //ResourcesPath


                    _obj.CharacterGameObject.GetComponent<GalManager_CharacterImg>().SetImage(GetCharacterImg(_obj.FromID, PlotData.NowPlotDataNode.Attribute("KeyName").Value));

                    // _obj.CharacterGameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Texture2D/Menhera/Plot/character/{GameManager.ServerManager.Config.CharacterInfo.GetValue(_obj.FromID, "ResourcePath")}/{GameManager.ServerManager.Config.CharacterInfo.GetValue(_obj.FromID, PlotData.NowPlotDataNode.Attribute("Img").Value)}");
                    // Debug.Log($"Texture2D/Menhera/Plot/character/{GameManager.ServerManager.Config.CharacterInfo.GetValue(, "ResourcePath")}/{GameManager.ServerManager.Config.CharacterInfo.GetValue(_obj.FromID, PlotData.NowPlotDataNode.Attribute("Img").Value)}");
                    Button_Click_NextPlot();
                    break;
                }
                case "ChangeBackAudio":
                {
                    ChangeBackAudio(AudioSystem.BackMix, AudioSystem.AudioList.Find(e => e.name == PlotData.NowPlotDataNode.Value).path);
                    Button_Click_NextPlot();
                    break;
                }
                case "ExitGame":
                {
                    foreach (var item in PlotData.CharacterInfo)
                    {
                        DestroyCharacterByID(item.CharacterID);
                    }
                    PlotData.MainPlot.Clear();
                    PlotData.BranchPlot.Clear();
                    PlotData.IsBranch = false;
                    break;
                }
            }
            if (PlotData.BranchPlotInfo.Count == 0)
            {
                PlotData.IsBranch = false;
            }
            return;
        }
        public void Button_Click_FastMode ()
        {
            GalManager_Text.IsFastMode = true;
            return;
        }
        public Struct_CharacterInfo GetCharacterObjectByName (string ID)
        {
            return PlotData.CharacterInfo.Find(t => t.CharacterID == ID);
        }
        public XElement GetBranchByID (string ID)
        {
            if (PlotData.BranchPlotInfo.Count == 0)
                foreach (var item in PlotData.BranchPlot.Find(t => t.Attribute("ID").Value == ID).Elements())
                {
                    PlotData.BranchPlotInfo.Enqueue(item);

                }
            PlotData.BranchPlotInfo.TryDequeue(out XElement t);
            return t;
        }
        /// <summary>
        /// 销毁一个角色
        /// </summary>
        /// <param name="ID"></param>
        public void DestroyCharacterByID (string ID)
        {
            var _ = PlotData.CharacterInfo.Find(t => t.CharacterID == ID);
            SendCharMessage(ID, "Quit");
            PlotData.CharacterInfo.Remove(_);
        }
        public void SendCharMessage (string CharacterID, string Message)
        {
            var _t = GetCharacterObjectByName(CharacterID);
            _t.CharacterGameObject.GetComponent<GalManager_CharacterMessage>().HandleMessage(Message);
        }


        /// <summary>
        /// 初始化音乐系统
        /// </summary>
        private void StartBackAudio ()
        {
            AudioSystem.Character_Voice = GameObject.Find("AudioSystem/Character_Voice").GetComponent<AudioSource>();
            AudioSystem.BackMix = GameObject.Find("AudioSystem/BackMix").GetComponent<AudioSource>();
            AudioSystem.TextMix = GameObject.Find("AudioSystem/TextMix").GetComponent<AudioSource>();
        }

        private void FixedUpdate ()
        {
            CharacterNum = PlotData.CharacterInfo.Count;
        }
        private void Update ()
        {

        }

        #region 建议自己实现的部分

        /// <summary>
        /// 根据角色ID获取角色立绘，建议自己重新实现。
        /// </summary>
        /// <returns></returns>
        private Sprite GetCharacterImg (string id, string imgName)
        {
            var _path = $"HGF/img/portrait/{id}/{PlotData.ConfigCharacterInfo[id]["Portraits"][imgName]}";
            Debug.Log(_path);

            return Resources.Load<Sprite>(_path);
        }

        /// <summary>
        /// 获取背景图片，建议自己重新实现。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private Sprite GetBackImg (string path)
        {
            Debug.Log($"HGF/img/back/{path}");
            return Resources.Load<Sprite>($"HGF/img/back/{path}");
        }

        /// <summary>
        /// 播放语音，建议自己实现指定文件夹路径
        /// </summary>
        /// <param name="audioSource"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private IEnumerator PlayCharacterVoice (AudioSource audioSource, string id, string fileName)
        {
            yield return null;//下一帧执行
            var _clip = Resources.Load<AudioClip>($"HGF/audio/{id}/{fileName}");
            audioSource.clip = _clip;
            audioSource.Play();
            //或者
            /*
             * audioSource.PlayOneShot(_clip);
             * 
             */
        }

        public IEnumerator ChangeBackAudio (AudioSource audioSource, string path)
        {
            yield return null;//下一帧执行
            var _clip = Resources.Load<AudioClip>($"HGF/audio/{path}");
            audioSource.clip = _clip;
            audioSource.Play();
        }

        #endregion
    }
}