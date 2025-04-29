using UnityEngine;


public class MenuUIManager : MonoBehaviour
{
    [SerializeField] private GameObject eventPanelUserInRange;
    [SerializeField] private GameObject eventPanelUserNotInRange;
    bool isUIPanelActive;
    int tempEvent;
    [SerializeField] private EventManager eventManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayStartEventPanel(int eventID)
    {
        tempEvent = eventID;
        eventPanelUserInRange.SetActive(true);
        isUIPanelActive = true;
    }

    public void OnJoinButtonClick()
    {
        eventManager.ActivateEvent(tempEvent);
    }
    public void DisplayUserNotInRangePanel()
    {
        eventPanelUserNotInRange.SetActive(true);
        isUIPanelActive = true;
    }


    public void CloseButtonClick()
    {
        eventPanelUserInRange.SetActive(false);
        eventPanelUserNotInRange.SetActive(false);
        isUIPanelActive = false;
    }
}
