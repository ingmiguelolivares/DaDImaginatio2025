using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Examples;
using Mapbox.Utils;

public class EventPointer : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 50f;
    [SerializeField] float amplitude = 2.0f;
    [SerializeField] float frequency = 0.50f;

    LocationStatus playerLocation;
    public Vector2d eventPose;
    public int eventID;
    MenuUIManager MenuUIManager;
    EventManager eventManager;

    void Start()
    {
        MenuUIManager = GameObject.Find("Canvas").GetComponent<MenuUIManager>();
        eventManager = GameObject.Find("-EventManager").GetComponent<EventManager>();
    }

    void Update()
    {
        FloatAndRotatePointer();
        DetectTouchInput(); // Añadimos detección de toques o clics
    }

    void FloatAndRotatePointer()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, (Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude) + 15, transform.position.z);
    }

    void DetectTouchInput()
    {
        // Toque en pantalla (solo en dispositivos móviles)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            {
                OnTap();
            }
        }

#if UNITY_EDITOR
        // Clic con el mouse (solo en editor)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            {
                OnTap();
            }
        }
#endif
    }

    void OnTap()
    {
        playerLocation = GameObject.Find("Canvas").GetComponent<LocationStatus>();
        var currentPlayerLocation = new GeoCoordinatePortable.GeoCoordinate(playerLocation.GetLocationLat(), playerLocation.GetLocationLon());
        var eventLocation = new GeoCoordinatePortable.GeoCoordinate(eventPose[0], eventPose[1]);
        var distance = currentPlayerLocation.GetDistanceTo(eventLocation);
        Debug.Log("Distance is " + distance);

        if (distance < eventManager.MaxDistance)
        {
            MenuUIManager.DisplayStartEventPanel(eventID);
        }
        else
        {
            MenuUIManager.DisplayUserNotInRangePanel();
        }
    }
}
