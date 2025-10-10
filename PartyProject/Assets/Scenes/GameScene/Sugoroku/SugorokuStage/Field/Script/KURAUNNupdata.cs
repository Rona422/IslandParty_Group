using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KURAUNNupdata : MonoBehaviour
{
    void Update()
    {
        this.transform.localEulerAngles += new Vector3(0.0f, 0.5f, 0.0f);
    }
}
