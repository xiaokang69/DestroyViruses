﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnibusEvent;
using UnityEngine.UI;

namespace DestroyViruses
{
    public class Bullet : EntityBase<Bullet>
    {
        public static float BULLET_HEIGH => ConstTable.table.bulletVDist;
        public static float BULLET_WIDTH => ConstTable.table.bulletHDist;

        public Image icon;
        public float bornCD = 0.03f;

        protected float mSpeed;
        protected float mDamage;

        private void SetEffect()
        {
            var proxy = ProxyManager.GetProxy<BuffProxy>();
            bool hasPower = proxy.Has_Effect_FirePower;
            bool hasCoin = proxy.Has_Effect_Coin;
            bool hasSpeed = proxy.Has_Effect_FireSpeed;
            bool hasKnockback = proxy.Has_Effect_Knockback;
            var _ico = $"bullet_{(hasKnockback ? 2 : (hasSpeed ? 1 : 0))}_{(hasCoin ? 1 : 0)}_{(hasPower ? 1 : 0)}";
            icon.SetSprite(_ico);
        }

        public void Reset(Vector2 position, float offsetX, float damage, float speed)
        {
            mDamage = damage;
            mSpeed = speed;
            rectTransform.anchoredPosition = position;
            rectTransform.DOAnchorPos3DX(position.x + offsetX, bornCD);
            isAlive = true;

            SetEffect();
        }

        private void OnTriggerEnter2D(Collider2D _collider)
        {
            if (!isAlive)
                return;

            if (_collider.tag == TagUtil.Virus)
            {
                var virus = _collider.GetComponent<VirusBase>();
                if (virus != null && virus.isAlive)
                {
                    Unibus.Dispatch(EventBullet.Get(EventBullet.Action.HIT, virus, mDamage));
                    PlayExplosion();
                    Recycle();
                    isAlive = false;
                }
            }
        }

        private void PlayExplosion()
        {
            if (TimeUtil.CheckInterval("BulletExplosion", 0.1f))
            {
                ExplosionBullet.Create().rectTransform.anchoredPosition = transform.GetUIPos() + Vector2.up * 10;
            }
        }

        private void Update()
        {
            Vector2 uiPos = UIUtil.GetUIPos(rectTransform);
            if (uiPos.y > UIUtil.height + BULLET_HEIGH
                || uiPos.x > UIUtil.width + BULLET_WIDTH || uiPos.x < -BULLET_WIDTH)
            {
                Recycle();
                isAlive = false;
            }

            rectTransform.anchoredPosition += Vector2.up * mSpeed * Time.deltaTime;
        }
    }
}