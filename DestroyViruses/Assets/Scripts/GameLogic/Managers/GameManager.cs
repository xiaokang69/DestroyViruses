﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DestroyViruses
{
    public class GameManager : Singleton<GameManager>
    {
        void Start()
        {
            UIManager.Instance.loadViewFunc = LoadViewFunc;
            Application.logMessageReceived += LogCallback;
        }

        static ViewBase LoadViewFunc(string panelName)
        {
            return ResourceUtil.Load<ViewBase>(PathUtil.Panel(panelName));
        }

        static void LogCallback(string condition, string stackTrace, LogType type)
        {
            // log error or exception
            if (type > LogType.Log)
            {
                Analytics.Event.Log(type, condition, stackTrace);
            }
        }

        private void Update()
        {
            // FirebaseChecker.Update(Time.deltaTime);
        }
    }
}