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
        [SerializeField] Transform muzzle;
        private float m_RpsCooldown = 0;
        [SerializeField]
        private float m_CurrentBullet;
        private Coroutine m_ReloadingRoutine;
        private Coroutine m_ShootRoutine;
        bool m_IsShooting = false;
        private float m_Rps => 1f / rps;
        private bool m_EmptyMagazine => m_CurrentBullet <= 0;

        [ContextMenu("Shot")]
        IEnumerator Shooting_FullAuto()
        {
            while (true)
            {
                if (Time.time > m_RpsCooldown)
                {
                    Debug.Log("FullAutoShoot");
                    HitTargetCheck();
                    m_CurrentBullet -= 1;
                    m_RpsCooldown = Time.time + m_Rps;
                    if (m_EmptyMagazine)
                        StopShooting();
                }
                yield return new WaitForEndOfFrame();
            }
        }
        void Shooting_Brust_3_Round()
        {
            IEnumerator Routine()
            {
                for (int i = 0; i < 3; i++)
                {
                    Debug.Log("Brust_3_RoundShoot" + i);
                    HitTargetCheck();
                    m_CurrentBullet -= 1;
                    if (m_EmptyMagazine)
                    {
                        StopShooting();
                        yield break;
                    }
                    yield return new WaitForSeconds(m_Rps);
                }
                StopShooting();
            }
            m_ShootRoutine = StartCoroutine(Routine());
        }
        void Shooting_Single()
        {
            Debug.Log("SingleShoot");
            HitTargetCheck();
            m_CurrentBullet -= 1;
            StopShooting();
        }
        void StopShooting()
        {
            if (m_ShootRoutine == null) return;
            StopCoroutine(m_ShootRoutine);
            m_ShootRoutine = null;
            m_IsShooting = false;
        }
        void HitTargetCheck()
        {
            Vector3 _muzzle = muzzle.transform.position;
            RaycastHit hit;
            if (Physics.Raycast(_muzzle, muzzle.transform.forward, out hit, 10f))
            {
                Debug.Log($"{gameObject.name} hit {hit.collider.name}");
            }
        }
        public void OnTriggerPull()
        {
            if (m_EmptyMagazine)
            {
                Reload();
                return;
            }

            if (m_IsShooting) return;
            m_IsShooting = true;

            switch (fireMode)
            {
                case GunFireMode.Single:
                    Shooting_Single();
                    break;
                case GunFireMode.Full_Auto:
                    m_ShootRoutine = StartCoroutine(Shooting_FullAuto());
                    break;
                case GunFireMode.Burst_3_Round:
                    Shooting_Brust_3_Round();
                    break;
            }
        }
        public void OnTriggerRelease()
        {
            if (fireMode == GunFireMode.Burst_3_Round) return;
            StopShooting();
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