
using UnityEngine;
using System.Collections.Generic;

// Beta modifier
[AddComponentMenu("Modifiers/Deformable")]
public class MegaDeformable : MegaModifier
{
	public override string ModName() { return "Deformable"; }

	public override void ModStart(MegaModifiers mc)
	{
		Vector3 s = mc.mesh.bounds.size;
		sizeFactor = Mathf.Min(s.x, s.y, s.z);

		if ( mc.mesh.colors != null )
			baseColors = mc.mesh.colors;

		LoadMap(mc);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		if ( offsets == null || offsets.Length != mc.mod.verts.Length )
			offsets = new Vector3[mc.mod.verts.Length];

		return true;
	}

	public override MegaModChannel ChannelsReq() { return MegaModChannel.Verts | MegaModChannel.Col; }
	public override MegaModChannel ChannelsChanged() { return MegaModChannel.Verts | MegaModChannel.Col; }

	public float			Hardness = 0.5f;	// Impact resistance to calculate amount of deformation applied to the mesh	
	public bool				DeformMeshCollider = true;	// Configure if the mesh at collider must also be deformed (only works if a MeshCollider is in use)
	public float			UpdateFrequency = 0.0f;	// Configure the mesh (and mesh collider) update frequency in times per second. 0 for real time (high CPU usage)
	public float			MaxVertexMov = 0.0f;	// Maximum movement allowed for a vertex from its original position (0 means no limit)
	public Color32			DeformedVertexColor = Color.gray;	// Color to be applied at deformed vertices (only works for shaders that handle vertices colors)
	public Texture2D		HardnessMap;	// Texture to modulate maximum movement allowed (uses alpha channel)

	public bool				usedecay = false;
	public float			decay = 0.999f;
	public Color[]			baseColors;	// Backup of original mesh vertices colors
	public float			sizeFactor;	// Size factor of mesh
	public float[]			map;
	public Vector3[]		offsets;
	public float			impactFactor = 0.1f;
	public float			ColForce = 0.5f;

	private void LoadMesh()
	{
	}

	public void LoadMap()
	{
		MegaModifiers mc = GetComponent<MegaModifyObject>();
		if ( mc )
		{
			LoadMap(mc);
		}
	}

	private void LoadMap(MegaModifiers mc)
	{
		// Load hardness map
		if ( HardnessMap )
		{
			Vector2[] uvs = mc.mesh.uv;
			map = new float[uvs.Length];

			for ( int c = 0; c < uvs.Length; c++ )
			{
				Vector2 uv = uvs[c];
				map[c] = HardnessMap.GetPixelBilinear(uv.x, uv.y).a;
			}
		}
		else
			map = null;
	}

	public override void Modify(MegaModifiers mc)
	{
		// Calc collision force	
		float colForce = ColForce;//Mathf.Min(1, collision.impactForceSum.sqrMagnitude / 1000);

		sizeFactor = mc.bbox.size.magnitude;
		// distFactor is the amount of deforming in local coordinates
		float distFactor = colForce * (sizeFactor * (impactFactor / Mathf.Max(impactFactor, Hardness)));

		// Deform process
		if ( colpoints != null )
		{
			//Debug.Log("points " + colpoints.Length);
			//Debug.Log("size " + mc.bbox.size);	//sizeFactor);

			for ( int c = 0; c < colpoints.Length; c++ )
			{
				ContactPoint contact = colpoints[c];

				for ( int i = 0; i < verts.Length; i++ )
				{
					// Apply deformation only on vertex inside "blast area"
					Vector3 vp = verts[i] + offsets[i];

					Vector3 p = transform.InverseTransformPoint(contact.point);
					float d  = (vp - p).sqrMagnitude;
					if ( d <= distFactor )
					{
						// Deformation is the collision normal with local deforming ratio
						// Vertices near the impact point must also be more deformed
						Vector3 n = transform.InverseTransformDirection(contact.normal * (1.0f - (d / distFactor)) * distFactor);

						// Apply hardness map if any
						if ( map != null )
							n *= 1.0f - map[i];

						// Deform vertex
						//vertices[i] += n;
						offsets[i] += n;

						// Apply max vertex movement if configured
						// Here the deformed vertex position is just scaled down to keep the best deformation while respecting limits
						if ( MaxVertexMov > 0.0f )
						{
							float max = MaxVertexMov;
							n = offsets[i];	//vertices[i] - baseVertices[i];
							d = n.magnitude;
							if ( d > max )
								offsets[i] = (n * (max / d));

							// Apply vertex color deformation
							// Deform color is applied in proportional amount with vertex distance and max deform
							if ( baseColors.Length > 0 )
							{
								d = (d / MaxVertexMov);
								mc.cols[i] = Color.Lerp(baseColors[i], DeformedVertexColor, d);
							}
						}
						else
						{
							if ( mc.cols.Length > 0 )
							{
								// Apply vertex color deformation
								// Deform color is applied in proportional amount with vertex distance and mesh distance factor
								mc.cols[i] = Color.Lerp(baseColors[i], DeformedVertexColor, offsets[i].magnitude / (distFactor * 10.0f));
							}
						}
					}
				}
			}
		}

		colpoints = null;

		if ( !usedecay )
		{
			for ( int i = 0; i < verts.Length; i++ )
			{
				sverts[i].x = verts[i].x + offsets[i].x;
				sverts[i].y = verts[i].y + offsets[i].y;
				sverts[i].z = verts[i].z + offsets[i].z;
			}
		}
		else
		{
			for ( int i = 0; i < verts.Length; i++ )
			{
				offsets[i].x *= decay;
				offsets[i].y *= decay;
				offsets[i].z *= decay;

				sverts[i].x = verts[i].x + offsets[i].x;
				sverts[i].y = verts[i].y + offsets[i].y;
				sverts[i].z = verts[i].z + offsets[i].z;
			}
		}
	}

	// We could find the megamod comp
	public void Repair(float repair, MegaModifiers mc)
	{
		Repair(repair, Vector3.zero, 0.0f, mc);
	}

	public void Repair(float repair, Vector3 point, float radius, MegaModifiers mc)
	{
		// Check mesh assigned
		if ( (!mc.mesh) )	//|| (!meshFilter) )
			return;

		// Transform world point to mesh local
		point = transform.InverseTransformPoint(point);

		// Repairing is done returning vertices position and color to original positions by requested amount
		//int i = 0;

		for ( int i = 0; i < mc.verts.Length; i++ )
		{
			// Check for repair inside radius
			if ( radius > 0.0f )
			{
				if ( (point - mc.sverts[i]).magnitude >= radius )
					continue;
			}

			// Repair
			Vector3 n = offsets[i];	//vertices[i] - baseVertices[i];
			offsets[i] = n * (1.0f - repair);
			if ( baseColors.Length > 0 )
				mc.cols[i] = Color.Lerp(mc.cols[i], baseColors[i], repair);
		}
	}

	//public bool	doDeform = false;
	public MegaModifyObject	modobj;
	ContactPoint[]	colpoints;

	void OnCollisionEnter(Collision collision)
	{
		if ( modobj == null )
			modobj = GetComponent<MegaModifyObject>();

		if ( modobj )
			modobj.Enabled = true;

		colpoints = collision.contacts;
		//doDeform = true;
	}

	void OnCollisionStay(Collision collision)
	{
		colpoints = collision.contacts;
		//doDeform = true;
	}

	void OnCollisionExit(Collision collision)
	{
		if ( modobj )
		{
			if ( !usedecay )
				modobj.Enabled = false;
		}
	}
}