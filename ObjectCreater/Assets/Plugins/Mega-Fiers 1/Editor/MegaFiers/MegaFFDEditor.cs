
using UnityEngine;
using UnityEditor;

public class MegaFFDEditor : MegaModifierEditor
{
	Vector3 pm = new Vector3();

	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\ffd_help.png"); }

	bool showpoints = false;

	public override bool Inspector()
	{
		MegaFFD mod = (MegaFFD)target;

		EditorGUIUtility.LookLikeControls();
		mod.KnotSize = EditorGUILayout.FloatField("Knot Size", mod.KnotSize);
		mod.inVol = EditorGUILayout.Toggle("In Vol", mod.inVol);

		handles = EditorGUILayout.Toggle("Handles", handles);
		handleSize = EditorGUILayout.Slider("Size", handleSize, 0.0f, 1.0f);
		mirrorX = EditorGUILayout.Toggle("Mirror X", mirrorX);
		mirrorY = EditorGUILayout.Toggle("Mirror Y", mirrorY);
		mirrorZ = EditorGUILayout.Toggle("Mirror Z", mirrorZ);

		showpoints = EditorGUILayout.Foldout(showpoints, "Points");

		if ( showpoints )
		{
			int gs = mod.GridSize();
			//int num = gs * gs * gs;

			for ( int x = 0; x < gs; x++ )
			{
				for ( int y = 0; y < gs; y++ )
				{
					for ( int z = 0; z < gs; z++ )
					{
						int i = (x * gs * gs) + (y * gs) + z;
						mod.pt[i] = EditorGUILayout.Vector3Field("p[" + x + "," + y + "," + z + "]", mod.pt[i]);
					}
				}
			}
		}
		return false;
	}

	static public float handleSize = 0.5f;
	static public bool handles = true;
	static public bool mirrorX = false;
	static public bool mirrorY = false;
	static public bool mirrorZ = false;

	public override void DrawSceneGUI()
	{
		MegaFFD ffd = (MegaFFD)target;

		bool snapshot = false;

		if ( ffd.DisplayGizmo )
		{
			MegaModifiers context = ffd.GetComponent<MegaModifiers>();

			if ( context && context.DrawGizmos )
			{
				Vector3 size = ffd.lsize;
				Vector3 osize = ffd.lsize;
				osize.x = 1.0f / size.x;
				osize.y = 1.0f / size.y;
				osize.z = 1.0f / size.z;

				Matrix4x4 tm1 = Matrix4x4.identity;
				Quaternion rot = Quaternion.Euler(ffd.gizmoRot);
				tm1.SetTRS(-(ffd.gizmoPos + ffd.Offset), rot, Vector3.one);

				Matrix4x4 tm = Matrix4x4.identity;
				Handles.matrix = Matrix4x4.identity;

				if ( context != null && context.sourceObj != null )
					tm = context.sourceObj.transform.localToWorldMatrix * tm1;
				else
					tm = ffd.transform.localToWorldMatrix * tm1;

				DrawGizmos(ffd, tm);	//Handles.matrix);

				Handles.color = Color.yellow;
	#if false
				int pc = ffd.GridSize();
				pc = pc * pc * pc;
				for ( int i = 0; i < pc; i++ )
				{
					Vector3 p = ffd.GetPoint(i);	// + ffd.bcenter;

					//pm = Handles.PositionHandle(p, Quaternion.identity);
					pm = Handles.FreeMoveHandle(p, Quaternion.identity, ffd.KnotSize * 0.1f, Vector3.zero, Handles.CircleCap);

					pm -= ffd.bcenter;
					p = Vector3.Scale(pm, osize);
					p.x += 0.5f;
					p.y += 0.5f;
					p.z += 0.5f;

					ffd.pt[i] = p;
				}
	#endif
				int gs = ffd.GridSize();
				//int i = 0;

				Vector3 ttp = Vector3.zero;

				for ( int z = 0; z < gs; z++ )
				{
					for ( int y = 0; y < gs; y++ )
					{
						for ( int x = 0; x < gs; x++ )
						{
							int index = ffd.GridIndex(x, y, z);
							//Vector3 p = ffd.GetPoint(i);	// + ffd.bcenter;
							Vector3 lp = ffd.GetPoint(index);
							Vector3 p = lp;	//tm.MultiplyPoint(lp);	//ffdi);	// + ffd.bcenter;

							Vector3 tp = tm.MultiplyPoint(p);
							if ( handles )
							{
								ttp = Handles.PositionHandle(tp, Quaternion.identity);

								//pm = tm.inverse.MultiplyPoint(Handles.PositionHandle(tm.MultiplyPoint(p), Quaternion.identity));
								//pm = PositionHandle(p, Quaternion.identity, handleSize, ffd.gizCol1.a);
							}
							else
								ttp = Handles.FreeMoveHandle(tp, Quaternion.identity, ffd.KnotSize * 0.1f, Vector3.zero, Handles.CircleCap);

							if ( ttp != tp )
							{
								if ( !snapshot )
								{
									MegaUndo.SetSnapshotTarget(ffd, "FFD Lattice Move");
									snapshot = true;
								}
							}

							pm = tm.inverse.MultiplyPoint(ttp);
							Vector3 delta = pm - p;

							//pm = lp + delta;

							//ffd.SetPoint(x, y, z, pm);
							pm -= ffd.bcenter;
							p = Vector3.Scale(pm, osize);
							p.x += 0.5f;
							p.y += 0.5f;
							p.z += 0.5f;

							ffd.pt[index] = p;

							if ( mirrorX )
							{
								int y1 = y - (gs - 1);
								if ( y1 < 0 )
									y1 = -y1;

								if ( y != y1 )
								{
									Vector3 p1 = ffd.GetPoint(ffd.GridIndex(x, y1, z));	// + ffd.bcenter;

									delta.y = -delta.y;
									p1 += delta;
									p1 -= ffd.bcenter;
									p = Vector3.Scale(p1, osize);
									p.x += 0.5f;
									p.y += 0.5f;
									p.z += 0.5f;

									ffd.pt[ffd.GridIndex(x, y1, z)] = p;
								}
							}

							if ( mirrorY )
							{
								int z1 = z - (gs - 1);
								if ( z1 < 0 )
									z1 = -z1;

								if ( z != z1 )
								{
									Vector3 p1 = ffd.GetPoint(ffd.GridIndex(x, y, z1));	// + ffd.bcenter;

									delta.z = -delta.z;
									p1 += delta;
									p1 -= ffd.bcenter;
									p = Vector3.Scale(p1, osize);
									p.x += 0.5f;
									p.y += 0.5f;
									p.z += 0.5f;

									ffd.pt[ffd.GridIndex(x, y, z1)] = p;
								}
							}

							if ( mirrorZ )
							{
								int x1 = x - (gs - 1);
								if ( x1 < 0 )
									x1 = -x1;

								if ( x != x1 )
								{
									Vector3 p1 = ffd.GetPoint(ffd.GridIndex(x1, y, z));	// + ffd.bcenter;

									delta.x = -delta.x;
									p1 += delta;
									p1 -= ffd.bcenter;
									p = Vector3.Scale(p1, osize);
									p.x += 0.5f;
									p.y += 0.5f;
									p.z += 0.5f;

									ffd.pt[ffd.GridIndex(x1, y, z)] = p;
								}
							}
						}
					}
				}

				Handles.matrix = Matrix4x4.identity;

				if ( GUI.changed && snapshot )
				{
					MegaUndo.CreateSnapshot();
					MegaUndo.RegisterSnapshot();
				}

				MegaUndo.ClearSnapshotTarget();
			}
		}
	}

	public static Vector3 PositionHandle(Vector3 position, Quaternion rotation, float size, float alpha)
	{
		float handlesize = handleSize;	//HandleUtility.GetHandleSize(position) * size;
		Color color = Handles.color;
		Color col = Color.red;
		col.a = alpha;
		Handles.color = col;	//Color.red;	//Handles..xAxisColor;
		position = Handles.Slider(position, rotation * Vector3.right, handlesize, new Handles.DrawCapFunction(Handles.ArrowCap), 0.0f);	//SnapSettings.move.x);
		col = Color.green;
		col.a = alpha;
		Handles.color = col;	//Color.green;	//Handles.yAxisColor;
		position = Handles.Slider(position, rotation * Vector3.up, handlesize, new Handles.DrawCapFunction(Handles.ArrowCap), 0.0f);	//SnapSettings.move.y);

		col = Color.blue;
		col.a = alpha;

		Handles.color = col;	//Color.blue;	//Handles.zAxisColor;
		position = Handles.Slider(position, rotation * Vector3.forward, handlesize, new Handles.DrawCapFunction(Handles.ArrowCap), 0.0f);	//SnapSettings.move.z);

		col = Color.yellow;
		col.a = alpha;

		Handles.color = col;	//Color.yellow;	//Handles.centerColor;
		position = Handles.FreeMoveHandle(position, rotation, handlesize * 0.15f, Vector3.zero, new Handles.DrawCapFunction(Handles.RectangleCap));
		Handles.color = color;
		return position;
	}

	Vector3[] pp3 = new Vector3[3];

#if false
	public void DrawGizmos(MegaFFD ffd, Matrix4x4 tm)
	{
		Handles.color = ffd.gizCol1;	//Color.red;

		int pc = ffd.GridSize();

		for ( int  i = 0; i < pc; i++ )
		{
			for ( int j = 0; j < pc; j++ )
			{
				for ( int k = 0; k < pc; k++ )
				{
					//pp3[0] = tm.MultiplyPoint(ffd.GetPoint(i, j, k));	// + ffd.bcenter);
					pp3[0] = ffd.GetPoint(i, j, k);	// + ffd.bcenter);

					if ( i < pc - 1 )
					{
						//pp3[1] = tm.MultiplyPoint(ffd.GetPoint(i + 1, j, k));	// + ffd.bcenter);
						pp3[1] = ffd.GetPoint(i + 1, j, k);	// + ffd.bcenter);
						Handles.DrawLine(pp3[0], pp3[1]);
					}

					if ( j < pc - 1 )
					{
						//pp3[1] = tm.MultiplyPoint(ffd.GetPoint(i, j + 1, k));	// + ffd.bcenter);
						pp3[1] = ffd.GetPoint(i, j + 1, k);	// + ffd.bcenter);
						Handles.DrawLine(pp3[0], pp3[1]);
					}

					if ( k < pc - 1 )
					{
						//pp3[1] = tm.MultiplyPoint(ffd.GetPoint(i, j, k + 1));	// + ffd.bcenter);
						pp3[1] = ffd.GetPoint(i, j, k + 1);	// + ffd.bcenter);
						Handles.DrawLine(pp3[0], pp3[1]);
					}
				}
			}
		}
	}
#else
	public void DrawGizmos(MegaFFD ffd, Matrix4x4 tm)
	{
		Handles.color = ffd.gizCol1;	//Color.red;

		int pc = ffd.GridSize();

		for ( int  i = 0; i < pc; i++ )
		{
			for ( int j = 0; j < pc; j++ )
			{
				for ( int k = 0; k < pc; k++ )
				{
					pp3[0] = tm.MultiplyPoint(ffd.GetPoint(i, j, k));	// + ffd.bcenter);
					//pp3[0] = ffd.GetPoint(i, j, k);	// + ffd.bcenter);

					if ( i < pc - 1 )
					{
						pp3[1] = tm.MultiplyPoint(ffd.GetPoint(i + 1, j, k));	// + ffd.bcenter);
						//pp3[1] = ffd.GetPoint(i + 1, j, k);	// + ffd.bcenter);
						Handles.DrawLine(pp3[0], pp3[1]);
					}

					if ( j < pc - 1 )
					{
						pp3[1] = tm.MultiplyPoint(ffd.GetPoint(i, j + 1, k));	// + ffd.bcenter);
						//pp3[1] = ffd.GetPoint(i, j + 1, k);	// + ffd.bcenter);
						Handles.DrawLine(pp3[0], pp3[1]);
					}

					if ( k < pc - 1 )
					{
						pp3[1] = tm.MultiplyPoint(ffd.GetPoint(i, j, k + 1));	// + ffd.bcenter);
						//pp3[1] = ffd.GetPoint(i, j, k + 1);	// + ffd.bcenter);
						Handles.DrawLine(pp3[0], pp3[1]);
					}
				}
			}
		}
	}
#endif
}