﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnibusEvent;
using System.IO;

namespace DestroyViruses
{
    public class CoinView : ViewBase
    {
        public FadeGroup fadeGroup;
        public CoinLevelPanel levelPanel;
        public ContentGroup shopGoods;
        public ButtonPro vipBtn;
        public Text vipRewardText;

        private void OnEnable()
        {
            fadeGroup.FadeIn(() =>
            {
                NavigationView.BlackSetting(true);
            });
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            this.BindUntilDisable<EventGameData>(OnEventGameData);
            RefreshUI();
        }

        private void RefreshUI()
        {
            levelPanel.SetData();
            shopGoods.SetData<ShopGoodsItem, TableShop>(TableShop.GetAll().ToList(a=>a.type == 0)
            , (index, item, _data) =>
            {
                item.SetData(_data.id);
            });
            vipBtn.targetImage.SetGrey(!D.I.IsVip());
            vipRewardText.gameObject.SetActive(D.I.IsVip());
        }

        private void OnEventGameData(EventGameData evt)
        {
            if (!isActiveAndEnabled)
                return;

            if (evt.action == EventGameData.Action.DataChange)
            {
                RefreshUI();
            }
        }

        private void OnClickClose()
        {
            NavigationView.BlackSetting(false);
            fadeGroup.FadeOut(Close);
        }

        private void OnClickVip()
        {
            UIManager.Open<VipView>(UILayer.Top);
        }
    }
}