using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Lapin
{
    [DefaultExecutionOrder(-9999)]
    public class MultiTouch : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct TouchEvent
        {
            public const int None = 0;
            public const int Begin = 1;
            public const int Update = 2;
            public const int End = 3;

            public int type;
            public int id;
            public float x;
            public float y;
        }

        public enum TouchPhase
        {
            Began,
            Update,
            Ended,
        }

        public struct TouchData
        {
            public int id;
            public TouchPhase phase;
            public Vector2 beginPosition;
            public Vector2 currentPosition;
        }

        private static MultiTouch s_Instance = null;
        private static readonly List<TouchData> s_Empty = new List<TouchData>();

        private IntPtr m_Context;
        private Dictionary<int, TouchData> m_Touches = new Dictionary<int, TouchData>();
        private List<TouchData> m_TempTouchList = new List<TouchData>();
        private List<TouchData> m_ExposedTouchList = new List<TouchData>();

        private void Awake()
        {
            m_Context = IntPtr.Zero;

            if (null != s_Instance)
            {
                enabled = false;
                Debug.LogError($"Only one {nameof(MultiTouch)} is allowed");
                return;
            }

            s_Instance = this;
            DontDestroyOnLoad(this.gameObject);
            m_Context = InitializeTouch();
        }

        private void OnDestroy()
        {
            FinalizeTouch(m_Context);
            if (ReferenceEquals(this, s_Instance))
            {
                s_Instance = null;
            }
        }

        private void Update()
        {
            if (m_Touches.Count > 0)
            {
                m_TempTouchList.Clear();
                m_TempTouchList.AddRange(m_Touches.Values);

                foreach (var touch in m_TempTouchList)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        var copy = touch;
                        copy.phase = TouchPhase.Update;
                        m_Touches[copy.id] = copy;
                    }
                    else if (touch.phase == TouchPhase.Ended)
                    {
                        m_Touches.Remove(touch.id);
                    }
                }
            }

            float h = Screen.height;
            PrepareTouch(m_Context);

            while (true)
            {
                var touch = GetNextTouchEvent(m_Context);
                if (touch.type == TouchEvent.None)
                {
                    m_ExposedTouchList.Clear();
                    foreach (var data in m_Touches.Values)
                    {
                        m_ExposedTouchList.Add(data);
                    }

                    return;
                }

                if (touch.type == TouchEvent.Begin)
                {
                    var data = new TouchData();
                    data.id = touch.id;
                    data.phase = TouchPhase.Began;
                    data.beginPosition = data.currentPosition = new Vector2(touch.x, h - touch.y);
                    m_Touches[touch.id] = data;
                }
                else if (touch.type == TouchEvent.Update)
                {
                    if (m_Touches.TryGetValue(touch.id, out var data))
                    {
                        data.currentPosition = new Vector2(touch.x, h - touch.y);
                        m_Touches[touch.id] = data;
                    }
                }
                else if (touch.type == TouchEvent.End)
                {
                    if (m_Touches.TryGetValue(touch.id, out var data))
                    {
                        data.phase = TouchPhase.Ended;
                        m_Touches[touch.id] = data;
                    }
                }
            }
        }

        public static List<TouchData> GetTouches()
        {
            return s_Instance is null ? s_Empty : s_Instance.m_ExposedTouchList;
        }

#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
        [DllImport("libmt.so",
            EntryPoint = "mt_initialize",
            CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr InitializeTouch();

        private static void PrepareTouch(IntPtr context)
        {
        }

        [DllImport("libmt.so",
            EntryPoint = "mt_get_next",
            CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        private extern static TouchEvent GetNextTouchEvent(IntPtr context);

        [DllImport("libmt.so",
            EntryPoint = "mt_finalize",
            CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        private extern static void FinalizeTouch(IntPtr context);
#else
        private static TouchEvent s_MouseEvent;

        private static IntPtr InitializeTouch()
        {
            return IntPtr.Zero;
        }

        private static void PrepareTouch(IntPtr context)
        {
            if (Input.GetMouseButtonDown(0))
            {
                s_MouseEvent = CreateTouchFromMouse(TouchEvent.Begin);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                s_MouseEvent = CreateTouchFromMouse(TouchEvent.End);
            }
            else if (Input.GetMouseButton(0))
            {
                s_MouseEvent = CreateTouchFromMouse(TouchEvent.Update);
            }
            else
            {
                s_MouseEvent = new TouchEvent() {type = TouchEvent.None};
            }
        }

        private static TouchEvent CreateTouchFromMouse(int type)
        {
            var pos = Input.mousePosition;
            var touch = new TouchEvent();
            touch.type = type;
            touch.id = 0;
            touch.x = pos.x;
            touch.y = Screen.height - pos.y;
            return touch;
        }

        private static TouchEvent GetNextTouchEvent(IntPtr context)
        {
            var touch = s_MouseEvent;
            // No more events next time in the same frame
            s_MouseEvent = new TouchEvent() {type = TouchEvent.None};
            return touch;
        }

        private static void FinalizeTouch(IntPtr context)
        {
        }
#endif
    }
}
