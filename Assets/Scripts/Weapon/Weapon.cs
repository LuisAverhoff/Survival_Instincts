using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using DigitalRuby.Pooling; // This namespace is used to help with caching previously created objects so that we don't constantly instantiate and destroy them.
using InControl; // This namespace is used to help with input management.

public class Weapon : MonoBehaviour
{
    private Animator weaponAnim;
    private AudioSource weaponAudioSource;

    [SerializeField]private Text BulletDisplayText;
    [SerializeField]private Text totalBulletsText;

    [SerializeField]private string tracer;
    [SerializeField]private string bulletImpact;

    [SerializeField]private ParticleSystem muzzleFlash;
    [SerializeField]private AudioClip weaponShootClip;
    [SerializeField]private AudioClip weaponReloadClip;

    // These two variables control the spread of the cone.
    [SerializeField]private float scaleLimit;
    [SerializeField]private float spreadZDirection;

    [SerializeField]private float tracerSpeed;

    [SerializeField]private Transform bulletSpawnPoint;
    [SerializeField]private int bulletRange;
    [SerializeField]private float bulletSpeed;

    private Vector3 originalPosition;

    [SerializeField]private Vector3 aimPosition;
    [SerializeField]private float aimDownSightSpeed = 8.0f;

    [SerializeField]private float maxRecoilZPosition;
    [SerializeField]private float minRecoilZPosition;
    [SerializeField]private float recoil;
    [SerializeField]private float recoilSpeed;

    [SerializeField]private enum ShootMode {Auto, Semi};
    [SerializeField]private ShootMode shootingMode;

    [SerializeField]private int bulletsPerMag = 30;
    [SerializeField]private int totalBullets = 200;

    [SerializeField]private int currentBullets;

    private bool isReloading;
    private bool shootInput;

    [SerializeField]private float fireDelay = 0.1f; // The delay between each shoot.

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
            {
                playReloadAnimation();
            }
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
        return totalBullets > 0;
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
        updateAmmoUI(currentBullets);
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
            {
                playReloadAnimation();
            }
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

        updateCurrentBulletsText();
        currentBullets--;
        fireTimer = 0.0f;
    }

    private IEnumerator checkIfHit()
    {
        Vector3 randomDirection = getRandomBulletDirection();
        RaycastHit hit;

        // Cast an exploratory raycast first from a randomly generated direction.
        if(Physics.Raycast(bulletSpawnPoint.position, randomDirection, out hit, bulletRange))
        {
            // calculate how long it would take the bullet to get to its target.
            float delay = hit.distance / bulletSpeed;
            // calculate the bullet drop at the target.
            Vector3 hitPoint = hit.point;
            hitPoint.y -= delay * 9.8f;
            // calculate the new vector's direction
            Vector3 dir = (hitPoint - bulletSpawnPoint.position).normalized;
            yield return new WaitForSeconds(delay);

            // spawn a bullet tracer from the direction(normalized) that was lowered to simulate a bullet drop.
            StartCoroutine(spawnBulletTracer(dir));

            // Now perform the actual shooting.
            if (Physics.Raycast(bulletSpawnPoint.position, dir, out hit, bulletRange))
            {
                GameObject bulletHole = SpawningPool.CreateFromCache(bulletImpact, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                yield return new WaitForSeconds(3.0f);
                SpawningPool.ReturnToCache(bulletHole);
            }
        }
    }

    private Vector3 getRandomBulletDirection()
    {
        // Generate a random XY point inside a circle.
        Vector3 randomDirection = Random.insideUnitCircle * scaleLimit;
        randomDirection.z = spreadZDirection;
        randomDirection = transform.TransformDirection(randomDirection.normalized);
        return randomDirection;
    }

    private IEnumerator spawnBulletTracer(Vector3 direction)
    {
        // Retrieve from the spawn pool a cached bullet tracer object.
        GameObject bulletTracer = SpawningPool.CreateFromCache(tracer, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);

        Rigidbody rigidbodyTracer = bulletTracer.GetComponent<Rigidbody>();

        rigidbodyTracer.velocity = direction * tracerSpeed;

        yield return new WaitForSeconds(1.0f);
        SpawningPool.ReturnToCache(bulletTracer, "Tracer");
        // Send it back to the cache so that it may be used later.
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
        if (totalBullets <= 0) return;

        int bulletsToLoad = bulletsPerMag - currentBullets;
        int bulletsToDeduct = (totalBullets >= bulletsToLoad) ? bulletsToLoad : totalBullets;

        totalBullets -= bulletsToDeduct;
        currentBullets += bulletsToDeduct;

        updateAmmoUI(currentBullets);
    }

    private void playReloadAnimation()
    {
        if (isReloading) return;

        weaponAnim.CrossFadeInFixedTime("Reload", 0.01f);
    }

    public void updateAmmoUI(int totalBulletsToDisplay)
    {
        StringBuilder activeBullets = new StringBuilder(bulletsPerMag);

        for (int i = 0; i < totalBulletsToDisplay; i++)
        {
            activeBullets.Append("I ");
        }

        BulletDisplayText.text = activeBullets.ToString();
        totalBulletsText.text = totalBullets.ToString();
    }

    public void updateCurrentBulletsText()
    {
        int bulletOffsetTextIndex = currentBullets * 2 - 1;
        BulletDisplayText.text = BulletDisplayText.text.Remove(bulletOffsetTextIndex);
    }
}