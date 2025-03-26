using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AllLaneObstacle: Obstacle
{
	public override IEnumerator Spawn(TrackSegment segment, float t)
	{
        Vector3 position;
        Quaternion rotation;
        segment.GetPointAt(t, out position, out rotation);

        Debug.Log("obstacles :: " + gameObject.name);

        // Load prefab from Resources
        GameObject prefab = Resources.Load<GameObject>("Themes/Default/Obstacles/" + gameObject.name);

        if (prefab == null)
        {
            Debug.LogWarning($"Unable to load obstacle {gameObject.name} from Resources.");
            yield break;
        }

        // Instantiate the loaded prefab
        GameObject obj = Instantiate(prefab, position, rotation);
        obj.transform.SetParent(segment.objectRoot, true);

        // TODO: Remove this hack related to #issue7
        Vector3 oldPos = obj.transform.position;
        obj.transform.position += Vector3.back;
        obj.transform.position = oldPos;
    }
}
