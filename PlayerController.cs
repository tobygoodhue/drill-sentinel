using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed; // The speed the player moves at
    public float jumpHeight; // How high the player jumps
    public float sensitivity; // How sensitive the player's rotation is to mouse movement
    public float playerVelDepricationRate; // Rate at which the player's velocity is depricated
    public float maxVel; // Maximum speed the player is allowed to move
    public float walkMaxVel; // Maximum speed player can move when walking
    public float sprintMaxVel; // Maximum speed player can move when sprinting
    public float curVel; // Current velocity of the player
    public float gunDamage; // Damage the gun does
    public float buildRange; // Range in which player can build
    public GameObject cam; // Main camera
    public GameObject crosshair; // Crosshair that appears when the player is looking at an object that he can interact with
    public GameObject muzzleFlash; // Muzzle flash
    public GameObject hitmarker; // Hitmarker
    public GameObject curHeld; // Currently held weapon
    public GameObject tempTurret; // Temporary turret for placeholding purposes
    public List<GameObject> weapons; // All weapons player has
    public AudioClip gunShot; // Gun shot sound
    public AudioClip hitmarkerSound; // Hitmarker sound
    public AudioSource hitmarkerSource; // Hitmarker sound source
    public AudioSource gunSource; // Gun sound

    private Rigidbody rb; // Player's Rigidbody
    private float x, y, xx; // Variables used for rotating the player and camera, as well as clamping camera rotation
    private bool isGrounded; // Whether or not the player is on solid ground
    private bool canBuild; // Whether or not player can build
    private RaycastHit buildHit; // Confirmed build hit, stemming from possBuildHit (CANNOT BE LOCAL VAR, DUE TO VAR ONLY BEING ASSIGNED WITHIN A SEPERATE IF STATEMENT)


    void Start()
    {
        Cursor.visible = false;// Hiding the cursor
        Cursor.lockState = CursorLockMode.Locked;// Locking the cursor so that it does not float around the screen
        rb = gameObject.GetComponent<Rigidbody>(); // Assigning the player's Rigidbody
        xx = 0; // Set to 0 so player doesn't spawn in looking at floor
        muzzleFlash.SetActive(false);
        curHeld = weapons[0];
    }

    void Update()
    {
        // Building
        //------------------------------------------------------------------------
        RaycastHit possBuildHit; // Possible placement for build

        if(curHeld == weapons[1]) // Check if player is holding hammer
        {
            if(Physics.Raycast(cam.transform.position, cam.transform.forward, out possBuildHit, buildRange)) // Cast ray to see if player is able to build
            {
                canBuild = true;
                buildHit = possBuildHit; // Set private var buildHit
            }
            else
            {
                canBuild = false;
            }
        }
        else
        {
            canBuild = false;
        }

        if(canBuild && Input.GetMouseButtonDown(0)) // Check to see if player can build, and is left clicking
        {
            Quaternion correctedRotation = Quaternion.LookRotation(buildHit.normal) * Quaternion.Euler(90, 0, 0);
            Instantiate(tempTurret, buildHit.point, correctedRotation);
            // Rotation of instantiated object is by default a -90 degree angle to the normal it is instantiated on to, rotation must be corrected before instiating
            // Rotation is set to match normal so built objects match the normal of the area of terrain they are built on
        }

        //------------------------------------------------------------------------

        // Weapons
        //------------------------------------------------------------------------
        if (Input.GetMouseButtonDown(0) && curHeld == weapons[0]) // Check for left mouse button click
        {
            StartCoroutine(Shoot());
        }

        if(Input.GetKeyDown(KeyCode.Alpha2) && curHeld == weapons[0]) // Check to see if player pressing 2 and gun in hand
        {
            curHeld.SetActive(false); // Disable old weapon
            curHeld = weapons[1]; // Switch
            curHeld.SetActive(true); // Enable new weapon
        }

        if(Input.GetKeyDown(KeyCode.Alpha1) && curHeld == weapons[1]) // Check to see if player pressing 1 and hammer in hand
        {
            curHeld.SetActive(false); // Disable old weapon
            curHeld = weapons[0]; // Switch
            curHeld.SetActive(true); // Enable new weapon
        }

        // Movement
        //------------------------------------------------------------------------
        curVel = FindVel(rb.linearVelocity); // Calculating the current absoluet velocity of the player

        // Checking for input and adding the velocity as long as the player is below their top speed
        //........................................................................
        if(Input.GetKey(KeyCode.W) && curVel < maxVel)
        {
            rb.linearVelocity += transform.forward * speed * Time.deltaTime; // Forward movement
        }
        if (Input.GetKey(KeyCode.S) && curVel < maxVel)
        {
            rb.linearVelocity += transform.forward * -speed * Time.deltaTime; // Backward Movement
        }
        if (Input.GetKey(KeyCode.A) && curVel < maxVel)
        {
            rb.linearVelocity += transform.right * -speed * Time.deltaTime; // Left movement
        }
        if (Input.GetKey(KeyCode.D) && curVel < maxVel)
        {
            rb.linearVelocity += transform.right * speed * Time.deltaTime; // Right movement
        }
        if(Input.GetKey(KeyCode.LeftShift)) // Check if player is sprinting
        {
            maxVel = sprintMaxVel;
        }
        else
        {
            maxVel = walkMaxVel;
        }
        //........................................................................

        Vector3 curVelTemp = new Vector3(Mathf.Lerp(rb.linearVelocity.x, 0, playerVelDepricationRate * Time.deltaTime), rb.linearVelocity.y, Mathf.Lerp(rb.linearVelocity.z, 0, playerVelDepricationRate * Time.deltaTime)); // Constantly depricating the player's velocity

        rb.linearVelocity = curVelTemp; // Constantly setting the player's velocity to the depricated velocity so they don't exceed their maximum velocity, and so the slow down at the proper rate
        //------------------------------------------------------------------------

        // Rotation
        //------------------------------------------------------------------------
        y = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime; // y = Mouse X because we are rotating the player on the Y axis
        x = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime; // x = Mouse Y because we are rotating the camera on the X axis

        xx -= x; // Keeps track of where the head shoud currently be rotated
        xx = Mathf.Clamp(xx, -90, 90); // Clamping the X rotation of the camera that way the player can't infinitely spin on the X axis

        cam.transform.localRotation = Quaternion.Euler(xx, 0f, 0f); // Setting appropriate camera rotation along the X axis

        transform.Rotate(transform.rotation.x, y, transform.rotation.z); // Rotate player along the Y axis
        //------------------------------------------------------------------------

        // Jumping
        //------------------------------------------------------------------------
        // Check if standing on the ground
        Ray groundCheckRay = new Ray(new Vector3(transform.position.x, transform.position.y - .9f, transform.position.z), Vector3.down); // Creating a ray coming out of the bottom of the player, going down
        // Ray must be cast from inside the player, instead of at the player's feet, or else the raycast doesn't always detect the collider beneath it 
        RaycastHit groundHit; // Ground Raycast Hit

        if (Physics.Raycast(groundCheckRay, out groundHit, 0.2f)) // Checking to see if the raycast hit anything
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) // Checking for proper input, and most importantly that the player is standing on solid ground
        {
            isGrounded = false;
            StartCoroutine(Jump());
        }
        //------------------------------------------------------------------------

    }

    IEnumerator Jump()
    {
        rb.AddForce(Vector3.up * jumpHeight * 100f); // Adding desired force upwards

        yield return new WaitForSeconds(0.15f); // Delay to prevent players from spamming jump and getting extra height
    }

    IEnumerator Shoot()
    {
        gunSource.PlayOneShot(gunShot);
        muzzleFlash.SetActive(true);
        RaycastHit hit;
        GameObject e; // Enemy
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 500f) && hit.collider.gameObject.tag == "Enemy") // Check to see if raycast hit enemy
        {
           e = hit.collider.gameObject; // Set e
           if(e.GetComponent<Enemy>().isDead == false) // Check to make sure enemy is still alive
           {
                hitmarker.SetActive(true);
                hitmarkerSource.PlayOneShot(hitmarkerSound);
                e.GetComponent<Enemy>().TakeDamage(gunDamage, transform); // Damage enemy
           }
           else
           {
                print("Enemy already dead");
           }
        }
        yield return new WaitForSeconds(0.05f); // Allow muzzleflash for set amount of time
        muzzleFlash.SetActive(false); // Disable muzzleflash
        hitmarker.SetActive(false);
    }

    float FindVel(Vector3 vel)
    {
        vel = new Vector3(Mathf.Abs(vel.x), 0, Mathf.Abs(vel.z)); // Getting the absolute value of X and Z

        float f = vel.x * vel.x + vel.z * vel.z; // Squaring and adding the absolute values of X and Z
        f = Mathf.Sqrt(f); // Finding the square root of f
        return f; // Outputting f
    }
}
