using System;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualVoid.Util.ObjectPooling
{
    public class ObjectPoolManager : MonoBehaviour
    {
        private static ObjectPoolManager instance;
        private void Awake()
        {
            instance = this;
        }

        public class ObjectPool
        {
            public GameObject prefab;
            public Transform poolHolder;
            public Queue<PooledObjectInstance> pool;

            public ObjectPool(GameObject prefab, Transform poolHolder, Queue<PooledObjectInstance> pool)
            {
                this.prefab = prefab;
                this.poolHolder = poolHolder;
                this.pool = pool;
            }
        }

        [Header("These pools will be created when the game starts. You can also call the CreatePool() method.")]
        public List<InspectorObjectPool> inspectorPools = new List<InspectorObjectPool>();
        public static Dictionary<string, ObjectPool> objectPools = new Dictionary<string, ObjectPool>();

        private void Start()
        {
            foreach (InspectorObjectPool pool in inspectorPools)
            {
                CreatePool(pool);
            }
        }

        private static void CreatePool(InspectorObjectPool pool)
        {
            if (objectPools.ContainsKey(pool.tag))
            {
                Debug.LogWarning($"Tried to create pool with tag '{pool.tag}', but a pool with that tag already exists!");
                return;
            }

            Transform holder = new GameObject(pool.tag + " - Object Pool").transform;
            holder.parent = instance.transform;

            Queue<PooledObjectInstance> objectPool = new Queue<PooledObjectInstance>();

            for (int i = 0; i < pool.numToSpawn; i++)
            {
                PooledObjectInstance obj = new PooledObjectInstance(Instantiate(pool.prefab, holder));
                objectPool.Enqueue(obj);
            }

            objectPools.Add(pool.tag, new ObjectPool(pool.prefab, holder, objectPool));
        }

        public static GameObject GetObject(string tag)
        {
            if (objectPools.TryGetValue(tag, out ObjectPool pool))
            {
                PooledObjectInstance obj = pool.pool.Dequeue();
                pool.pool.Enqueue(obj);

                obj.Spawn();
                return obj.gameObject;
            }
            else
            {
                Debug.LogWarning($"Tried to get object from pool '{tag}', but pool does not exist! Call the CreatePool() method?");
            }

            return null;
        }

        public static GameObject GetObject(string tag, Vector3 position, Quaternion rotation)
        {
            if (objectPools.TryGetValue(tag, out ObjectPool pool))
            {
                PooledObjectInstance obj = pool.pool.Dequeue();
                pool.pool.Enqueue(obj);

                obj.Spawn(position, rotation);
                return obj.gameObject;
            }
            else
            {
                Debug.LogWarning($"Tried to get object from pool '{tag}', but pool does not exist! Call the CreatePool() method?");
            }

            return null;
        }

        public static void IncreasePoolSize(string tag, int numAdditionalObjects = 1)
        {
            if (numAdditionalObjects <= 0) return;

            if (objectPools.TryGetValue(tag, out ObjectPool pool))
            {
                for (int i = 0; i < numAdditionalObjects; i++)
                {
                    PooledObjectInstance obj = new PooledObjectInstance(Instantiate(pool.prefab, pool.poolHolder));
                    pool.pool.Enqueue(obj);
                }
            }
            else
            {
                Debug.LogWarning($"Tried to increase size of pool '{tag}', but pool does not exist! Call the CreatePool() method?");
            }
        }

        public static void DecreasePoolSize(string tag, int numObjectsToDelete = 1)
        {
            if (numObjectsToDelete <= 0) return;

            if (objectPools.TryGetValue(tag, out ObjectPool pool))
            {
                if (numObjectsToDelete > pool.pool.Count)
                {
                    Debug.LogWarning($"Tried to remove more items from pool than the pools entire size! \n-Pool: {tag}\n-Pool size: {pool.pool.Count}\n-Num objects requested: {numObjectsToDelete})");
                    numObjectsToDelete = pool.pool.Count;
                }

                for (int i = 0; i < numObjectsToDelete; i++)
                {
                    PooledObjectInstance obj = pool.pool.Dequeue();
                    obj.Destroy();
                }

                if (pool.pool.Count == 0)
                {
                    objectPools.Remove(tag);
                }
            }
            else
            {
                Debug.LogWarning($"Tried to decrease size of pool '{tag}', but pool does not exist! Call the CreatePool() method?");
            }
        }

        public static void CreatePool(string tag, GameObject prefab, int numObjects)
        {
            CreatePool(new InspectorObjectPool(tag, prefab, numObjects));
        }
    }

    [Serializable]
    public class InspectorObjectPool
    {
        [Tooltip("The tag that the objects will be stored under")]
        public string tag;

        [Tooltip("The prefab to spawn copies of")]
        public GameObject prefab;

        [Tooltip("The number of objects to spawn when the game starts")]
        public int numToSpawn;

        public InspectorObjectPool(string tag, GameObject prefab, int numToSpawn)
        {
            this.tag = tag;
            this.prefab = prefab;
            this.numToSpawn = numToSpawn;
        }
    }

    public class PooledObjectInstance
    {
        public GameObject gameObject { get; set; }
        Transform transform;

        bool isPooledObject;
        PoolObject poolObject;

        public PooledObjectInstance(GameObject objectInstance)
        {
            gameObject = objectInstance;
            gameObject.SetActive(false);

            transform = gameObject.transform;

            if (gameObject.TryGetComponent(out poolObject))
            {
                isPooledObject = true;
            }
        }

        public void Spawn()
        {
            if (isPooledObject) poolObject.OnObjectSpawn();

            gameObject.SetActive(true);
        }

        public void Spawn(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;

            Spawn();
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(gameObject);
            transform = null;
            poolObject = null;
        }
    }
}
