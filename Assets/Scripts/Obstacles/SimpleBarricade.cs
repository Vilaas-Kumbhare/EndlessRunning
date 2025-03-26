using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SimpleBarricade : Obstacle
{
    protected const int k_MinObstacleCount = 1;
    protected const int k_MaxObstacleCount = 2;
    protected const int k_LeftMostLaneIndex = -1;
    protected const int k_RightMostLaneIndex = 1;

    public override IEnumerator Spawn(TrackSegment segment, float t)
    {
        // The tutorial's very first barricade needs to be centered and alone,
        // so the player can swipe safely in both directions to avoid it.
        bool isTutorialFirst = TrackManager.instance.isTutorial && TrackManager.instance.firstObstacle && segment == segment.manager.currentSegment;

        if (isTutorialFirst)
            TrackManager.instance.firstObstacle = false;

        int count = isTutorialFirst ? 1 : Random.Range(k_MinObstacleCount, k_MaxObstacleCount + 1);
        int startLane = isTutorialFirst ? 0 : Random.Range(k_LeftMostLaneIndex, k_RightMostLaneIndex + 1);

        Vector3 position;
        Quaternion rotation;
        segment.GetPointAt(t, out position, out rotation);

        for (int i = 0; i < count; ++i)
        {
            int lane = startLane + i;
            lane = lane > k_RightMostLaneIndex ? k_LeftMostLaneIndex : lane;

            Debug.Log("obstacles :: " + gameObject.name);

            // ✅ Load the obstacle prefab from Resources
            GameObject prefab = Resources.Load<GameObject>("Themes/Default/Obstacles/" + gameObject.name);

            if (prefab == null)
            {
                Debug.LogWarning($"Unable to load obstacle {gameObject.name} from Resources.");
                yield break;
            }

            // ✅ Instantiate the obstacle
            GameObject obj = Instantiate(prefab, position, rotation);

            if (obj == null)
            {
                Debug.LogError($"Failed to instantiate {gameObject.name}");
            }
            else
            {
                // ✅ Adjust the position based on lane
                obj.transform.position += obj.transform.right * lane * segment.manager.laneOffset;

                // ✅ Parent to track segment
                obj.transform.SetParent(segment.objectRoot, true);

                // ✅ Preserve initial position (Fixes issue #7)
                Vector3 oldPos = obj.transform.position;
                obj.transform.position += Vector3.back;
                obj.transform.position = oldPos;
            }
        }
    }

}
