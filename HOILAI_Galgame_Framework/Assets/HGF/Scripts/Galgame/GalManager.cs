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
        /// <summary>
        /// 当前剧情的唯一标识符
        /// </summary>
        public static string PlotID;

        /// <summary>
        /// 当前对话的UI组件
        /// </summary>
        [Title("当前对话")]
        public GalManager_Text Gal_Text;

        /// <summary>
        /// 当前角色立绘的UI组件
        /// </summary>
        [Title("当前角色部分")]
        public GalManager_CharacterImg Gal_CharacterImg;

        /// <summary>
        /// 控制选项的UI组件
        /// </summary>
        [Title("控制选项")]
        public GalManager_Choice Gal_Choice;

        /// <summary>
        /// 控制背景图片的UI组件
        /// </summary>
        [Title("控制背景图片的组件")]
        public GalManager_BackImg Gal_BackImg;

        /// <summary>
        /// 当前场景中角色的数量
        /// </summary>
        [Title("当前场景角色数量")]
        public int CharacterNum;

        /// <summary>
        /// 音频系统模型，包含角色语音、背景音乐和文本音效的AudioSource
        /// </summary>
        public class AudioSystemModel
        {
            public AudioSource Character_Voice; // 角色语音的AudioSource
            public AudioSource BackMix; // 背景音乐的AudioSource
            public AudioSource TextMix; // 文本音效的AudioSource

            /// <summary>
            /// 音频信息类，包含音频名称和路径
            /// </summary>
            public class AudioInfo
            {
                public string name;
                public string path;
            }

            /// <summary>
            /// 音频列表，存储所有音频信息
            /// </summary>
            public List<AudioInfo> AudioList = new();
        }

        /// <summary>
        /// 音频系统的静态实例
        /// </summary>
        public static AudioSystemModel AudioSystem = new();

        /// <summary>
        /// 存储整个剧本的XML文档
        /// </summary>
        private XDocument PlotxDoc;

        /// <summary>
        /// 剧情数据结构，包含角色信息、分支剧情、主剧情等
        /// </summary>
        [Serializable]
        public class Struct_PlotData
        {
            public JObject ConfigCharacterInfo = new(); // 角色配置信息
            public JObject ConfigDepartment = new(); // 部门配置信息
            public string Title; // 剧情标题
            public string Synopsis; // 剧情简介
            public List<XElement> BranchPlot = new(); // 分支剧情列表
            public Queue<XElement> BranchPlotInfo = new(); // 分支剧情信息队列
            public Queue<XElement> MainPlot = new(); // 主剧情队列

            /// <summary>
            /// 选项结构，包含选项标题和跳转ID
            /// </summary>
            public class Struct_Choice
            {
                public string Title;
                public string JumpID;
            }

            /// <summary>
            /// 角色信息结构，包含角色ID、GameObject、名称、所属部门和来源ID
            /// </summary>
            public class Struct_CharacterInfo
            {
                public string CharacterID;
                public GameObject CharacterGameObject;
                public string Name;
                public string Affiliation;
                public string FromID;
            }

            public List<Struct_CharacterInfo> CharacterInfo = new(); // 角色信息列表
            public List<Struct_Choice> ChoiceText = new(); // 选项文本列表

            /// <summary>
            /// 当前的剧情节点
            /// </summary>
            public XElement NowPlotDataNode;

            /// <summary>
            /// 当前是否为分支剧情节点
            /// </summary>
            public bool IsBranch = false;

            /// <summary>
            /// 当前跳转的ID
            /// </summary>
            public string NowJumpID;
        }

        /// <summary>
        /// 剧情数据的静态实例
        /// </summary>
        public static Struct_PlotData PlotData = new();

        /// <summary>
        /// 开始游戏，初始化剧情数据并加载剧本
        /// </summary>
        /// <param name="model">游戏启动模型，包含角色信息、部门信息和剧本文本</param>
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
        /// 解析框架文本，加载剧本
        /// </summary>
        /// <param name="plotText">剧本文本内容</param>
        /// <returns>协程</returns>
        public IEnumerator LoadPlot (string plotText)
        {
            yield return null;
            try
            {
                GameAPI.Print($"游戏剧本：{plotText}");
                PlotxDoc = XDocument.Parse(plotText);

                // 开始读取数据
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
        /// 点击屏幕，进入下一句剧情
        /// </summary>
        public void Button_Click_NextPlot ()
        {
            if (PlotData.MainPlot.Count == 0)
            {
                GameAPI.Print("游戏结束!");
                return;
            }

            // 如果正在说话或不能跳转，则返回
            if (GalManager_Text.IsSpeak || !GalManager_Text.IsCanJump) { return; }

            if (!PlotData.IsBranch)
            {
                PlotData.MainPlot.TryDequeue(out PlotData.NowPlotDataNode); // 队列出队，获取当前剧情节点
                PlotData.BranchPlotInfo.Clear();
            }
            else // 当前为分支节点
            {
                PlotData.NowPlotDataNode = GetBranchByID(PlotData.NowJumpID);
            }

            PlotData.ChoiceText.Clear();
            if (PlotData.NowPlotDataNode == null)
            {
                GameAPI.Print("无效的剧情结点", "error");
                return;
            }

            // 根据当前剧情节点的类型进行处理
            switch (PlotData.NowPlotDataNode.Name.ToString())
            {
                case "AddCharacter": // 处理添加角色信息
                {
                    var _ = new Struct_CharacterInfo();
                    var _From = PlotData.NowPlotDataNode.Attribute("From").Value;
                    var _CharacterId = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                    _.Name = PlotData.ConfigCharacterInfo[_From]["Name"].ToString();
                    _.CharacterID = _CharacterId;
                    _.Affiliation = PlotData.ConfigDepartment[(PlotData.ConfigCharacterInfo[_From]["Department"].ToString())]["Name"].ToString();
                    _.FromID = _From;
                    var _CameObj = Resources.Load<GameObject>("HGF/Img-Character");

                    // 首次添加角色，默认添加Normal立绘
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
                case "Speak": // 处理发言
                {
                    var _nodeinfo = GetCharacterObjectByName(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);
                    if (PlotData.NowPlotDataNode.Elements().Count() != 0) // 有选项，因为存在子节点
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

                    // 处理消息
                    if (PlotData.NowPlotDataNode.Attributes("SendMessage").Count() != 0)
                        SendCharMessage(_nodeinfo.CharacterID, PlotData.NowPlotDataNode.Attribute("SendMessage").Value);
                    if (PlotData.NowPlotDataNode.Attributes("AudioPath").Count() != 0)
                        StartCoroutine(PlayCharacterVoice(AudioSystem.Character_Voice, _nodeinfo.FromID, PlotData.NowPlotDataNode.Attribute("AudioPath").Value));
                    break;
                }
                case "ChangeBackImg": // 更换背景图片
                {
                    var _Path = PlotData.NowPlotDataNode.Attribute("Path").Value;
                    Gal_BackImg.SetImage(GetBackImg(_Path));
                    Button_Click_NextPlot();
                    break;
                }
                case "DeleteCharacter": // 删除角色
                {
                    DestroyCharacterByID(PlotData.NowPlotDataNode.Attribute("CharacterID").Value);
                    break;
                }
                case "ChangeCharacterImg": // 更换角色立绘
                {
                    var _CharacterID = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
                    var _obj = GetCharacterObjectByName(_CharacterID);
                    _obj.CharacterGameObject.GetComponent<GalManager_CharacterImg>().SetImage(GetCharacterImg(_obj.FromID, PlotData.NowPlotDataNode.Attribute("KeyName").Value));
                    Button_Click_NextPlot();
                    break;
                }
                case "ChangeBackAudio": // 更换背景音乐
                {
                    ChangeBackAudio(AudioSystem.BackMix, AudioSystem.AudioList.Find(e => e.name == PlotData.NowPlotDataNode.Value).path);
                    Button_Click_NextPlot();
                    break;
                }
                case "ExitGame": // 退出游戏
                {
                    PlotData.MainPlot.Clear();
                    PlotData.BranchPlot.Clear();
                    foreach (var item in PlotData.CharacterInfo)
                    {
                        DestroyCharacterByID(item.CharacterID, false);
                    }
                    PlotData.CharacterInfo.Clear();
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

        /// <summary>
        /// 进入快速模式
        /// </summary>
        public void Button_Click_FastMode ()
        {
            GalManager_Text.IsFastMode = true;
            return;
        }

        /// <summary>
        /// 根据角色ID获取角色对象
        /// </summary>
        /// <param name="ID">角色ID</param>
        /// <returns>角色信息</returns>
        public Struct_CharacterInfo GetCharacterObjectByName (string ID)
        {
            return PlotData.CharacterInfo.Find(t => t.CharacterID == ID);
        }

        /// <summary>
        /// 根据分支ID获取分支剧情节点
        /// </summary>
        /// <param name="ID">分支ID</param>
        /// <returns>分支剧情节点</returns>
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
        /// 销毁指定ID的角色
        /// </summary>
        /// <param name="ID">角色ID</param>
        public void DestroyCharacterByID (string ID, bool removeCharacterInfo = true)
        {
            var _ = PlotData.CharacterInfo.Find(t => t.CharacterID == ID);
            SendCharMessage(ID, "Quit");
            if (removeCharacterInfo)
                PlotData.CharacterInfo.Remove(_);
        }

        /// <summary>
        /// 发送角色消息
        /// </summary>
        /// <param name="CharacterID">角色ID</param>
        /// <param name="Message">消息内容</param>
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

        /// <summary>
        /// 每帧更新角色数量
        /// </summary>
        private void FixedUpdate ()
        {
            CharacterNum = PlotData.CharacterInfo.Count;
        }

        private void Update ()
        {
        }

        #region 建议自己实现的部分

        /// <summary>
        /// 根据角色ID和立绘名称获取角色立绘
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="imgName">立绘名称</param>
        /// <returns>角色立绘</returns>
        private Sprite GetCharacterImg (string id, string imgName)
        {
            var _path = $"HGF/img/portrait/{id}/{PlotData.ConfigCharacterInfo[id]["Portraits"][imgName]}";
            Debug.Log(_path);
            return Resources.Load<Sprite>(_path);
        }

        /// <summary>
        /// 获取背景图片
        /// </summary>
        /// <param name="path">背景图片路径</param>
        /// <returns>背景图片</returns>
        private Sprite GetBackImg (string path)
        {
            Debug.Log($"HGF/img/back/{path}");
            return Resources.Load<Sprite>($"HGF/img/back/{path}");
        }

        /// <summary>
        /// 播放角色语音
        /// </summary>
        /// <param name="audioSource">音频源</param>
        /// <param name="id">角色ID</param>
        /// <param name="fileName">音频文件名</param>
        /// <returns>协程</returns>
        private IEnumerator PlayCharacterVoice (AudioSource audioSource, string id, string fileName)
        {
            yield return null; // 下一帧执行
            var _clip = Resources.Load<AudioClip>($"HGF/audio/{id}/{fileName}");
            audioSource.clip = _clip;
            audioSource.Play();
        }

        /// <summary>
        /// 更换背景音乐
        /// </summary>
        /// <param name="audioSource">音频源</param>
        /// <param name="path">背景音乐路径</param>
        /// <returns>协程</returns>
        public IEnumerator ChangeBackAudio (AudioSource audioSource, string path)
        {
            yield return null; // 下一帧执行
            var _clip = Resources.Load<AudioClip>($"HGF/audio/{path}");
            audioSource.clip = _clip;
            audioSource.Play();
        }

        #endregion
    }
}