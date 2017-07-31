using System.Collections;
using UnityEngine;
using InControl;

public class WeaponManager : MonoBehaviour
{
    [SerializeField]private GameObject[] weapons;
    [SerializeField]private float switchDelay = 1.0f;
    private Animator[] weaponAnimators;
    private Weapon currentWeapon;
    private bool isSwitchingGuns;
    private int weaponIndex;

	// Use this for initialization
	void Start ()
    {
        weaponAnimators = new Animator[weapons.Length];

        int i = 0;

        foreach (GameObject weapon in weapons)
        {
            weaponAnimators[i] = weapons[i].GetComponent<Animator>();
            i++;
        }

        weaponIndex = 0;
        switchWeapons(weaponIndex);
        currentWeapon = weapons[weaponIndex].GetComponent<Weapon>();
        isSwitchingGuns = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        InputDevice device = InputManager.ActiveDevice;

        if (device.DPadRight && !isSwitchingGuns && !currentWeapon.isShooting())
        {
            weaponIndex = (weaponIndex + 1) % weapons.Length;
            StartCoroutine(SwitchAfterDelay(weaponIndex));
        }
        else if(device.DPadLeft && !isSwitchingGuns && !currentWeapon.isShooting())
        {
            weaponIndex = (Mathf.Abs(weaponIndex - 1)) % weapons.Length;
            StartCoroutine(SwitchAfterDelay(weaponIndex));
        }
    }

    private IEnumerator SwitchAfterDelay(int weaponIndex)
    {
        isSwitchingGuns = true;
        yield return new WaitForSeconds(switchDelay);
        switchWeapons(weaponIndex);
        isSwitchingGuns = false;
    }

    private void switchWeapons(int weaponIndex)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weaponAnimators[i].Rebind();
            weapons[i].SetActive(false);
        }

        weapons[weaponIndex].SetActive(true);
        currentWeapon = weapons[weaponIndex].GetComponent<Weapon>();
    }
}
