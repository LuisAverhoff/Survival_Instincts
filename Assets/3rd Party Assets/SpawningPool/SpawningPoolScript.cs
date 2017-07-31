//
// Spawning Pool for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// http://www.digitalruby.com
//

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR


#endif

namespace DigitalRuby.Pooling
{
    /// <summary>
    /// This script only needs to be in the first object in your scene and is an alternative way to setup prefabs in the inspector, rather than code.
    /// </summary>
    public class SpawningPoolScript : MonoBehaviour
    {
        private static SpawningPoolScript instance;
        public static SpawningPoolScript Instance { get { return instance; } }

        private bool awake;

        [System.Serializable]
        public struct SpawningPoolEntry
        {
            public string Key;
            public GameObject Prefab;
        }

        /// <summary>
        /// Dictionary of prefabs
        /// </summary>
        [Tooltip("List of prefabs to add to the spawning pool upon start")]
        public SpawningPoolEntry[] Prefabs;

        private void Awake()
        {
            if (awake)
            {
                return;
            }
            else if (instance == null)
            {
                awake = true;
                instance = this;
                DontDestroyOnLoad(this);
                if (Prefabs != null)
                {
                    foreach (var prefab in Prefabs)
                    {
                        SpawningPool.AddPrefab(prefab.Key, prefab.Prefab);
                    }
                }
				UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => 
				{
					SpawningPool.RecycleActiveObjects();
				};
            }
            else
            {
                Object.Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Contains methods to assist in object caching / pooling
    /// </summary>
    public static class SpawningPool
    {
        private static readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, List<GameObject>> cachedKeysAndGameObjects = new Dictionary<string, List<GameObject>>();
        private static readonly Dictionary<GameObject, string> activeGameObjectsAndKeys = new Dictionary<GameObject, string>();
        private static readonly Dictionary<string, List<GameObject>> activeKeysAndGameObjects = new Dictionary<string, List<GameObject>>();
        private static int cacheCount;

        /// <summary>
        /// The hide flags to apply to newly created objects
        /// </summary>
        public static HideFlags DefaultHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

        private static List<GameObject> GetOrCreateCacheList(string key)
        {
            List<GameObject> list;
            if (!cachedKeysAndGameObjects.TryGetValue(key, out list))
            {
                list = new List<GameObject>();
                cachedKeysAndGameObjects[key] = list;
            }

            return list;
        }

        private static void ActivateObject(GameObject obj, string key)
        {
            obj.SetActive(true);
            activeGameObjectsAndKeys[obj] = key;
            List<GameObject> list;
            if (!activeKeysAndGameObjects.TryGetValue(key, out list))
            {
                list = new List<GameObject>();
                activeKeysAndGameObjects[key] = list;
            }
            list.Add(obj);
        }

        /// <summary>
        /// Add (or replace) a prefab with a key and source object
        /// </summary>
        /// <param name="key">Unique key</param>
        /// <param name="prefab">Source object</param>
        public static void AddPrefab(string key, GameObject prefab)
        {
            GameObject.DontDestroyOnLoad(prefab);
            prefabs[key] = prefab;
        }

        /// <summary>
        /// Remove a prefab, all cached objects and active objects
        /// </summary>
        /// <param name="key">Unique key to remove</param>
        public static void RemovePrefab(string key)
        {
            if (prefabs.Remove(key))
            {
                List<GameObject> list = GetOrCreateCacheList(key);
                foreach (GameObject obj in list)
                {
                    Object.Destroy(obj);
                }
                cacheCount -= list.Count;
                cachedKeysAndGameObjects.Remove(key);
                if (activeKeysAndGameObjects.TryGetValue(key, out list))
                {
                    activeKeysAndGameObjects.Remove(key);
                    foreach (GameObject obj in list)
                    {
                        activeGameObjectsAndKeys.Remove(obj);
                        Object.Destroy(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Removes all prefabs and cached objects from memory
        /// </summary>
        public static void RemoveAllPrefabs()
        {
            foreach (string key in prefabs.Keys)
            {
                RemovePrefab(key);
            }
            prefabs.Clear();
        }

        /// <summary>
        /// Pull an object from the cache, or create new if not found in cache
        /// </summary>
        /// <param name="key">Unique key</param>
        /// <returns>Instance of the prefab for the specified key, or null if error creating</returns>
        public static GameObject CreateFromCache(string key, Vector3 hitpoint, Quaternion rotationPoint)
        {
            List<GameObject> list = GetOrCreateCacheList(key);
            GameObject pooledObject;
            string message;
            if (list.Count == 0)
            {
                GameObject prefab;
                if (!prefabs.TryGetValue(key, out prefab))
                {
                    return null;
                }
                pooledObject = GameObject.Instantiate(prefab);
                GameObject.DontDestroyOnLoad(pooledObject);
                pooledObject.hideFlags = DefaultHideFlags;
                message = "SpawningPoolInstantiated";
            }
            else
            {
                int index = list.Count - 1;
                pooledObject = list[index];
                list.RemoveAt(index);
                cacheCount--;
                message = "SpawningPoolRetrievedFromCache";
            }

            ActivateObject(pooledObject, key);
            pooledObject.transform.position = hitpoint;
            pooledObject.transform.rotation = rotationPoint;
            pooledObject.SendMessage(message, pooledObject, SendMessageOptions.DontRequireReceiver);

            return pooledObject;
        }

        /// <summary>
        /// Returns an object to the cache
        /// </summary>
        /// <param name="pooledObject">The pooled object that was created</param>
        /// <returns>True if recycled back into the pool, false if error</returns>
        public static bool ReturnToCache(this GameObject pooledObject)
        {
            string key;
            if (!activeGameObjectsAndKeys.TryGetValue(pooledObject, out key))
            {
                return false;
            }
            return ReturnToCache(pooledObject, key);
        }

        /// <summary>
        /// Returns an object to the cache
        /// </summary>
        /// <param name="pooledObject">The pooled object that was created</param>
        /// <param name="key">The key for the object</param>
        /// <returns>True if recycled back into the pool, false if error</returns>
        public static bool ReturnToCache(this GameObject pooledObject, string key)
        {
            List<GameObject> list;
            if (!cachedKeysAndGameObjects.TryGetValue(key, out list))
            {
                return false;
            }
            list.Add(pooledObject);
            activeGameObjectsAndKeys.Remove(pooledObject);
            if (activeKeysAndGameObjects.TryGetValue(key, out list))
            {
                list.Remove(pooledObject);
            }
            pooledObject.SendMessage("SpawningPoolReturnedToCache", pooledObject, SendMessageOptions.DontRequireReceiver);
            pooledObject.SetActive(false);
            cacheCount++;

            return true;
        }

        /// <summary>
        /// Enumerate all active objects that have been pulled from cache or created new using the CreateFromCache method
        /// </summary>
        /// <returns>Enumerable of active objects</returns>
        public static IEnumerable<KeyValuePair<GameObject, string>> EnumerateActiveObjects()
        {
            foreach (var v in activeGameObjectsAndKeys)
            {
                yield return v;
            }
        }

        /// <summary>
        /// Recycle all active objects and return them to the cache
        /// </summary>
        public static void RecycleActiveObjects()
        {
            foreach (var v in activeGameObjectsAndKeys)
            {
                List<GameObject> list;
                if (cachedKeysAndGameObjects.TryGetValue(v.Value, out list))
                {
                    list.Add(v.Key);
                    v.Key.SendMessage("SpawningPoolReturnedToCache", v.Key, SendMessageOptions.DontRequireReceiver);
                    v.Key.SetActive(false);
                    cacheCount++;
                }
            }
            activeGameObjectsAndKeys.Clear();
            activeKeysAndGameObjects.Clear();
        }

        /// <summary>
        /// Clear out all caches, active objects and frees all memory. All active and cached objects are destroyed. Does not remove prefabs.
        /// </summary>
        public static void Clear()
        {
            foreach (var v in activeGameObjectsAndKeys)
            {
                Object.Destroy(v.Key);
            }
            foreach (var v in cachedKeysAndGameObjects)
            {
                foreach (var vv in v.Value)
                {
                    Object.Destroy(vv);
                }
            }
            activeGameObjectsAndKeys.Clear();
            activeKeysAndGameObjects.Clear();
            cachedKeysAndGameObjects.Clear();
            cacheCount = 0;
        }

        /// <summary>
        /// Get the number of active objects for a specific key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Number of active objects for that key</returns>
        public static int ActiveCountForKey(string key)
        {
            List<GameObject> list;
            if (activeKeysAndGameObjects.TryGetValue(key, out list))
            {
                return list.Count;
            }
            return 0;
        }

        /// <summary>
        /// Gets the number of cached objects. Does not include active objects.
        /// </summary>
        public static int CacheCount
        {
            get { return cacheCount; }
        }

        /// <summary>
        /// Gets the number of active objects. Does not include cached objects.
        /// </summary>
        public static int ActiveCount
        {
            get { return activeGameObjectsAndKeys.Count; }
        }
    }
}