using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Edger.Unity;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Edger.Unity.Pool {
    public class BaseData : BaseMono {
#if ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
#endif
        public string PoolKey { get; private set; }

#if ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
#endif
        public int TakenTimes { get; private set; }

#if ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
        [DisplayAsString]
#endif
        public DateTime CreatedTime { get; private set; }

#if ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
        [DisplayAsString]
#endif
        public DateTime LastTakenTime { get; private set; }

#if ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
        [DisplayAsString]
#endif
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

        public virtual void OnTaken() {
            TakenTimes++;
            LastTakenTime = DateTimeUtil.GetStartTime();
        }

        public virtual void OnReleased() {
            LastReleasedTime = DateTimeUtil.GetStartTime(); ;
        }
    }
}