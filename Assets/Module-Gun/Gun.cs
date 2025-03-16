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
        public float currentBullet;
        public float magazineSize;
        public float reloadTime;
        public float recoil;
        public float aimSpeed;
        public float effectiveRange;
        public GunFireMode fireMode;
        public float noise;
        public bool oneRoundReload;

        Coroutine reloading_routine;

        public void Shot()
        {

        }
        [ContextMenu("Reload")]
        public void Reload()
        {
            bool reloading = reloading_routine != null;
            bool magazineFull = currentBullet >= magazineSize;

            if (reloading || magazineFull)
            {
                Debug.Log("Can't reload");
                return;
            }

            reloading_routine = StartCoroutine(Reloading());
        }
        IEnumerator Reloading()
        {
            Debug.Log("Reloading");
            while (true)
            {
                yield return new WaitForSeconds(reloadTime);
                if (oneRoundReload)
                {
                    currentBullet += 1;
                }
                else
                {
                    //Bullet in inventory - (magazineSize - currentBullet)
                    currentBullet = magazineSize;
                }

                if (currentBullet >= magazineSize)
                    break;
            }
            reloading_routine = null;
            Debug.Log("Reload success");
        }
        [ContextMenu("StopReload")]
        public void StopReload()
        {
            if (reloading_routine == null) return;
            StopCoroutine(reloading_routine);
            reloading_routine = null;
        }
        public void Aim()
        {

        }
    }
}