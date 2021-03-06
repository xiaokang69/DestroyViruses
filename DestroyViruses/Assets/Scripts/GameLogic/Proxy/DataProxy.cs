﻿using UnityEngine;
using System;
using System.Data;
using System.Collections.Generic;

namespace DestroyViruses
{
    public class DataProxy : ProxyBase<DataProxy>
    {
        private GameLocalData localData { get { return GameLocalData.Instance; } }
        private BookData bookData { get { return BookData.Instance; } }
        private WeaponLevelData weaponLevelData { get { return WeaponLevelData.Instance; } }

        public float coin { get { return localData.coin; } }
        public float diamond { get { return localData.diamond; } }
        public int gameLevel { get { return localData.gameLevel; } }
        public bool isGameLevelMax { get { return TableGameLevel.Get(gameLevel + 1) == null; } }
        public int unlockedGameLevel { get { return localData.unlockedGameLevel; } }
        public bool isUnlockedGameLevelMax { get { return TableGameLevel.Get(unlockedGameLevel + 1) == null; } }
        public bool noAd { get { return localData.noAd; } }

        public int firePowerLevel { get { return localData.firePowerLevel; } }
        public int firePowerMaxLevel { get { return TableFirePower.GetAll().Max(a => a.id).id; } }
        public bool isFirePowerLevelMax { get { return firePowerLevel >= firePowerMaxLevel; } }
        public float firePowerUpCost { get { return FormulaUtil.FirePowerUpCost(firePowerLevel); } }
        public float firePower { get { return FormulaUtil.FirePower(firePowerLevel); } }

        public int fireSpeedLevel { get { return localData.fireSpeedLevel; } }
        public int fireSpeedMaxLevel { get { return TableFireSpeed.GetAll().Max(a => a.id).id; } }
        public bool isFireSpeedLevelMax { get { return fireSpeedLevel >= fireSpeedMaxLevel; } }
        public float fireSpeedUpCost { get { return FormulaUtil.FireSpeedUpCost(fireSpeedLevel); } }
        public float fireSpeed { get { return FormulaUtil.FireSpeed(fireSpeedLevel); } }

        public int streak { get { return Mathf.Clamp(localData.streak, -6, 6); } }
        public int signDays { get { return localData.signDays; } }
        public int weaponId { get { return localData.weaponId; } }
        public int energy { get { return Mathf.Min(localData.energy, maxEnergy); } }
        public int maxEnergy { get { return CT.table.energyMax; } }
        public bool isEnergyMax { get { return energy >= maxEnergy; } }
        public int energyRechargeRemain
        {
            get
            {
                if (isEnergyMax)
                    return 0;
                var elapse = (int)(DateTime.Now - new DateTime(localData.lastEnergyTicks)).TotalSeconds;
                return Mathf.Max(0, CT.table.energyRecoverInterval - elapse);
            }
        }

        public float vipCoinValueMul { get { return (IsVip() ? 3 : 1); } }
        public int coinValueLevel { get { return localData.coinValueLevel; } }
        public float coinValue { get { return TableCoinValue.Get(coinValueLevel).value; } }
        public float coinValueUpCost { get { return TableCoinValue.Get(coinValueLevel).upcost; } }
        public int coinValueMaxLevel { get { return TableCoinValue.GetAll().Max(a => a.id).id; } }
        public bool isCoinValueLevelMax { get { return coinValueLevel >= coinValueMaxLevel; } }

        public int coinIncomeLevel { get { return localData.coinIncomeLevel; } }
        public float coinIncome { get { return TableCoinIncome.Get(coinIncomeLevel).income * vipCoinValueMul; } }
        public float coinIncomeUpCost { get { return TableCoinIncome.Get(coinIncomeLevel).upcost; } }
        public int coinIncomeMaxLevel { get { return TableCoinIncome.GetAll().Max(a => a.id).id; } }
        public bool isCoinIncomeLevelMax { get { return coinIncomeLevel >= coinIncomeMaxLevel; } }
        public bool isCoinIncomePoolFull
        {
            get
            {
                var span = DateTime.Now - new DateTime(localData.lastTakeIncomeTicks);
                return span.TotalSeconds >= CT.table.coinIncomeMaxDuration;
            }
        }

        public float coinIncomeTotal
        {
            get
            {
                if (localData.lastTakeIncomeTicks == 0)
                {
                    localData.lastTakeIncomeTicks = DateTime.Now.Ticks;
                    SaveLocalData();
                }
                var span = DateTime.Now - new DateTime(localData.lastTakeIncomeTicks);
                var secMax = Mathf.Min(CT.table.coinIncomeMaxDuration, (float)span.TotalSeconds);
                return secMax * coinIncome;
            }
        }

        // 临时数据（外部可修改）
        public bool gameEndWin { get; set; }
        public bool adRevive { get; set; }
        public int adReviveCount { get; set; }
        public int diamondReviveCount { get; set; }
        public int kills4Buff { get; set; }

        // 战斗数据
        public float battleGetCoin
        {
            get
            {
                if (GameModeManager.Instance.currentMode.GetType() == typeof(LevelMode))
                {
                    return (GameModeManager.Instance.currentMode as LevelMode).getCoin;
                }
                return 0;
            }
        }

        // 战斗进度
        public float battleProgress
        {
            get
            {
                if (GameModeManager.Instance.currentMode.GetType() == typeof(LevelMode))
                {
                    return (GameModeManager.Instance.currentMode as LevelMode).progress;
                }
                return 0;
            }
        }


        protected override void OnUpdate()
        {
            base.OnUpdate();
            UpdateEnergy();
            UpdateCheckAnalytics();
        }

        private float checkAnalyticsTimeout = 30;
        private void UpdateCheckAnalytics()
        {
            checkAnalyticsTimeout -= Time.deltaTime;
            if (checkAnalyticsTimeout <= 0 || !AnalyticsProxy.Ins.IsInit)
                return;
            checkAnalyticsTimeout = 0;
            Analytics.Event.Login(DeviceID.UUID);
            Analytics.UserProperty.Set("coin", coin.KMB());
            Analytics.UserProperty.Set("diamond", diamond.KMB());
            Analytics.UserProperty.Set("game_level", gameLevel.ToString());
            Analytics.UserProperty.Set("unlocked_game_level", unlockedGameLevel.ToString());
            Analytics.UserProperty.Set("fire_power_level", firePowerLevel.ToString());
            Analytics.UserProperty.Set("fire_speed_level", fireSpeedLevel.ToString());
            Analytics.UserProperty.Set("streak", streak.ToString());
            Analytics.UserProperty.Set("daily_sign", signDays.ToString());
            Analytics.UserProperty.Set("weapon_id", weaponId.ToString());
        }

        #region MAIN
        public void FirePowerUp()
        {
            if (isFirePowerLevelMax)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.ALREADY_LEVEL_MAX.LT());
                return;
            }

            var cost = firePowerUpCost;
            if (coin < cost)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.UPGRADE_LACK_OF_COIN.LT());
                return;
            }

            localData.coin -= cost;
            localData.firePowerLevel += 1;
            SaveLocalData();
            DispatchEvent(EventGameData.Action.DataChange);

            Analytics.Event.Upgrade("fire_power", firePowerLevel);
        }

        public void FireSpeedUp()
        {
            if (isFireSpeedLevelMax)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.ALREADY_LEVEL_MAX.LT());
                return;
            }

            var cost = fireSpeedUpCost;
            if (coin < cost)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.UPGRADE_LACK_OF_COIN.LT());
                return;
            }

            localData.coin -= cost;
            localData.fireSpeedLevel += 1;
            SaveLocalData();
            DispatchEvent(EventGameData.Action.DataChange);

            Analytics.Event.Upgrade("fire_speed", fireSpeedLevel);
        }

        private void AddCoin(float count)
        {
            localData.coin += count;
        }

        private void AddDiamond(float count)
        {
            localData.diamond += count;
        }

        private void AddDiamondWithEffect(float count)
        {
            AddDiamond(count);
            ResAddEffect.Play(ResAddView.ResType.Diamond, (int)count);
        }
        #endregion

        #region BATTLE
        public void BattleEnd(bool isWin)
        {
            gameEndWin = isWin;
            // 解锁新关卡
            if (isWin && gameLevel >= unlockedGameLevel)
            {
                UnlockNewLevel();
                SelectGameLevel(unlockedGameLevel);
            }
            if (isWin && streak >= 0)
            {
                localData.streak += 1;
            }
            else if (!isWin && streak <= 0)
            {
                localData.streak -= 1;
            }
            else
            {
                localData.streak = 0;
            }
            // energy
            if (isWin)
            {
                AddEnergy(CT.table.energyRecoverWin);
            }
            // trial end
            if (localData.trialWeaponID != 0 || localData.isInTrial)
            {
                TrialEnd();
            }
            SaveLocalData();
            SaveBookData();
            DispatchEvent(EventGameData.Action.DataChange);
        }

        public void UnlockNewLevel()
        {
            if (isUnlockedGameLevelMax)
            {
                return;
            }
            localData.unlockedGameLevel += 1;
            // auto equip
            if (weaponId <= 0 && CT.table.weaponUnlockLevel <= unlockedGameLevel)
            {
                localData.weaponId = 1;
            }

            SaveLocalData();
            DispatchEvent(EventGameData.Action.UnlockNewLevel);
            DispatchEvent(EventGameData.Action.DataChange);
        }

        public void SelectGameLevel(int level)
        {
            if (level <= 0)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.GAME_LEVEL_INVALID.LT());
                return;
            }

            if (level > localData.unlockedGameLevel)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.GAME_LEVEL_LOCKED.LT());
                return;
            }

            localData.gameLevel = level;
            SaveLocalData();
            DispatchEvent(EventGameData.Action.DataChange);
        }
        #endregion

        #region Daily Sign
        public void DailySign(float multiple)
        {
            if (!CanDailySign())
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.DAILY_SIGN_CANT_SIGN);
                return;
            }

            var days = localData.signDays;
            localData.signDays = (days % 7) + 1;
            localData.lastSignDateTicks = DateTime.Now.Date.Ticks;
            var t = TableDailySign.Get(days);
            if (t.type == 1)
            {
                AddDiamondWithEffect(t.count);
            }
            else if (t.type == 2)
            {
                AddCoin(t.count * multiple * FormulaUtil.Expresso(CT.table.formulaArgsDailySignCoin));
            }
            SaveLocalData();
            DispatchEvent(EventGameData.Action.DataChange);

            Analytics.Event.DailySign(days, multiple);
        }

        public bool CanDailySign()
        {
            var last = new DateTime(localData.lastSignDateTicks);
            return IsDailySignUnlocked() && (DateTime.Now.Date - last).TotalDays >= 1;
        }

        public bool IsDailySignUnlocked()
        {
            return gameLevel >= CT.table.dailySignUnlockLevel;
        }
        #endregion

        #region Coin Value & Income
        public void TakeIncomeCoins(int multiple = 1)
        {
            var gain = coinIncomeTotal * multiple;
            localData.lastTakeIncomeTicks = DateTime.Now.Ticks;
            AddCoin(gain);
            SaveLocalData();
            DispatchEvent(EventGameData.Action.DataChange);

            Analytics.Event.CoinIncomeTake(gain);
        }

        public void GameEndReceive(int multiple = 1)
        {
            AddCoin(D.I.battleGetCoin * multiple);
        }

        public void CoinIncomeLevelUp()
        {
            if (isCoinIncomeLevelMax)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.ALREADY_LEVEL_MAX.LT());
                return;
            }

            var cost = coinIncomeUpCost;
            if (coin < cost)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.UPGRADE_LACK_OF_COIN.LT());
                return;
            }

            localData.coin -= cost;
            localData.coinIncomeLevel += 1;
            SaveLocalData();
            DispatchEvent(EventGameData.Action.DataChange);

            Analytics.Event.Upgrade("coin_income", coinIncomeLevel);
        }

        public void CoinValueLevelUp()
        {
            if (isCoinValueLevelMax)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.ALREADY_LEVEL_MAX.LT());
                return;
            }

            var cost = coinValueUpCost;
            if (coin < cost)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.UPGRADE_LACK_OF_COIN.LT());
                return;
            }

            localData.coin -= cost;
            localData.coinValueLevel += 1;
            SaveLocalData();
            DispatchEvent(EventGameData.Action.DataChange);

            Analytics.Event.Upgrade("coin_value", coinIncomeLevel);
        }
        #endregion

        #region Exchange
        public void ExchangeCoin(float diamond)
        {
            if (diamond > this.diamond)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.EXCHANGE_DIAMOND_NOT_ENOUGH.LT());
                return;
            }

            localData.diamond -= diamond;
            var addCoin = diamond * FormulaUtil.Expresso(CT.table.formulaArgsCoinExchange);
            localData.coin += addCoin;

            SaveLocalData();
            DispatchEvent(EventGameData.Action.DataChange);

            Analytics.Event.Exchange(diamond, addCoin);
        }

        public void ExchangeEnergy(float diamond)
        {
            if (diamond > this.diamond)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.EXCHANGE_DIAMOND_NOT_ENOUGH.LT());
                return;
            }

            localData.diamond -= diamond;
            var addEnergy = (int)diamond * CT.table.energyExchange;
            localData.energy += addEnergy;
            ResAddEffect.Play(ResAddView.ResType.Energy, addEnergy);

            SaveLocalData();
            DispatchEvent(EventGameData.Action.DataChange);

            // TODO: Analytics
            // Analytics.Event.Exchange(diamond, addCoin);
        }
        #endregion

        #region Virus Book
        public void BookAddCollectCount(int virusID)
        {
            if (!bookData.Exist(virusID))
            {
                bookData.Set(virusID, 0);
            }

            bookData.Add(virusID, 1);
        }

        public int GetBookCountBegin(int virusID)
        {
            if (IsBookCollectMax(virusID))
                return 0;
            if (bookData.GetIndex(virusID) <= 0)
                return 0;
            return CT.table.bookVirusCollectKillCount[bookData.GetIndex(virusID) - 1];
        }

        public int GetBookCountEnd(int virusID)
        {
            if (IsBookCollectMax(virusID))
                return 0;
            return CT.table.bookVirusCollectKillCount[bookData.GetIndex(virusID)];
        }

        public int GetBookDiamond(int virusID)
        {
            if (IsBookCollectMax(virusID))
                return 0;
            return CT.table.bookVirusCollectRewardDiamond[bookData.GetIndex(virusID)];
        }

        public bool IsBookCollectMax(int virusID)
        {
            return bookData.GetIndex(virusID) >= CT.table.bookVirusCollectKillCount.Length
                || bookData.GetIndex(virusID) >= CT.table.bookVirusCollectRewardDiamond.Length;
        }

        public bool IsBookCollectNeedPlayAd(int virusID)
        {
            return bookData.GetIndex(virusID) < CT.table.bookVirusCollectNeedPlayAD.Length
                && CT.table.bookVirusCollectNeedPlayAD[bookData.GetIndex(virusID)] > 0;
        }

        public int BookGetCollectCount(int virusID)
        {
            return bookData.Get(virusID);
        }

        //TODO: CODE
        public bool CanBookCollect()
        {
            return false;
        }

        public bool BookIsUnlock(int virusID)
        {
            return bookData.Exist(virusID);
        }

        public void BookCollect(int virusID)
        {
            if (!bookData.Exist(virusID))
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.VIRUS_LOCKED.LT());
                return;
            }
            if (bookData.Get(virusID) < GetBookCountEnd(virusID))
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.VIRUS_COLLECT_NOT_ENOUGH.LT());
                return;
            }

            AddDiamondWithEffect(GetBookDiamond(virusID));
            bookData.AddIndex(virusID, 1);
            SaveLocalData();
            SaveBookData();
            DispatchEvent(EventGameData.Action.DataChange);
        }

        private void SaveBookData()
        {
            bookData.Save();
        }

        #endregion

        #region Weapon
        public int weaponSpeedLevel
        {
            get
            {
                if (GetTrialWeaponID() == weaponId && IsInTrial())
                    return weaponSpeedMaxLevel;
                return weaponLevelData.GetSpeedLevel(weaponId);
            }
        }
        public int weaponSpeedMaxLevel { get { return TableWeaponSpeedLevel.GetAll().Max(a => (a.weaponId == weaponId) ? a.level : 0).level; } }
        public bool isWeaponSpeedLevelMax { get { return weaponSpeedLevel >= weaponSpeedMaxLevel; } }
        public float weaponSpeedUpCost { get { return TableWeaponSpeedLevel.Get(a => a.weaponId == weaponId && a.level == weaponSpeedLevel).upCost; } }

        public int weaponPowerLevel
        {
            get
            {
                if (GetTrialWeaponID() == weaponId && IsInTrial())
                    return weaponPowerMaxLevel;
                return weaponLevelData.GetPowerLevel(weaponId);
            }
        }
        public int weaponPowerMaxLevel { get { return TableWeaponPowerLevel.GetAll().Max(a => (a.weaponId == weaponId) ? a.level : 0).level; } }
        public bool isWeaponPowerLevelMax { get { return weaponPowerLevel >= weaponPowerMaxLevel; } }
        public float weaponPowerUpCost { get { return TableWeaponPowerLevel.Get(a => a.weaponId == weaponId && a.level == weaponPowerLevel).upCost; } }

        public void ChangeWeapon(int id)
        {
            if (id > 0 && unlockedGameLevel < TableWeapon.Get(id).unlockLevel)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.NOT_REACH_WEAPON_UNLOCK_GAME_LEVEL.LT());
                return;
            }

            localData.weaponId = id;
            SaveLocalData();

            DispatchEvent(EventGameData.Action.ChangeWeapon);
            DispatchEvent(EventGameData.Action.DataChange);
            Analytics.Event.ChangeWeapon(id);
        }

        public void WeaponSpeedLevelUp()
        {
            if (isWeaponSpeedLevelMax)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.ALREADY_LEVEL_MAX.LT());
                return;
            }

            if (weaponSpeedLevel >= fireSpeedLevel)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.CANNOT_EXCEED_FIRE_SPEED_LEVEL.LT());
                return;
            }

            var cost = weaponSpeedUpCost;
            if (coin < cost)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.UPGRADE_LACK_OF_COIN.LT());
                return;
            }

            localData.coin -= cost;
            weaponLevelData.SetSpeedLevel(weaponId, weaponSpeedLevel + 1);
            SaveLocalData();
            SaveWeaponLevelData();
            DispatchEvent(EventGameData.Action.DataChange);

            Analytics.Event.Upgrade($"{TableWeapon.Get(weaponId).type.ToLower()}_speed", weaponSpeedLevel);
        }

        public void WeaponPowerLevelUp()
        {
            if (isWeaponPowerLevelMax)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.ALREADY_LEVEL_MAX.LT());
                return;
            }

            if (weaponPowerLevel >= firePowerLevel)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.CANNOT_EXCEED_FIRE_POWER_LEVEL.LT());
                return;
            }

            var cost = weaponPowerUpCost;
            if (coin < cost)
            {
                DispatchEvent(EventGameData.Action.Error, LTKey.UPGRADE_LACK_OF_COIN.LT());
                return;
            }

            localData.coin -= cost;
            weaponLevelData.SetPowerLevel(weaponId, weaponPowerLevel + 1);
            SaveLocalData();
            SaveWeaponLevelData();
            DispatchEvent(EventGameData.Action.DataChange);

            Analytics.Event.Upgrade($"{TableWeapon.Get(weaponId).type.ToLower()}_power", weaponPowerLevel);
        }

        private void SaveWeaponLevelData()
        {
            weaponLevelData.Save();
        }
        #endregion

        #region ENERGY
        bool isLastEnergyMax = false;
        private void UpdateEnergy()
        {
            if (!GameUtil.isInHome)
                return;
            // energy 
            if (isEnergyMax)
            {
                localData.lastEnergyTicks = DateTime.Now.Ticks;
                if (!isLastEnergyMax)
                {
                    SaveLocalData();
                    DispatchEvent(EventGameData.Action.DataChange);
                }
            }
            else
            {
                DateTime _egLast = new DateTime(localData.lastEnergyTicks);
                var diff = (DateTime.Now - _egLast).TotalSeconds;
                if (diff >= CT.table.energyRecoverInterval)
                {
                    var add = (int)(diff / CT.table.energyRecoverInterval);
                    localData.lastEnergyTicks = DateTime.Now.Ticks
                        - (long)(diff - add * CT.table.energyRecoverInterval) * 10000000L;
                    var energys = Mathf.Min(add, maxEnergy - energy);
                    AddEnergy(energys);
                }
            }

            isLastEnergyMax = isEnergyMax;
        }

        public void AddEnergy(int count)
        {
            localData.energy += count;
            SaveLocalData();
            DispatchEvent(EventGameData.Action.DataChange);
        }

        public void CostEnergy(int count)
        {
            localData.energy -= count;
            SaveLocalData();
            DispatchEvent(EventGameData.Action.DataChange);
        }
        #endregion

        #region Weapon Trial
        public void TrialBegin()
        {
            if (!HasTrialWeapon())
                return;
            localData.isInTrial = true;
            SaveLocalData();

            DispatchEvent(EventGameData.Action.DataChange);
        }

        public void TrialEnd()
        {
            localData.trialWeaponID = 0;
            localData.isInTrial = false;
            localData.trialCount += 1;

            SaveLocalData();
        }

        public bool HasTrialWeapon()
        {
            return GetTrialWeaponID() != 0;
        }

        public int GetTrialWeaponID()
        {
            if (unlockedGameLevel < CT.table.weaponUnlockLevel)
                return 0;

            var _last = new DateTime(localData.lastTrialTicks);
            if (_last.DayOfYear != DateTime.Now.DayOfYear)
            {
                localData.lastTrialTicks = DateTime.Now.Ticks;
                localData.trialCount = 0;
                SaveLocalData();
            }
            if (localData.trialCount >= CT.table.maxWeaponTrialCount)
            {
                return 0;
            }
            if (localData.trialWeaponID == 0 && streak < 0)
            {
                var _weapons = new List<int>();
                foreach (var w in TableWeapon.GetAll())
                {
                    if (w.unlockLevel <= unlockedGameLevel)
                    {
                        _weapons.Add(w.id);
                    }
                }
                var _id = FormulaUtil.RandomInArray(_weapons.ToArray());
                if (_id != 0)
                {
                    localData.trialWeaponID = _id;
                    SaveLocalData();
                }
            }
            return localData.trialWeaponID;
        }

        public bool IsInTrial()
        {
            return localData.isInTrial;
        }
        #endregion

        #region PURCHASE
        public void OnPurchaseSuccess(int shopGoodsID, DateTime date)
        {
            var t = TableShop.Get(shopGoodsID);
            if (t.type == 0) // Consumable
            {
                AddDiamondWithEffect(t.diamonds + t.extra);
                SaveLocalData();
                DispatchEvent(EventGameData.Action.DataChange);
            }
            else if (t.type == 2) // Subscription
            {
                localData.lastVipTicks = date.Ticks;
                SaveLocalData();
                DispatchEvent(EventGameData.Action.DataChange);
            }
        }

        public void Purchase(int shopGoodsID)
        {
            var t = TableShop.Get(shopGoodsID);
            // IAPManager.Instance.Purchase(t.productID);
        }
        #endregion

        #region VIP
        public bool IsVip()
        {
            return VipRemain() > 0;
        }

        public DateTime VipExpirationDate()
        {
            if (localData.lastVipTicks > 0)
                return new DateTime(localData.lastVipTicks + 7 * 24 * 3600 * 10000000L);
            return DateTime.MinValue;
        }

        public float VipRemain()
        {
            if (localData.lastVipTicks <= 0)
                return 0;
            var dt = new DateTime(localData.lastVipTicks);
            return 7 * 24 * 3600 - (float)(DateTime.Now - dt).TotalSeconds;
        }

        public bool HasVipReward()
        {
            return IsVip() && localData.lastVipRewardDays != DateTime.Now.DayOfYear;
        }

        public void ReceiveVipReward()
        {
            localData.lastVipRewardDays = DateTime.Now.DayOfYear;
            AddDiamondWithEffect(5);
            SaveLocalData();
            DispatchEvent(EventGameData.Action.DataChange);
        }
        #endregion

        public void ReviveUseDiamond()
        {
            AddDiamond(-1);
            SaveLocalData();
        }


        private void SaveLocalData()
        {
            localData.Save();
        }

        private void DispatchEvent(EventGameData.Action action, string errorMsg = "")
        {
            UnibusEvent.Unibus.Dispatch(EventGameData.Get(action, errorMsg));
        }
    }

    public static class D
    {
        public static DataProxy I { get { return DataProxy.Ins; } }
    }
}