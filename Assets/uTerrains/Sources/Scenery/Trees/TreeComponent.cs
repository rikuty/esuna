using UnityEngine;

namespace UltimateTerrains
{
    public sealed class TreeComponent : MonoBehaviour
    {
        public bool IsFree { get; set; }
        public int Index { get; set; }
        [SerializeField] public bool RemoveOnAffectedByOperation = true;

        public void Awake()
        {
            // No need for update
            enabled = false;
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        // This method is called when the tree is affected by an operation on the terrain.
        // WORKS ONLY IF THERE IS A COLLIDER ON THE GAMEOBJECT
        public void OnAffectedByTerrainOperation(IOperation operation)
        {
            if (RemoveOnAffectedByOperation) {
                TreesObjectPools.Free(this);
            }
        }
    }
}