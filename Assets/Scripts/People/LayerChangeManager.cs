using System.Collections.Generic;
using UnityEngine;

public class LayerChangeManager : MonoBehaviour
{
    private static LayerChangeManager _instance;
    public static LayerChangeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LayerChangeManager>();
                if (_instance == null)
                {
                    GameObject manager = new GameObject("LayerChangeManager");
                    _instance = manager.AddComponent<LayerChangeManager>();
                }
            }
            return _instance;
        }
    }
    public string[] layers = { "Floor 2 L", "Floor 2 R", "Floor 3 L", "Floor 3 R", "Floor 4 L", "Floor 4 R" }; // 추가되는 Layer는 여기에 넣기
    private Dictionary<int, int[]> layerMapping;

    private void Awake()
    {
        layerMapping = new Dictionary<int, int[]>()
        {
            { LayerMask.NameToLayer("Floor 1"), new int[] { LayerMask.NameToLayer("Floor 2"), LayerMask.NameToLayer("Floor 1") } },
            { LayerMask.NameToLayer("Floor 2"), new int[] { LayerMask.NameToLayer("Floor 3"), LayerMask.NameToLayer("Floor 2") } },
            { LayerMask.NameToLayer("Floor 3"), new int[] { LayerMask.NameToLayer("Floor 4"), LayerMask.NameToLayer("Floor 3") } },
            { LayerMask.NameToLayer("Floor 1 L"), new int[] { LayerMask.NameToLayer("Floor 1 L"), LayerMask.NameToLayer("Floor 1") } },
            { LayerMask.NameToLayer("Floor 1 R"), new int[] { LayerMask.NameToLayer("Floor 1 R"), LayerMask.NameToLayer("Floor 1") } },
            { LayerMask.NameToLayer("Floor 2 L"), new int[] { LayerMask.NameToLayer("Floor 2 L"), LayerMask.NameToLayer("Floor 2") } },
            { LayerMask.NameToLayer("Floor 2 R"), new int[] { LayerMask.NameToLayer("Floor 2 R"), LayerMask.NameToLayer("Floor 2") } },
            { LayerMask.NameToLayer("Floor 3 L"), new int[] { LayerMask.NameToLayer("Floor 3 L"), LayerMask.NameToLayer("Floor 3") } },
            { LayerMask.NameToLayer("Floor 3 R"), new int[] { LayerMask.NameToLayer("Floor 3 R"), LayerMask.NameToLayer("Floor 3") } },
            { LayerMask.NameToLayer("Floor 4 L"), new int[] { LayerMask.NameToLayer("Floor 4 L"), LayerMask.NameToLayer("Floor 4") } },
            { LayerMask.NameToLayer("Floor 4 R"), new int[] { LayerMask.NameToLayer("Floor 4 R"), LayerMask.NameToLayer("Floor 4") } },
        };
    }
    public void ChangeLayerBasedOnCollider(GameObject npc, GameObject colliderObject, int waypointIndex)
    {
        int colliderLayer = colliderObject.layer;

        if (layerMapping.TryGetValue(colliderLayer, out int[] layers))
        {
            if (waypointIndex == 1)
            {
                SetLayerRecursively(npc, layers[0]);
            }
            else if (waypointIndex == 4 || npc.GetComponent<PatientController>().isExiting)
            {
                SetLayerRecursively(npc, layers[1]);
            }
            else
            {
                Debug.LogWarning($"Invalid waypointIndex {waypointIndex} for NPC {npc.name}");
            }
        }
        else
        {
            Debug.LogWarning($"Layer {colliderLayer} not found in layer mapping.");
        }
    }

    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child != null)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}
