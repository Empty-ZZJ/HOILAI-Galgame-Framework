using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HGF
{
    /// <summary>
    /// �ַ����б��������ԣ�������Inspector����ʾһ�������б�
    /// </summary>
    public class StringInList : PropertyAttribute
    {
        /// <summary>
        /// ��ȡ�ַ����б��ί��
        /// </summary>
        public delegate string[] GetStringList ();

        /// <summary>
        /// ʹ��ָ�����ַ����б��ʼ������
        /// </summary>
        /// <param name="list">�ַ����б�</param>
        public StringInList (params string[] list)
        {
            List = list;
        }

        /// <summary>
        /// ʹ��ָ�����͵ķ�������ȡ�ַ����б��ʼ������
        /// </summary>
        /// <param name="type">��������������</param>
        /// <param name="methodName">��������</param>
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
        /// ��ȡ�ַ����б�
        /// </summary>
        public string[] List
        {
            get;
            private set;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// �ַ����б����Ի�������������Inspector�л��������б�
    /// </summary>
    [CustomPropertyDrawer(typeof(StringInList))]
    public class StringInListDrawer : PropertyDrawer
    {
        /// <summary>
        /// ��Inspector�л�������
        /// </summary>
        /// <param name="position">����λ��</param>
        /// <param name="property">���л�����</param>
        /// <param name="label">���Ա�ǩ</param>
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