﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace DestroyViruses
{
    public class DailySignItem : MonoBehaviour
    {
        public Text title;
        public Image image;
        public Text count;
        public ButtonPro btn;
        [Header("0-未领取, 1-可领取, 2-已领取")]
        public RadioGameObject stateRadio;
        [Header("0-未领取, 1-可领取, 2-已领取")]
        public RadioGameObject bgRadio;

        public void SetData(int days)
        {
            var t = TableDailySign.Get(days);
            title.text = LT.Get(t.nameID);
            image.SetSprite(t.icon);
            if (t.type == 2) // coin
                count.text = "x" + t.count * FormulaUtil.Expresso(CT.table.formulaArgsDailySignCoin);
            else
                count.text = "x" + t.count;
            if (days == D.I.signDays)
            {
                if (D.I.CanDailySign()) stateRadio.Radio(1);
                else stateRadio.Radio(0);
            }
            else
            {
                if (days < D.I.signDays) stateRadio.Radio(2);
                else stateRadio.Radio(0);
            }
            bgRadio.Radio(stateRadio.index);
            btn.clickEventID = days;
        }
    }
}