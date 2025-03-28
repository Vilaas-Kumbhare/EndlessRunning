﻿using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PatrollingObstacle : Obstacle
{
	static int s_SpeedRatioHash = Animator.StringToHash("SpeedRatio");
	static int s_DeadHash = Animator.StringToHash("Dead"); 

	[Tooltip("Minimum time to cross all lanes.")]
    public float minTime = 2f;
    [Tooltip("Maximum time to cross all lanes.")]
    public float maxTime = 5f;
	[Tooltip("Leave empty if no animation")]
	public Animator animator;

	public AudioClip[] patrollingSound;

	protected TrackSegment m_Segement;

	protected Vector3 m_OriginalPosition = Vector3.zero;
	protected float m_MaxSpeed;
	protected float m_CurrentPos;

	protected AudioSource m_Audio;
    private bool m_isMoving = false;

    protected const float k_LaneOffsetToFullWidth = 2f;

    public override IEnumerator Spawn(TrackSegment segment, float t)
    {
        Vector3 position;
        Quaternion rotation;
        segment.GetPointAt(t, out position, out rotation);

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
        obj.transform.SetParent(segment.objectRoot, true);

        // ✅ Assign TrackSegment to PatrollingObstacle component
        PatrollingObstacle po = obj.GetComponent<PatrollingObstacle>();
        if (po != null)
        {
            po.m_Segement = segment;
            po.Setup();
        }

        // ✅ Preserve initial position
        Vector3 oldPos = obj.transform.position;
        obj.transform.position += Vector3.back;
        obj.transform.position = oldPos;
    }

    public override void Setup()
	{
		m_Audio = GetComponent<AudioSource>();
		if(m_Audio != null && patrollingSound != null && patrollingSound.Length > 0)
		{
			m_Audio.loop = true;
			m_Audio.clip = patrollingSound[Random.Range(0,patrollingSound.Length)];
			m_Audio.Play();
		}

		m_OriginalPosition = transform.localPosition + transform.right * m_Segement.manager.laneOffset;
		transform.localPosition = m_OriginalPosition;

		float actualTime = Random.Range(minTime, maxTime);

        //time 2, becaus ethe animation is a back & forth, so we need the speed needed to do 4 lanes offset in the given time
        m_MaxSpeed = (m_Segement.manager.laneOffset * k_LaneOffsetToFullWidth * 2) / actualTime;

		if (animator != null)
		{
			AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
            animator.SetFloat(s_SpeedRatioHash, clip.length / actualTime);
		}

	    m_isMoving = true;
	}

	public override void Impacted()
	{
	    m_isMoving = false;
		base.Impacted();

		if (animator != null)
		{
			animator.SetTrigger(s_DeadHash);
		}
	}

	void Update()
	{
		if (!m_isMoving)
			return;

		m_CurrentPos += Time.deltaTime * m_MaxSpeed;

        transform.localPosition = m_OriginalPosition - transform.right * Mathf.PingPong(m_CurrentPos, m_Segement.manager.laneOffset * k_LaneOffsetToFullWidth);
	}
}
