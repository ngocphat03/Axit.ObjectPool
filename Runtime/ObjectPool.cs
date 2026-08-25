namespace AXitUnityTemplate.ObjectPool
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Minimal, static object pool. Callers always provide the prefab.
    /// </summary>
    public static class ObjectPool
    {
        private const int DefaultCapacity = 10;

        private static readonly Dictionary<GameObject, Pool> Pools = new();
        private static readonly Dictionary<GameObject, Pool> Owners = new();
        private static Transform root;

        public static GameObject Spawn(
            GameObject prefab,
            Transform parent = null,
            Vector3 position = default,
            Quaternion rotation = default)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            if (rotation == default)
            {
                rotation = Quaternion.identity;
            }

            var pool = GetOrCreatePool(prefab);
            var instance = pool.Get();

            var instanceTransform = instance.transform;
            instanceTransform.SetParent(parent, false);
            instanceTransform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);

            Owners[instance] = pool;
            return instance;
        }

        public static T Spawn<T>(
            T prefab,
            Transform parent = null,
            Vector3 position = default,
            Quaternion rotation = default)
            where T : Component
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            return Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
        }

        public static void Recycle(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (!Owners.TryGetValue(instance, out var pool))
            {
                Debug.LogWarning($"[ObjectPool] '{instance.name}' was not spawned by this pool.");
                return;
            }

            Owners.Remove(instance);
            instance.SetActive(false);
            instance.transform.SetParent(Root, false);
            pool.Release(instance);
        }

        public static void Recycle(Component instance)
        {
            if (instance != null)
            {
                Recycle(instance.gameObject);
            }
        }

        public static void Prewarm(GameObject prefab, int count = DefaultCapacity)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            GetOrCreatePool(prefab).EnsureCapacity(count);
        }

        public static void Clear(GameObject prefab)
        {
            if (prefab == null || !Pools.TryGetValue(prefab, out var pool))
            {
                return;
            }

            pool.DestroyAll(Owners);
            Pools.Remove(prefab);
        }

        public static void ClearAll()
        {
            foreach (var pool in Pools.Values)
            {
                pool.DestroyAll(Owners);
            }

            Pools.Clear();
            Owners.Clear();

            if (root != null)
            {
                Object.Destroy(root.gameObject);
                root = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            Pools.Clear();
            Owners.Clear();
            root = null;
        }

        private static Pool GetOrCreatePool(GameObject prefab)
        {
            if (!Pools.TryGetValue(prefab, out var pool))
            {
                pool = new Pool(prefab);
                Pools.Add(prefab, pool);
                pool.EnsureCapacity(DefaultCapacity);
            }

            return pool;
        }

        private static Transform Root
        {
            get
            {
                if (root != null)
                {
                    return root;
                }

                var rootObject = new GameObject("[Axit.ObjectPool]");
                Object.DontDestroyOnLoad(rootObject);
                root = rootObject.transform;
                return root;
            }
        }

        private sealed class Pool
        {
            private readonly GameObject prefab;
            private readonly Stack<GameObject> inactive = new();
            private readonly HashSet<GameObject> instances = new();

            public Pool(GameObject prefab)
            {
                this.prefab = prefab;
            }

            public GameObject Get()
            {
                GameObject instance;

                while (inactive.Count > 0)
                {
                    instance = inactive.Pop();
                    if (instance != null)
                    {
                        return instance;
                    }
                }

                instance = Object.Instantiate(prefab, Root);
                instance.SetActive(false);
                instances.Add(instance);
                return instance;
            }

            public void Release(GameObject instance)
            {
                inactive.Push(instance);
            }

            public void EnsureCapacity(int count)
            {
                while (instances.Count < count)
                {
                    var instance = Object.Instantiate(prefab, Root);
                    instance.SetActive(false);
                    instances.Add(instance);
                    inactive.Push(instance);
                }
            }

            public void DestroyAll(Dictionary<GameObject, Pool> owners)
            {
                foreach (var instance in instances)
                {
                    if (instance != null)
                    {
                        owners.Remove(instance);
                        Object.Destroy(instance);
                    }
                }

                inactive.Clear();
                instances.Clear();
            }
        }
    }
}
