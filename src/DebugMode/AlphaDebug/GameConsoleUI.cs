using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using LBoL.Core;
using LBoL.Presentation.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DebugMode.AlphaDebug
{
    public class GameConsoleUI : MonoBehaviour
    {
        private void Awake()
        {
            GameConsoleUI._instance = this;
            this._hintBg = new Texture2D(1, 1)
            {
                wrapMode = TextureWrapMode.Repeat
            };
            this._hintBg.SetPixel(0, 0, Color.black);
            this._hintBg.Apply();
            foreach (ValueTuple<string, string, string> valueTuple in this._commandHandler.EnumerateCommands())
            {
                string item = valueTuple.Item1;
                string item2 = valueTuple.Item2;
                string item3 = valueTuple.Item3;
                this._commandTable.Add(item, new ValueTuple<string, string>(item2, item3));
            }
            UiManager.Instance.DebugConsoleAction.performed += delegate (InputAction.CallbackContext context)
            {
                this.OnDebugConsoleClick();
            };
            base.enabled = false;
        }

        public static void ListAll()
        {
            if (GameConsoleUI._instance == null)
            {
                return;
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("All commands:");
            foreach (KeyValuePair<string, ValueTuple<string, string>> keyValuePair in GameConsoleUI._instance._commandTable)
            {
                string text;
                ValueTuple<string, string> valueTuple;
                keyValuePair.Deconstruct(out text, out valueTuple);
                ValueTuple<string, string> valueTuple2 = valueTuple;
                string item = valueTuple2.Item1;
                string item2 = valueTuple2.Item2;
                stringBuilder
                    //.Append("<color=#FFEB04>")
                    .Append(item)
                    //.Append("</color>")
                    ;
                if (!string.IsNullOrWhiteSpace(item2))
                {
                    stringBuilder.Append("    (").Append(item2).Append(")");
                }
                stringBuilder.AppendLine();
            }
            Debug.Log(stringBuilder.ToString());
        }

        private void OnDebugConsoleClick()
        {
            base.enabled = !base.enabled;
        }

        private void OnEnable()
        {
            int? historyIndex = this._historyIndex;
            string text;
            if (historyIndex != null)
            {
                int valueOrDefault = historyIndex.GetValueOrDefault();
                text = this._history[valueOrDefault];
            }
            else
            {
                text = "";
            }
            this._command = text;
            this._enableFocus = true;
        }

        private void OnGUI()
        {
            Event current = Event.current;
            if (current.type == EventType.KeyDown)
            {
                KeyCode keyCode = current.keyCode;
                if (keyCode != KeyCode.Return)
                {
                    if (keyCode != KeyCode.UpArrow)
                    {
                        if (keyCode == KeyCode.DownArrow)
                        {
                            int? num = this._historyIndex;
                            if (num != null)
                            {
                                int valueOrDefault = num.GetValueOrDefault();
                                this._historyIndex = new int?(valueOrDefault + 1);
                                num = this._historyIndex;
                                int count = this._history.Count;
                                if ((num.GetValueOrDefault() >= count) & (num != null))
                                {
                                    this._historyIndex = null;
                                    this._command = "";
                                }
                                else
                                {
                                    this._command = this._history[this._historyIndex.Value];
                                }
                            }
                        }
                    }
                    else
                    {
                        int? num = this._historyIndex;
                        if (num != null)
                        {
                            int valueOrDefault2 = num.GetValueOrDefault();
                            this._historyIndex = new int?(Math.Max(0, valueOrDefault2 - 1));
                        }
                        else if (this._history.Count > 0)
                        {
                            this._historyIndex = new int?(this._history.Count - 1);
                        }
                        else
                        {
                            this._historyIndex = null;
                        }
                        num = this._historyIndex;
                        string text;
                        if (num != null)
                        {
                            int valueOrDefault3 = num.GetValueOrDefault();
                            text = this._history[valueOrDefault3];
                        }
                        else
                        {
                            text = "";
                        }
                        this._command = text;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(this._command))
                    {
                        return;
                    }
                    this._history.Add(this._command);
                    base.enabled = false;
                    IEnumerator enumerator;
                    if (this._commandHandler.TryHandleCommand(this._command, out enumerator))
                    {
                        if (enumerator != null)
                        {
                            base.StartCoroutine(enumerator);
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to handle command: '" + this._command + "'");
                    }
                    this._command = null;
                    return;
                }
            }
            float num2 = (float)Screen.height / 600f;
            float num3 = (float)Screen.width / num2 - 100f;
            float num4 = 500f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(num2, num2, 1f)) * Matrix4x4.Translate(new Vector3(50f, 50f, 0f));
            Rect rect = new Rect(0f, 0f, num3, num4);
            GUI.Box(rect, "");
            GUI.Box(rect, "");
            GUI.Box(rect, "");
            GUILayout.BeginArea(rect);
            this._scrollPosition = GUILayout.BeginScrollView(this._scrollPosition, Array.Empty<GUILayoutOption>());
            Color color = GUI.color;
            GUIStyle guistyle = new GUIStyle(GUI.skin.label)
            {
                margin = new RectOffset(10, 0, 10, 0),
                border = new RectOffset(2, 2, 2, 2)
            };
            foreach (LogCache.LogEntry logEntry in LogCache.Logs.Reverse<LogCache.LogEntry>())
            {
                Color color2;
                switch (logEntry.Type)
                {
                    case LogType.Error:
                        color2 = Color.red;
                        break;
                    case LogType.Assert:
                        color2 = Color.magenta;
                        break;
                    case LogType.Warning:
                        color2 = Color.yellow;
                        break;
                    case LogType.Log:
                        color2 = Color.white;
                        break;
                    case LogType.Exception:
                        color2 = Color.red;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                GUI.color = color2;
                GUILayout.Label(new GUIContent(logEntry.Message, logEntry.Trace), guistyle, Array.Empty<GUILayoutOption>());
                GUILayout.Label("", GUI.skin.horizontalSlider, Array.Empty<GUILayoutOption>());
            }
            GUI.color = color;
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUI.SetNextControlName("DebugCommandInput");
            this._command = GUILayout.TextField(this._command, Array.Empty<GUILayoutOption>());
            if (this._enableFocus)
            {
                this._enableFocus = false;
                GUI.FocusControl("DebugCommandInput");
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUIStyle guistyle2 = new GUIStyle(GUI.skin.label)
            {
                normal =
                {
                    background = this._hintBg
                }
            };
            int num5 = 450;
            if (!string.IsNullOrWhiteSpace(this._command))
            {
                string text2 = this._command.TrimStart();
                int num6 = text2.IndexOf(' ');
                string text3 = ((num6 >= 0) ? text2.Substring(0, num6) : text2);
                foreach (KeyValuePair<string, ValueTuple<string, string>> keyValuePair in this._commandTable)
                {
                    string text4;
                    ValueTuple<string, string> valueTuple;
                    keyValuePair.Deconstruct(out text4, out valueTuple);
                    ValueTuple<string, string> valueTuple2 = valueTuple;
                    string text5 = text4;
                    string item = valueTuple2.Item1;
                    string item2 = valueTuple2.Item2;
                    if (text5.StartsWith(text3, StringComparison.InvariantCultureIgnoreCase))
                    {
                        GUI.Box(new Rect(20f, (float)num5, 500f, 20f), GUIContent.none, guistyle2);
                        Color color3 = GUI.color;
                        GUI.color = Color.yellow;
                        GUI.Label(new Rect(20f, (float)num5, 300f, 20f), item);
                        GUI.color = new Color(0.75f, 0.75f, 0.75f, 1f);
                        GUI.Label(new Rect(320f, (float)num5, 200f, 20f), item2);
                        GUI.color = color3;
                        num5 -= 20;
                    }
                }
            }
            string tooltip = GUI.tooltip;
            if (!string.IsNullOrWhiteSpace(tooltip))
            {
                GUIContent guicontent = new GUIContent(tooltip);
                Vector2 mousePosition = Event.current.mousePosition;
                GUIStyle guistyle3 = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    normal =
                    {
                        background = Texture2D.whiteTexture
                    }
                };
                Vector2 vector = guistyle3.CalcSize(guicontent);
                Rect rect2 = new Rect(mousePosition.x + 10f, mousePosition.y + 10f, vector.x + 10f, vector.y + 5f);
                Color backgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
                GUI.Label(rect2, tooltip, guistyle3);
                GUI.backgroundColor = backgroundColor;
            }
        }

        private Texture2D _hintBg;

        private bool _enableFocus;

        private Vector2 _scrollPosition;

        private string _command;

        private readonly List<string> _history = new List<string>();

        //[TupleElementNames(new string[] { "hint", "description" })]
        private readonly SortedDictionary<string, ValueTuple<string, string>> _commandTable = new SortedDictionary<string, ValueTuple<string, string>>();

        private int? _historyIndex;

        private readonly RuntimeCommandHandler _commandHandler = RuntimeCommandHandler.Create(typeof(DebugCommands));

        private static GameConsoleUI _instance;
    }
}
