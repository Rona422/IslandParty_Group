using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : SingletonMonoBehaviour<SceneController>
{
    public BaseScene currentScene;

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = 60;
    }
    public void SceneChange(string _SceneName)
    {
        SceneManager.LoadScene(_SceneName);
    }
}
