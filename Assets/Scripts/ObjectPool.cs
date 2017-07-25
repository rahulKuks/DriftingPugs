using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    [Serializable]
    public class ObjectPoolItem
    {
        public string identifier;
        public GameObject pooledItem;
        public int pooledCount;
    }

    // Singleton pattern
    private static ObjectPool _instance;
    public static ObjectPool Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<ObjectPool>();
            }
            return _instance;
        }
    }

    [SerializeField]
    private List<ObjectPoolItem> itemsToPool;
    private Dictionary<string, List<GameObject>> pooledObjects;

    private void Awake()
    {
        _instance = this;
        pooledObjects = new Dictionary<string, List<GameObject>>();
    }

    private void Start()
    {
        // Create variables
        List<GameObject> objectList;
        GameObject obj;

        // Instantiate all pooled items
        foreach (ObjectPoolItem i in itemsToPool)
        {
            objectList = new List<GameObject>(i.pooledCount);
            for (int x = 0; x < i.pooledCount; x++)
            {
                obj = (GameObject)Instantiate(i.pooledItem);
                obj.SetActive(false);
                obj.transform.SetParent(this.transform);
                objectList.Add(obj);
            }
            pooledObjects.Add(i.identifier, objectList);
        }
    }

    public GameObject GetPooledObject(string identifier)
    {
        // Check if there object wanted is pooled
        if (!pooledObjects.ContainsKey(identifier))
            return null;
        // Return an object that is not in use
        foreach (GameObject obj in pooledObjects[identifier])
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        // If can't find one, instantiate a new one
        ObjectPoolItem poolItem = new ObjectPoolItem();
        foreach (ObjectPoolItem item in itemsToPool)
        {
            if (item.identifier == identifier)
            {
                poolItem = item;
                break;
            }
        }
        GameObject newObject = (GameObject)Instantiate(poolItem.pooledItem);
        newObject.SetActive(false);
        pooledObjects[identifier].Add(newObject);
        return newObject;
    }

    public void Destroy(GameObject go)
    {
        go.transform.SetParent(this.transform);
        go.SetActive(false);
    }
}