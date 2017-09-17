
using UnityEngine;

#if !UNITY_METRO && !UNITY_WP8
public class MegaSpawnTest : MonoBehaviour
{
	public GameObject	spawnobj;
	public Vector3		range = new Vector3(4.0f, 0.0f, 4.0f);
	public float		time = 1.73170f;

	float t = 0.0f;

	void Update()
	{
		t += Time.deltaTime;

		if ( t > time )
		{
			t -= time;
			Vector3 pos = Vector3.zero;

			pos.x = Random.Range(-range.x, range.x);
			pos.y = Random.Range(-range.y, range.y);
			pos.z = Random.Range(-range.z, range.z);

			GameObject newobj = MegaCopyObject.DeepCopy(spawnobj);
			newobj.transform.position = pos;

			MegaMorph mor = newobj.GetComponent<MegaMorph>();
			if ( mor )
			{
				mor.animtime = 0.0f;
			}
		}
	}
}
#endif