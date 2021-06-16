using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using Photon.Pun;
using Random = UnityEngine.Random;

public class WeaponMenadger : MonoBehaviourPunCallbacks
{
    #region Variables

    //Assignables
    public Gun[] loadout;
    public Transform weaponParent;
    public Camera fpsCam;
    public LayerMask canBeShot;
    public GameObject BulletImpactPistol;
    public GameObject[] Crosshair;
    public float bulletLifeTime;
    public AudioSource sfx;
    public AudioSource sfxR;
    
    //Privates
    private float currentCooldown;
    private int currentInd = 0;
    private GameObject currentWeapon;
    private GameObject currentBulletPrefab;
    private Transform currentAtackPoint;
    private Transform anchor, stateHip, stateAds;

    private PlayerControl playerControl;
    private float mouseXSens;
    private float mouseYSens;
    
    private bool isReloading;
    private bool buttonToShootAllow;

    private int prefCroshair;
    [HideInInspector]public bool aiming = false;

    #endregion

    #region System methods

    private void Start()
    {
        foreach (Gun g in loadout)
        {
            if(g == null) continue;
            g.Initialize();
        }

        if (photonView.IsMine)
        {
            prefCroshair = 0;
            playerControl = gameObject.GetComponent<PlayerControl>();
            mouseXSens = playerControl.mouseSensitivityX;
            mouseYSens = playerControl.mouseSensitivityY;
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            if(currentWeapon != null)
            {
                //weapon position elasticity 
                        currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero,
                            Time.deltaTime * 4f);
            }
            return;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            MyInput();
        }
    }
    
    

    #endregion

    #region Private Methods

    [PunRPC]
    void Equip(int p_ind)
    {
        if (currentWeapon != null)
        {
            if(isReloading) StopCoroutine(Reload());
            Destroy(currentWeapon);
        }
        GameObject newWeapon =
            Instantiate(loadout[p_ind].prefabGun, weaponParent.position, weaponParent.rotation, weaponParent) as
                GameObject;
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localEulerAngles = Vector3.zero;
        newWeapon.GetComponent<Sway>().isMine = photonView.IsMine;
        currentInd = p_ind;
        loadout[currentInd].isMIne = photonView.IsMine;
        Crosshair[prefCroshair].SetActive(false);
        Crosshair[currentInd + 1].SetActive(true);
        prefCroshair = currentInd + 1;
        currentWeapon = newWeapon;

        if (p_ind == 2 && photonView.IsMine)
        {

            currentWeapon.transform.Find("Anchor/Design/Sniper/SniperScope/Scope1").gameObject.SetActive(true);

        }

        currentAtackPoint = currentWeapon.transform.Find("Anchor/attackPoint");
    }

    void MyInput()
    {

        //Aim
        if (currentWeapon != null)
        {
            if (loadout[currentInd].Aiming)
            {
                aiming = Input.GetMouseButton(1);
                Aim(aiming);
            }

            if (!isReloading)
            {
                buttonToShootAllow = Input.GetKey(KeyCode.Mouse0);
            }
            
            if (buttonToShootAllow && currentCooldown <= 0 &&
                loadout[currentInd].allowButtonHold)
            {
                if (loadout[currentInd].FireBullet())
                {
                    photonView.RPC("Shoot", RpcTarget.All);
                    
                }
                else
                {
                    StartCoroutine(Reload());
                }
            }
            else if (Input.GetKeyDown(KeyCode.Mouse0) && currentCooldown <= 0 &&
                     !loadout[currentInd].allowButtonHold)
            {
                if (loadout[currentInd].FireBullet())
                {
                    photonView.RPC("Shoot", RpcTarget.All);
                }
                else
                {
                    if (!isReloading)
                    {
                        StartCoroutine(Reload());
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(Reload());
            }

            //weapon position elasticity 

            currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition,
                Vector3.zero,
                Time.deltaTime * 4f);


            //cooldown
            if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
        }

        //Switch the guns
        if (Input.GetKeyDown(KeyCode.Alpha1) && loadout[0] != null)
        {
            EquipPistol();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && loadout[1] != null)
        {
            EquipRifle();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && loadout[2] != null)
        {
            EquipSniperRifle();
        }
    }


    IEnumerator Reload()
    {
        sfxR.Stop();
        sfxR.clip = loadout[currentInd].reloadSound;
        sfxR.volume = loadout[currentInd].volumeReload;
        sfxR.Play();
        isReloading = true;
        float time = loadout[currentInd].reloadTime;
        currentWeapon.SetActive(false);
        yield return new WaitForSeconds(time);
        loadout[currentInd].Reload();
        currentWeapon.SetActive(true);
        isReloading = false;
    }

    public void EquipPistol()
    {
        photonView.RPC("Equip", RpcTarget.All, 0);
    }
    public void EquipRifle()
    {
        photonView.RPC("Equip", RpcTarget.All, 1);
    }
    public void EquipSniperRifle()
    {
        photonView.RPC("Equip", RpcTarget.All, 2);
    }

    public void RefreshAmmo(Text ammo)
    {
        int clip;
        int stash;
        if (currentWeapon == null)
        {
            clip = 0;
            stash = 0;
        }
        else
        {
            clip = loadout[currentInd].GetClip();
            stash = loadout[currentInd].GetStash();
        }


        ammo.text = clip.ToString("D2") + " / " + stash.ToString("D2");

    }

    void Aim(bool isAiming)
    {
        anchor = currentWeapon.transform.Find("Anchor");
        stateHip = currentWeapon.transform.Find("States/Hip");
        stateAds = currentWeapon.transform.Find("States/ADS");

        if (isAiming)
        {
            Crosshair[currentInd + 1].SetActive(false);
            //aim
            anchor.position = Vector3.Lerp(anchor.position, stateAds.position,
                Time.deltaTime * loadout[currentInd].aimSpeed);
            if (currentInd == 2)
            {
                playerControl.mouseSensitivityX = mouseXSens / 3;
                playerControl.mouseSensitivityY = mouseYSens / 3;
            }
        }
        else
        {
            Crosshair[currentInd + 1].SetActive(true);
            //hip
            anchor.position = Vector3.Lerp(anchor.position, stateHip.position,
                Time.deltaTime * loadout[currentInd].aimSpeed);
            playerControl.mouseSensitivityX = mouseXSens;
            playerControl.mouseSensitivityY = mouseYSens;
        }
    }

    [PunRPC]
    void Shoot()
    {
        sfx.Stop();
        sfx.clip = loadout[currentInd].gunShotSound;
        sfx.pitch = 1 - loadout[currentInd].pitchRand +
                    Random.Range(-loadout[currentInd].pitchRand, loadout[currentInd].pitchRand);
        sfx.volume = loadout[currentInd].volumeShot;
        sfx.Play();
        if (loadout[currentInd].typeOfGun == 0)
        {
            ShootRaycast();
        }
        else if (loadout[currentInd].typeOfGun == 1)
        {
            ShootManyRaycast();
        }
    }

    void ShootRaycast()
    {

        //raycast
        Transform spawn = transform.Find("Cameras/Normal camera");
          
        //bloom
        Vector3 bloom = spawn.position + spawn.forward * 1000f;
        Debug.Log(aiming);
        if (aiming)
        {
            bloom += Random.Range(-loadout[currentInd].bloomWhenIsAiming, loadout[currentInd].bloomWhenIsAiming) * spawn.up;
            Debug.Log(bloom);
            bloom += Random.Range(-loadout[currentInd].bloomWhenIsAiming, loadout[currentInd].bloomWhenIsAiming) * spawn.right;
            Debug.Log(bloom);
            bloom -= spawn.position;
            Debug.Log(bloom);
            bloom.Normalize();
            Debug.Log(bloom);
        }
        else
        {
            bloom += Random.Range(-loadout[currentInd].bloom, loadout[currentInd].bloom) * spawn.up;
            Debug.Log(bloom);
            bloom += Random.Range(-loadout[currentInd].bloom, loadout[currentInd].bloom) * spawn.right;
            Debug.Log(bloom);
            bloom -= spawn.position;
            Debug.Log(bloom);
            bloom.Normalize();
            Debug.Log(bloom);
        }
        RaycastHit hit = new RaycastHit();
        GameObject currentBullet = Instantiate(loadout[currentInd].prefabBullet, currentAtackPoint.position,
            Quaternion.identity);
        currentBullet.transform.forward = bloom;
        if (Physics.Raycast(spawn.position, bloom, out hit, 100f, canBeShot))
        {
            EnemyHits enemy = hit.transform.GetComponent<EnemyHits>();
            if (hit.collider.gameObject.layer == 11)
            {
                GameObject newHole =
                    Instantiate(BulletImpactPistol, hit.point + hit.normal * 0.001f, Quaternion.identity,
                            hit.collider.transform) as
                        GameObject;
                newHole.transform.LookAt(hit.point + hit.normal);
                Destroy(newHole, 10f);
            }
            else
            {
                GameObject newHole =
                    Instantiate(BulletImpactPistol, hit.point + hit.normal * 0.001f, Quaternion.identity) as
                        GameObject;
                newHole.transform.LookAt(hit.point + hit.normal);
                Destroy(newHole, 10f);
                WallHits wall = hit.transform.GetComponent<WallHits>();
                if (wall != null)
                {
                    wall.OnHit(hit);
                }
            }

            if (photonView.IsMine)
            {
                //shooting other player on network
                if (hit.collider.gameObject.layer == 11)
                {
                    bool applyDamage = false;
                    
                    if (GameSettings.gameMode == GameMode.DEATHMATCH)
                    {
                        applyDamage = true;
                    }
                    
                    if (GameSettings.gameMode == GameMode.TEAMDEATHMATCH)
                    {
                        if (hit.collider.transform.root.gameObject.GetComponent<PlayerControl>().awayTeam !=
                            GameSettings.isAwayTeam)
                        {
                            applyDamage = true;
                        }
                    }

                    if (applyDamage)
                    {
                        enemy.OnHit(hit);
                        hit.collider.gameObject.GetPhotonView()
                            .RPC("Take_Damage", RpcTarget.All, loadout[currentInd].damage,
                                PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                }
            }
        }
        
        currentWeapon.transform.Rotate(-loadout[currentInd].recoil, 0, 0);
        currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentInd].kickback;


        //cooldown
        currentCooldown = loadout[currentInd].fireRate;

    }

    void ShootManyRaycast()
    {

        //Find the exact hit position using a raycast
        //Just a ray through the middle of your current view
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f,
            0));
        RaycastHit hit;
        //check if ray hits something
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75); //Just a point far away from the player
        //Calculate direction from attackPoint to targetPoint
        Vector3 directionWithoutSpread = targetPoint - currentAtackPoint.position;
        //Calculate spread

        float x;
        float y;
        Debug.Log(aiming);
        if (aiming)
        {
            Debug.Log(loadout[currentInd].bloomWhenIsAiming);
            x = Random.Range(-loadout[currentInd].bloom, loadout[currentInd].bloomWhenIsAiming);
            y = Random.Range(-loadout[currentInd].bloom, loadout[currentInd].bloomWhenIsAiming);
        }
        else
        {
            Debug.Log(loadout[currentInd].bloom);
            x = Random.Range(-loadout[currentInd].bloom, loadout[currentInd].bloom);
            y = Random.Range(-loadout[currentInd].bloom, loadout[currentInd].bloom);
        }
        
        Debug.Log($"Bloom x = {x}, y = {y} ");


        //Calculate new direction with spread
        Vector3 directionWithSpread =
            directionWithoutSpread + new Vector3(x, y, 0); //Just add spread to last direction
        //Instantiate bullet/projectile
        GameObject currentBullet = Instantiate(loadout[currentInd].prefabBullet, currentAtackPoint.position,
            Quaternion.identity); //store instantiated bullet in currentBullet
        //Rotate bullet to shoot direction
        currentBullet.transform.forward = directionWithSpread;
        Debug.Log(currentBullet.transform.forward);
        ManyRaycastBullet bulletScript = currentBullet.GetComponent<ManyRaycastBullet>();
        if (bulletScript)
        {
            currentAtackPoint.forward = directionWithSpread;
            bulletScript.Initialize(currentAtackPoint);
            bulletScript.isMine = photonView.IsMine;
        }

        Destroy(currentBullet, bulletLifeTime);
        //gun fx
        
        currentWeapon.transform.Rotate(-loadout[currentInd].recoil, 0, 0);
        currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentInd].kickback;

        //cooldown
        currentCooldown = loadout[currentInd].fireRate;

    }


    [PunRPC]
    public void Take_Damage(int _damage, int _actor)
    {
        GetComponent<Health>().TakeDamage(_damage, _actor);
    }

    #endregion

}
