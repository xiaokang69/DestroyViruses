﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DestroyViruses
{
    public class BookVirusItem : ViewBase
    {
        public Text title;
        public Transform modelRoot;
        public Slider fill;
        public RadioGroup radioState;

        private VirusBase mVirus;
        private string mLastPrefabPath;
        private VirusData v;
        private int mColorIndex;

        public void SetData(VirusData v)
        {
            this.v = v;
            title.text = v.name;

            if (v.prefabPath != mLastPrefabPath)
            {
                if (mVirus != null)
                    DestroyImmediate(mVirus.gameObject);

                var prefab = ResourceUtil.Load<VirusBase>(v.prefabPath);
                if (mVirus == null && prefab != null)
                {
                    mVirus = Instantiate(prefab);
                    if (mVirus != null)
                    {
                        mVirus.rectTransform.SetParent(modelRoot, false);
                        mVirus.GetComponent<Collider2D>().enabled = false;
                        mVirus.stunEffect.gameObject.SetActive(false);
                        mVirus.cureEffect.gameObject.SetActive(false);
                        mColorIndex = UnityEngine.Random.Range(0, 6);
                        mVirus.SetColor(mColorIndex);
                    }
                }

                mLastPrefabPath = v.prefabPath;
            }

            if (mVirus != null)
            {
                mVirus.stunEffect.Stop(true);
            }
            if (v.isMax)
                fill.value = 1f;
            else
                fill.value = v.progress;
            if (v.isUnlock)
            {
                if (v.isReceivable)
                {
                    radioState.Radio(2);
                }
                else
                {
                    radioState.Radio(1);
                }
            }
            else
            {
                radioState.Radio(0);
            }
        }

        private void OnClickSelf()
        {
            BookVirusView.VirusID = v.id;
            BookVirusView.ColorIndex = mColorIndex;
            UIManager.Open<BookVirusView>();
        }
    }
}