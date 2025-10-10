using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class TerrainWave_WaveJump : MonoBehaviour
{
    public GameObject Collider;
    //穏やかな波
    private float baseWaveSpeed = 1.5f;
    private float baseWaveHeight = 0.003f;
    private float baseWaveFrequency = 0.1f;
    //大きな波
    private float pulseSpeed = 65.0f;
    private float pulseHeight = 0.010f;
    private float pulseWidth = 10.0f;
    private float pulseDuration = 6.0f;
    // 使用中のテライン
    private Terrain terrain;
    // テラインのデータ（高さ情報など）
    private TerrainData terrainData;
    // 高さマップの解像度
    private int heightmapResolution;
    //更新させる高さ情報
    float[,] heights;
    // 時間の蓄積（波の進行に使用）
    private float baseTime;
    //中心からの距離一覧
    float[,] distFromCenter;
    //複数のパルスを記録する構造体とリスト
    public class Pulse
    {
        //開始時間の保存先
        public float startTime;
        //コライダー用のオブジェクトの保存先
        public GameObject Collider;
        //コンストラクタ
        public Pulse(float _startTime, GameObject _collider)
        {
            this.startTime = _startTime;
            this.Collider = _collider;
        }
    }
    private List<Pulse> activePulses = new List<Pulse>();
    void Awake()
    {
        // 初期時間リセット
        baseTime = 0.0f;
        // シーン内のテラインを取得
        terrain = Terrain.activeTerrain;
        // テラインのデータを取得
        terrainData = terrain.terrainData;
        // 解像度取得
        heightmapResolution = terrainData.heightmapResolution;
        //フィールドを更新させる高さ
        heights = new float[heightmapResolution, heightmapResolution];
        //フィールドのリセットする高さ
        float[,] flatHeights = new float[heightmapResolution, heightmapResolution];
        //フィールドの中心からの距離
        distFromCenter = new float[heightmapResolution, heightmapResolution];
        {
            int _center = heightmapResolution / 2;
            for (int y = 0; y < heightmapResolution; y++)
            {
                for (int x = 0; x < heightmapResolution; x++)
                {
                    //全てに0を代入
                    flatHeights[y, x] = 0.0f;
                    //中心からの距離をあらかじめ計算
                    float dx = x - _center;
                    float dy = y - _center;
                    distFromCenter[y, x] = Mathf.Sqrt(dx * dx + dy * dy);
                }
            }
        }
        terrainData.SetHeights(0, 0, flatHeights);
    }
    void Update()
    {
        //クリックで波追加
        if (Input.GetMouseButtonDown(0))
        {
            SetPulses();
        }
        baseTime += Time.deltaTime;
        int center = heightmapResolution / 2;
        //全ポリゴン分回る
        for (int y = 0; y < heightmapResolution; y++)
        {
            for (int x = 0; x < heightmapResolution; x++)
            {
                //穏やかな波の生成
                {
                    float wave = Mathf.Sin(distFromCenter[y,x] * baseWaveFrequency - baseTime * baseWaveSpeed) * baseWaveHeight;
                    heights[y, x] = wave;
                }
            }
        }
        //全パルスを反映(消えた時のため逆順で)
        for (int i = activePulses.Count - 1; i >= 0; i--)
        {
            float elapsed = Time.time - activePulses[i].startTime;
            //削除
            if (elapsed > pulseDuration)
            {
                //コライダーを削除
                Destroy(activePulses[i].Collider);
                //Pulseを削除
                activePulses.RemoveAt(i);
                continue;
            }
            float currentRadius = elapsed * pulseSpeed;

            float _maxHeights = 0.0f;
            for (int y = 0; y < heightmapResolution; y++)
            {
                for (int x = 0; x < heightmapResolution; x++)
                {
                    //現在の波の中心との距離差
                    float delta = distFromCenter[y, x] - currentRadius;
                    //盛り上がり
                    float pulse = Mathf.Exp(-delta * delta / (2 * pulseWidth * pulseWidth)) * pulseHeight;
                    //今の高さに加算
                    heights[y, x] += pulse;
                    _maxHeights = (heights[y,x]>_maxHeights)? heights[y,x]:_maxHeights;
                }
            }
            // コライダーを拡大
            float diameter = currentRadius * 0.12f;
            activePulses[i].Collider.transform.localScale = new Vector3(diameter,_maxHeights*2700f, diameter);
        }
        terrainData.SetHeights(0, 0, heights);
    }
    public void SetPulses()
    {
        //開始時間の保存とコライダーの生成
        activePulses.Add(new Pulse(Time.time, Instantiate(Collider)));
        activePulses[activePulses.Count - 1].Collider.SetActive(true);
        // 中心座標（地形の中心を基準）
        Vector3 centerPos = terrain.transform.position + new Vector3(terrainData.size.x * 0.5f, 0f, terrainData.size.z * 0.5f);
        activePulses[activePulses.Count - 1].Collider.transform.position = centerPos;
    }
}