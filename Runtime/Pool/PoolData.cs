using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Edger.Unity;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Edger.Unity.Pool {
    [DisallowMultipleComponent()]
    public class PoolData : BaseData {
#if ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
#endif
        public int TakenCount { get; private set; }


        public override void OnTaken() {
            TakenCount++;
            base.OnTaken();
        }

        public override void OnReleased() {
            TakenCount--;
            base.OnReleased();
        }
    }
}