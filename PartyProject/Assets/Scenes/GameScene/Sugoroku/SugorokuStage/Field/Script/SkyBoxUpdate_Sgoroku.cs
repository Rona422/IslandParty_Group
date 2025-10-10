using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyBoxUpdate_Sgoroku : MonoBehaviour
{
    [SerializeField]
    private List<Transform> m_SkyBoxs = new ();
    // 各オブジェクトの回転速度と軸を保持するための辞書
    private Dictionary<Transform, Vector3> m_RotationAxis = new();
    private Dictionary<Transform, float> m_RotationSpeed = new();
    private Dictionary<Transform, float> m_TimeOffsets = new();
    void Start()
    {
        foreach (Transform t in m_SkyBoxs)
        {
            if (t == null) continue;
            m_RotationAxis[t] = Random.onUnitSphere;
            m_TimeOffsets[t] = Random.Range(0f, 1000f);
        }
    }
    void Update()
    {
        float globalTime = Time.time;
        foreach (Transform t in m_SkyBoxs)
        {
            if (t == null) continue;
            Vector3 baseAxis = m_RotationAxis[t];
            float timeOffset = m_TimeOffsets[t];
            Vector3 noiseAxis = new Vector3(
                Mathf.PerlinNoise(globalTime + timeOffset, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, globalTime + timeOffset) - 0.5f,
                Mathf.PerlinNoise(globalTime + timeOffset, globalTime + timeOffset) - 0.5f
            );
            Vector3 blendedAxis = Vector3.Lerp(baseAxis, noiseAxis.normalized, 0.2f).normalized;
            t.Rotate(blendedAxis * 20.0f * Time.deltaTime);
        }
    }
}
