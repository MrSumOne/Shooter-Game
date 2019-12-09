using UnityEngine;

public class GunController : MonoBehaviour {

    Gun equippedGun;

    public Transform gunHolder;

    public Gun[] allGuns;

    Transform weaponTransform;

    private void Start()
    {
    }

    public void EquipGun(Gun gunToEquip)
    {
        if (equippedGun != null)
            Destroy(equippedGun.gameObject);
        equippedGun = Instantiate(gunToEquip, gunHolder.position, gunHolder.rotation) as Gun;
        equippedGun.transform.parent = gunHolder;
        gunHolder.localPosition = gunToEquip.transform.position;
    }

    public void EquipGun(int weaponIndex)
    {
        //I remembered this array loop from the stealth game:
        //if the index runs out of bound of the array it will start at the beginning, 0
        weaponIndex = weaponIndex % allGuns.Length;
        EquipGun(allGuns[weaponIndex]);
    }

    public void OnTriggerHold()
    {
        if (equippedGun != null)
            equippedGun.OnTriggerHold();
    }

    public void OnTriggerRelease()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerRelease();
        }
    }

    public float GunHeight
    {
        get
        {
            return gunHolder.position.y;
        }
    }

    public void Aim(Vector3 aimPoint)
    {
        if(equippedGun != null)
        {
            equippedGun.Aim(aimPoint);
        }
    }

    public void Reload()
    {
        if (equippedGun != null)
        {
            equippedGun.Reload();
        }
    }
}
