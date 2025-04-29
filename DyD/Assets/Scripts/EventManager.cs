using UnityEngine;
using UnityEngine.SceneManagement;

public class EventManager : MonoBehaviour
{
    public int MaxDistance = 35;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ActivateEvent(int eventID)
    {
        if(eventID ==1)//Calabozo 1
        {
            SceneManager.LoadScene("Calabozo1");
        }
        else if (eventID == 2) //Calabozo 2
        {
            SceneManager.LoadScene("Calabozo2");
        }
    }
}
