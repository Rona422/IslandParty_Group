using UnityEngine;


// �g���� : AddComponent������
public class DontDestroyOnLoadBehaviour : MonoBehaviour
{
    private  void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}