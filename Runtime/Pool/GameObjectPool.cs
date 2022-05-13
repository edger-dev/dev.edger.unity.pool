using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

using Edger.Unity;

namespace Edger.Unity.Pool {
    public class GameObjectPool : BaseMono {
        public const int Default_MaxSize = 100;
        public const bool Default_UpdateParent = true;
        public const bool Default_SetActive = true;
        public const bool Default_UpdateData = true;

        public GameObject Prefab = null;
        public int MaxSize = Default_MaxSize;
        public bool UpdateParent = Default_UpdateParent;
        public bool SetActive = Default_SetActive;
        public bool UpdateData = Default_UpdateData;

        public PoolData Data { get; private set; }

        public string PoolKey {
            get {
                return Data.PoolKey;
            }
        }

        private ObjectPool<GameObject> _Pool = null;

        protected override void OnAwake() {
            Data = gameObject.GetOrAddComponent<PoolData>();
            Data.Setup(name);
        }

        private void CheckPool() {
            if (_Pool == null) {
                _Pool = new ObjectPool<GameObject>(PoolCreate, PoolOnGet, PoolOnRelease, PoolOnDestroy, maxSize: MaxSize);
            }
        }

        private GameObject PoolCreate() {
            var startTime = DateTimeUtil.GetStartTime();
            GameObject result = GameObject.Instantiate(Prefab) as GameObject;
            InPoolData data = result.GetOrAddComponent<InPoolData>();
            data.Setup(Data.PoolKey);
            Info("PoolCreate() -> {0} in {1:F2} ms", result.name, startTime.GetPassedSeconds() * 1000.0);
            if (DebugMode) {
                Log.ErrorFrom(result, "<GameObjectPool>.PoolCreate() -> {0} in {1:F2} ms", result.name, startTime.GetPassedSeconds() * 1000.0);
            }
            return result;
        }

        private void PoolOnGet(GameObject go) {
            if (SetActive && Prefab.activeSelf) {
                go.SetActive(true);
            }
            if (UpdateData) {
                go.GetComponent<InPoolData>().OnTaken();
            }
            if (LogDebug) {
                Debug("PoolOnGet() -> {0}", go.name);
            }
        }

        private void PoolOnRelease(GameObject go) {
            if (SetActive) {
                go.SetActive(false);
            }
            if (UpdateData) {
                go.GetComponent<InPoolData>().OnReleased();
            }
            if (LogDebug) {
                Debug("PoolOnRelease() -> {0}", go.name);
            }
        }

        private void PoolOnDestroy(GameObject go) {
            if (LogDebug) {
                Debug("PoolOnDestroy() -> {0}", name, go.name);
            }
        }

        public GameObject Take(UnityEngine.Object caller = null) {
            CheckPool();
            GameObject result = _Pool.Get();
            if (UpdateParent && result != null) {
                result.transform.SetParent(transform, false);
            }
            Info("Take() -> {0}", result == null ? "null" : result.name);
            if (DebugMode) {
                Log.ErrorFrom(caller == null ? result : caller,
                    "{0}Take() -> {2}", LogPrefix, result == null ? "null" : result.name);
            }
            return result;
        }

        public void Release(GameObject go, UnityEngine.Object caller = null) {
            if (go == null) return;

            if (_Pool == null) {
                Log.ErrorFrom(caller == null ? go : caller,
                    "{0}Release() Pool Not Setup: [{1}] -> {2}", LogPrefix, name, go.name);
                return;
            }
            if (UpdateParent && go.transform.parent != transform) {
                go.transform.SetParent(transform, false);
            }
            Info("Release() -> {0}", go.name);
            if (DebugMode) {
                Log.ErrorFrom(caller == null ? go : caller,
                    "{0}Release() -> {1}", LogPrefix, go.name);
            }
            try {
                _Pool.Release(go);
            } catch (Exception e) {
                Log.ErrorFrom(caller == null ? go : caller, "{0}Release() Got Exception: {1} -> {2}", LogPrefix, transform.name, e);
            }
        }
    }
}
