using UnityEngine;
namespace FpsGame.Gun
{
    public enum GunFireMode { Single, Full_Auto, Burst_3_Round }
    public class Gun
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
        public bool isSingleReload;

        public virtual void Shot()
        {

        }
        public virtual void Reload()
        {

        }
        public virtual void Aim()
        {

        }
    }
}