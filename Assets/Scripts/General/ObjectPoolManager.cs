using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable
{
  void Clear();
}
public class ObjectPool<T> where T : MonoBehaviour, IPoolable
{

  int currentIndex;
  T defaultObject;
  List<T> pool;

  public ObjectPool(T defaultObj)
  {
    pool = new List<T>();
    defaultObject = defaultObj;
  }

  public IEnumerator Populate(int quantity)
  {
    System.Diagnostics.Stopwatch stopwatch1 = new System.Diagnostics.Stopwatch();
    System.Diagnostics.Stopwatch stopwatch2 = new System.Diagnostics.Stopwatch();
    stopwatch1.Start();
    stopwatch2.Start();
    int objectCount = 0;
    T obj;
    while (pool.Count < quantity)
    {
      obj = GameObject.Instantiate(defaultObject);
      obj.gameObject.SetActive(false);
      pool.Add(obj);
      objectCount++;
      if (stopwatch2.ElapsedMilliseconds > .5f)
      {
        objectCount = 0;
        yield return null;
        stopwatch2.Restart();
      }
    }
    yield break;
  }

  public void PopulateInstant(int quantity)
  {
    T obj;
    while (pool.Count < quantity)
    {
      obj = GameObject.Instantiate(defaultObject);
      obj.gameObject.SetActive(false);
      pool.Add(obj);
    }
  }
  public T GetObject()
  {
    if (pool.Count == 0)
    {
      PopulateInstant(30);
    }
    while (currentIndex < pool.Count)
    {
      currentIndex++;
      if (!pool[currentIndex - 1].gameObject.activeInHierarchy)
      {
        pool[currentIndex - 1].gameObject.SetActive(true);
        return pool[currentIndex - 1];
      }
    }
    if (!pool[0].gameObject.activeInHierarchy)
    {
      currentIndex = 0;
      return pool[0];
    }
    else
    {
      T objectInstance = GameObject.Instantiate(defaultObject);
      pool.Add(objectInstance);
      Debug.LogWarning("WARNING: wall object pool size exceeded! benefits of pooling lost!");
      return objectInstance;
    }
  }
  public void Release(T objectToRelease)
  {
    objectToRelease.Clear();
    objectToRelease.gameObject.SetActive(false);
  }
  public void ReleaseAll()
  {
    foreach (T obj in pool)
    {
      obj.gameObject.SetActive(false);
    }
    currentIndex = 0;
  }
  public void Clear()
  {
    foreach (T obj in pool)
    {
      GameObject.DestroyImmediate(obj.gameObject);
    }
    pool.Clear();
    currentIndex = 0;
  }
}
public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
  public WallObject defaultWallObject;
  public ObjectPool<WallObject> wallObjectPool;

  void Awake()
  {
    wallObjectPool = new ObjectPool<WallObject>(defaultWallObject);
  }
  public ObjectPool<WallObject> GetWallObjectPool()
  {
    if (wallObjectPool == null)
    {
      wallObjectPool = new ObjectPool<WallObject>(defaultWallObject);
    }
    return wallObjectPool;
  }
}