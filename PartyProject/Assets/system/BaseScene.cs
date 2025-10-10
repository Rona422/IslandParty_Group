using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseScene : MonoBehaviour
{
    protected void Awake()
    {
        SceneController.instance.currentScene = this;
    }
}
