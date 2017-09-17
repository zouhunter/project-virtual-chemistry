
using UnityEngine;

[AddComponentMenu("Modifiers/Selection/Material")]
public class MegaMatSelect : MegaSelectionMod
{
	public override MegaModChannel ChannelsReq() { return MegaModChannel.Tris; }
	public override string ModName() { return "Material Select"; }
	public override string GetHelpURL() { return "?page_id=1305"; }

	public int matnum = 0;

	float[]	modselection;

	public float[] GetSel() { return modselection; }

	public float	gizSize = 0.01f;
	public bool		displayWeights = true;
	public bool		update = true;
	public float	weight = 1.0f;
	public float	otherweight = 0.0f;

	public override void GetSelection(MegaModifiers mc)
	{
		if ( ModEnabled )
		{
			if ( modselection == null || modselection.Length != mc.verts.Length )
				modselection = new float[mc.verts.Length];

			if ( update )
			{
				update = false;

				if ( matnum < 0 )
					matnum = 0;

				if ( matnum >= mc.mesh.subMeshCount )
					matnum = mc.mesh.subMeshCount - 1;

				int[] tris = mc.mesh.GetTriangles(matnum);

				for ( int i = 0; i < modselection.Length; i++ )
					modselection[i] = otherweight;

				for ( int i = 0; i < tris.Length; i++ )
					modselection[tris[i]] = weight;
			}

			mc.selection = modselection;
		}
	}
}