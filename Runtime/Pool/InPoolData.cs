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
    public class InPoolData : BaseData {
#if ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
#endif
        public bool Taken { get; private set; }

        public override void OnTaken() {
            if (!Taken) {
                Taken = true;
                base.OnTaken();
            }
        }

        public override void OnReleased() {
            if (Taken) {
                Taken = false;
                base.OnReleased();
            }
        }
    }
}