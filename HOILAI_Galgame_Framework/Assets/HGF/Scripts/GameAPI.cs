using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HGF
{
    /// <summary>
    /// 游戏内通用API
    /// </summary>
    public static class GameAPI
    {
        /// <summary>
        /// 返回可读可写路径
        /// PC端：streamingAssetsPath
        /// 移动端：Application.persistentDataPath
        /// </summary>
        /// <returns></returns>
        public static string GetWritePath ()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Application.streamingAssetsPath;
#elif UNITY_IOS || UNITY_ANDROID
            return  Application.persistentDataPath;
#endif
        }
        /// <summary>
        /// 接管Debug.Log(...)
        /// </summary>
        /// <param name="_Message">调试信息</param>
        /// <param name="_Type">1.debug 2.warn 3.error</param>
        public static void Print (object _Message, string _Type = "debug")
        {
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string res = $"[{currentTime}] {_Type} : {_Message}";
            switch (_Type)
            {
                case "debug":
                    Debug.Log(res);
                    return;
                case "warn":
                    Debug.LogWarning(res);
                    return;
                case "error":
                    Debug.LogError(res);
                    return;
                default:
                    Debug.Log(res);
                    break;
            }
        }
        /// <summary>
        /// 暴力查找一个物体，找不到返回Null
        /// </summary>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public static GameObject FindGameObject_Force (string _Name)
        {

            GameObject[] all = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
            for (int i = 0; i < all.Length; i++)
            {
                var item = all[i];
                if (item.name == _Name) return item;
            }
            return null;

        }
        /// <summary>
        /// 生成SHA256值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GenerateSha256 (string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString().ToUpper();
            }
        }
        /// <summary>
        /// 通过UnityWebRequest获取本地StreamingAssets文件夹中的文件
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public static string UnityWebRequestFile (string fileName)
        {
            string url;
            #region 分平台判断 StreamingAssets 路径
            //如果在编译器或者单机中
#if UNITY_EDITOR || UNITY_STANDALONE

            url = "file://" + Application.dataPath + "/StreamingAssets/" + fileName;
            //否则如果在Iphone下
#elif UNITY_IPHONE
        url = "file://" + Application.dataPath + "/Raw/"+ fileName;
            //否则如果在android下
#elif UNITY_ANDROID
        url = "jar:file://" + Application.dataPath + "!/assets/"+ fileName;
#endif
            #endregion
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SendWebRequest();//读取数据
            while (true)
            {
                if (request.downloadHandler.isDone)//是否读取完数据
                {
                    return request.downloadHandler.text;
                }
            }
        }
        /// <summary>
        /// 从外部指定文件中加载图片
        /// </summary>
        /// <returns></returns>
        public static Sprite LoadTextureByIO (string Path)
        {
            FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.Begin);//游标的操作，可有可无
            byte[] bytes = new byte[fs.Length];//生命字节，用来存储读取到的图片字节
            try
            {
                fs.Read(bytes, 0, bytes.Length);//开始读取，这里最好用trycatch语句，防止读取失败报错

            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            fs.Close();//切记关闭

            int width = 2048;//图片的宽（这里两个参数可以提到方法参数中）
            int height = 2048;//图片的高（这里说个题外话，pico相关的开发，这里不能大于4k×4k不然会显示异常，当时开发pico的时候应为这个问题找了大半天原因，因为美术给的图是6000*3600，导致出现切几张图后就黑屏了。。。
            Texture2D texture = new Texture2D(width, height);
            if (texture.LoadImage(bytes))
            {
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));//将生成的texture2d返回，到这里就得到了外部的图片，可以使用了
            }
            else
            {
                return null;
            }
        }

    }
}
