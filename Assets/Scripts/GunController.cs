using UnityEngine;

public class GunController : MonoBehaviour {

    Gun equippedGun;

    public Transform gunHolder;

    public Gun startingGun;

    private void Start()
    {
        if (startingGun != null)
            EquipGun(startingGun);
    }

    public void EquipGun(Gun gunToEquip)
    {
        if (equippedGun != null)
            Destroy(equippedGun.gameObject);
        equippedGun = Instantiate(gunToEquip, gunHolder.position, gunHolder.rotation) as Gun;
        equippedGun.transform.parent = gunHolder;
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
