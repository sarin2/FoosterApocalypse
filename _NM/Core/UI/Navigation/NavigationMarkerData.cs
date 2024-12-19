using UnityEngine;

namespace _NM.Core.UI.Navigation
{
    public class NavigationMarkerData
    {
        public string Key;
        public Vector3 Position;
        public ENavigationType Type;
        
        public static readonly NavigationMarkerData StageMarker = new ()
        {
            Key = "Stage",
            Position = new Vector3(-13.1f, 3.26f, 51.9f),
            Type = ENavigationType.Quest
        };
        public static readonly NavigationMarkerData TruckMarker = new ()
        {
            Key = "Truck",
            Position = new Vector3(2.03f, 2.26f, 18.88f),
            Type = ENavigationType.Quest
        };
    }
}
