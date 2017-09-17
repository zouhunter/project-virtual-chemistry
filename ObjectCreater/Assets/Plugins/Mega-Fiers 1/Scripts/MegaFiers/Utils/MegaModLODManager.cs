
using UnityEngine;

using System.Collections;

[System.Serializable]
public class MegaModLOD
{
	public Mesh mesh;
	public float distance;
}

public class MegaModLODManager : MonoBehaviour
{
	public GameObject theCamera;
	public GameObject meshContainer;
	public MegaModLOD[] levelsOfDetail;
	public int currentLOD = -1;

	private float lastLODCheckTime = 0;

	// Change this value to specify how many seconds elapsed to check LOD change
	private float LODCheckInterval = 0.2f;

	void Update()
	{
		// Not need to check every frame
		if ( (Time.time - lastLODCheckTime) > LODCheckInterval )
		{
			float distanceToCamera = Vector3.Distance(transform.position, theCamera.transform.position);
			int selectedLOD = levelsOfDetail.Length - 1;
			for ( int i = levelsOfDetail.Length - 1; i >= 0; i-- )
			{
				if ( distanceToCamera > levelsOfDetail[i].distance )
				{
					selectedLOD = i;
					break;
				}
			}

			if ( selectedLOD != currentLOD )
			{
				currentLOD = selectedLOD;
				MegaModifyObject modifyObject;
				modifyObject = meshContainer.GetComponent<MegaModifyObject>();
				if ( modifyObject != null )
				{
					// Change meshFilter to use new mesh
					meshContainer.GetComponent<MeshFilter>().mesh = levelsOfDetail[selectedLOD].mesh;
					modifyObject.MeshUpdated();

					// Update modifiers stack state depending on its MaxLOD specified in Unity Editor
					foreach ( MegaModifier m in modifyObject.mods )
					{
						m.ModEnabled = (m.MaxLOD >= currentLOD);
					}
				}
			}
			lastLODCheckTime = Time.time;
		}
	}
}