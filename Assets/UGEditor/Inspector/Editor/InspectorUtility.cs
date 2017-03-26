using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UGFramework.Editor.Inspector
{
    /**
     * --- DOC BEGIN ---
     * Custom inspector utility, draw fields in *Inspector*
     * --- DOC END ---
     */
    public static class InspectorUtility
    {
        static readonly string ROOT = "";

        public static string Path { get; set; }

        public static object Object { get; private set; }

        public static string SelectedPath { get; private set; }

        /**
         * Call reset before drawing new root object
         */
        public static void Setup(object obj)
        {
            Object = obj;
            Path = ROOT;
        }

        static Dictionary<object, Dictionary<string, bool>> foldoutRecords = new Dictionary<object, Dictionary<string, bool>>();
        public static void AddFolder(string folder)
        {
            Path += folder + "/";
        }
        public static void Foldout(bool foldout, object obj = null, string path = null, bool recursive = false)
        {
            obj = obj == null ? Object : obj;
            path = string.IsNullOrEmpty(path) ? Path : path;
            foldoutRecords[obj][path] = foldout;
            if (recursive)
            {
                var tmpPaths = new string[foldoutRecords[obj].Keys.Count];
                foldoutRecords[obj].Keys.CopyTo(tmpPaths, 0);
                foreach (var tmpPath in tmpPaths)
                {
                    if (tmpPath.StartsWith(path) == false)
                        continue;
                    foldoutRecords[obj][tmpPath] = foldout;
                }
            }
        }
        public static bool ForceFoldout { get; set; }
        public static bool DrawFoldout(GUIContent content)
        {
            if (foldoutRecords.ContainsKey(Object) == false)
                foldoutRecords[Object] = new Dictionary<string, bool>();
            if (foldoutRecords[Object].ContainsKey(Path) == false)
                foldoutRecords[Object][Path] = false;
            
            foldoutRecords[Object][Path] = EditorGUILayout.Foldout(foldoutRecords[Object][Path], content, true);
            // Debug.Log("DrawFoldout " + Path);

            var contentRect = GUILayoutUtility.GetLastRect();
            var evt = Event.current;
            var mousePos = evt.mousePosition;
            if (evt.type == EventType.ContextClick && contentRect.Contains(mousePos)) {
                SelectedPath = Path;
                EditorUtility.DisplayPopupMenu(new Rect(mousePos.x, mousePos.y, 0, 0), TopbarConfig.INSPECTOR, null);
                Event.current.Use();
            }

            return ForceFoldout ? true : foldoutRecords[Object][Path];
        }
        public static void DrawTab()
        {
            int width = (Path.Split('/').Length - 1) * 1;
            EditorGUILayout.LabelField("", GUILayout.Width(width));
        }
        public static bool DrawTabAndFoldout(GUIContent content)
        {
            var foldout = false;
            EditorGUILayout.BeginHorizontal();
            DrawTab();
            foldout = DrawFoldout(content);
            EditorGUILayout.EndHorizontal();
            return foldout;
        }

        public static bool CheckMember(MemberInfo info)
        {
            return MemberDrawer.Check(info);
        }
        public static bool DrawMember(ref MemberInfo info, GUIContent content = null)
        {
            return MemberDrawer.Draw(ref info, content);
        }

        public static bool DrawObject(object value, GUIContent content = null)
        {
            return ObjectDrawer.Draw(value, content);
        }

        public static bool DrawInt(ref int value, GUIContent content)
        {
            var nextValue = EditorGUILayout.IntField(content, value);
            var changed = value != nextValue;
            value = nextValue;
            return changed;
        }

        public static bool DrawFloat(ref float value, GUIContent content)
        {
            var nextValue = EditorGUILayout.FloatField(content, value);
            var changed = value != nextValue;
            value = nextValue;
            return changed;
        }

        public static bool DrawEnum(ref Enum value, GUIContent content)
        {
            var nextValue = EditorGUILayout.EnumPopup(content, value);
            var changed = value.CompareTo(nextValue) != 0;
            value = nextValue;
            return changed;
        }

        public static bool DrawString(ref string value, GUIContent content)
        {
            var nextValue = EditorGUILayout.TextField(content, value);
            var changed = value != nextValue;
            value = nextValue;
            return changed;
        }

#region Collections
        public static bool DrawList(List<object> values, IList iValues, GUIContent content, bool isReadonly)
        {
            ListDrawer.IsReadonly = isReadonly;
            return ListDrawer.Draw(values, iValues, content);
        }
#endregion
    }
}