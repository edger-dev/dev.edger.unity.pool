using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Edger.Unity;
using Edger.Unity.Remote;

namespace Edger.Unity.Pool {
    public class PoolUtil : BaseMono {
        private static PoolUtil _Instance;
        public static PoolUtil Instance {
            get {
                if (_Instance == null) {
                    GameObject go = GameObjectUtil.GetOrSpawnRoot("_PoolUtil_");
                    _Instance = go.GetOrAddComponent<PoolUtil>();
                }
                return _Instance;
            }
        }

        private Dictionary<string, GameObjectPool> _Pools = new Dictionary<string, GameObjectPool>();

        public void Start() {
            //this.DebugMode = true;
            foreach (Transform child in transform) {
                var pool = child.GetComponent<GameObjectPool>();
                if (pool != null) {
                    _Pools[pool.name] = pool;
                }
            }
        }

        public GameObjectPool GetPool(string key) {
            GameObjectPool pool = null;
            if (_Pools.TryGetValue(key, out pool)) {
                return pool;
            }
            return null;
        }

        public bool Release(string poolKey, GameObject go) {
            var pool = PoolUtil.Instance.GetPool(poolKey);
            if (pool == null) {
                Error("Release() Pool Not Found: [{0}] {1}", poolKey, go.name);
                return false;
            }
            try {
                pool.Release(go, go);
                return true;
            } catch (Exception e) {
                Error("GameObjectDespawn() Got Exception: {0} -> {1}", transform.name, e);
            }
            return false;
        }

        public GameObjectPool GetOrAddPool(string key,
                    Func<GameObject> createPrefab,
                    int maxSize = GameObjectPool.Default_MaxSize,
                    bool updateParent = GameObjectPool.Default_UpdateParent,
                    bool setActive = GameObjectPool.Default_SetActive,
                    Action<string, GameObjectPool> onPoolAdded = null) {
            GameObjectPool pool = GetPool(key);
            if (pool == null) {
                GameObject child = new GameObject();
                child.name = key;
                child.transform.SetParent(transform, false);
                child.transform.localPosition = Vector3.zero;
                pool = child.AddComponent<GameObjectPool>();
                pool.Prefab = createPrefab();
                pool.MaxSize = maxSize;
                pool.UpdateParent = updateParent;
                pool.SetActive = setActive;
                pool.DebugMode = this.DebugMode;
                if (onPoolAdded != null) {
                    onPoolAdded(key, pool);
                }
                _Pools[key] = pool;
                Info("<GameObjectPool> Created: [{0}] maxSize = {1}, updateParent = {2}", key, maxSize, updateParent);
            }
            return pool;
        }

        protected void ClearPools() {
            foreach (var kv in _Pools) {
                GameObjectUtil.Destroy(kv.Value.gameObject);
            }
            _Pools.Clear();
            OnClear();
        }

        protected virtual void OnClear() {}
    }
}
