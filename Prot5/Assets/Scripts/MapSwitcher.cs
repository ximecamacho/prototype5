
using TarodevController;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class MapSwitcher : MonoBehaviour
{
    [SerializeField] private MicrophoneInput _micInput;

    [Header("Platform Tilemaps (one per zone)")]
    [SerializeField] private GameObject _greenPlatforms;
    [SerializeField] private GameObject _yellowPlatforms;
    [SerializeField] private GameObject _redPlatforms;

    [SerializeField] private GameObject _environmentObj;

    [SerializeField] private PlayerController player;
     private Rigidbody rb;
    private float speed;


private float movementX = 0;

    private float movementSpeed = 0.5f;

    private int _lastZone = -1;
    private GameObject[] _zonePlatforms;

    private void Awake()
    {
         rb = _environmentObj.GetComponent<Rigidbody>();
        _zonePlatforms = new[] { _greenPlatforms, _yellowPlatforms, _redPlatforms };

    }

    private void Update()
    {
        movementX = 0;
        speed = 0;
        if (_micInput == null) return;




        int zone = _micInput.Zone;
        if (zone == _lastZone) return;

        _lastZone = zone;



        for (int i = 1; i < _zonePlatforms.Length; i++)
        {
            if (_zonePlatforms[i] != null)
            {
                
                    _zonePlatforms[i].SetActive(i == zone);
    
            }
                

        }

        if (zone != 0)
        {
            moveMap();
            
        }
        



    }

    private void moveMap()
    {

        player.ExecuteJump();
        /*Vector3 movementVector = new Vector3(-10, 0, 0);
        movementX = movementVector.x;*/

        Vector3 move = new Vector3(-5, 0, 0);
         _environmentObj.transform.Translate(move * speed * Time.deltaTime);
         _environmentObj.transform.position = _environmentObj.transform.position + new Vector3(-2, 0, 0);


    }

    

    private void FixedUpdate()
    {
        /*speed = 6f;


             Vector3 movement = new Vector3(movementX, 0.0f, 0);
             rb.AddForce(movement * speed);*/






    }
    
     

    
}
