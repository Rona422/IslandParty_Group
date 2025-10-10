using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorController_MeteorPanic : MonoBehaviour
{
    [SerializeField] private ParticleSystem m_ExplosionEffectPrefab;

    // 覐΂����ɐG�ꂽ�Ƃ�����(�p�[�e�B�N������)
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name.Contains("Floor"))
        {
            // �v���n�u�𕡐����čĐ�
            ParticleSystem explosion = Instantiate(m_ExplosionEffectPrefab, transform.position, Quaternion.identity);
            explosion.Play();
            // �p�[�e�B�N���Đ���(duration + �p�[�e�B�N���̎����̒��̍ő�l)�����I�ɍ폜����
            Destroy(explosion.gameObject, explosion.main.duration + explosion.main.startLifetime.constantMax);
            // 覐΂�����
            Destroy(this.gameObject);
        }
    }
}
