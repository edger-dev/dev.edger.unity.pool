using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Edger.Unity;
using Edger.Unity.Context;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Edger.Unity.Pool {
    public class GameObjectPools : DictAspect<string, GameObjectPool> {
    }

    public class Pools : Env, ISingleton {
        private static Pools _Instance;
        public static Pools Instance { get => Singleton.GetInstance(ref _Instance); }

        public static bool IsApplicationQuiting { get; private set; } = false;

        public AspectReference<GameObjectPools> GameObjectPools { get; private set; }

        protected override void OnAwake() {
            if (Singleton.SetupInstance(ref _Instance, this)) {
                GameObjectPools = CacheAspect<GameObjectPools>();
            }
        }

        public void Start() {
            foreach (Transform child in transform) {
                var pool = child.GetComponent<GameObjectPool>();
                if (pool != null) {
                    GameObjectPools.Target[pool.name] = pool;
                }
            }
        }

        public void OnApplicationQuit() {
            IsApplicationQuiting = true;
        }

        public GameObjectPool GetPool(string key) {
            if (IsApplicationQuiting) return null;

            GameObjectPool pool = null;
            if (GameObjectPools.Target.TryGetValue(key, out pool)) {
                return pool;
            }
            return null;
        }

        public bool Release(string poolKey, GameObject go) {
            if (IsApplicationQuiting) return false;

            var pool = GetPool(poolKey);
            if (pool == null) {
                ErrorFrom(go, "Release() Pool Not Found: [{0}] {1}", poolKey, go.name);
                return false;
            }
            pool.Release(go, go);
            return true;
        }

        public GameObjectPool GetOrAddPool(string key,
                    Func<GameObject> createPrefab,
                    PoolConfig config = null,
                    Action<string, GameObjectPool> onPoolAdded = null) {
            if (IsApplicationQuiting) return null;

            GameObjectPool pool = GetPool(key);
            if (pool == null) {
                GameObject child = new GameObject();
                child.name = key;
                child.transform.SetParent(transform, false);
                child.transform.localPosition = Vector3.zero;
                pool = child.AddComponent<GameObjectPool>();
                pool.Setup(createPrefab(), config);
                pool.DebugMode = this.DebugMode;
                if (onPoolAdded != null) {
                    onPoolAdded(key, pool);
                }
                GameObjectPools.Target[key] = pool;
                InfoFrom(pool, "Pool Created: {0}Config = {1}", pool.LogPrefix, pool.Config.ToString());
            }
            return pool;
        }

        public void Release(GameObject go, UnityEngine.Object caller = null) {
            if (IsApplicationQuiting) return;

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
            if (IsApplicationQuiting) return;

            foreach (var kv in GameObjectPools.Target) {
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
            if (IsApplicationQuiting) return;

            List<GameObjectPool> unusedPools = null;
            foreach (var kv in GameObjectPools.Target) {
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
                    GameObjectPools.Target.Remove(pool.PoolKey);
                    InfoFrom(pool, "Unused Pool Destroyed: {0}", pool.LogPrefix);
                    GameObjectUtil.Destroy(pool.gameObject);
                }
            }
        }

#if ODIN_INSPECTOR
        [Button(ButtonSizes.Large)]
#endif
        public void ReleaseAndDestroyUnused(Func<GameObjectPool, bool> shouldKeep = null) {
            if (IsApplicationQuiting) return;

            ReleaseUnused(shouldKeep);
            DestroyUnused(shouldKeep);
        }

        protected void ClearPools() {
            if (IsApplicationQuiting) return;

            foreach (var kv in GameObjectPools.Target) {
                var pool = kv.Value;
                InfoFrom(pool, "Pool Destroyed: {0}: TakenCount = {0}, UnusedCount = {1}", pool.LogPrefix, pool.Data.TakenCount, pool.UnusedCount);
                GameObjectUtil.Destroy(pool.gameObject);
            }
            GameObjectPools.Target.Clear();
            OnClear();
        }

        protected virtual void OnClear() {}
    }
}
