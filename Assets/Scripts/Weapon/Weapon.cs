using UnityEngine;
using System.Collections;
using DigitalRuby.Pooling; // This namespace is used to help with caching previously created objects so that we don't constantly instantiate and destroy them.
using InControl; // This namespace is used to help with input management.

public class Weapon : MonoBehaviour
{
    private Animator weaponAnim;
    private AudioSource weaponAudioSource;

    public string tracer;
    public string bulletImpact;

    public ParticleSystem muzzleFlash;
    public AudioClip weaponShootClip;
    public AudioClip weaponReloadClip;

    public float tracerSpeed;

    public Transform bulletSpawnPoint;
    public int bulletRange;
    public float bulletSpeed;

    private Vector3 originalPosition;

    public Vector3 aimPosition;
    public float aimDownSightSpeed = 8.0f;

    public float maxRecoilZPosition;
    public float minRecoilZPosition;
    public float recoil;
    public float recoilSpeed;

    public enum ShootMode {Auto, Semi};
    public ShootMode shootingMode;

    public int bulletsPerMag = 30;
    public int bulletsLeft = 200;

    public float damage = 5.0f;

    public int currentBullets;

    private bool isReloading;
    private bool shootInput;

    public float fireDelay = 0.1f; // The delay between each shoot.

    private float fireTimer; // Time counter for the delay.

    void Awake()
    {
        originalPosition = transform.localPosition;
    }

    // Use this for initialization
    void Start ()
    {
        weaponAnim = GetComponent<Animator>();
        weaponAudioSource = GetComponent<AudioSource>();
        currentBullets = bulletsPerMag;
    }
	
	// Update is called once per frame
	void Update ()
    {
        InputDevice device = InputManager.ActiveDevice;

        switch(shootingMode)
        {
            case ShootMode.Auto:
                shootInput = device.RightBumper;
                break;
            case ShootMode.Semi:
                shootInput = device.RightBumper;
                break;
        }

        if (device.Action3)
        {
            if (currentBullets < bulletsPerMag && thereAreBulletsLeft())
                playReloadAnimation();
        }

        if (fireTimer < fireDelay)
            fireTimer += Time.deltaTime;

        aimDownSights(device);
    }

    private bool canFire()
    {
        return currentBullets > 0 && !isReloading;
    }

    private bool thereAreBulletsLeft()
    {
        return bulletsLeft > 0;
    }

    public bool isShooting()
    {
        return shootInput;
    }

    void OnEnable()
    {
        transform.localPosition = originalPosition;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    void FixedUpdate()
    {
        AnimatorStateInfo info = weaponAnim.GetCurrentAnimatorStateInfo(0);

        isReloading = info.IsName("Reload");

        if (shootInput)
        {
            if (canFire())
            {
                if (fireTimer > fireDelay)
                {
                    fire();
                    pushBackWeapon();
                }
            }
            else if (thereAreBulletsLeft())
                playReloadAnimation();
        }
    }

    private void aimDownSights(InputDevice device)
    {
        if(device.LeftBumper && !isReloading)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPosition, Time.deltaTime * aimDownSightSpeed);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * aimDownSightSpeed);
        }
    }

    private void fire()
    {
        StartCoroutine(checkIfHit());

        muzzleFlash.Play();
        weaponAudioSource.PlayOneShot(weaponShootClip);

        currentBullets--;
        fireTimer = 0.0f;
    }

    private IEnumerator checkIfHit()
    {
        RaycastHit hit;

        // Cast an exploratory raycast first
        if(Physics.Raycast(bulletSpawnPoint.position, bulletSpawnPoint.transform.forward, out hit, bulletRange))
        {
            StartCoroutine(spawnBulletTracer());

            float delay = hit.distance / bulletSpeed;
            // calculate the flight time.
            Vector3 hitPoint = hit.point;
            hitPoint.y -= delay * 9.8f;
            // calculate the bullet drop at the target.
            Vector3 dir = hitPoint - bulletSpawnPoint.position;
            yield return new WaitForSeconds(delay);

            // Now perform the actual shooting.
            if(Physics.Raycast(bulletSpawnPoint.position, dir, out hit, bulletRange))
            {
                GameObject bulletHole = SpawningPool.CreateFromCache(bulletImpact, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                yield return new WaitForSeconds(3.0f);
                SpawningPool.ReturnToCache(bulletHole);
            }
        }
    }

    private IEnumerator spawnBulletTracer()
    {
        GameObject bulletTracer = SpawningPool.CreateFromCache(tracer, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);

        Rigidbody rigidbodyTracer = bulletTracer.GetComponent<Rigidbody>();

        rigidbodyTracer.velocity = transform.TransformDirection(Vector3.forward * tracerSpeed);

        yield return new WaitForSeconds(1.0f);
        SpawningPool.ReturnToCache(bulletTracer, "Tracer");
    }


    private void pushBackWeapon()
    {
       Vector3 weaponObjectLocalPosition = transform.localPosition;
       weaponObjectLocalPosition.z = Mathf.Clamp(weaponObjectLocalPosition.z - recoil, minRecoilZPosition, maxRecoilZPosition);
       transform.localPosition = Vector3.Lerp(transform.localPosition, weaponObjectLocalPosition, Time.deltaTime * recoilSpeed);
    }

    public void playReloadAudioClip()
    {
        weaponAudioSource.PlayOneShot(weaponReloadClip);
    }

    public void reloadBullets()
    {
        if (bulletsLeft <= 0) return;

        int bulletsToLoad = bulletsPerMag - currentBullets;
        int bulletsToDeduct = (bulletsLeft >= bulletsToLoad) ? bulletsToLoad : bulletsLeft;

        bulletsLeft -= bulletsToDeduct;
        currentBullets += bulletsToDeduct;
    }

    private void playReloadAnimation()
    {
        if (isReloading) return;

        weaponAnim.CrossFadeInFixedTime("Reload", 0.01f);
    }
}
