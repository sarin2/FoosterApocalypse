using _NM.Core.Character;
using UnityEngine;

namespace _NM.Core.UI.Minimap
{
    public class Player_Icon : MonoBehaviour
    {
        // Start is called before the first frame update

        private MeshFilter _meshFilter;
        public Vector3[] _vec;
        public Vector3 offset;
        [Range(0, 1), SerializeField] private float x,z;
        void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _vec = new Vector3[]
            {
                new (x, 0, -z), new (-x, 0, -z), new(0, 0, z)
            };
        
        }

        void OnVariableChange()
        {
            _vec = new Vector3[]
            {
                new (x, 0, -z), new (-x, 0, -z), new(0, 0, z+0.2f)
            };
        }

        private void Update()
        {
            if (x != _vec[0].x || z != _vec[2].z)
            {
                OnVariableChange();
            }
            
            _meshFilter.mesh.vertices = _vec;
            _meshFilter.mesh.triangles = new int[]
            {
                0, 1, 2
            };

            Transform playerPos = Core.Character.Character.Local.transform;
        }
    }
}
