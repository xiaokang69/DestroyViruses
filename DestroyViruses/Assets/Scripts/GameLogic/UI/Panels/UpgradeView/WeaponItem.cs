﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DestroyViruses
{
    public class WeaponItem : ViewBase
    {
        public Text title;
        public Image icon;
        public RadioGroup radioState;
        public GameObject selectedObj;
        public Text unlockText;

        private int mId = -1;

        public void SetData(int id)
        {
            mId = id;
            var table = TableWeapon.Get(id);
            icon.SetSprite(table.icon);
            title.text = LT.Get(table.nameID);
            selectedObj.SetActive(id == D.I.weaponId);
            radioState.Radio(D.I.unlockedGameLevel < table.unlockLevel ? 0 : D.I.weaponId == id ? 2 : 1);
            unlockText.text = $"{table.unlockLevel}级解锁";
        }

        private void OnClickSelf()
        {
            if (mId == D.I.weaponId)
            {
                D.I.ChangeWeapon(0);
            }
            else
            {
                D.I.ChangeWeapon(mId);
            }
        }
    }
}