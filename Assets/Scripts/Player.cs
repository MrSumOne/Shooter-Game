using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public Crosshairs crosshairs;

    public float playerSpeed;
    Rigidbody playerRigidbody;
    
    Camera mainCamera;

    GunController gunController;

    Vector3 velocity;
    Vector3 heightCorrectedPoint;

    protected override void Start()
    {
        base.Start();
    }

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        gunController = GetComponent<GunController>();
        mainCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        health = startingHealth;
        gunController.EquipGun(waveNumber - 1);
    }

    private void FixedUpdate()
    {
        GetPlayerControls();
        //move the player
        playerRigidbody.MovePosition(playerRigidbody.position + velocity * Time.deltaTime);
        //rotate the player
        transform.LookAt(heightCorrectedPoint);

        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }

        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }
    }

    void GetPlayerControls()
    {
        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        velocity = direction * playerSpeed;

        //ray from camera towards mouse poisiton to inifinity
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        //new basic plane, like the one we have in the scene, that goes out on x and z forever
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;

        //if there's a intersection between ray and the plane, giving out the distance between the two
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            //get that intersection point, the ray towards the mouse at the length of the distance
            Vector3 pointOfIntersection = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin, pointOfIntersection, Color.red);
            //add the player's height to the point, so he doesn't point downwards
            heightCorrectedPoint = new Vector3(pointOfIntersection.x, transform.position.y, pointOfIntersection.z);

            crosshairs.transform.position = pointOfIntersection;
            crosshairs.DetectTargets(ray);
            if((new Vector2(pointOfIntersection.x, pointOfIntersection.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1f){
                gunController.Aim(pointOfIntersection);
            }

            
        }

        //this will work to, but it will only draw the ray when it collides with something, and gives out hitInfo
        //RaycastHit hitInfo;
        //if (Physics.Raycast(ray, out hitInfo))
        //{
        //    Vector3 point = hitInfo.point;
        //    Debug.DrawLine(ray.origin, point, Color.red);
        //    heightCorrectedPoint = new Vector3(point.x, transform.position.y, point.z);
        //}
    }
}
