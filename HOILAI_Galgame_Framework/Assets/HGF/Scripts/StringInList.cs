using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HGF
{
    /// <summary>
    /// 字符串列表属性特性，用于在Inspector中显示一个下拉列表
    /// </summary>
    public class StringInList : PropertyAttribute
    {
        /// <summary>
        /// 获取字符串列表的委托
        /// </summary>
        public delegate string[] GetStringList ();

        /// <summary>
        /// 使用指定的字符串列表初始化特性
        /// </summary>
        /// <param name="list">字符串列表</param>
        public StringInList (params string[] list)
        {
            List = list;
        }

        /// <summary>
        /// 使用指定类型的方法来获取字符串列表初始化特性
        /// </summary>
        /// <param name="type">包含方法的类型</param>
        /// <param name="methodName">方法名称</param>
        public StringInList (Type type, string methodName)
        {
            var method = type.GetMethod(methodName);
            if (method != null)
            {
                List = method.Invoke(null, null) as string[];
            }
            else
            {
                Debug.LogError("NO SUCH METHOD " + methodName + " FOR " + type);
            }
        }

        /// <summary>
        /// 获取字符串列表
        /// </summary>
        public string[] List
        {
            get;
            private set;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 字符串列表属性绘制器，用于在Inspector中绘制下拉列表
    /// </summary>
    [CustomPropertyDrawer(typeof(StringInList))]
    public class StringInListDrawer : PropertyDrawer
    {
        /// <summary>
        /// 在Inspector中绘制属性
        /// </summary>
        /// <param name="position">绘制位置</param>
        /// <param name="property">序列化属性</param>
        /// <param name="label">属性标签</param>
        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            var stringInList = attribute as StringInList;
            var list = stringInList.List;
            if (property.propertyType == SerializedPropertyType.String)
            {
                int index = Mathf.Max(0, Array.IndexOf(list, property.stringValue));
                index = EditorGUI.Popup(position, property.displayName, index, list);

                property.stringValue = list[index];
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = EditorGUI.Popup(position, property.displayName, property.intValue, list);
            }
            else
            {
                base.OnGUI(position, property, label);
            }
        }
    }
#endif
}