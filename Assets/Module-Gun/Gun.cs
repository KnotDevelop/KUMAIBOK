using System.Collections;
using UnityEngine;
namespace FpsGame.Gun
{
    public enum GunFireMode { Single, Full_Auto, Burst_3_Round }
    public class Gun : MonoBehaviour
    {
        public float damage;
        public float accuracy;
        public float rps;
        public float magazineSize;
        public float reloadTime;
        public float recoil;
        public float aimSpeed;
        public float effectiveRange;
        public GunFireMode fireMode;
        public float noise;
        public bool oneRoundReload;

        private float m_CurrentBullet;
        private Coroutine m_ReloadingRoutine;

        public void Shot()
        {

        }
        [ContextMenu("Reload")]
        public void Reload()
        {
            bool reloading = m_ReloadingRoutine != null;
            bool magazineFull = m_CurrentBullet >= magazineSize;

            if (reloading || magazineFull)
            {
                Debug.Log("Can't reload");
                return;
            }

            m_ReloadingRoutine = StartCoroutine(Reloading());
        }
        IEnumerator Reloading()
        {
            Debug.Log("Reloading");
            while (true)
            {
                yield return new WaitForSeconds(reloadTime);
                if (oneRoundReload)
                {
                    m_CurrentBullet += 1;
                }
                else
                {
                    //Bullet in inventory - (magazineSize - currentBullet)
                    m_CurrentBullet = magazineSize;
                }

                if (m_CurrentBullet >= magazineSize)
                    break;
            }
            m_ReloadingRoutine = null;
            Debug.Log("Reload success");
        }
        [ContextMenu("StopReload")]
        public void StopReload()
        {
            if (m_ReloadingRoutine == null) return;
            StopCoroutine(m_ReloadingRoutine);
            m_ReloadingRoutine = null;
        }
        public void Aim()
        {

        }
    }
}