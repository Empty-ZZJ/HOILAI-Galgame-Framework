using DG.Tweening;
using TetraCreations.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace HGF
{
    /// <summary>
    /// 角色动画管理器，用于处理角色的入场、出场和即时动画
    /// </summary>
    public class GalManager_CharacterAnimate : MonoBehaviour
    {
        /// <summary>
        /// 出入场动画类型
        /// <para>ToShow：逐渐显示</para>
        /// <para>Outside-ToLeft：从屏幕左侧滑入</para>
        /// <para>Outside-ToRight：从屏幕右侧滑入</para>
        /// </summary>
        [StringInList("ToShow", "Outside-ToLeft", "Outside-ToRight")]
        public string Animate_StartOrOutside = "ToShow";

        /// <summary>
        /// 即时动画类型
        /// <para>Shake：颤抖</para>
        /// <para>Shake-Y-Once：向下抖动一次</para>
        /// <para>ToLeft：移动到左侧</para>
        /// <para>ToCenter：移动到中间</para>
        /// <para>ToRight：移动到右侧</para>
        /// </summary>
        [StringInList("Shake", "Shake-Y-Once", "ToLeft", "ToCenter", "ToRight")]
        public string Animate_type = "Shake";

        /// <summary>
        /// 角色立绘
        /// </summary>
        private Image CharacterImg;

        /// <summary>
        /// 主画布，注意主画布的名称必须是MainCanvas
        /// </summary>
        [Title("注意，主画布的名称必须是MainCanvas")]
        public Canvas MainCanvas;

        /// <summary>
        /// 初始化，获取角色立绘和主画布
        /// </summary>
        private void Awake ()
        {
            CharacterImg = gameObject.GetComponent<Image>();
            if (MainCanvas == null) MainCanvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();
        }

        /// <summary>
        /// 重新执行入场动画
        /// </summary>
        [Button(nameof(Start), "重新执行入场动画")]
        private void Start ()
        {
            HandleInOrOutsideMessgae(Animate_StartOrOutside);
        }

        /// <summary>
        /// 处理即时动画消息
        /// </summary>
        [Button(nameof(Start), "重新执行及时动画")]
        public void HandleMessgae ()
        {
            var _rect = CharacterImg.GetComponent<RectTransform>();
            switch (Animate_type)
            {
                case "Shake":
                {
                    _rect.DOShakePosition(0.5f, 30f);
                    break;
                }
                case "Shake-Y-Once":
                {
                    _rect.DOAnchorPosY(_rect.anchoredPosition.y - 50f, 0.6f).OnComplete(() =>
                    {
                        _rect.DOAnchorPosY(_rect.anchoredPosition.y + 50f, 0.6f);
                    });
                    break;
                }
                case "ToLeft":
                {
                    DOTween.To(() => _rect.anchoredPosition, x => _rect.GetComponent<RectTransform>().anchoredPosition = x, PositionImageInside(_rect, -1), 1f);
                    break;
                }
                case "ToCenter":
                {
                    DOTween.To(() => _rect.anchoredPosition, x => _rect.GetComponent<RectTransform>().anchoredPosition = x, PositionImageInside(_rect, 0), 0.8f);
                    break;
                }
                case "ToRight":
                {
                    DOTween.To(() => _rect.anchoredPosition, x => _rect.GetComponent<RectTransform>().anchoredPosition = x, PositionImageInside(_rect, 1), 1f);
                    break;
                }
                case "Quit":
                {
                    CharacterImg.DOFade(0, 0.7f).OnComplete(() =>
                    {
                        Destroy(gameObject);
                    });
                    break;
                }
                default:
                {
                    GameAPI.Print("当前剧情文本受损，请重新安装游戏尝试", "error");
                    break;
                }
            }
        }

        /// <summary>
        /// 处理出场动画消息
        /// </summary>
        /// <param name="Messgae">动画消息类型</param>
        public void HandleInOrOutsideMessgae (string Messgae)
        {
            CharacterImg.color = new Color32(255, 255, 255, 0); // 完全透明
            var rect = gameObject.GetComponent<RectTransform>();
            switch (Messgae)
            {
                // 逐渐显示
                case "ToShow":
                {
                    PositionImageOutside(gameObject.GetComponent<RectTransform>(), 0);
                    break;
                }
                // 从屏幕左侧滑入
                case "Outside-ToLeft":
                {
                    PositionImageOutside(gameObject.GetComponent<RectTransform>(), -1);
                    DOTween.To(() => rect.anchoredPosition, x => rect.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(rect.anchoredPosition.x + CharacterImg.sprite.texture.width, rect.anchoredPosition.y), 1f);
                    break;
                }
                // 从屏幕右侧滑入
                case "Outside-ToRight":
                {
                    PositionImageOutside(gameObject.GetComponent<RectTransform>(), 1);
                    DOTween.To(() => rect.anchoredPosition, x => rect.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(rect.anchoredPosition.x - CharacterImg.sprite.texture.width, rect.anchoredPosition.y), 1f);
                    break;
                }
                default:
                {
                    GameAPI.Print("当前剧情文本受损，请重新安装游戏尝试", "error");
                    break;
                }
            }
            // 都需要指定的
            CharacterImg.DOFade(1, 0.7f);
        }

        /// <summary>
        /// 设置Image的位置到屏幕之外
        /// </summary>
        /// <param name="ImageGameObject">Image的RectTransform</param>
        /// <param name="Position">位置类型：-1：左侧 0：中间 1：右侧</param>
        private void PositionImageOutside (RectTransform ImageGameObject, int Position)
        {
            switch (Position)
            {
                case -1:
                    gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2((-MainCanvas.GetComponent<RectTransform>().sizeDelta.x / 2) - (ImageGameObject.gameObject.GetComponent<Image>().sprite.texture.width / 2), ImageGameObject.anchoredPosition.y);
                    break;
                case 1:
                    gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2((MainCanvas.GetComponent<RectTransform>().sizeDelta.x / 2) + (ImageGameObject.gameObject.GetComponent<Image>().sprite.texture.width / 2), ImageGameObject.anchoredPosition.y);
                    break;
                case 0:
                    gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, ImageGameObject.anchoredPosition.y);
                    break;
                default: break;
            }
        }

        /// <summary>
        /// 获取Image的位置到屏幕之内的位置
        /// </summary>
        /// <param name="ImageGameObject">Image的RectTransform</param>
        /// <param name="Position">位置类型：-1：左侧 0：中间 1：右侧</param>
        /// <returns>目标位置</returns>
        private Vector2 PositionImageInside (RectTransform ImageGameObject, int Position)
        {
            switch (Position)
            {
                case -1:
                    return new Vector2((-MainCanvas.GetComponent<RectTransform>().sizeDelta.x / 2) + (ImageGameObject.gameObject.GetComponent<Image>().sprite.texture.width / 2), ImageGameObject.anchoredPosition.y);
                case 1:
                    return new Vector2((MainCanvas.GetComponent<RectTransform>().sizeDelta.x / 2) - (ImageGameObject.gameObject.GetComponent<Image>().sprite.texture.width / 2), ImageGameObject.anchoredPosition.y);
                case 0:
                    return new Vector2(0, ImageGameObject.anchoredPosition.y);
                default:
                {
                    GameAPI.Print("当前剧情文本受损，请重新安装游戏尝试", "error");
                    return new Vector2(0, 0);
                }
            }
        }
    }
}