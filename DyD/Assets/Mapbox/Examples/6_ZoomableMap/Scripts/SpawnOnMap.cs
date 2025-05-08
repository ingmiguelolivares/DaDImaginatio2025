namespace Mapbox.Examples
{
    using UnityEngine;
    using Mapbox.Utils;
    using Mapbox.Unity.Map;
    using Mapbox.Unity.Utilities;
    using System.Collections.Generic;

    public class SpawnOnMap : MonoBehaviour
    {
        [SerializeField]
        AbstractMap _map;

        [SerializeField]
        [Geocode]
        string[] _locationStrings;

        Vector2d[] _locations;

        [SerializeField]
        float _spawnScale = 100f;

        [SerializeField]
        float yOffset = -2f; // 👈 Valor editable desde el Inspector

        [SerializeField]
        GameObject _markerPrefab;

        List<GameObject> _spawnedObjects;

        void Start()
        {
            _locations = new Vector2d[_locationStrings.Length];
            _spawnedObjects = new List<GameObject>();

            for (int i = 0; i < _locationStrings.Length; i++)
            {
                var locationString = _locationStrings[i];
                _locations[i] = Conversions.StringToLatLon(locationString);

                var instance = Instantiate(_markerPrefab);
                var pointer = instance.GetComponentInChildren<EventPointer>();

                if (pointer != null)
                {
                    pointer.eventPose = _locations[i];
                    pointer.eventID = i + 1;
                }

                Vector3 pos = _map.GeoToWorldPosition(_locations[i], true);
                pos.y += yOffset; // 👈 Aquí lo bajamos visualmente
                instance.transform.localPosition = pos;

                instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
                _spawnedObjects.Add(instance);
            }
        }

        void Update()
        {
            int count = _spawnedObjects.Count;
            for (int i = 0; i < count; i++)
            {
                var spawnedObject = _spawnedObjects[i];
                var location = _locations[i];

                Vector3 pos = _map.GeoToWorldPosition(location, true);
                pos.y += yOffset; // 👈 también lo aplicamos en Update
                spawnedObject.transform.localPosition = pos;

                spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
            }
        }
    }
}
