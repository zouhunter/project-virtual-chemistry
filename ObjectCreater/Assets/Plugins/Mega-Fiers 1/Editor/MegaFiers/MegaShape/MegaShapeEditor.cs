
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(MegaShape))]
public class MegaShapeEditor : Editor
{
	public bool		showcommon	= true;
	public float	outline		= 0.0f;
	int				selected	= -1;
	Vector3			pm			= new Vector3();
	Vector3			delta		= new Vector3();
	bool			showsplines	= false;
	bool			showknots	= false;
	bool			showlabels	= true;
	float			ImportScale	= 1.0f;
	bool			hidewire	= false;
	bool			addingknot	= false;
	Vector3			addpos;
	int				knum;
	int				snum;
	Bounds			bounds;
	string			lastpath	= "";
	MegaKnotAnim	ma;

	public bool		showfuncs = false;

	static public Vector3	CursorPos		= Vector3.zero;
	static public Vector3	CursorSpline	= Vector3.zero;
	static public Vector3	CursorTangent	= Vector3.zero;
	static public int		CursorKnot		= 0;

	public delegate bool ParseBinCallbackType(BinaryReader br, string id);
	public delegate void ParseClassCallbackType(string classname, BinaryReader br);

	public virtual bool Params()
	{
		MegaShape shape = (MegaShape)target;

		bool rebuild = false;

		float radius = EditorGUILayout.FloatField("Radius", shape.defRadius);
		if ( radius != shape.defRadius )
		{
			if ( radius < 0.001f )
				radius = 0.001f;

			shape.defRadius = radius;
			rebuild = true;
		}

		return rebuild;
	}

	public override void OnInspectorGUI()
	{
		//undoManager.CheckUndo();
		bool buildmesh = false;
		MegaShape shape = (MegaShape)target;

		EditorGUILayout.BeginHorizontal();

		int curve = shape.selcurve;

		if ( GUILayout.Button("Add Knot") )
		{
			if ( shape.splines == null || shape.splines.Count == 0 )
			{
				MegaSpline spline = new MegaSpline();	// Have methods for these
				shape.splines.Add(spline);
			}

			MegaKnot knot = new MegaKnot();
			float per = shape.CursorPercent * 0.01f;

			CursorTangent = shape.splines[curve].Interpolate(per + 0.01f, true, ref CursorKnot);	//this.GetPositionOnSpline(i) - p;
			CursorPos = shape.splines[curve].Interpolate(per, true, ref CursorKnot);	//this.GetPositionOnSpline(i) - p;

			knot.p = CursorPos;
			knot.outvec = (CursorTangent - knot.p);
			knot.outvec.Normalize();
			knot.outvec *= shape.splines[curve].knots[CursorKnot].seglength * 0.25f;
			knot.invec = -knot.outvec;
			knot.invec += knot.p;
			knot.outvec += knot.p;
			knot.twist = shape.splines[curve].knots[CursorKnot].twist;
			knot.id = shape.splines[curve].knots[CursorKnot].id;

			shape.splines[curve].knots.Insert(CursorKnot + 1, knot);
			shape.CalcLength();	//10);
			EditorUtility.SetDirty(target);
			buildmesh = true;
		}

		if ( GUILayout.Button("Delete Knot") )
		{
			if ( selected != -1 )
			{
				shape.splines[curve].knots.RemoveAt(selected);
				selected--;
				shape.CalcLength();	//10);
			}
			EditorUtility.SetDirty(target);
			buildmesh = true;
		}
		
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();

		if ( GUILayout.Button("Match Handles") )
		{
			if ( selected != -1 )
			{
				Vector3 p = shape.splines[curve].knots[selected].p;
				Vector3 d = shape.splines[curve].knots[selected].outvec - p;
				shape.splines[curve].knots[selected].invec = p - d;
				shape.CalcLength();	//10);
			}
			EditorUtility.SetDirty(target);
			buildmesh = true;
		}

		if ( GUILayout.Button("Load") )
		{
			LoadShape(ImportScale);
			buildmesh = true;
		}

		if ( GUILayout.Button("Load SXL") )
		{
			LoadSXL(ImportScale);
			buildmesh = true;
		}

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		if ( GUILayout.Button("AutoCurve") )
		{
			shape.AutoCurve();
			EditorUtility.SetDirty(target);
			buildmesh = true;
		}

		if ( GUILayout.Button("Reverse") )
		{
			shape.Reverse(curve);
			//shape.CalcLength();
			EditorUtility.SetDirty(target);
			buildmesh = true;
		}

		EditorGUILayout.EndHorizontal();

		if ( GUILayout.Button("Apply Scaling") )
		{
			shape.Scale(shape.transform.localScale);
			EditorUtility.SetDirty(target);
			shape.transform.localScale = Vector3.one;
			buildmesh = true;
		}

		if ( GUILayout.Button("Import SVG") )
		{
			LoadSVG(ImportScale);
			buildmesh = true;
		}

		showcommon = EditorGUILayout.Foldout(showcommon, "Common Params");

		bool rebuild = false;	//Params();

		if ( showcommon )
		{
			shape.CursorPercent = EditorGUILayout.FloatField("Cursor", shape.CursorPercent);
			shape.CursorPercent = Mathf.Repeat(shape.CursorPercent, 100.0f);

			ImportScale = EditorGUILayout.FloatField("Import Scale", ImportScale);

			MegaAxis av = (MegaAxis)EditorGUILayout.EnumPopup("Axis", shape.axis);
			if ( av != shape.axis )
			{
				shape.axis = av;
				rebuild = true;
			}

			if ( shape.splines.Count > 1 )
				shape.selcurve = EditorGUILayout.IntSlider("Curve", shape.selcurve, 0, shape.splines.Count - 1);

			if ( shape.selcurve < 0 )
				shape.selcurve = 0;

			if ( shape.selcurve > shape.splines.Count - 1 )
				shape.selcurve = shape.splines.Count - 1;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Colors");
			shape.col1 = EditorGUILayout.ColorField(shape.col1);
			shape.col2 = EditorGUILayout.ColorField(shape.col2);
			EditorGUILayout.EndHorizontal();

			shape.VecCol = EditorGUILayout.ColorField("Vec Col", shape.VecCol);

			shape.KnotSize = EditorGUILayout.FloatField("Knot Size", shape.KnotSize);
			shape.stepdist = EditorGUILayout.FloatField("Step Dist", shape.stepdist);

			MegaSpline spline = shape.splines[shape.selcurve];

			if ( shape.stepdist < 0.01f )
				shape.stepdist = 0.01f;

			shape.dolateupdate = EditorGUILayout.Toggle("Do Late Update", shape.dolateupdate);
			shape.normalizedInterp = EditorGUILayout.Toggle("Normalized Interp", shape.normalizedInterp);

			spline.constantSpeed = EditorGUILayout.Toggle("Constant Speed", spline.constantSpeed);
			int subdivs = EditorGUILayout.IntField("Calc Subdivs", spline.subdivs);

			if ( subdivs < 2 )
				subdivs = 2;
			if ( subdivs != spline.subdivs )
				spline.CalcLength(subdivs);

			shape.drawHandles = EditorGUILayout.Toggle("Draw Handles", shape.drawHandles);
			shape.drawKnots = EditorGUILayout.Toggle("Draw Knots", shape.drawKnots);
			shape.drawTwist = EditorGUILayout.Toggle("Draw Twist", shape.drawTwist);
			shape.drawspline = EditorGUILayout.Toggle("Draw Spline", shape.drawspline);
			shape.lockhandles = EditorGUILayout.Toggle("Lock Handles", shape.lockhandles);
			shape.autosmooth = EditorGUILayout.Toggle("Auto Smooth", shape.autosmooth);
			shape.smoothness = EditorGUILayout.FloatField("Smoothness", shape.smoothness);

			shape.handleType = (MegaHandleType)EditorGUILayout.EnumPopup("Handle Type", shape.handleType);

			showlabels = EditorGUILayout.Toggle("Labels", showlabels);

			hidewire = EditorGUILayout.Toggle("Hide Wire", hidewire);

			EditorUtility.SetSelectedWireframeHidden(shape.GetComponent<Renderer>(), hidewire);

			shape.animate = EditorGUILayout.Toggle("Animate", shape.animate);
			if ( shape.animate )
			{
				shape.time = EditorGUILayout.FloatField("Time", shape.time);
				shape.MaxTime = EditorGUILayout.FloatField("Loop Time", shape.MaxTime);
				shape.speed = EditorGUILayout.FloatField("Speed", shape.speed);
				shape.LoopMode = (MegaRepeatMode)EditorGUILayout.EnumPopup("Loop Mode", shape.LoopMode);
			}

			AnimationKeyFrames(shape);

			if ( shape.splines.Count > 0 )
			{
				if ( spline.outlineSpline != -1 )
				{
					int outlineSpline = EditorGUILayout.IntSlider("Outline Spl", spline.outlineSpline, 0, shape.splines.Count - 1);
					float outline = EditorGUILayout.FloatField("Outline", spline.outline);

					if ( outline != spline.outline || outlineSpline != spline.outlineSpline )
					{
						spline.outlineSpline = outlineSpline;
						spline.outline = outline;
						if ( outlineSpline != shape.selcurve )
						{
							shape.OutlineSpline(shape.splines[spline.outlineSpline], spline, spline.outline, true);
							spline.CalcLength();	//10);
							EditorUtility.SetDirty(target);
							buildmesh = true;
						}
					}
				}
				else
				{
					outline = EditorGUILayout.FloatField("Outline", outline);

					if ( GUILayout.Button("Outline") )
					{
						shape.OutlineSpline(shape, shape.selcurve, outline, true);
						shape.splines[shape.splines.Count - 1].outline = outline;
						shape.splines[shape.splines.Count - 1].outlineSpline = shape.selcurve;
						shape.selcurve = shape.splines.Count - 1;
						EditorUtility.SetDirty(target);
						buildmesh = true;
					}
				}
			}

			// Mesher
			shape.makeMesh = EditorGUILayout.Toggle("Make Mesh", shape.makeMesh);

			if ( shape.makeMesh )
			{
				shape.meshType = (MeshShapeType)EditorGUILayout.EnumPopup("Mesh Type", shape.meshType);
				shape.Pivot = EditorGUILayout.Vector3Field("Pivot", shape.Pivot);

				shape.CalcTangents = EditorGUILayout.Toggle("Calc Tangents", shape.CalcTangents);
				shape.GenUV = EditorGUILayout.Toggle("Gen UV", shape.GenUV);

				EditorGUILayout.BeginVertical("Box");
				switch ( shape.meshType )
				{
					case MeshShapeType.Fill:
						shape.DoubleSided = EditorGUILayout.Toggle("Double Sided", shape.DoubleSided);
						shape.Height = EditorGUILayout.FloatField("Height", shape.Height);
						shape.UseHeightCurve = EditorGUILayout.Toggle("Use Height Crv", shape.UseHeightCurve);
						if ( shape.UseHeightCurve )
						{
							shape.heightCrv = EditorGUILayout.CurveField("Height Curve", shape.heightCrv);
							shape.heightOff = EditorGUILayout.Slider("Height Off", shape.heightOff, -1.0f, 1.0f);
						}
						shape.mat1 = (Material)EditorGUILayout.ObjectField("Top Mat", shape.mat1, typeof(Material), true);
						shape.mat2 = (Material)EditorGUILayout.ObjectField("Bot Mat", shape.mat2, typeof(Material), true);
						shape.mat3 = (Material)EditorGUILayout.ObjectField("Side Mat", shape.mat3, typeof(Material), true);

						shape.PhysUV = EditorGUILayout.Toggle("Physical UV", shape.PhysUV);
						shape.UVOffset = EditorGUILayout.Vector2Field("UV Offset", shape.UVOffset);
						shape.UVRotate = EditorGUILayout.Vector2Field("UV Rotate", shape.UVRotate);
						shape.UVScale = EditorGUILayout.Vector2Field("UV Scale", shape.UVScale);
						shape.UVOffset1 = EditorGUILayout.Vector2Field("UV Offset1", shape.UVOffset1);
						shape.UVRotate1 = EditorGUILayout.Vector2Field("UV Rotate1", shape.UVRotate1);
						shape.UVScale1 = EditorGUILayout.Vector2Field("UV Scale1", shape.UVScale1);
						break;

					case MeshShapeType.Tube:
						shape.TubeStart = EditorGUILayout.Slider("Start", shape.TubeStart, -1.0f, 2.0f);
						shape.TubeLength = EditorGUILayout.Slider("Length", shape.TubeLength, 0.0f, 1.0f);
						shape.rotate = EditorGUILayout.FloatField("Rotate", shape.rotate);
						shape.tsides = EditorGUILayout.IntField("Sides", shape.tsides);
						shape.tradius = EditorGUILayout.FloatField("Radius", shape.tradius);
						shape.offset = EditorGUILayout.FloatField("Offset", shape.offset);

						shape.scaleX = EditorGUILayout.CurveField("Scale X", shape.scaleX);
						shape.unlinkScale = EditorGUILayout.BeginToggleGroup("unlink Scale", shape.unlinkScale);
						shape.scaleY = EditorGUILayout.CurveField("Scale Y", shape.scaleY);
						EditorGUILayout.EndToggleGroup();

						shape.strands = EditorGUILayout.IntField("Strands", shape.strands);
						if ( shape.strands > 1 )
						{
							shape.strandRadius = EditorGUILayout.FloatField("Strand Radius", shape.strandRadius);
							shape.TwistPerUnit = EditorGUILayout.FloatField("Twist", shape.TwistPerUnit);
							shape.startAng = EditorGUILayout.FloatField("Start Twist", shape.startAng);
						}
						shape.UVOffset = EditorGUILayout.Vector2Field("UV Offset", shape.UVOffset);
						shape.uvtilex = EditorGUILayout.FloatField("UV Tile X", shape.uvtilex);
						shape.uvtiley = EditorGUILayout.FloatField("UV Tile Y", shape.uvtiley);

						shape.cap = EditorGUILayout.Toggle("Cap", shape.cap);
						shape.RopeUp = (MegaAxis)EditorGUILayout.EnumPopup("Up", shape.RopeUp);
						shape.mat1 = (Material)EditorGUILayout.ObjectField("Mat", shape.mat1, typeof(Material), true);
						break;

					case MeshShapeType.Ribbon:
						shape.TubeStart = EditorGUILayout.Slider("Start", shape.TubeStart, -1.0f, 2.0f);
						shape.TubeLength = EditorGUILayout.Slider("Length", shape.TubeLength, 0.0f, 1.0f);
						shape.boxwidth = EditorGUILayout.FloatField("Width", shape.boxwidth);
						shape.raxis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", shape.raxis);
						shape.rotate = EditorGUILayout.FloatField("Rotate", shape.rotate);
						shape.ribsegs = EditorGUILayout.IntField("Segs", shape.ribsegs);
						if ( shape.ribsegs < 1 )
							shape.ribsegs = 1;
						shape.offset = EditorGUILayout.FloatField("Offset", shape.offset);

						shape.scaleX = EditorGUILayout.CurveField("Scale X", shape.scaleX);

						shape.strands = EditorGUILayout.IntField("Strands", shape.strands);
						if ( shape.strands > 1 )
						{
							shape.strandRadius = EditorGUILayout.FloatField("Strand Radius", shape.strandRadius);
							shape.TwistPerUnit = EditorGUILayout.FloatField("Twist", shape.TwistPerUnit);
							shape.startAng = EditorGUILayout.FloatField("Start Twist", shape.startAng);
						}

						shape.UVOffset = EditorGUILayout.Vector2Field("UV Offset", shape.UVOffset);
						shape.uvtilex = EditorGUILayout.FloatField("UV Tile X", shape.uvtilex);
						shape.uvtiley = EditorGUILayout.FloatField("UV Tile Y", shape.uvtiley);

						shape.RopeUp = (MegaAxis)EditorGUILayout.EnumPopup("Up", shape.RopeUp);
						shape.mat1 = (Material)EditorGUILayout.ObjectField("Mat", shape.mat1, typeof(Material), true);
						break;

					case MeshShapeType.Box:
						shape.TubeStart = EditorGUILayout.Slider("Start", shape.TubeStart, -1.0f, 2.0f);
						shape.TubeLength = EditorGUILayout.Slider("Length", shape.TubeLength, 0.0f, 1.0f);
						shape.rotate = EditorGUILayout.FloatField("Rotate", shape.rotate);
						shape.boxwidth = EditorGUILayout.FloatField("Box Width", shape.boxwidth);
						shape.boxheight = EditorGUILayout.FloatField("Box Height", shape.boxheight);
						shape.offset = EditorGUILayout.FloatField("Offset", shape.offset);

						shape.scaleX = EditorGUILayout.CurveField("Scale X", shape.scaleX);
						shape.unlinkScale = EditorGUILayout.BeginToggleGroup("unlink Scale", shape.unlinkScale);
						shape.scaleY = EditorGUILayout.CurveField("Scale Y", shape.scaleY);
						EditorGUILayout.EndToggleGroup();

						shape.strands = EditorGUILayout.IntField("Strands", shape.strands);
						if ( shape.strands > 1 )
						{
							shape.tradius = EditorGUILayout.FloatField("Radius", shape.tradius);
							shape.TwistPerUnit = EditorGUILayout.FloatField("Twist", shape.TwistPerUnit);
							shape.startAng = EditorGUILayout.FloatField("Start Twist", shape.startAng);
						}

						shape.UVOffset = EditorGUILayout.Vector2Field("UV Offset", shape.UVOffset);
						shape.uvtilex = EditorGUILayout.FloatField("UV Tile X", shape.uvtilex);
						shape.uvtiley = EditorGUILayout.FloatField("UV Tile Y", shape.uvtiley);

						shape.cap = EditorGUILayout.Toggle("Cap", shape.cap);
						shape.RopeUp = (MegaAxis)EditorGUILayout.EnumPopup("Up", shape.RopeUp);
						shape.mat1 = (Material)EditorGUILayout.ObjectField("Mat", shape.mat1, typeof(Material), true);
						break;
				}

				if ( shape.strands < 1 )
					shape.strands = 1;

				EditorGUILayout.EndVertical();

				// Conform
				shape.conform = EditorGUILayout.BeginToggleGroup("Conform", shape.conform);

				GameObject contarget = (GameObject)EditorGUILayout.ObjectField("Target", shape.target, typeof(GameObject), true);

				if ( contarget != shape.target )
					shape.SetTarget(contarget);
				shape.conformAmount = EditorGUILayout.Slider("Amount", shape.conformAmount, 0.0f, 1.0f);
				shape.raystartoff = EditorGUILayout.FloatField("Ray Start Off", shape.raystartoff);
				shape.conformOffset = EditorGUILayout.FloatField("Conform Offset", shape.conformOffset);
				shape.raydist = EditorGUILayout.FloatField("Ray Dist", shape.raydist);
				EditorGUILayout.EndToggleGroup();
			}
			else
			{
				shape.ClearMesh();
			}

			showsplines = EditorGUILayout.Foldout(showsplines, "Spline Data");

			if ( showsplines )
			{
				EditorGUILayout.BeginVertical("Box");
				if ( shape.splines != null && shape.splines.Count > 0 )
					DisplaySpline(shape, shape.splines[shape.selcurve]);
				EditorGUILayout.EndVertical();
			}

			EditorGUILayout.BeginHorizontal();

			Color col = GUI.backgroundColor;
			GUI.backgroundColor = Color.green;
			if ( GUILayout.Button("Add") )
			{
				// Create a new spline in the shape
				MegaSpline spl = MegaSpline.Copy(shape.splines[shape.selcurve]);

				shape.splines.Add(spl);
				shape.selcurve = shape.splines.Count - 1;
				EditorUtility.SetDirty(shape);
				buildmesh = true;
			}

			if ( shape.splines.Count > 1 )
			{
				GUI.backgroundColor = Color.red;
				if ( GUILayout.Button("Delete") )
				{
					// Delete current spline
					shape.splines.RemoveAt(shape.selcurve);


					for ( int i = 0; i < shape.splines.Count; i++ )
					{
						if ( shape.splines[i].outlineSpline == shape.selcurve )
							shape.splines[i].outlineSpline = -1;

						if ( shape.splines[i].outlineSpline > shape.selcurve )
							shape.splines[i].outlineSpline--;
					}

					shape.selcurve--;
					if ( shape.selcurve < 0 )
						shape.selcurve = 0;

					EditorUtility.SetDirty(shape);
					buildmesh = true;
				}
			}
			GUI.backgroundColor = col;
			EditorGUILayout.EndHorizontal();
		}

		if ( !shape.imported )
		{
			if ( Params() )
			{
				rebuild = true;
			}
		}

		showfuncs = EditorGUILayout.Foldout(showfuncs, "Extra Functions");

		if ( showfuncs )
		{
			if ( GUILayout.Button("Flatten") )
			{
				shape.SetHeight(shape.selcurve, 0.0f);
				shape.CalcLength();
				EditorUtility.SetDirty(target);
			}

			if ( GUILayout.Button("Remove Twist") )
			{
				shape.SetTwist(shape.selcurve, 0.0f);
				EditorUtility.SetDirty(target);
			}
		}

		if ( GUI.changed )
		{
			EditorUtility.SetDirty(target);
			buildmesh = true;
		}

		if ( rebuild )
		{
			shape.MakeShape();
			EditorUtility.SetDirty(target);
			buildmesh = true;
		}

		if ( buildmesh )
		{
			if ( shape.makeMesh )
			{
				shape.SetMats();
				shape.BuildMesh();
			}
		}

		//undoManager.CheckDirty();
	}

	void DisplayKnot(MegaShape shape, MegaSpline spline, MegaKnot knot)
	{
		bool recalc = false;

		Vector3 p = EditorGUILayout.Vector3Field("Pos", knot.p);
		delta = p - knot.p;

		knot.invec += delta;
		knot.outvec += delta;

		if ( knot.p != p )
		{
			recalc = true;
			knot.p = p;
		}

		if ( recalc )
		{
			shape.CalcLength();	//10);
		}
		knot.twist = EditorGUILayout.FloatField("Twist", knot.twist);
		knot.id = EditorGUILayout.IntField("ID", knot.id);
	}

	void DisplaySpline(MegaShape shape, MegaSpline spline)
	{
		bool closed = EditorGUILayout.Toggle("Closed", spline.closed);

		if ( closed != spline.closed )
		{
			spline.closed = closed;
			shape.CalcLength();	//10);
			EditorUtility.SetDirty(target);
			//shape.BuildMesh();
		}

		spline.reverse = EditorGUILayout.Toggle("Reverse", spline.reverse);

		EditorGUILayout.LabelField("Length ", spline.length.ToString("0.000"));
		spline.twistmode = (MegaShapeEase)EditorGUILayout.EnumPopup("Twist Mode", spline.twistmode);

		showknots = EditorGUILayout.Foldout(showknots, "Knots");

		if ( showknots )
		{
			for ( int i = 0; i < spline.knots.Count; i++ )
			{
				DisplayKnot(shape, spline, spline.knots[i]);
				//EditorGUILayout.Separator();
			}
		}
	}

	public void OnSceneGUI()
	{
		MegaShape shape = (MegaShape)target;

		if ( Event.current.type == EventType.mouseUp )
		{
			if ( addingknot )
			{
				addingknot = false;
				MegaKnot knot = new MegaKnot();

				knot.p = addpos;
				knot.id = shape.splines[snum].knots[knum].id;
				knot.twist = shape.splines[snum].knots[knum].twist;

				shape.splines[snum].knots.Insert(knum + 1, knot);
				shape.AutoCurve(shape.splines[snum]);	//, knum, knum + 2);
				shape.CalcLength();	//10);
				EditorUtility.SetDirty(target);
				if ( shape.makeMesh )
				{
					shape.SetMats();
					shape.BuildMesh();
				}
			}
		}

		Handles.matrix = shape.transform.localToWorldMatrix;

		if ( shape.selcurve > shape.splines.Count - 1 )
			shape.selcurve = 0;

		bool recalc = false;

		Vector3 dragplane = Vector3.one;

		Color nocol = new Color(0, 0, 0, 0);

		bounds.size = Vector3.zero;

		Color twistcol = new Color(0.5f, 0.5f, 1.0f, 0.25f);

		for ( int s = 0; s < shape.splines.Count; s++ )
		{
			for ( int p = 0; p < shape.splines[s].knots.Count; p++ )
			{
				if ( s == shape.selcurve )
				{
					bounds.Encapsulate(shape.splines[s].knots[p].p);
				}

				if ( shape.drawKnots && s == shape.selcurve )
				{
					pm = shape.splines[s].knots[p].p;

					if ( showlabels )
					{
						if ( p == selected && s == shape.selcurve )
						{
							Handles.color = Color.white;
							Handles.Label(pm, " Selected\n" + pm.ToString("0.000"));
						}
						else
						{
							Handles.color = shape.KnotCol;
							Handles.Label(pm, " " + p);
						}
					}

					Handles.color = nocol;
					Vector3 newp = PosHandles(shape, pm, Quaternion.identity);
					if ( newp != pm )
					{
						MegaUndo.SetSnapshotTarget(shape, "Knot Move");
					}

					Vector3 dl = Vector3.Scale(newp - pm, dragplane);

					shape.splines[s].knots[p].p += dl;	//Vector3.Scale(newp - pm, dragplane);

					shape.splines[s].knots[p].invec += dl;	//delta;
					shape.splines[s].knots[p].outvec += dl;	//delta;

					if ( newp != pm )
					{
						selected = p;
						recalc = true;
					}
				}

				if ( shape.drawHandles && s == shape.selcurve )
				{
					Handles.color = shape.VecCol;
					pm = shape.splines[s].knots[p].p;

					Vector3 ip = shape.splines[s].knots[p].invec;
					Vector3 op = shape.splines[s].knots[p].outvec;
					Handles.DrawLine(pm, ip);
					Handles.DrawLine(pm, op);

					Handles.color = shape.HandleCol;

					Vector3 invec = shape.splines[s].knots[p].invec;
					Handles.color = nocol;
					Vector3 newinvec = PosHandles(shape, invec, Quaternion.identity);

					if ( newinvec != invec )	//shape.splines[s].knots[p].invec )
					{
						MegaUndo.SetSnapshotTarget(shape, "Handle Move");
					}
					Vector3 dl = Vector3.Scale(newinvec - invec, dragplane);
					shape.splines[s].knots[p].invec += dl;	//Vector3.Scale(newinvec - invec, dragplane);
					if ( invec != newinvec )	//shape.splines[s].knots[p].invec )
					{
						if ( shape.lockhandles )
						{
							shape.splines[s].knots[p].outvec -= dl;
						}

						selected = p;
						recalc = true;
					}
					Vector3 outvec = shape.splines[s].knots[p].outvec;

					Vector3 newoutvec = PosHandles(shape, outvec, Quaternion.identity);
					if ( newoutvec != outvec )	//shape.splines[s].knots[p].outvec )
					{
						MegaUndo.SetSnapshotTarget(shape, "Handle Move");
					}
					dl = Vector3.Scale(newoutvec - outvec, dragplane);
					shape.splines[s].knots[p].outvec += dl;
					if ( outvec != newoutvec )	//shape.splines[s].knots[p].outvec )
					{
						if ( shape.lockhandles )
						{
							shape.splines[s].knots[p].invec -= dl;
						}

						selected = p;
						recalc = true;
					}
					Vector3 hp = shape.splines[s].knots[p].invec;
					if ( selected == p )
						Handles.Label(hp, " " + p);

					hp = shape.splines[s].knots[p].outvec;

					if ( selected == p )
						Handles.Label(hp, " " + p);
				}

				// TODO: Draw a wedge to show the twist angle
				// Twist handles
				if ( shape.drawTwist && s == shape.selcurve )
				{
					Vector3 np = Vector3.zero;

					if ( p < shape.splines[s].knots.Count - 2 )
					{
						np = shape.splines[s].knots[p].Interpolate(0.002f, shape.splines[s].knots[p + 1]);
						np = np - shape.splines[s].knots[p].p;
						//np = shape.splines[s].knots[p].outvec;	//(0.0f, shape.splines[s].knots[p + 1]);

						//Debug.Log("np " + np);
					}
					else
					{
						if ( shape.splines[s].closed )
						{
							np = shape.splines[s].knots[p].Interpolate(0.002f, shape.splines[s].knots[0]);
							np = np - shape.splines[s].knots[p].p;
							//np = shape.splines[s].knots[p].outvec;	//Tangent(0.0f, shape.splines[s].knots[0]);
							//Debug.Log("np " + np);
						}
						else
						{
							np = shape.splines[s].knots[p - 1].Interpolate(0.998f, shape.splines[s].knots[p]);
							np = shape.splines[s].knots[p].p - np;
							//np = shape.splines[s].knots[p - 1].outvec;	//Tangent(0.9999f, shape.splines[s].knots[p]);
							//Debug.Log("np " + np);
						}
					}

					if ( np == Vector3.zero )
						np = Vector3.forward;

					np = np.normalized;
					// np holds the tangent so we can align the arc
					Quaternion fwd = Quaternion.LookRotation(np);

					Vector3 rg = Vector3.Cross(np, Vector3.up);
					//Vector3 up = Vector3.Cross(np, rg);

					Handles.color = twistcol;
					float twist = shape.splines[s].knots[p].twist;

					Handles.DrawSolidArc(shape.splines[s].knots[p].p, np, rg, twist, shape.KnotSize * 0.1f);

					Vector3 tang = new Vector3(0.0f, 0.0f, shape.splines[s].knots[p].twist);
					Quaternion inrot = fwd * Quaternion.Euler(tang);
					//Quaternion rot = Handles.RotationHandle(inrot, shape.splines[s].knots[p].p);
					Handles.color = Color.white;
					Quaternion rot = Handles.Disc(inrot, shape.splines[s].knots[p].p, np, shape.KnotSize * 0.1f, false, 0.0f);
					
					if ( rot != inrot )
					{
						tang = rot.eulerAngles;
						float diff = (tang.z - shape.splines[s].knots[p].twist);
						if ( Mathf.Abs(diff) > 0.0001f  )
						{
							while ( diff > 180.0f )
								diff -= 360.0f;

							while ( diff < -180.0f )
								diff += 360.0f;

							shape.splines[s].knots[p].twist += diff;
							recalc = true;
						}
					}
				}

				// Midpoint add knot code
				if ( s == shape.selcurve && p < shape.splines[s].knots.Count - 1 )
				{
					Handles.color = Color.white;

					Vector3 mp = shape.splines[s].knots[p].InterpolateCS(0.5f, shape.splines[s].knots[p + 1]);

					Vector3 mp1 = Handles.FreeMoveHandle(mp, Quaternion.identity, shape.KnotSize * 0.01f, Vector3.zero, Handles.CircleCap);
					if ( mp1 != mp )
					{
						if ( !addingknot )
						{
							addingknot = true;
							addpos = mp;
							knum = p;
							snum = s;
						}
					}
				}
			}
		}

		// Draw nearest point (use for adding knot)
		Vector3 wcp = CursorPos;
		Vector3 newCursorPos = PosHandles(shape, wcp, Quaternion.identity);
		
		if ( newCursorPos != wcp )
		{
			Vector3 cd = newCursorPos - wcp;

			CursorPos += cd;

			float calpha = 0.0f;
			CursorPos = shape.FindNearestPoint(CursorPos, 5, ref CursorKnot, ref CursorTangent, ref calpha);
			shape.CursorPercent = calpha * 100.0f;
		}

		Handles.Label(CursorPos, "Cursor " + shape.CursorPercent.ToString("0.00") + "% - " + CursorPos);

		// Move whole spline handle
		Handles.Label(bounds.min, "Origin");
		Vector3 spos = Handles.PositionHandle(bounds.min, Quaternion.identity);
		if ( spos != bounds.min )
		{
			// Move the spline
			shape.MoveSpline(spos - bounds.min, shape.selcurve, false);
			recalc = true;
		}

		if ( recalc )
		{
			shape.CalcLength();	//10);
			shape.BuildMesh();
		}

		Handles.matrix = Matrix4x4.identity;
		//undoManager.CheckDirty(target);

		// This is wrong gui not changing here
		if ( GUI.changed )
		{
			MegaUndo.CreateSnapshot();
			MegaUndo.RegisterSnapshot();
		}

		MegaUndo.ClearSnapshotTarget();
	}

	Vector3 PosHandles(MegaShape shape, Vector3 pos, Quaternion q)
	{
		switch ( shape.handleType )
		{
			case MegaHandleType.Position:
				pos = Handles.PositionHandle(pos, q);
				break;

			case MegaHandleType.Free:
				pos = Handles.FreeMoveHandle(pos, q, shape.KnotSize * 0.01f, Vector3.zero, Handles.CircleCap);
				break;
		}
		return pos;
	}

	[DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.Selected)]
	static void RenderGizmo(MegaShape shape, GizmoType gizmoType)
	{
		//if ( (gizmoType & GizmoType.NotSelected) != 0 )
		{
			if ( (gizmoType & GizmoType.Active) != 0 )
			{
				if ( shape.splines == null || shape.splines.Count == 0 )
					return;

				DrawGizmos(shape, new Color(1.0f, 1.0f, 1.0f, 1.0f));
				Color col = Color.yellow;
				col.a = 0.5f;
				Gizmos.color = col;	//Color.yellow;
				CursorPos = shape.InterpCurve3D(shape.selcurve, shape.CursorPercent * 0.01f, true);
				Gizmos.DrawSphere(shape.transform.TransformPoint(CursorPos), shape.KnotSize * 0.01f);
				Handles.color = Color.white;

				if ( shape.handleType == MegaHandleType.Free )
				{
					int s = shape.selcurve;
					{
						for ( int p = 0; p < shape.splines[s].knots.Count; p++ )
						{
							if ( shape.drawKnots )	//&& s == shape.selcurve )
							{
								Gizmos.color = Color.green;
								Gizmos.DrawSphere(shape.transform.TransformPoint(shape.splines[s].knots[p].p), shape.KnotSize * 0.01f);
							}

							if ( shape.drawHandles )
							{
								Gizmos.color = Color.red;
								Gizmos.DrawSphere(shape.transform.TransformPoint(shape.splines[s].knots[p].invec), shape.KnotSize * 0.01f);
								Gizmos.DrawSphere(shape.transform.TransformPoint(shape.splines[s].knots[p].outvec), shape.KnotSize * 0.01f);
							}
						}
					}
				}
			}
			else
				DrawGizmos(shape, new Color(1.0f, 1.0f, 1.0f, 0.25f));
		}
		Gizmos.DrawIcon(shape.transform.position, "MegaSpherify icon.png", false);
		Handles.Label(shape.transform.position, " " + shape.name);
	}

	// Dont want this in here, want in editor
	// If we go over a knot then should draw to the knot
	static void DrawGizmos(MegaShape shape, Color modcol1)
	{
		if ( ((1 << shape.gameObject.layer) & Camera.current.cullingMask) == 0 )
			return;

		if ( !shape.drawspline )
			return;

		Matrix4x4 tm = shape.transform.localToWorldMatrix;

		for ( int s = 0; s < shape.splines.Count; s++ )
		{
			float ldist = shape.stepdist * 0.1f;
			if ( ldist < 0.01f )
				ldist = 0.01f;

			Color modcol = modcol1;

			if ( s != shape.selcurve && modcol1.a == 1.0f )
				modcol.a *= 0.5f;

			if ( shape.splines[s].length / ldist > 500.0f )
				ldist = shape.splines[s].length / 500.0f;

			float ds = shape.splines[s].length / (shape.splines[s].length / ldist);

			if ( ds > shape.splines[s].length )
				ds = shape.splines[s].length;

			int c	= 0;
			int k	= -1;
			int lk	= -1;

			Vector3 first = shape.splines[s].Interpolate(0.0f, shape.normalizedInterp, ref lk);

			for ( float dist = ds; dist < shape.splines[s].length; dist += ds )
			{
				float alpha = dist / shape.splines[s].length;
				Vector3 pos = shape.splines[s].Interpolate(alpha, shape.normalizedInterp, ref k);

				if ( (c & 1) == 1 )
					Gizmos.color = shape.col1 * modcol;
				else
					Gizmos.color = shape.col2 * modcol;

				if ( k != lk )
				{
					for ( lk = lk + 1; lk <= k; lk++ )
					{
						Gizmos.DrawLine(tm.MultiplyPoint(first), tm.MultiplyPoint(shape.splines[s].knots[lk].p));
						first = shape.splines[s].knots[lk].p;
					}
				}

				lk = k;

				Gizmos.DrawLine(tm.MultiplyPoint(first), tm.MultiplyPoint(pos));

				c++;

				first = pos;
			}

			if ( (c & 1) == 1 )
				Gizmos.color = shape.col1 * modcol;
			else
				Gizmos.color = shape.col2 * modcol;

			Vector3 lastpos;
			if ( shape.splines[s].closed )
				lastpos = shape.splines[s].Interpolate(0.0f, shape.normalizedInterp, ref k);
			else
				lastpos = shape.splines[s].Interpolate(1.0f, shape.normalizedInterp, ref k);

			Gizmos.DrawLine(tm.MultiplyPoint(first), tm.MultiplyPoint(lastpos));
		}
	}

	void LoadSVG(float scale)
	{
		MegaShape ms = (MegaShape)target;

		string filename = EditorUtility.OpenFilePanel("SVG File", lastpath, "svg");

		if ( filename == null || filename.Length < 1 )
			return;

		lastpath = filename;

		bool opt = true;
		if ( ms.splines != null && ms.splines.Count > 0 )
			opt = EditorUtility.DisplayDialog("Spline Import Option", "Splines already present, do you want to 'Add' or 'Replace' splines with this file?", "Add", "Replace");

		int startspline = 0;
		if ( opt )
			startspline = ms.splines.Count;

		StreamReader streamReader = new StreamReader(filename);
		string text = streamReader.ReadToEnd();
		streamReader.Close();
		MegaShapeSVG svg = new MegaShapeSVG();
		svg.importData(text, ms, scale, opt, startspline);	//.splines[0]);
		ms.imported = true;
	}

	void LoadSXL(float scale)
	{
		MegaShape ms = (MegaShape)target;

		string filename = EditorUtility.OpenFilePanel("SXL File", lastpath, "sxl");

		if ( filename == null || filename.Length < 1 )
			return;

		lastpath = filename;

		bool opt = true;
		if ( ms.splines != null && ms.splines.Count > 0 )
			opt = EditorUtility.DisplayDialog("Spline Import Option", "Splines already present, do you want to 'Add' or 'Replace' splines with this file?", "Add", "Replace");

		int startspline = 0;
		if ( opt )
			startspline = ms.splines.Count;

		StreamReader streamReader = new StreamReader(filename);
		string text = streamReader.ReadToEnd();
		streamReader.Close();
		MegaShapeSXL sxl = new MegaShapeSXL();
		sxl.importData(text, ms, scale, opt, startspline);	//.splines[0]);
		ms.imported = true;
	}

	void LoadShape(float scale)
	{
		MegaShape ms = (MegaShape)target;

		string filename = EditorUtility.OpenFilePanel("Shape File", lastpath, "spl");

		if ( filename == null || filename.Length < 1 )
			return;

		lastpath = filename;

		// Clear what we have
		bool opt = true;
		if ( ms.splines != null && ms.splines.Count > 0 )
			opt = EditorUtility.DisplayDialog("Spline Import Option", "Splines already present, do you want to 'Add' or 'Replace' splines with this file?", "Add", "Replace");

		int startspline = 0;
		if ( opt )
			startspline = ms.splines.Count;
		else
			ms.splines.Clear();

		ParseFile(filename, ShapeCallback);

		ms.Scale(scale, startspline);

		ms.MaxTime = 0.0f;

		for ( int s = 0; s < ms.splines.Count; s++ )
		{
			if ( ms.splines[s].animations != null )
			{
				for ( int a = 0; a < ms.splines[s].animations.Count; a++ )
				{
					MegaControl con = ms.splines[s].animations[a].con;
					if ( con != null )
					{
						float t = con.Times[con.Times.Length - 1];
						if ( t > ms.MaxTime )
							ms.MaxTime = t;
					}
				}
			}
		}
		ms.imported = true;
	}

	public void ShapeCallback(string classname, BinaryReader br)
	{
		switch ( classname )
		{
			case "Shape": LoadShape(br); break;
		}
	}

	public void LoadShape(BinaryReader br)
	{
		MegaParse.Parse(br, ParseShape);
	}

	public void ParseFile(string assetpath, ParseClassCallbackType cb)
	{
		FileStream fs = new FileStream(assetpath, FileMode.Open, FileAccess.Read, System.IO.FileShare.Read);

		BinaryReader br = new BinaryReader(fs);

		bool processing = true;

		while ( processing )
		{
			string classname = MegaParse.ReadString(br);

			if ( classname == "Done" )
				break;

			int	chunkoff = br.ReadInt32();
			long fpos = fs.Position;

			cb(classname, br);

			fs.Position = fpos + chunkoff;
		}

		br.Close();
	}

	static public Vector3 ReadP3(BinaryReader br)
	{
		Vector3 p = Vector3.zero;

		p.x = br.ReadSingle();
		p.y = br.ReadSingle();
		p.z = br.ReadSingle();

		return p;
	}

	bool SplineParse(BinaryReader br, string cid)
	{
		MegaShape ms = (MegaShape)target;
		MegaSpline ps = ms.splines[ms.splines.Count - 1];

		switch ( cid )
		{
			case "Transform":
				Vector3 pos = ReadP3(br);
				Vector3 rot = ReadP3(br);
				Vector3 scl = ReadP3(br);
				rot.y = -rot.y;
				ms.transform.position = pos;
				ms.transform.rotation = Quaternion.Euler(rot * Mathf.Rad2Deg);
				ms.transform.localScale = scl;
				break;

			case "Flags":
				int count = br.ReadInt32();
				ps.closed = (br.ReadInt32() == 1);
				count = br.ReadInt32();
				ps.knots = new List<MegaKnot>(count);
				ps.length = 0.0f;
				break;

			case "Knots":
				for ( int i = 0; i < ps.knots.Capacity; i++ )
				{
					MegaKnot pk = new MegaKnot();

					pk.p = ReadP3(br);
					pk.invec = ReadP3(br);
					pk.outvec = ReadP3(br);
					pk.seglength = br.ReadSingle();

					ps.length += pk.seglength;
					pk.length = ps.length;
					ps.knots.Add(pk);
				}
				break;
		}
		return true;
	}

	bool AnimParse(BinaryReader br, string cid)
	{
		MegaShape ms = (MegaShape)target;

		switch ( cid )
		{
			case "V":
				int v = br.ReadInt32();
				ma = new MegaKnotAnim();
				int s = ms.GetSpline(v, ref ma);	//.s, ref ma.p, ref ma.t);

				if ( ms.splines[s].animations == null )
					ms.splines[s].animations = new List<MegaKnotAnim>();

				ms.splines[s].animations.Add(ma);
				break;

			case "Anim":
				ma.con = MegaParseBezVector3Control.LoadBezVector3KeyControl(br);
				break;
		}
		return true;
	}

	bool ParseShape(BinaryReader br, string cid)
	{
		MegaShape ms = (MegaShape)target;

		switch ( cid )
		{
			case "Num":
				int count = br.ReadInt32();
				ms.splines = new List<MegaSpline>(count);
				break;

			case "Spline":
				MegaSpline spl = new MegaSpline();
				ms.splines.Add(spl);
				MegaParse.Parse(br, SplineParse);
				break;

			case "Anim":
				MegaParse.Parse(br, AnimParse);
				break;
		}

		return true;
	}

	[MenuItem("GameObject/Create MegaShape Prefab")]
	static void DoCreateSimplePrefabNew()
	{
		if ( Selection.activeGameObject != null )
		{
			if ( !Directory.Exists("Assets/MegaPrefabs") )
			{
				AssetDatabase.CreateFolder("Assets", "MegaPrefabs");
				//string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);
				//Debug.Log("folder path " + newFolderPath);
			}

			GameObject obj = Selection.activeGameObject;

			GameObject prefab = PrefabUtility.CreatePrefab("Assets/MegaPrefabs/" + Selection.activeGameObject.name + ".prefab", Selection.activeGameObject);
			MeshFilter mf = obj.GetComponent<MeshFilter>();

			if ( mf )
			{
				MeshFilter newmf = prefab.GetComponent<MeshFilter>();

				Mesh mesh = CloneMesh(mf.sharedMesh);

				mesh.name = obj.name + " copy";
				AssetDatabase.AddObjectToAsset(mesh, prefab);
				//AssetDatabase.CreateAsset(mesh, "Assets/MegaPrefabs/" + Selection.activeGameObject.name + ".asset");
				newmf.sharedMesh = mesh;

				MeshCollider mc = prefab.GetComponent<MeshCollider>();
				if ( mc )
				{
					mc.sharedMesh = null;
					mc.sharedMesh = mesh;
				}
			}

			//PrefabUtility.DisconnectPrefabInstance(prefab);
		}
	}

	static Mesh CloneMesh(Mesh mesh)
	{
		Mesh clonemesh = new Mesh();
		clonemesh.vertices = mesh.vertices;
		clonemesh.uv2 = mesh.uv2;
		clonemesh.uv2 = mesh.uv2;
		clonemesh.uv = mesh.uv;
		clonemesh.normals = mesh.normals;
		clonemesh.tangents = mesh.tangents;
		clonemesh.colors = mesh.colors;

		clonemesh.subMeshCount = mesh.subMeshCount;

		for ( int s = 0; s < mesh.subMeshCount; s++ )
			clonemesh.SetTriangles(mesh.GetTriangles(s), s);

		clonemesh.boneWeights = mesh.boneWeights;
		clonemesh.bindposes = mesh.bindposes;
		clonemesh.name = mesh.name + "_copy";
		clonemesh.RecalculateBounds();

		return clonemesh;
	}

	// Animation keyframe stuff
	// Need system to grab state of curve
	void AnimationKeyFrames(MegaShape shape)
	{
		MegaSpline spline = shape.splines[shape.selcurve];

		shape.showanimations = EditorGUILayout.Foldout(shape.showanimations, "Animations");

		if ( shape.showanimations )
		{
			shape.keytime = EditorGUILayout.FloatField("Key Time", shape.keytime);
			if ( shape.keytime < 0.0f )
				shape.keytime = 0.0f;

			spline.splineanim.Enabled = EditorGUILayout.BeginToggleGroup("Enabled", spline.splineanim.Enabled);
			EditorGUILayout.BeginHorizontal();
			//if ( spline.splineanim == null )
			//{
			//}
			//else
			{
				//if ( GUILayout.Button("Create") )
				//{
					//spline.splineanim = new MegaSplineAnim();
					//spline.splineanim.Init(spline);
				//}

				if ( GUILayout.Button("Add Key") )
				{
					AddKeyFrame(shape, shape.keytime);
				}

				if ( GUILayout.Button("Clear") )
				{
					ClearAnim(shape);
				}

				//if ( GUILayout.Button("Delete") )
				//{
				//	spline.splineanim = null;
				//}
			}

			EditorGUILayout.EndHorizontal();
			//if ( spline.splineanim == null )
			//	return;

			// Need to show each keyframe
			if ( spline.splineanim != null )
			{
				//EditorGUILayout.LabelField("Frames " + spline.splineanim.knots[0].)
				int nk = spline.splineanim.NumKeys();

				float mt = 0.0f;
				for ( int i = 0; i < nk; i++ )
				{
					EditorGUILayout.BeginHorizontal();

					mt = spline.splineanim.GetKeyTime(i);

					EditorGUILayout.LabelField("" + i);	//" + " Time: " + mt);
					float t = EditorGUILayout.FloatField("", mt);

					if ( t != mt )
						spline.splineanim.SetKeyTime(spline, i, t);

					if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
						spline.splineanim.RemoveKey(i);

					if ( GUILayout.Button("Update", GUILayout.MaxWidth(50)) )
						spline.splineanim.UpdateKey(spline, i);

					if ( GUILayout.Button("Get", GUILayout.MaxWidth(50)) )
					{
						spline.splineanim.GetKey(spline, i);
						EditorUtility.SetDirty(target);
					}

					EditorGUILayout.EndHorizontal();
				}

				shape.MaxTime = mt;

				float at = EditorGUILayout.Slider("T", shape.testtime, 0.0f, mt);
				if ( at != shape.testtime )
				{
					shape.testtime = at;
					if ( !shape.animate )
					{
						for ( int s = 0; s < shape.splines.Count; s++ )
						{
							if ( shape.splines[s].splineanim != null && shape.splines[s].splineanim.Enabled )
							{
								shape.splines[s].splineanim.GetState1(shape.splines[s], at);
								shape.splines[s].CalcLength();	//(10);	// could use less here
							}
						}
					}
				}
			}

			EditorGUILayout.EndToggleGroup();
		}
	}

	void ClearAnim(MegaShape shape)
	{
		MegaSpline spline = shape.splines[shape.selcurve];

		if ( spline.splineanim != null )
		{
			spline.splineanim.Init(spline);
		}
	}

	void AddKeyFrame(MegaShape shape, float t)
	{
		MegaSpline spline = shape.splines[shape.selcurve];

		//if ( spline.splineanim == null )
		//{
			//Debug.Log("1");
			//MegaSplineAnim sa = new MegaSplineAnim();
			//sa.Init(spline);
			//spline.splineanim = sa;
		//}
		//else
		//{
			spline.splineanim.AddState(spline, t);
		//}
	}
}
