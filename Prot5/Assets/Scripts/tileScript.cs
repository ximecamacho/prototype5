using UnityEngine;

public class tileScript : MonoBehaviour
{

     [SerializeField] private MicrophoneInput _micInput;
      private GameObject[] _zonePlatforms;

      [SerializeField] private GameObject _greenPlatforms;
    [SerializeField] private GameObject _yellowPlatforms;
    [SerializeField] private GameObject _redPlatforms;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _zonePlatforms = new[] { _greenPlatforms, _yellowPlatforms, _redPlatforms };

    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void OnCollsion(Collider other)
    {
    
        if (other.CompareTag("Player"))
        {
            this.gameObject.tag = "StandingOn";
        }
    }
}
