﻿using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.U2D;
using System.Collections.Generic;

namespace DestroyViruses
{
    public static class UIUtil
    {
        private static RectTransform s_uiRoot = null;
        public static RectTransform uiRoot
        {
            get
            {
                if (s_uiRoot == null)
                {
                    var go = GameObject.Find("UIRoot");
                    if (go == null)
                    {
                        Debug.LogError("Can't find UI Root.");
                        return null;
                    }
                    s_uiRoot = go.GetComponent<RectTransform>();
                }
                return s_uiRoot;
            }
        }

        private static RectTransform s_uiBattleRoot = null;
        public static RectTransform uiBattleRoot
        {
            get
            {
                if (s_uiBattleRoot == null)
                {
                    if (uiRoot != null)
                    {
                        var go = uiRoot.Find("Battle");
                        if (go == null)
                        {
                            Debug.LogError("Can't find UI Battle Root.");
                            return null;
                        }
                        s_uiBattleRoot = go.GetComponent<RectTransform>();
                    }
                }
                return s_uiBattleRoot;
            }
        }

        private static CanvasScaler s_canvasScaler = null;
        public static CanvasScaler canvasScaler
        {
            get
            {
                if (s_canvasScaler != null)
                    return s_canvasScaler;
                if (uiRoot == null)
                    return null;
                s_canvasScaler = uiRoot.GetComponent<CanvasScaler>();
                return s_canvasScaler;
            }
        }

        public static float defaultWidth { get { return canvasScaler == null ? 1080 : canvasScaler.referenceResolution.x; } }
        public static float defaultHeight { get { return canvasScaler == null ? 1920 : canvasScaler.referenceResolution.y; } }
        public static float defaultAspect { get { return defaultWidth / defaultHeight; } }
        public static float width { get { return aspect > defaultAspect ? defaultWidth * aspect / defaultAspect : defaultWidth; } }
        public static float height { get { return 1 / aspect * width; } }
        public static float aspect { get { return (float)Screen.width / Screen.height; } }
        public static Vector2 center { get { return size * 0.5f; } }
        public static Vector2 size { get { return new Vector2(width, height); } }
        private static float sRealToVirtualRate { get { return width / Screen.width; } }

        public static float FormatToVirtual(float value)
        {
            return sRealToVirtualRate * value;
        }

        public static Vector2 FormatToVirtual(Vector2 value)
        {
            return sRealToVirtualRate * value;
        }

        public static Vector2 UIWorldToUIPos(Vector3 worldPos)
        {
            if (s_uiRoot == null)
                return Vector2.zero;
            Vector2 uiPos = worldPos / s_uiRoot.localScale.x;
            return uiPos + center;
        }

        public static Vector2 GetUIPos(Transform uiTransform)
        {
            return UIWorldToUIPos(uiTransform.position);
        }

        private static Dictionary<string, SpriteAtlas> sAtlasDict = new Dictionary<string, SpriteAtlas>();
        public static SpriteAtlas LoadSpriteAtlas(string path)
        {
            if (sAtlasDict.ContainsKey(path))
            {
                return sAtlasDict[path];
            }
            SpriteAtlas atlas = Resources.Load<SpriteAtlas>(path);
            sAtlasDict.Add(path, atlas);
            return atlas;
        }

        public static Sprite GetSprite(string atlasPath, string spriteName)
        {
            var atlas = LoadSpriteAtlas(atlasPath);
            Sprite sprite = atlas.GetSprite(spriteName);
            return sprite;
        }
    }
}