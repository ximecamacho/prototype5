using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    public InputAction restart_;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        restart_.Enable();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (restart_.IsPressed())
        {

            SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
            
    	
        }
        
    }
}