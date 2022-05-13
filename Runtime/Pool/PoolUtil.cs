using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Edger.Unity;

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

        public void Release(GameObject go, UnityEngine.Object caller = null) {
            var data = go.GetComponent<InPoolData>();
            if (data == null) {
                Log.ErrorFrom(caller == null ? go : caller,
                    "{0}Release() InPoolData Not Found: {1}", LogPrefix, go.name);
                return;
            }
            var pool = GetPool(data.PoolKey);
            if (pool == null) {
                Log.ErrorFrom(caller == null ? go : caller,
                    "{0}Release() Pool Not Found: {1} -> {2}", LogPrefix, go.name, data.PoolKey);
                return;
            }
            pool.Release(go, caller);
        }
    }
}
