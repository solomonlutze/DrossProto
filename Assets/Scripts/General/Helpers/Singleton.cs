using UnityEngine;

/// <summary>
/// Inherit from this base class to create a singleton.
/// e.g. public class MyClassName : Singleton<MyClassName> {}
/// </summary>

// Extremely borrowed from http://wiki.unity3d.com/index.php/Singleton

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
  // Check to see if we're about to be destroyed.
  public static bool m_ShuttingDown = false;
  private static object m_Lock = new object();
  private static T m_Instance;

  /// <summary>
  /// Access singleton instance through this propriety.
  /// </summary>
  public static T Instance
  {
    get
    {
      // m_ShuttingDown = false;
      // if (m_ShuttingDown)
      // {
      //   Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
      //       "' already destroyed. Returning null.");
      //   return null; // yolo
      // }

      lock (m_Lock)
      {
        if (m_Instance == null)
        {
          // Search for existing inst
          // Debug.Log("singleton  - instance null");
          m_Instance = (T)FindObjectOfType(typeof(T));

          // Debug.Log("singleton  - searched for instance?");
          // Create new instance if one doesn't already exist.

          if (m_Instance == null)
          {
            // Debug.Log("singleton  - instance still null!!");
            // Need to create a new GameObject to attach the singleton to.
            var singletonObject = new GameObject();
            m_Instance = singletonObject.AddComponent<T>();
            singletonObject.name = typeof(T).ToString() + " (Singleton)";

            // Make instance persistent.
            Debug.Log("making instance persistent " + singletonObject);
            DontDestroyOnLoad(singletonObject);
          }
          // else
          // {
          //   Debug.Log("there was an instance I guess??");
          //   DontDestroyOnLoad(m_Instance.gameObject);
          // }
        }

        return m_Instance;
      }
    }
  }


  // private void OnApplicationQuit()
  // {
  //   m_ShuttingDown = true;
  // }


  private void OnDestroy()
  {
    Debug.Log("destroying " + gameObject.name + " because ????");
    m_ShuttingDown = true;
  }
}