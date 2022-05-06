using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

using Edger.Unity;
using Edger.Unity.Remote;

namespace Edger.Unity.Pool {
    public class GameObjectPool : BaseMono {
        public const int Default_MaxSize = 100;
        public const bool Default_UpdateParent = true;
        public const bool Default_SetActive = true;
        public GameObject Prefab = null;
        public int MaxSize = Default_MaxSize;
        public bool UpdateParent = Default_UpdateParent;
        public bool SetActive = Default_SetActive;

        private ObjectPool<GameObject> _Pool = null;

        private void CheckPool() {
            if (_Pool == null) {
                _Pool = new ObjectPool<GameObject>(PoolCreate, PoolOnGet, PoolOnRelease, PoolOnDestroy, maxSize: MaxSize);
            }
        }

        private GameObject PoolCreate() {
            var startTime = DateTimeUtil.GetStartTime();
            GameObject result = GameObject.Instantiate(Prefab) as GameObject;
            Info("PoolCreate(): [{0}] -> {1} in {2:F2} ms", name, result.name, startTime.GetPassedSeconds() * 1000.0);
            if (DebugMode) {
                Log.ErrorFrom(result, "<GameObjectPool>.PoolCreate(): [{0}] -> {1} in {2:F2} ms", name, result.name, startTime.GetPassedSeconds() * 1000.0);
            }
            return result;
        }

        private void PoolOnGet(GameObject go) {
            if (SetActive && Prefab.activeSelf) {
                go.SetActive(true);
            }
            if (LogDebug) {
                Debug("PoolOnGet(): {0} -> {1}", name, go.name);
            }
        }

        private void PoolOnRelease(GameObject go) {
            if (SetActive) {
                go.SetActive(false);
            }
            if (LogDebug) {
                Debug("PoolOnRelease(): {0} -> {1}", name, go.name);
            }
        }

        private void PoolOnDestroy(GameObject go) {
            if (LogDebug) {
                Debug("PoolOnDestroy(): {0} -> {1}", name, go.name);
            }
        }

        public GameObject Take(UnityEngine.Object caller = null) {
            CheckPool();
            GameObject result = _Pool.Get();
            if (UpdateParent && result != null) {
                result.transform.SetParent(transform, false);
            }
            Info("Take(): [{0}] -> {1}", name, result == null ? "null" : result.name);
            if (DebugMode) {
                Log.ErrorFrom(caller == null ? result : caller,
                    "<GameObjectPool>.Take(): [{0}] -> {1}", name, result == null ? "null" : result.name);
            }
            return result;
        }

        public void Release(GameObject go, UnityEngine.Object caller = null) {
            if (go == null) return;

            if (_Pool == null) {
                Error("Release(): Pool Not Setup: [{0}] -> {1}", name, go.name);
                return;
            }
            if (UpdateParent && go.transform.parent != transform) {
                go.transform.SetParent(transform, false);
            }
            Info("Release(): [{0}] -> {1}", name, go.name);
            if (DebugMode) {
                Log.ErrorFrom(caller == null ? go : caller,
                    "<GameObjectPool>.Release(): [{0}] -> {1}", name, go.name);
            }
            _Pool.Release(go);
        }
    }
}
