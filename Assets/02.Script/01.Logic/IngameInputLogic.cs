using System;
using UnityEngine;



namespace SCC
{
    //!<===============================================================================
    public static class IngameInputLogic
    {
        //!<============================================================================
        //MultiTouchEnabled
        //!<============================================================================

        public static readonly Vector2 INVALID_TOUCH_POSTION = new (-1, -1);

        //!<============================================================================
        //MultiTouchEnabled
        //!<============================================================================
        public static bool MultiTouchEnabled
        {
            get => UnityEngine.Input.multiTouchEnabled;
            set => UnityEngine.Input.multiTouchEnabled = value;
        }

        //!<============================================================================
        //IsTouch
        //!<============================================================================
        public static bool IsTouch => IngameInputLogic.GetTouchCount() > 0;

        //!<============================================================================
        //GetSingleTouchPostion
        //!<============================================================================
        public static Vector2 GetSingleTouchPostion()
        {
            if (UnityEngine.Application.isMobilePlatform == true)
            {
                return UnityEngine.Input.touchCount > 0
                    ? UnityEngine.Input.GetTouch(0)
                        .position
                    : INVALID_TOUCH_POSTION;
            }
            else
            {
                return UnityEngine.Input.GetMouseButtonDown(0) ||
                    UnityEngine.Input.GetMouseButton(0)
                    ? (Vector2)UnityEngine.Input.mousePosition
                    : INVALID_TOUCH_POSTION;
            }
        }
        //!<============================================================================
        //GetTouchCount
        //!<============================================================================
        public static int GetTouchCount()
        {
            if(UnityEngine.Application.isMobilePlatform == true)
            {
                return UnityEngine.Input.touchCount;
            }
            else
            {
                var count = 0;
                if( UnityEngine.Input.GetMouseButtonDown(0) || 
                    UnityEngine.Input.GetMouseButton(0))
                {
                    count++;
                }
                if (UnityEngine.Input.GetMouseButtonDown(1) || 
                    UnityEngine.Input.GetMouseButton(1))
                {
                    count++;
                }
                return count;
            }
        }

        //!<============================================================================
    }
}
