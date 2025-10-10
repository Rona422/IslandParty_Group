using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorController_MeteorPanic : MonoBehaviour
{
    [SerializeField] private ParticleSystem m_ExplosionEffectPrefab;

    // 隕石が床に触れたとき壊れる(パーティクル発生)
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name.Contains("Floor"))
        {
            // プレハブを複製して再生
            ParticleSystem explosion = Instantiate(m_ExplosionEffectPrefab, transform.position, Quaternion.identity);
            explosion.Play();
            // パーティクル再生後(duration + パーティクルの寿命の中の最大値)自動的に削除する
            Destroy(explosion.gameObject, explosion.main.duration + explosion.main.startLifetime.constantMax);
            // 隕石を消す
            Destroy(this.gameObject);
        }
    }
}
