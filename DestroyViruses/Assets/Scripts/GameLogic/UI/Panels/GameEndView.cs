﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using UnibusEvent;

namespace DestroyViruses
{
    public class GameEndView : ViewBase
    {
        public RectTransform coinTransform;
        public Button receiveBtns;
        public Text coinText;
        public RadioObjects winLoseRadio;

        private void Awake()
        {
            receiveBtns.OnClickAsObservable().Subscribe(_ => OnClickReceive());
        }

        private void OnClickReceive()
        {
            GDM.ins.AddCoin((int)GDM.ins.battleGetCoin);
            Close();
            GameManager.ChangeState<MainState>();

            // 注意放到最后
            if (!Mathf.Approximately(GDM.ins.battleGetCoin ,0))
            {
                Coin.CreateGroup(coinTransform.GetUIPos(), UIUtil.COIN_POS, 20);
            }
        }

        protected override void OnOpen()
        {
            coinText.text = GDM.ins.battleGetCoin.KMB();
            winLoseRadio.Radio(!GDM.ins.gameEndWin);
        }
    }
}