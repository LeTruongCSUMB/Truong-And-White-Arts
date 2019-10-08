﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponScript : MonoBehaviour
{
    // NON ALT UI
    public static GameObject AmmoObject;
    public Text AmmoClipNumber;
    public Text AmmoTotalNumber;

    // ALT UI
    public static GameObject AmmoAltObject;
    public Text AmmoPrimClipNumber;
    public Text AmmoPrimTotalNumber;
    public Text AmmoAltAmmoNumber;

    // WEAPON UI
    public static Text PickUpText;
    public static Text AmmoTypeIcon;
    public static Text AmmoPrimTypeIcon;
    public static Text AmmoAltTypeIcon;

    // PRIMARY FIRE
    public int weaponBulletShots;
    public static int currentClipAmmo;
    public int MaxClipAmmo;
    public static int currentTotalAmmo;
    public int MaxTotalAmmo;
    public float weaponRange;

    // ALT FIRE
    public int weaponAltBulletShots;
    public static int currentTotalAltAmmo;
    public int MaxTotalAltAmmo;
    public float weaponAltRange;

    // PRIME STATS
    public static float cooldownRef;
    public static float cooldown;
    private float weaponSpread;
    public static float weaponForce;
    public float weaponDamage;

    // ALT STATS
    public static float altCooldownRef;
    public static float altCooldown;
    private float weaponAltSpread;
    public static float weaponAltForce;
    public float weaponAltDamage;
    public static GameObject projectile;

    // RAYCASTING
    private GameObject shotPos;
    private Vector3 randomizedVector;
    private RaycastHit endpointInfo;

    // WEAPON INFO
    private GameObject gunCam;
    private GameObject firstPersonCamera;
    private GameObject gunCamera;
    private int weaponLayerMask;
    public static GameObject activeWeapon;
    public static GameObject muzzleFlash;
    public static bool weaponSwitch;
    public GameObject bulletHole;

    // GUN RECOIL
    public static GameObject playerGunCam;
    public static float gunCamRotateX;
    public static float n_gunCamRotateX;
    static float t = 0.0f;
    public static bool recoil;

    // CROSSBOW
    private GameObject crossA;
    private GameObject crossUA;
    public RawImage crossScope;
    public static bool isScoped;

    // Start is called before the first frame update
    void Start()
    {
        firstPersonCamera = GameObject.Find("FirstPersonCamera");
        gunCamera = GameObject.Find("GunCamera");
        playerGunCam = GameObject.Find("GunCam");
        weaponLayerMask = gunCamera.GetComponent<Camera>().cullingMask;

        shotPos = GameObject.Find("ShotPos");                                               //Where the raycast for weapons shots from
        gunCam = GameObject.Find("GunCam");                                                 //The empty gameobject that holds all the weapons
        PickUpText = GameObject.Find("PickUpText").GetComponent<Text>();                    //UI that updates on new weapon pick ups or ammo

        AmmoTypeIcon = GameObject.Find("AmmoType").GetComponent<Text>();
        // PRIME WEAPON UI
        AmmoObject = GameObject.Find("AmmoObject");                                         //Gameobject that determines visibility of UI
        AmmoObject.SetActive(false);                                                        //Player starts without suit e.g. no UI

        AmmoPrimTypeIcon = GameObject.Find("AmmoPrimType").GetComponent<Text>();
        AmmoAltTypeIcon = GameObject.Find("AmmoAltType").GetComponent<Text>();
        // ALT WEAPON UI
        AmmoAltObject = GameObject.Find("AmmoAltObject");                                   //Gameobject that determines visibility of UI
        AmmoAltObject.SetActive(false);                                                     //Player starts without suit e.g. no UI

        weaponSwitch = false;                                                               //Checks is the weapon has been switched
    }
    void Update()
    {
        // recoil for guns
        if (recoil)
        {
            playerGunCam.transform.localRotation = Quaternion.Euler((Mathf.Lerp(-n_gunCamRotateX, gunCamRotateX, t)), 0.0f, 0.0f);
            playerGunCam.transform.localPosition = new Vector3 (0.0f, (Mathf.Lerp(0.085f, 0.0f, t)), 0.0f);
            t += 3.25f * Time.deltaTime;
            if (t > 1.0f)
            {
                t = 0.0f;
                playerGunCam.transform.localRotation = Quaternion.Euler(gunCamRotateX, 0.0f, 0.0f);
                playerGunCam.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                recoil = false;
            }
        }
    }
    // FixedUpdate is called once per frame
    void FixedUpdate()
    {
        // if any weapon not crowbar is active (also planned to have grenades which will also have it's own if)
        if (activeWeapon != null)
        {
            string nameCheck = activeWeapon.GetComponent<WeaponStats>().getWeaponName();

            if (!(nameCheck.Equals("Crowbar")) && !(nameCheck.Equals("GravityGun")) && !(nameCheck.Equals("Grenade")) && !(nameCheck.Equals("RPG")))
            {
                if (weaponSwitch)
                {
                    // Activates the Ammo UI if not a crowbar
                    if (PlayerHealth.hasSuit)
                    {
                        if (activeWeapon.GetComponent<WeaponStats>().isWeaponHasAltFire())
                        {
                            if (!activeWeapon.GetComponent<WeaponStats>().isWeaponUsingPrimeAmmo())
                            {
                                AmmoObject.SetActive(false);
                                AmmoAltObject.SetActive(true);
                            }
                            else if (activeWeapon.GetComponent<WeaponStats>().isWeaponUsingPrimeAmmo())
                            {
                                AmmoAltObject.SetActive(false);
                                AmmoObject.SetActive(true);
                            }
                        }
                        else
                        {
                            AmmoAltObject.SetActive(false);
                            AmmoObject.SetActive(true);
                        }
                    }

                    // Assign Primary stats
                    AssignPrimeStats(activeWeapon);

                    // Assign Alternate stats
                    weaponAltBulletShots = activeWeapon.GetComponent<WeaponStats>().getAltWeaponBulletShots();
                    weaponAltRange = activeWeapon.GetComponent<WeaponStats>().getAltWeaponRange();

                    altCooldownRef = activeWeapon.GetComponent<WeaponStats>().getAltWeaponFireCooldown();
                    altCooldown = activeWeapon.GetComponent<WeaponStats>().getAltWeaponFireCooldown();

                    weaponAltForce = activeWeapon.GetComponent<WeaponStats>().getAltWeaponForce();
                    weaponAltSpread = activeWeapon.GetComponent<WeaponStats>().getAltWeaponSpread();
                    weaponAltDamage = activeWeapon.GetComponent<WeaponStats>().getAltWeaponDamage();

                    // CHECKS IF USING PRIMARY AMMO
                    if (!activeWeapon.GetComponent<WeaponStats>().isWeaponUsingPrimeAmmo())
                    {
                        currentTotalAltAmmo = activeWeapon.GetComponent<WeaponStats>().getAltWeaponCurrentAmmo();
                        MaxTotalAltAmmo = activeWeapon.GetComponent<WeaponStats>().getAltWeaponMaxAmmo();
                    }

                    // CHECKS IF INSTANTIATES PROJECTILE
                    if (activeWeapon.GetComponent<WeaponStats>().isWeaponAltInstantiate() || nameCheck.Equals("Crossbow"))
                    {
                        projectile = activeWeapon.GetComponent<WeaponStats>().getProjectile();
                        if (nameCheck.Equals("Crossbow"))
                        {
                            crossA = GameObject.Find("CrossArmed");
                            crossUA = GameObject.Find("CrossUnarmed");
                        }
                    }

                    weaponSwitch = false;
                }

                if (activeWeapon.GetComponent<WeaponStats>().isWeaponUsingPrimeAmmo() || !activeWeapon.GetComponent<WeaponStats>().isWeaponHasAltFire())
                {
                    AmmoClipNumber.text = currentClipAmmo.ToString();
                    AmmoTotalNumber.text = currentTotalAmmo.ToString();
                }
                else if (activeWeapon.GetComponent<WeaponStats>().isWeaponHasAltFire())
                {
                    AmmoPrimClipNumber.text = currentClipAmmo.ToString();
                    AmmoPrimTotalNumber.text = currentTotalAmmo.ToString();
                    AmmoAltAmmoNumber.text = currentTotalAltAmmo.ToString();
                }

                if (!PlayerSight.isHolding && !PlayerSight.isZoomed)
                {
                    // PRIME FIRE (MOUSE 1)
                    if (Input.GetKey(KeyCode.Mouse0))
                    {
                        if (!nameCheck.Equals("Crossbow"))
                        {
                            if (currentClipAmmo != 0 && !(currentClipAmmo < 0))
                            {
                                if (Time.time > cooldownRef)
                                {
                                    cooldownRef = Time.time + cooldown;

                                    currentClipAmmo--;

                                    for (int i = 0; i < weaponBulletShots; i++)
                                    {
                                        //crounching increases aim   
                                        if (PlayerMovement.isCrouching)
                                        {
                                            weaponSpread = Mathf.CeilToInt(weaponSpread / 1.5f);
                                            weaponForce = weaponForce / 1.5f;
                                        }
                                        else
                                        {
                                            weaponForce = activeWeapon.GetComponent<WeaponStats>().getWeaponForce();
                                            weaponSpread = activeWeapon.GetComponent<WeaponStats>().getWeaponSpread();
                                        }
                                        //visualize the raycast
                                        randomizedVector = RandomInsideCone(weaponSpread) * transform.forward;
                                        Debug.DrawRay(shotPos.transform.position, randomizedVector * weaponRange, Color.red, 1);

                                        if (Physics.Raycast(shotPos.transform.position, randomizedVector, out endpointInfo))
                                        {
                                            float rayRange = endpointInfo.distance;

                                            if (rayRange <= weaponRange)
                                            {
                                                WeaponRayCastHit(endpointInfo, weaponForce, weaponDamage);

                                            }
                                        }
                                    }
                                    AudioClip fireSFX = activeWeapon.GetComponent<WeaponStats>().getFireSFX();
                                    StartCoroutine(SoundController.gunSounds(fireSFX, cooldown));
                                    WeaponRecoil(weaponForce);
                                }
                            }
                            else
                            {
                                if (Time.time > cooldownRef)
                                {
                                    cooldownRef = Time.time + cooldown;
                                    AudioClip emptySFX = activeWeapon.GetComponent<WeaponStats>().getEmptySFX();
                                    StartCoroutine(SoundController.gunSounds(emptySFX, cooldown));
                                }
                            }
                        }
                        else
                        {
                            // CROSSBOW
                            if (currentClipAmmo != 0 && !(currentClipAmmo < 0))
                            {
                                if (Time.time > cooldownRef)
                                {
                                    cooldownRef = Time.time + cooldown;

                                    for (int i = 0; i < weaponBulletShots; i++)
                                    {
                                        Quaternion shotPosRotation = shotPos.transform.rotation;
                                        Rigidbody projectileShot = Instantiate(projectile.GetComponent<Rigidbody>(), shotPos.transform.position, shotPosRotation) as Rigidbody;
                                        projectileShot.transform.LookAt(shotPos.transform.position);
                                        projectileShot.AddForce(shotPos.transform.forward * weaponForce);
                                        currentClipAmmo--;
                                    }

                                    AudioClip fireSFX = activeWeapon.GetComponent<WeaponStats>().getFireSFX();
                                    StartCoroutine(SoundController.gunSounds(fireSFX, cooldown));

                                    if (activeWeapon.GetComponent<WeaponStats>().getWeaponName().Equals("Crossbow"))
                                    {
                                        crossA.SetActive(false);
                                        crossUA.SetActive(true);
                                    }
                                }
                            }
                            else
                            {
                                if (activeWeapon.GetComponent<WeaponStats>().getWeaponName().Equals("Crossbow"))
                                {
                                    crossA.SetActive(false);
                                    crossUA.SetActive(true);
                                }
                            }
                        }
                    }

                    // CROSSBOW SCOPE IN
                    if (activeWeapon.GetComponent<WeaponStats>().getWeaponName().Equals("Crossbow"))
                    {
                        if (Input.GetKeyDown(KeyCode.Mouse1))
                        {
                            if (!isScoped)
                            {
                                gunCamera.GetComponent<Camera>().cullingMask = 0;
                                firstPersonCamera.GetComponent<Camera>().fieldOfView = 10f;
                                crossScope.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
                                isScoped = true;
                            }
                            else if (isScoped)
                            {
                                gunCamera.GetComponent<Camera>().cullingMask = weaponLayerMask;
                                crossScope.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                                firstPersonCamera.GetComponent<Camera>().fieldOfView = 70f;
                                isScoped = false;
                            }
                        }
                    }

                    // ALT FIRE (MOUSE 2)
                    if (Input.GetKey(KeyCode.Mouse1))
                    {
                        if (activeWeapon.GetComponent<WeaponStats>().isWeaponHasAltFire())
                        {
                            if (activeWeapon.GetComponent<WeaponStats>().isWeaponUsingPrimeAmmo())
                            {
                                if (currentClipAmmo != 0 && !(currentClipAmmo < 0))
                                {
                                    if (Time.time > altCooldownRef)
                                    {
                                        altCooldownRef = Time.time + altCooldown;

                                        currentClipAmmo--;

                                        for (int i = 0; i < weaponAltBulletShots; i++)
                                        {
                                            //crounching increases aim   
                                            if (PlayerMovement.isCrouching)
                                            {
                                                weaponAltSpread = Mathf.CeilToInt(weaponAltSpread / 1.5f);
                                                weaponAltForce = weaponAltForce / 1.5f;
                                            }
                                            else
                                            {
                                                weaponAltForce = activeWeapon.GetComponent<WeaponStats>().getAltWeaponForce();
                                                weaponAltSpread = activeWeapon.GetComponent<WeaponStats>().getAltWeaponSpread();
                                            }
                                            //visualize the raycast
                                            randomizedVector = RandomInsideCone(weaponAltSpread) * transform.forward;
                                            Debug.DrawRay(shotPos.transform.position, randomizedVector * weaponAltRange, Color.red, 1);

                                            if (Physics.Raycast(shotPos.transform.position, randomizedVector, out endpointInfo))
                                            {
                                                float rayRange = endpointInfo.distance;

                                                if (rayRange <= weaponAltRange)
                                                {
                                                    WeaponRayCastHit(endpointInfo, weaponAltForce, weaponAltDamage);
                                                }
                                            }
                                        }
                                        AudioClip projectileSFX = activeWeapon.GetComponent<WeaponStats>().getProjectileSFX();
                                        StartCoroutine(SoundController.gunSounds(projectileSFX, altCooldown));
                                        WeaponRecoil(weaponAltForce);
                                    }
                                }
                                else
                                {
                                    if (Time.time > altCooldownRef)
                                    {
                                        altCooldownRef = Time.time + altCooldown;
                                        AudioClip emptySFX = activeWeapon.GetComponent<WeaponStats>().getEmptySFX();
                                        StartCoroutine(SoundController.gunSounds(emptySFX, altCooldown));
                                    }
                                }
                            }
                            else if (activeWeapon.GetComponent<WeaponStats>().isWeaponAltInstantiate())
                            {
                                if (currentTotalAltAmmo != 0 && !(currentTotalAltAmmo < 0))
                                {
                                    if (Time.time > altCooldownRef)
                                    {
                                        altCooldownRef = Time.time + altCooldown;
                                        for (int i = 0; i < weaponAltBulletShots; i++)
                                        {
                                            Quaternion shotPosRotation = shotPos.transform.rotation;
                                            Rigidbody projectileShot = Instantiate(projectile.GetComponent<Rigidbody>(), shotPos.transform.position, shotPosRotation) as Rigidbody;
                                            projectileShot.transform.LookAt(shotPos.transform.position);
                                            projectileShot.AddForce(shotPos.transform.forward * weaponAltForce);
                                            currentTotalAltAmmo--;

                                            AudioClip projectileSFX = activeWeapon.GetComponent<WeaponStats>().getProjectileSFX();
                                            StartCoroutine(SoundController.gunSounds(projectileSFX, altCooldown));
                                        }

                                    }
                                }
                                else
                                {
                                    if (Time.time > altCooldownRef)
                                    {
                                        altCooldownRef = Time.time + altCooldown;
                                        AudioClip emptySFX = activeWeapon.GetComponent<WeaponStats>().getEmptySFX();
                                        StartCoroutine(SoundController.gunSounds(emptySFX, altCooldown));
                                    }
                                }
                            }
                        }
                    }

                    //Saves the data into the weaponStats
                    activeWeapon.GetComponent<WeaponStats>().weaponCurrentClipSize = currentClipAmmo;
                    activeWeapon.GetComponent<WeaponStats>().altWeaponCurrentAmmo = currentTotalAltAmmo;

                    //reload weapon and update the UI
                    if (Input.GetKey("r"))
                    {
                        if (!isScoped)
                        {
                            if (currentClipAmmo != MaxClipAmmo)
                            {
                                if (currentTotalAmmo != 0)
                                {
                                    if (activeWeapon.GetComponent<WeaponStats>().getWeaponName().Equals("Crossbow"))
                                    {
                                        if (crossA != null && crossUA != null)
                                        {
                                            crossA.SetActive(true);
                                            crossUA.SetActive(false);
                                        }
                                    }

                                    int reloadNumber = (MaxClipAmmo - currentClipAmmo);

                                    AudioClip reloadSFX = activeWeapon.GetComponent<WeaponStats>().getReloadSFX();
                                    StartCoroutine(SoundController.gunSounds(reloadSFX, cooldown));

                                    if ((currentTotalAmmo - reloadNumber) < 0)
                                    {
                                        currentClipAmmo += currentTotalAmmo;

                                        currentTotalAmmo = 0;
                                    }
                                    else if ((currentTotalAmmo - reloadNumber) >= 0)
                                    {
                                        currentTotalAmmo = (currentTotalAmmo - reloadNumber);

                                        currentClipAmmo += reloadNumber;
                                    }
                                }
                            }
                        }
                    }
                    activeWeapon.GetComponent<WeaponStats>().weaponCurrentAmmo = currentTotalAmmo;
                }
            }
            //if crowbar is active weapon
            else if (activeWeapon.GetComponent<WeaponStats>().getWeaponName().Equals("Crowbar"))
            {
                if (weaponSwitch)
                {
                    AmmoObject.SetActive(false);
                    AmmoAltObject.SetActive(false);

                    AssignPrimeStats(activeWeapon);

                    weaponSwitch = false;
                }

                if (Input.GetKey(KeyCode.Mouse0))
                {
                    if (Time.time > cooldownRef)
                    {
                        cooldownRef = Time.time + cooldown;

                        //Firing SFX is assigned to clip
                        AudioClip fireSFX = activeWeapon.GetComponent<WeaponStats>().getFireSFX();
                        StartCoroutine(SoundController.gunSounds(fireSFX, cooldown));

                        //visualize the raycast
                        randomizedVector = RandomInsideCone(weaponSpread) * transform.forward;
                        Debug.DrawRay(shotPos.transform.position, randomizedVector * weaponRange, Color.blue, 1);

                        if (Physics.Raycast(shotPos.transform.position, randomizedVector, out endpointInfo))
                        {
                            float rayRange = endpointInfo.distance;

                            if (rayRange <= weaponRange)
                            {
                                WeaponRayCastHit(endpointInfo, weaponForce, weaponDamage);
                            }
                        }
                    }
                }
            }
        }
        if (activeWeapon != null)
        {
            if (!PlayerSight.isHolding && !PlayerSight.isZoomed && !isScoped)
            {
                if(Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    //print("minus 1");
                }
                if(Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    //print("plus 1");
                }
            }
            if (!PlayerSight.isHolding && !PlayerSight.isZoomed && !isScoped)
            {
                //Switch to Crowbar if player already has it
                if (Input.GetKey(KeyCode.Alpha1))
                {
                    string weaponTag = "PlayerCrowbar";
                    SwitchWeapon(weaponTag);
                }

                //Switch to Pistol if player has it
                if (Input.GetKey(KeyCode.Alpha2))
                {
                    string weaponTag = "PlayerPistol";
                    SwitchWeapon(weaponTag);
                }

                //Switch to Shotgun if player has it
                if (Input.GetKey(KeyCode.Alpha3))
                {
                    string weaponTag = "PlayerShotgun";
                    SwitchWeapon(weaponTag);
                }

                //Switch to SMG if player has it
                if (Input.GetKey(KeyCode.Alpha4))
                {
                    string weaponTag = "PlayerSMG";
                    SwitchWeapon(weaponTag);
                }

                //Switch to CombineRifle if player has it
                if (Input.GetKey(KeyCode.Alpha5))
                {
                    string weaponTag = "PlayerCombineRifle";
                    SwitchWeapon(weaponTag);
                }

                //Switch to Gravity Gun if player has it
                if (Input.GetKey(KeyCode.Alpha6))
                {
                    string weaponTag = "PlayerGravityGun";
                    SwitchWeapon(weaponTag);
                }
                //Switch to 357 if player has it
                if (Input.GetKey(KeyCode.Alpha7))
                {
                    string weaponTag = "Player357";
                    SwitchWeapon(weaponTag);
                }
                //Switch to crossbow if player has it
                if (Input.GetKey(KeyCode.Alpha8))
                {
                    string weaponTag = "PlayerCrossbow";
                    SwitchWeapon(weaponTag);
                }
            }
        }
    }
    // ASSIGNS THE PRIMARY STATS FOR WEAPONS
    public void AssignPrimeStats(GameObject activeWeapon)
    {
        weaponBulletShots = activeWeapon.GetComponent<WeaponStats>().getWeaponBulletShots();
        weaponRange = activeWeapon.GetComponent<WeaponStats>().getWeaponRange();

        MaxClipAmmo = activeWeapon.GetComponent<WeaponStats>().getWeaponMaxClipSize();
        currentClipAmmo = activeWeapon.GetComponent<WeaponStats>().getWeaponCurrentClipSize();

        MaxTotalAmmo = activeWeapon.GetComponent<WeaponStats>().getWeaponMaxAmmo();
        currentTotalAmmo = activeWeapon.GetComponent<WeaponStats>().getWeaponCurrentAmmo();

        cooldownRef = activeWeapon.GetComponent<WeaponStats>().getWeaponFireCooldown();
        cooldown = activeWeapon.GetComponent<WeaponStats>().getWeaponFireCooldown();

        weaponForce = activeWeapon.GetComponent<WeaponStats>().getWeaponForce();
        weaponSpread = activeWeapon.GetComponent<WeaponStats>().getWeaponSpread();

        weaponDamage = activeWeapon.GetComponent<WeaponStats>().getWeaponDamage();
    }

    // RANDOMIZES RAYCASTS
    Quaternion RandomInsideCone(float radius)
    {
        Quaternion randomTilt = Quaternion.AngleAxis(Random.Range(-radius, radius), Vector3.up);
        Quaternion randomSpin = Quaternion.AngleAxis(Random.Range(-radius, radius), Vector3.right);
        return (randomSpin * randomTilt);
    }

    // IF NEW WEAPON IS PICKED UP, CHANGE IT TO ACTIVE WEAPON
    public GameObject changeActiveWeapon(GameObject tempWeapon)
    {
        foreach (Transform child in gunCam.transform)
        {
            if (child.gameObject.activeInHierarchy == true)
            {
                child.gameObject.GetComponent<WeaponStats>().weaponActive = false;
                child.gameObject.SetActive(false);
            }
        }
        tempWeapon.gameObject.SetActive(true);
        activeWeapon = tempWeapon;
        weaponSwitch = true;
        return activeWeapon;
    }

    // SWITCHES WEAPONS BY WEAPON TAG
    public void SwitchWeapon(string weaponTag)
    {
        GameObject tempWeapon;
        foreach (Transform child in gunCam.transform)
        {
            if (child.gameObject.tag.Equals(weaponTag))
            {
                tempWeapon = child.gameObject;
                if (tempWeapon.GetComponent<WeaponStats>().isWeaponPickedUp() == true)
                {
                    if (tempWeapon.GetComponent<WeaponStats>().isWeaponActive() != true)
                    {
                        changeActiveWeapon(tempWeapon);
                        string weaponName = tempWeapon.GetComponent<WeaponStats>().getWeaponName();
                        WeaponStats.FindAmmoType(weaponName);
                    }
                }
            }
        }
    }

    // CHECKS RAYCAST BULLET
    public void WeaponRayCastHit(RaycastHit endpointInfo, float weaponForce, float weaponDamage)
    {
        GameObject myBulletHole = Instantiate(bulletHole, endpointInfo.point, Quaternion.LookRotation(endpointInfo.normal));
        int _weaponDamage = Mathf.CeilToInt(weaponDamage);
        if (endpointInfo.transform.gameObject.GetComponent<EntityHealth>() != null)
        {
            GameObject entity = endpointInfo.transform.gameObject;
            GameObject tagEntity = endpointInfo.transform.gameObject;
            _weaponDamage = CheckHeadshot(tagEntity, _weaponDamage);
            entity.GetComponent<EntityHealth>().entityCurrentHealth -= _weaponDamage;
        }
        else if (endpointInfo.transform.parent.transform.gameObject.GetComponent<EntityHealth>() != null)
        {
            GameObject entity = endpointInfo.transform.parent.transform.gameObject;
            GameObject tagEntity = endpointInfo.transform.gameObject;
            _weaponDamage = CheckHeadshot(tagEntity, _weaponDamage);
            entity.GetComponent<EntityHealth>().entityCurrentHealth -= _weaponDamage;
        }

        myBulletHole.transform.parent = endpointInfo.transform;

        EntityHealth.entityEndpoint = endpointInfo;
        EntityHealth.entityWeaponForce = weaponForce;

        if (endpointInfo.rigidbody != null)
        {
            endpointInfo.rigidbody.AddForce(-endpointInfo.normal * weaponForce);
        }
    }

    // WEAPONS RECOIL
    public static void WeaponRecoil(float weaponForce)
    {
        gunCamRotateX = playerGunCam.transform.localRotation.x;
        float weaponRecoil = weaponForce / 200;
        n_gunCamRotateX = playerGunCam.transform.localRotation.x + weaponRecoil;
        playerGunCam.transform.localRotation = Quaternion.Euler(-n_gunCamRotateX, 0.0f, 0.0f);
        playerGunCam.transform.localPosition = new Vector3 (0.0f, 0.085f, 0.0f);
        recoil = true;
    }

    // CHECKS IF HEADSHOT
    private int CheckHeadshot(GameObject tagEntity, int _weaponDamage)
    {
        if (tagEntity.tag == "Head")
        {
            return (_weaponDamage * 2);
        }
        else
        {
            return _weaponDamage;
        }
    }
}