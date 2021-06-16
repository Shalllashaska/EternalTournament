using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
public class Gun : ScriptableObject
{
    public int typeOfGun;
    public string nameOfGun;
    public int damage;
    public float reloadTime;
    public int ammo;
    public int clipsize;
    public float fireRate;
    public float recoil;
    public float bloom;
    public float bloomWhenIsAiming;
    public float kickback;
    public float aimSpeed;
    public float bulletSpeed;
    public GameObject prefabGun;
    public GameObject prefabBullet;
    public GameObject prefabBulletImpact;
    public Sprite prefabCrosshair;
    public bool Aiming;
    public bool allowButtonHold;
    public bool isMIne;

    public AudioClip gunShotSound;
    public AudioClip reloadSound;
    public float pitchRand;
    public float volumeShot;
    public float volumeReload;


    private int stash;
    private int clip;
    
    public void Initialize ()
    {
        stash = ammo;
        clip = clipsize;
    }

    public bool FireBullet ()
    {
        if (clip > 0)
        {
            clip -= 1;
            return true;
        }
        else return false;
    }

    public void Reload ()
    {
        stash += clip;
        clip = Mathf.Min(clipsize, stash);
        stash -= clip;
    }

    public int GetStash() { return stash; }
    public int GetClip() { return clip; }
    public void SetStash(int stashValue) { if(isMIne) stash += stashValue; }
}
