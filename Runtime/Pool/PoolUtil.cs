using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Edger.Unity;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

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

#if ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
#endif
        public int PoolCount {
            get {
                return _Pools.Count;
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
                ErrorFrom(go, "Release() Pool Not Found: [{0}] {1}", poolKey, go.name);
                return false;
            }
            pool.Release(go, go);
            return true;
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
                InfoFrom(pool, "Pool Created: {0}maxSize = {1}, updateParent = {2}", pool.LogPrefix, maxSize, updateParent);
            }
            return pool;
        }

        public void Release(GameObject go, UnityEngine.Object caller = null) {
            var data = go.GetComponent<InPoolData>();
            if (data == null) {
                ErrorFrom(caller == null ? go : caller,
                    "Release() InPoolData Not Found: {0}", go.name);
                return;
            }
            var pool = GetPool(data.PoolKey);
            if (pool == null) {
                ErrorFrom(caller == null ? go : caller,
                    "Release() Pool Not Found: {0} -> {1}", go.name, data.PoolKey);
                return;
            }
            pool.Release(go, caller);
        }

#if ODIN_INSPECTOR
        [Button(ButtonSizes.Large)]
#endif
        public void ReleaseUnused(Func<GameObjectPool, bool> shouldKeep = null) {
            foreach (var kv in _Pools) {
                bool keep = shouldKeep == null ? false : shouldKeep(kv.Value);
                if (!keep) {
                    kv.Value.ReleaseUnused();
                }
            }
        }

#if ODIN_INSPECTOR
        [Button(ButtonSizes.Large)]
#endif
        public void DestroyUnused(Func<GameObjectPool, bool> shouldKeep = null) {
            List<GameObjectPool> unusedPools = null;
            foreach (var kv in _Pools) {
                if (kv.Value.Data.TakenCount == 0 && kv.Value.UnusedCount == 0) {
                    bool keep = shouldKeep == null ? false : shouldKeep(kv.Value);
                    if (!keep) {
                        if (unusedPools == null) {
                            unusedPools = new List<GameObjectPool>();
                        }
                        unusedPools.Add(kv.Value);
                    }
                }
            }
            if (unusedPools != null) {
                for (int i = 0; i < unusedPools.Count; i++) {
                    var pool = unusedPools[i];
                    _Pools.Remove(pool.PoolKey);
                    InfoFrom(pool, "Unused Pool Destroyed: {0}", pool.LogPrefix);
                    GameObjectUtil.Destroy(pool.gameObject);
                }
            }
        }

#if ODIN_INSPECTOR
        [Button(ButtonSizes.Large)]
#endif
        public void ReleaseAndDestroyUnused(Func<GameObjectPool, bool> shouldKeep = null) {
            ReleaseUnused(shouldKeep);
            DestroyUnused(shouldKeep);
        }

        protected void ClearPools() {
            foreach (var kv in _Pools) {
                var pool = kv.Value;
                InfoFrom(pool, "Pool Destroyed: {0}: TakenCount = {0}, UnusedCount = {1}", pool.LogPrefix, pool.Data.TakenCount, pool.UnusedCount);
                GameObjectUtil.Destroy(pool.gameObject);
            }
            _Pools.Clear();
            OnClear();
        }

        protected virtual void OnClear() {}
    }
}
