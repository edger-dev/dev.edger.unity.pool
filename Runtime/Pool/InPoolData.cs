using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Edger.Unity;

namespace Edger.Unity.Pool {
    [DisallowMultipleComponent()]
    public class InPoolData : BaseMono {
        public string PoolKey { get; private set; }
        public bool Taken { get; private set; }
        public int TakenCount { get; private set; }
        public DateTime CreatedTime { get; private set; }
        public DateTime LastTakenTime { get; private set; }
        public DateTime LastReleasedTime { get; private set; }

        public bool Setup(string key) {
            if (PoolKey == key) return true;
            if (!string.IsNullOrEmpty(PoolKey)) {
                Error("Setup() failed, already setup: {0} -> {1}", PoolKey, key);
                return false;
            }
            PoolKey = key;
            CreatedTime = DateTimeUtil.GetStartTime();
            return true;
        }

        internal void OnTaken() {
            if (!Taken) {
                Taken = true;
                TakenCount++;
                LastTakenTime = DateTimeUtil.GetStartTime();
            }
        }

        internal void OnReleased() {
            if (Taken) {
                LastReleasedTime = DateTimeUtil.GetStartTime();;
            }
        }
    }
}