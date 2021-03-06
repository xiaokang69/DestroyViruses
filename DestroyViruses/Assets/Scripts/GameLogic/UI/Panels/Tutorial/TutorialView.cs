﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnibusEvent;
using System.IO;
using System;

namespace DestroyViruses
{
    public class TutorialView : ViewBase
    {
        public Text title;
        public Text title1;
        public Text title2;
        public Text title3;
        public RectTransform handRect;
        public RadioGameObject radioDirection;

        private static RectTransform target;
        private static string titleStr;
        private static Action onClickCallback;
        private static int direction = 0;

        private bool hasCanvas = false;
        private bool hasRaycaster = false;
        private int rawCanvasOrder = 0;
        private bool rawCanvasOverrideSorting = false;

        protected override void OnOpen()
        {
            base.OnOpen();
            Update();

            var canvas = target.GetComponent<Canvas>();
            if (canvas != null)
            {
                hasCanvas = true;
                rawCanvasOrder = canvas.sortingOrder;
                rawCanvasOverrideSorting = canvas.overrideSorting;
            }
            else
            {
                hasCanvas = false;
                canvas = target.gameObject.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1100;

            var raycaster = target.GetComponent<GraphicRaycaster>();
            hasCanvas = raycaster != null;
            if (!hasCanvas)
            {
                target.gameObject.AddComponent<GraphicRaycaster>();
            }

            target.GetComponentInChildren<Button>().onClick.AddListener(OnClickTarget);
        }

        private void Update()
        {
            if (target == null)
                return;

            handRect.anchoredPosition = UIUtil.GetUIPos(target);
            handRect.sizeDelta = target.rect.size;
            title1.text = title2.text = title3.text = title.text = titleStr;
            radioDirection.Radio(direction);
        }

        protected override void OnClose()
        {
            if (!hasRaycaster)
            {
                DestroyImmediate(target.gameObject.GetComponent<GraphicRaycaster>());
            }
            if (!hasCanvas)
            {
                DestroyImmediate(target.gameObject.GetComponent<Canvas>());
            }
            else
            {
                var canvas = target.GetComponent<Canvas>();
                canvas.sortingOrder = rawCanvasOrder;
                canvas.overrideSorting = rawCanvasOverrideSorting;
            }
            target.GetComponentInChildren<Button>().onClick.RemoveListener(OnClickTarget);
            base.OnClose();
        }

        void OnClickTarget()
        {
            Close();
            onClickCallback?.Invoke();
        }

        public static void Begin(RectTransform target, string title = "", int direction = 0, Action callback = null)
        {
            TutorialView.titleStr = title;
            TutorialView.target = target;
            TutorialView.onClickCallback = callback;
            TutorialView.direction = direction;
            UIManager.Open<TutorialView>();
        }
    }
}