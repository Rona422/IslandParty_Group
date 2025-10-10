using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using static PlayerManager;
public class PlayerController_WaveJump : MonoBehaviour
{
    public GameObject JumpPlayer;
    public GameObject PulsePlayer;
    public List<GameObject> jumpPlyaers = new ();
    private int num = 0;
    private void Awake()
    {
        for (int i = 0; i < PlayerManager.instance.m_Characters.Count; i++)
        {
            //PlayerManager.characters[i]
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)&&num<3)
        {
            Debug.Log("Active");
            jumpPlyaers[num].SetActive(true);
            num++;
        }        
    }
}
