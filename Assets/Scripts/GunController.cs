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

    public void Shoot()
    {
        if (equippedGun != null)
            equippedGun.Shoot();
    }
}
