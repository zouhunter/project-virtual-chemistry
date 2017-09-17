
using UnityEngine;
using System.Collections.Generic;

public enum MegaVolumeType
{
	Box,
	Sphere,
}

[System.Serializable]
public class MegaVolume
{
	public MegaVolume()
	{
		//curve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f));

		//offamount = Vector3.one;
		//sclamount = Vector3.one;
		//axis = MegaAxis.X;
		falloff = 1.0f;
		enabled = true;
		weight = 1.0f;
		name = "None";
		uselimits = false;
	}

	//public AnimationCurve	curve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f));

	//public Vector3		offamount = Vector3.one;
	//public Vector3		sclamount = Vector3.one;
	//public MegaAxis		axis = MegaAxis.X;
	public bool				enabled = true;
	public float			weight = 1.0f;
	public string			name = "None";
	public Color			regcol = Color.yellow;
	public Vector3			origin = Vector3.zero;
	public Vector3			boxsize = Vector3.one;
	// need type ie box or sphere

	public float			falloff = 1.0f;
	public MegaVolumeType	volType = MegaVolumeType.Sphere;
	public float			radius = 1.0f;

	public bool				uselimits = false;
	public Vector3			size = Vector3.zero;

	public Transform		target;

	static public MegaVolume Create()
	{
		MegaVolume vol = new MegaVolume();
		return vol;
	}
}

[AddComponentMenu("Modifiers/Selection/Multi Volume")]
public class MegaMultiVolSelect : MegaSelectionMod
{
	public override MegaModChannel ChannelsReq() { return MegaModChannel.Col | MegaModChannel.Verts; }

	public override string ModName()	{ return "Multi Vol Select"; }
	public override string GetHelpURL() { return "?page_id=3904"; }

	float[]	modselection;

	public float[] GetSel() { return modselection; }

	//public Vector3	origin = Vector3.zero;
	//public float	falloff = 1.0f;
	//public float	radius = 1.0f;
	public Color	gizCol = new Color(0.5f, 0.5f, 0.5f, 0.25f);
	public float	gizSize = 0.01f;
	public bool		useCurrentVerts = true;
	public bool		displayWeights = true;

	public bool		freezeSelection = false;

	public List<MegaVolume>	volumes = new List<MegaVolume>();

	float GetDistBox(MegaVolume vol, Vector3 p)
	{
		// Work in the box's coordinate system.
		Vector3 diff = p - vol.origin;

		// Compute squared distance and closest point on box.
		float sqrDistance = 0.0f;
		float delta;

		Vector3 closest = diff;
		//closest.x = diff.x;	//Vector3.Dot(diff, Vector3.right);

		if ( closest.x < -vol.boxsize.x )
		{
			delta = closest.x + vol.boxsize.x;
			sqrDistance += delta * delta;
			closest.x = -vol.boxsize.x;
		}
		else
		{
			if ( closest.x > vol.boxsize.x )
			{
				delta = closest.x - vol.boxsize.x;
				sqrDistance += delta * delta;
				closest.x = vol.boxsize.x;
			}
		}

		//closest.x = diff.x;	//Vector3.Dot(diff, Vector3.right);

		if ( closest.y < -vol.boxsize.y )
		{
			delta = closest.y + vol.boxsize.y;
			sqrDistance += delta * delta;
			closest.y = -vol.boxsize.y;
		}
		else
		{
			if ( closest.y > vol.boxsize.y )
			{
				delta = closest.y - vol.boxsize.y;
				sqrDistance += delta * delta;
				closest.y = vol.boxsize.y;
			}
		}

		//closest.x = diff.x;	//Vector3.Dot(diff, Vector3.right);

		if ( closest.z < -vol.boxsize.z )
		{
			delta = closest.z + vol.boxsize.z;
			sqrDistance += delta * delta;
			closest.z = -vol.boxsize.z;
		}
		else
		{
			if ( closest.z > vol.boxsize.z )
			{
				delta = closest.z - vol.boxsize.z;
				sqrDistance += delta * delta;
				closest.z = vol.boxsize.z;
			}
		}

		return Mathf.Sqrt(sqrDistance);	// * 0.5f;
	}


	public override void GetSelection(MegaModifiers mc)
	{
		if ( modselection == null || modselection.Length != mc.verts.Length )
		{
			modselection = new float[mc.verts.Length];
		}

		int volcount = 0;

		if ( !freezeSelection )
		{
			if ( volumes != null && volumes.Count > 0 )
			{
				for ( int v = 0; v < volumes.Count; v++ )
				{
					MegaVolume vol = volumes[v];

					if ( vol.enabled )
					{
						Vector3 origin = Vector3.zero;

						if ( vol.target )
							origin = transform.worldToLocalMatrix.MultiplyPoint(vol.target.position);
						else
							origin = vol.origin;

						vol.origin = origin;

						if ( volcount == 0 )
						{
							if ( vol.volType == MegaVolumeType.Sphere )
							{
								if ( useCurrentVerts )
								{
									for ( int i = 0; i < verts.Length; i++ )
									{
										float d = Vector3.Distance(origin, verts[i]) - vol.radius;

										if ( d < 0.0f )
											modselection[i] = vol.weight;
										else
										{
											float w = Mathf.Exp(-vol.falloff * Mathf.Abs(d));
											modselection[i] = w * vol.weight;	//mc.cols[i][c];
										}
									}
								}
								else
								{
									for ( int i = 0; i < verts.Length; i++ )
									{
										float d = Vector3.Distance(origin, verts[i]) - vol.radius;

										if ( d < 0.0f )
											modselection[i] = vol.weight;
										else
										{
											float w = Mathf.Exp(-vol.falloff * Mathf.Abs(d));
											modselection[i] = w * vol.weight;	//mc.cols[i][c];
										}
									}
								}
							}
							else
							{
								if ( useCurrentVerts )
								{
									for ( int i = 0; i < verts.Length; i++ )
									{
										float d = GetDistBox(vol, verts[i]);

										if ( d < 0.0f )
											modselection[i] = vol.weight;
										else
										{
											float w = Mathf.Exp(-vol.falloff * Mathf.Abs(d));
											if ( w > 1.0f )
												w = 1.0f;
											modselection[i] = w * vol.weight;	//mc.cols[i][c];
										}
									}
								}
								else
								{
									for ( int i = 0; i < verts.Length; i++ )
									{
										float d = GetDistBox(vol, verts[i]);

										//float wg = modselection[i];

										if ( d < 0.0f )
											modselection[i] = vol.weight;
										else
										{
											float w = Mathf.Exp(-vol.falloff * Mathf.Abs(d));
											if ( w > 1.0f )
												w = 1.0f;
											modselection[i] = w * vol.weight;	//mc.cols[i][c];
										}
									}
								}
							}
						}
						else
						{
							if ( vol.volType == MegaVolumeType.Box )
							{
								if ( useCurrentVerts )
								{
									for ( int i = 0; i < verts.Length; i++ )
									{
										float d = GetDistBox(vol, verts[i]);

										float wg = modselection[i];
										if ( d < 0.0f )
											wg += vol.weight;
										else
										{
											float w = Mathf.Exp(-vol.falloff * Mathf.Abs(d));
											wg += w * vol.weight;	//mc.cols[i][c];
										}

										if ( wg > 1.0f )
											modselection[i] = 1.0f;
										else
											modselection[i] = wg;
									}
								}
								else
								{
									for ( int i = 0; i < verts.Length; i++ )
									{
										float d = GetDistBox(vol, verts[i]);

										float wg = modselection[i];

										if ( d < 0.0f )
											wg += vol.weight;
										else
										{
											float w = Mathf.Exp(-vol.falloff * Mathf.Abs(d));
											wg += w * vol.weight;	//mc.cols[i][c];
										}

										if ( wg > 1.0f )
											modselection[i] = 1.0f;
										else
											modselection[i] = wg;
									}
								}
							}
							else
							{
								if ( useCurrentVerts )
								{
									for ( int i = 0; i < verts.Length; i++ )
									{
										float d = Vector3.Distance(origin, verts[i]) - vol.radius;

										float wg = modselection[i];
										if ( d < 0.0f )
											wg += vol.weight;
										else
										{
											float w = Mathf.Exp(-vol.falloff * Mathf.Abs(d));
											wg += w * vol.weight;	//mc.cols[i][c];
										}

										if ( wg > 1.0f )
											modselection[i] = 1.0f;
										else
											modselection[i] = wg;
									}
								}
								else
								{
									for ( int i = 0; i < verts.Length; i++ )
									{
										float d = Vector3.Distance(origin, verts[i]) - vol.radius;

										float wg = modselection[i];

										if ( d < 0.0f )
											wg += vol.weight;
										else
										{
											float w = Mathf.Exp(-vol.falloff * Mathf.Abs(d));
											wg += w * vol.weight;	//mc.cols[i][c];
										}

										if ( wg > 1.0f )
											modselection[i] = 1.0f;
										else
											modselection[i] = wg;
									}
								}
							}
						}

						volcount++;
					}
				}
			}

			if ( volcount == 0 )
			{
				for ( int i = 0; i < verts.Length; i++ )
				{
					modselection[i] = 0.0f;
				}
			}
		}

		if ( (mc.dirtyChannels & MegaModChannel.Verts) == 0 )
		{
			mc.InitVertSource();
		}

		mc.selection = modselection;
	}

	public override void DrawGizmo(MegaModContext context)
	{
		if ( ModEnabled )
		{
			base.DrawGizmo(context);

			Matrix4x4 tm = gameObject.transform.localToWorldMatrix;
			Gizmos.matrix = tm;
			for ( int i = 0; i < volumes.Count; i++ )
			{
				if ( volumes[i].enabled && volumes[i].volType == MegaVolumeType.Box )
				{
					Gizmos.color = volumes[i].regcol;	//Color.yellow;
					Gizmos.DrawWireCube(volumes[i].origin, volumes[i].boxsize * 2.0f);	// * 0.5f);
				}

				if ( volumes[i].enabled && volumes[i].volType == MegaVolumeType.Sphere )
				{
					Gizmos.color = volumes[i].regcol;	//Color.yellow;
					Gizmos.DrawWireSphere(volumes[i].origin, volumes[i].radius);	// * 0.5f);
				}
			}
			Gizmos.matrix = Matrix4x4.identity;
		}
	}
}
