using UnityEngine;

public class Wave_Sugoroku : MonoBehaviour
{
    // 穏やかな波のパラメータ
    [SerializeField]
    private float baseWaveSpeed = 1.5f;
    [SerializeField]
    private float baseWaveHeight = 0.03f;
    [SerializeField]
    private float baseWaveFrequency = 0.1f;

    private Terrain terrain;
    private TerrainData terrainData;
    private int heightmapResolution;
    private float[,] heights;
    private float baseTime;
    private float[,] distFromCenter;
    void Awake()
    {
        baseTime = 0.0f;
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        heightmapResolution = terrainData.heightmapResolution;
        heights = new float[heightmapResolution, heightmapResolution];
        float[,] flatHeights = new float[heightmapResolution, heightmapResolution];
        distFromCenter = new float[heightmapResolution, heightmapResolution];
        int center = heightmapResolution / 2;

        for (int y = 0; y < heightmapResolution; y++)
        {
            for (int x = 0; x < heightmapResolution; x++)
            {
                flatHeights[y, x] = 0.0f;
                float dx = x - center;
                float dy = y - center;
                distFromCenter[y, x] = Mathf.Sqrt(dx * dx + dy * dy);
            }
        }
        terrainData.SetHeights(0, 0, flatHeights);
    }
    void Update()
    {
        baseTime += Time.deltaTime;
        int center = heightmapResolution / 2;
        for (int y = 0; y < heightmapResolution; y++)
        {
            for (int x = 0; x < heightmapResolution; x++)
            {
                // 穏やかな波の計算
                float wave = Mathf.Sin(distFromCenter[y, x] * baseWaveFrequency - baseTime * baseWaveSpeed) * baseWaveHeight;
                heights[y, x] = wave;
            }
        }
        terrainData.SetHeights(0, 0, heights);
    }
}