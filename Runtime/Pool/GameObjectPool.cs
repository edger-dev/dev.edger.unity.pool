using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

using Edger.Unity;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Edger.Unity.Pool {
    public class GameObjectPool : BaseMono {
        [SerializeField]
        private GameObject _Prefab = null;
        public GameObject Prefab { get => _Prefab; }

        [SerializeField]
        private PoolConfig _Config = PoolConfig.DEFAULT;
        public PoolConfig Config { get => _Config; }

        public PoolData Data { get; private set; }

        public string PoolKey {
            get {
                return Data.PoolKey;
            }
        }

        private ObjectPool<GameObject> _Pool = null;

#if ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
#endif
        public int UnusedCount {
            get {
                return _Pool == null ? 0 : _Pool.CountInactive;
            }
        }

        public void Setup(GameObject prefab, PoolConfig config) {
            if (_Prefab == null) {
                _Prefab = prefab;
                if (config != null) {
                    _Config = config;
                }
            } else {
                Critical("Already Setup: ", _Prefab, _Config.ToString());
            }
        }

        protected override void OnAwake() {
            if (_Config == null) {
                _Config = PoolConfig.DEFAULT;
            }
            Data = gameObject.GetOrAddComponent<PoolData>();
            Data.Setup(name);
        }

        private void CheckPool() {
            if (_Pool == null) {
                _Pool = new ObjectPool<GameObject>(PoolCreate, PoolOnGet, PoolOnRelease, PoolOnDestroy, maxSize: Config.MaxSize);
            }
        }

        private GameObject PoolCreate() {
            var startTime = DateTimeUtil.GetStartTime();
            GameObject result = GameObject.Instantiate(Prefab) as GameObject;
            InPoolData data = result.GetOrAddComponent<InPoolData>();
            data.Setup(Data.PoolKey);
            InfoFrom(result, "PoolCreate() -> {0} in {1:F2} ms", result.name, startTime.GetPassedSeconds() * 1000.0);
            if (DebugMode) {
                ErrorFrom(result, "PoolCreate() -> {0} in {1:F2} ms", result.name, startTime.GetPassedSeconds() * 1000.0);
            }
            return result;
        }

        private void PoolOnGet(GameObject go) {
            if (Config.SetActive && Prefab.activeSelf) {
                go.SetActive(true);
            }
            if (Config.UpdateInPoolData) {
                go.GetComponent<InPoolData>().OnTaken();
            }
            Data.OnTaken();
            if (LogDebug) {
                DebugFrom(go, "PoolOnGet() -> {0}", go.name);
            }
        }

        private void PoolOnRelease(GameObject go) {
            if (Config.SetActive) {
                go.SetActive(false);
            }
            if (Config.UpdateInPoolData) {
                go.GetComponent<InPoolData>().OnReleased();
            }
            Data.OnReleased();
            if (LogDebug) {
                DebugFrom(go, "PoolOnRelease() -> {0}", go.name);
            }
        }

        private void PoolOnDestroy(GameObject go) {
            if (LogDebug) {
                DebugFrom(go, "PoolOnDestroy() -> {0}", name, go.name);
            }
            GameObjectUtil.Destroy(go);
        }

        public GameObject Take(UnityEngine.Object caller = null) {
            CheckPool();
            GameObject result = _Pool.Get();
            if (Config.UpdateParent && result != null) {
                result.transform.SetParent(transform, false);
            }
            InfoFrom(caller == null ? result : caller,
                "Take() -> {0}", result == null ? "null" : result.name);
            return result;
        }

        public void Release(GameObject go, UnityEngine.Object caller = null) {
            if (go == null) return;

            if (_Pool == null) {
                ErrorFrom(caller == null ? go : caller,
                    "Release() Pool Not Setup -> {0}", go.name);
                return;
            }
            if (Config.UpdateParent && go.transform.parent != transform) {
                go.transform.SetParent(transform, false);
            }
            InfoFrom(caller == null ? go : caller,
                "Release() -> {0}", go.name);
            try {
                _Pool.Release(go);
            } catch (Exception e) {
                ErrorFrom(caller == null ? go : caller, "Release() Got Exception: {0} -> {1}", transform.name, e);
            }
        }

        public void ReleaseUnused() {
            if (_Pool != null) {
                _Pool.Clear();
            }
        }
    }
}
