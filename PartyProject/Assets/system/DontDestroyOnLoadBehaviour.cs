using UnityEngine;


// Žg‚¢•û : AddComponent‚ð‚·‚é
public class DontDestroyOnLoadBehaviour : MonoBehaviour
{
    private  void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}