
using UnityEditor;
using UnityEngine;

// TODO: Select axis for shapes
// TODO: Add new spline to shape
// TODO: Button to recalc lengths
// TEST: Build a simple scene in max then have a road, barrier, fence etc
// Import of simple text file for path
public class MegaFiersWindow : EditorWindow
{
	static bool		showcommon;
	//string name = "Shape";
	//static MegaAxis	axis = MegaAxis.Y;
	//static bool		drawknots = true;
	//static bool		drawhandles = true;
	//static float	stepdist = 0.1f;
	//static float	knotsize = 0.05f;
	static Color	col1 = Color.yellow;
	static Color	col2 = Color.green;

	static MegaNormalMethod	NormalMethod = MegaNormalMethod.Mega;

	// Add menu named "My Window" to the Window menu
	[MenuItem("Component/MegaFiers")]
	static void Init()
	{
		// Get existing open window or if none, make a new one:
		//MegaShapeWindow window = (MegaShapeWindow)EditorWindow.GetWindow(typeof(MegaShapeWindow));
		EditorWindow.GetWindow(typeof(MegaFiersWindow), false, "MegaFiers");
	}

#if false
	public class Buttons
	{
		public Buttons()	{}
		public Buttons(string _but, System.Type _classname)
		{
			Create(null, _but, _classname);
		}

		public Buttons(string _tooltip, string _but, System.Type _classname)
		{
			Create(_tooltip, _but, _classname);
		}

		public void Create(string _tooltip, string _but, System.Type _classname)
		{
			classname = _classname;

			Texture image = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Mega-Fiers/Editor/Icons/" + _but, typeof(Texture));
			//Texture image = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Mega-Fiers/Editor/Icons/" + "mybendicon.png", typeof(Texture));
			if ( image == null )
				Debug.Log("Didnt load icon " + _but);
			else
				content = new GUIContent(image, classname.ToString() + " a Mesh");
		}
		public System.Type	classname;
		public GUIContent	content;
	}

	static Buttons[] buts = new Buttons[] {
		new Buttons("Bend a Mesh",		"mybendicon.png", typeof(MegaBend)),
		new Buttons("Bubble a Mesh",	"mybubbleicon.png", typeof(MegaBubble)),
		new Buttons("Bulge a Mesh",		"mybendicon.png", typeof(MegaBulge)),
		new Buttons("mybendicon.png", typeof(MegaCrumple)),
		new Buttons("mybendicon.png", typeof(MegaCurveDeform)),
		new Buttons("mybendicon.png", typeof(MegaCylindrify)),
		new Buttons("mybendicon.png", typeof(MegaDisplace)),
		new Buttons("mybendicon.png", typeof(MegaFFD2x2x2)),
		new Buttons("mybendicon.png", typeof(MegaFFD3x3x3)),
		new Buttons("mybendicon.png", typeof(MegaFFD4x4x4)),
		new Buttons("myhumpicon.png", typeof(MegaHump)),
		new Buttons("mymelticon.png", typeof(MegaMelt)),
		new Buttons("mynoiseicon.png", typeof(MegaNoise)),
		new Buttons("mybendicon.png", typeof(MegaPageFlip)),
		new Buttons("mybendicon.png", typeof(MegaPaint)),
		new Buttons("mybendicon.png", typeof(MegaPathDeform)),
		new Buttons("mybendicon.png", typeof(MegaPivotAdjust)),
		new Buttons("mybendicon.png", typeof(MegaPointCache)),
		new Buttons("mypushicon.png", typeof(MegaPush)),
		new Buttons("mybendicon.png", typeof(MegaRadialSkew)),
		new Buttons("mybendicon.png", typeof(MegaRipple)),
		new Buttons("mybendicon.png", typeof(MegaRopeDeform)),
		new Buttons("mybendicon.png", typeof(MegaRubber)),
		new Buttons("mybendicon.png", typeof(MegaSimpleMod)),
		new Buttons("mybendicon.png", typeof(MegaSinusCurve)),
		new Buttons("myskewicon.png", typeof(MegaSkew)),
		new Buttons("myspherifyicon.png", typeof(MegaSpherify)),
		new Buttons("mystretchicon.png", typeof(MegaStretch)),
		new Buttons("mytapericon.png", typeof(MegaTaper)),
		new Buttons("mytwisticon.png", typeof(MegaTwist)),
		new Buttons("mybendicon.png", typeof(MegaVertexAnim)),
		new Buttons("mybendicon.png", typeof(MegaVertNoise)),
		new Buttons("mybendicon.png", typeof(MegaWave)),
		new Buttons("mybendicon.png", typeof(MegaWorldPathDeform)),

#if false
				"Bend",			"MegaBend",
		"Bubble",		"MegaBubble",
		"Bulge",		"MegaBulge",
		"Crumple",		"MegaCrumple",
		"Curve Deform",	"MegaCurveDeform",
		"Cylindrify",	"MegaCylindrify",
		"Displace",		"MegaDisplace",
		"FFD2x2x2",		"MegaFFD2x2x2",
		"FFD3x3x3",		"MegaFFD3x3x3",
		"FFD4x4x4",		"MegaFFD4x4x4",
		"Hump",			"MegaHump",
		"Melt",			"MegaMelt",
		"Noise",		"MegaNoise",
		"PageFlip",		"MegaPageFlip",
		"Paint",		"MegaPaint",
		"Path Deform",	"MegaPathDeform",
		"Pivot Adjust",	"MegaPivotAdjust",
		"Point Cache",	"MegaPointCache",
		"Push",			"MegaPush",
		"Radial Skew",	"MegaRadialSkew",
		"Ripple",		"MegaRipple",
		"Rope 2D",		"MegaRopeDeform",
		"Rubber",		"MegaRubber",
		"SimpleMod",	"MegaSimpleMod",
		"SinusCurve",	"MegaSinusCurve",
		"Skew",			"MegaSkew",
		"Spherify",		"MegaSpherify",
		"Stretch",		"MegaStretch",
		"Taper",		"MegaTaper",
		"Twist",		"MegaTwist",
		"VertexAnim",	"MegaVertexAnim",
		"VertNoise",	"MegaVertNoise",
		"Wave",			"MegaWave",
		"World PathDeform",	"MegaWorldPathDeform",
#endif
	};
#endif

	//void Update()
	//{
		//Debug.Log("Mouse " + Input.mousePosition);
		//Debug.Log("Update");
	//}

#if false
	public float scrollx = 0.0f;
	void DoScrollIcons(GameObject obj, Buttons[] buttons, float width, int bstep, bool modobj)
	{
		//EditorGUILayout.BeginHorizontal(GUIStyle.none);
		//EditorGUILayout.BeginArea(Rect(10, 10, 100, 100));

		Rect pos = new Rect(0, 0, 400.0f, 64.0f);

		pos.x = scrollx;

		for ( int i = 0; i < buttons.Length; i++ )
		{
			//if ( GUILayout.Button(buttons[i].image, GUILayout.Width(width)) )
			if ( buttons[i].content != null && GUI.Button(pos, buttons[i].content, GUIStyle.none) )
			{
				AddModType(obj, buttons[i].classname, modobj);
			}

			pos.x += width;
		}
		//EditorGUILayout.EndHorizontal();
	}
#endif

#if false
	void DoIcons(GameObject obj, Buttons[] buttons, float width, int bstep, bool modobj)
	{
		int off = 0;
		//width /= 4.0f;
		//bstep *= 4;
		for ( int i = 0; i < buttons.Length; i++ )
		{
			if ( off == 0 )
			{
				EditorGUILayout.BeginHorizontal(GUIStyle.none);
			}

			//if ( GUILayout.Button(buttons[i].image, GUILayout.Width(width)) )
			if ( buttons[i].content != null && GUILayout.Button(buttons[i].content, GUIStyle.none, GUILayout.Width(width), GUILayout.Height(48.0f)) )
			{
				AddModType(obj, buttons[i].classname, modobj);
			}

			off++;
			if ( off == bstep )
			{
				off = 0;
				EditorGUILayout.EndHorizontal();
			}
		}

		if ( off != 0 )
		{
			EditorGUILayout.EndHorizontal();
		}
	}
#endif

	static Color modcol = new Color(0.75f, 0.75f, 1.0f);
	static Color uvmodscol = new Color(1.0f, 0.75f, 0.75f);
	static Color warpcol = new Color(0.75f, 1.0f, 0.75f);
	static Color selmodscol = new Color(1.0f, 1.0f, 0.75f);
	static Color utilmodscol = new Color(0.75f, 1.0f, 1.0f);

	static MegaModBut[] mods = new MegaModBut[] {
		new MegaModBut("Bend",			"Bend a mesh",									typeof(MegaBend), modcol),
		new MegaModBut("Bubble",		"Bubble a mesh",								typeof(MegaBubble), modcol),
		new MegaModBut("Bulge",			"Add a Bulge to a mesh",						typeof(MegaBulge), modcol),
		new MegaModBut("Crumple",		"Crumple up a mesh, based on Unity Crumple",	typeof(MegaCrumple), modcol),
		new MegaModBut("Curve",			"Use a curve to bend a mesh",					typeof(MegaCurveDeform), modcol),
		new MegaModBut("Cylindrify",	"Cylindrify a mesh",							typeof(MegaCylindrify), modcol),
		new MegaModBut("Displace",		"Displace vertices using a texture",			typeof(MegaDisplace), modcol),
		new MegaModBut("FFD 2x2x2",		"FFD with a 2x2x2 lattice",						typeof(MegaFFD2x2x2), modcol),
		new MegaModBut("FFD 3x3x3",		"FFD with a 3x3x3 lattice",						typeof(MegaFFD3x3x3), modcol),
		new MegaModBut("FFD 4x4x4",		"FFD with a 4x4x4 lattice",						typeof(MegaFFD4x4x4), modcol),
		new MegaModBut("Hump",			"Add humps to a mesh",							typeof(MegaHump), modcol),
		new MegaModBut("Melt",			"Melt a mesh",									typeof(MegaMelt), modcol),
		new MegaModBut("Morph",			"Morph a mesh",									typeof(MegaMorph), modcol),
		new MegaModBut("MorphOMat",		"MorphOMatic a mesh",							typeof(MegaMorphOMatic), modcol),
		new MegaModBut("Noise",			"Add noise to a mesh",							typeof(MegaNoise), modcol),
		new MegaModBut("PageFlip",		"Make a page turning effect",					typeof(MegaPageFlip), modcol),
		new MegaModBut("Paint",			"Paint deformation onto a mesh",				typeof(MegaPaint), modcol),
		new MegaModBut("Path",			"Deform a mesh along a path",					typeof(MegaPathDeform), modcol),
		new MegaModBut("Pivot",			"Alter pivot point on a mesh",					typeof(MegaPivotAdjust), modcol),
		new MegaModBut("PointCache",	"Point cache animation or vertices",			typeof(MegaPointCache), modcol),
		new MegaModBut("Push",			"Push vertices along their normals",			typeof(MegaPush), modcol),
		new MegaModBut("RadialSkew",	"Radial skew a mesh",							typeof(MegaRadialSkew), modcol),
		new MegaModBut("Ripple",		"Add a ripple to a mesh",						typeof(MegaRipple), modcol),
		new MegaModBut("Rope",			"Deform a mesh using 2D Rope physics",			typeof(MegaRopeDeform), modcol),
		new MegaModBut("Rubber",		"Add secondary rubber motion to a mesh",		typeof(MegaRubber), modcol),
		new MegaModBut("Simple",		"Example of a simple mod",						typeof(MegaSimpleMod), modcol),
		new MegaModBut("Sinus",			"Sin wave deformation based on Unity example",	typeof(MegaSinusCurve), modcol),
		new MegaModBut("Skew",			"Skew a mesh",									typeof(MegaSkew), modcol),
		new MegaModBut("Spherify",		"Turn a mesh into a sphere",					typeof(MegaSpherify), modcol),
		new MegaModBut("Squeeze",		"Squeeze a mesh",								typeof(MegaSqueeze), modcol),
		new MegaModBut("Stretch",		"Squash and stretch a mesh",					typeof(MegaStretch), modcol),
		new MegaModBut("Taper",			"Taper a mesh",									typeof(MegaTaper), modcol),
		new MegaModBut("Twist",			"Twist a mesh",									typeof(MegaTwist), modcol),
		new MegaModBut("VertAnim",		"Animate vertices on a mesh",					typeof(MegaVertexAnim), modcol),
		new MegaModBut("Vert Noise",	"Add Vertical noise to a mesh",					typeof(MegaVertNoise), modcol),
		new MegaModBut("Wave",			"Add a wave to a mesh",							typeof(MegaWave), modcol),
		new MegaModBut("World Path",	"Deform a mesh along a path in world space",	typeof(MegaWorldPathDeform), modcol),
	};

	static MegaModBut[] warpmods = new MegaModBut[] {
		new MegaModBut("Warp Bind",		"Bind a mesh to a World Space Warp",			typeof(MegaWarpBind), warpcol),
		new MegaModBut("Bend",			"Bend Warp",									typeof(MegaBendWarp), warpcol),
		new MegaModBut("Noise",			"Noise Warp",									typeof(MegaNoiseWarp), warpcol),
		new MegaModBut("Ripple",		"Ripple Warp",									typeof(MegaRippleWarp), warpcol),
		new MegaModBut("Skew",			"Skew Warp",									typeof(MegaSkewWarp), warpcol),
		new MegaModBut("Stretch",		"Stretch Warp",									typeof(MegaStretchWarp), warpcol),
		new MegaModBut("Taper",			"Taper Warp",									typeof(MegaTaperWarp), warpcol),
		new MegaModBut("Twist",			"Twist Warp",									typeof(MegaTwistWarp), warpcol),
		new MegaModBut("Wave",			"Wave Warp",									typeof(MegaWaveWarp), warpcol),
	};

	static MegaModBut[] uvmods = new MegaModBut[] {
		new MegaModBut("UVAdjust",		"Transform a meshes UV coords",					typeof(MegaUVAdjust), uvmodscol),
		new MegaModBut("UVTile",		"Animate UVs to playback sprite anim",			typeof(MegaUVTiles), uvmodscol),
	};

	static MegaModBut[] selmods = new MegaModBut[] {
		new MegaModBut("VertCol",		"Select vertices by vertex color",				typeof(MegaVertColSelect), selmodscol),
		new MegaModBut("Vol Select",	"Select vertices by volumes",					typeof(MegaVolSelect), selmodscol),
	};

	static MegaModBut[] utilmods = new MegaModBut[] {
		new MegaModBut("Anim",			"Animate morph percents",						typeof(MegaMorphAnim), utilmodscol),
		new MegaModBut("Animator",		"Use anim clips to animate morphs",				typeof(MegaMorphAnimator), utilmodscol),
		new MegaModBut("BallBounce",	"Simulate soft ball bouncing",					typeof(MegaBallBounce), utilmodscol),
		new MegaModBut("Book",			"Book builder",									typeof(MegaBook), utilmodscol),
		new MegaModBut("MultiCore",		"Script to toggle multicore support",			typeof(MegaToggleMultiCore), utilmodscol),
		new MegaModBut("Page",			"Build a page mesh for books",					typeof(MegaMeshPage), utilmodscol),
		new MegaModBut("Scroll",		"Simulate an old paper scroll",					typeof(MegaScroll), utilmodscol),
		new MegaModBut("WalkBridge",	"Helper to move a character across a bridge",	typeof(MegaWalkBridge), utilmodscol),
		new MegaModBut("WalkRope",		"Helper to move a character across a rope bridge",	typeof(MegaWalkRope), utilmodscol),
	};

#if false
	static MegaModBut[] uvmods1 = new MegaModBut[] {
		new ModBut("UVAdjust",		"Transform a meshes UV coords",					typeof(MegaUVAdjust), uvmodscol),
		new ModBut("UVTile",		"Animate UVs to playback sprite anim",			typeof(MegaUVTiles), uvmodscol),
	};

	static string[] mods1 = new string[] {
		"Bend",			"MegaBend",
		"Bubble",		"MegaBubble",
		"Bulge",		"MegaBulge",
		"Crumple",		"MegaCrumple",
		"Curve",		"MegaCurveDeform",
		"Cylindrify",	"MegaCylindrify",
		"Displace",		"MegaDisplace",
		"FFD 2x2x2",	"MegaFFD2x2x2",
		"FFD 3x3x3",	"MegaFFD3x3x3",
		"FFD 4x4x4",	"MegaFFD4x4x4",
		"Hump",			"MegaHump",
		"Melt",			"MegaMelt",
		"Noise",		"MegaNoise",
		"PageFlip",		"MegaPageFlip",
		"Paint",		"MegaPaint",
		"Path",			"MegaPathDeform",
		"Pivot",		"MegaPivotAdjust",
		"PointCache",	"MegaPointCache",
		"Push",			"MegaPush",
		"RadialSkew",	"MegaRadialSkew",
		"Ripple",		"MegaRipple",
		"Rope 2D",		"MegaRopeDeform",
		"Rubber",		"MegaRubber",
		"SimpleMod",	"MegaSimpleMod",
		"SinusCurve",	"MegaSinusCurve",
		"Skew",			"MegaSkew",
		"Spherify",		"MegaSpherify",
		"Stretch",		"MegaStretch",
		"Taper",		"MegaTaper",
		"Twist",		"MegaTwist",
		"VertexAnim",	"MegaVertexAnim",
		"VertNoise",	"MegaVertNoise",
		"Wave",			"MegaWave",
		"World Path",	"MegaWorldPathDeform",
	};

	static string[] uvmods = new string[] {
		"UVAdjust",	"MegaUVAdjust",
		"UVTile",	"MegaUVTile",
	};

	static string[] selmods = new string[] {
		"Vert Col",		"MegaVertColSelect",
		"Vol Select",	"MegaVolSelect",
	};

	static string[] morphmods = new string[] {
		"Morph",		"MegaMorph",
		"MorphOMatic",	"MegaMorphOMatic",
	};

	static string[] utils = new string[] {
		"Anim",				"MegaMorphAnim",
		"Animator",			"MegaMorphAnimator",
		"Ball Bounce",		"BallBounce",
		"Book Builder",		"MegaBook",
		"MultiCore",		"ToggleMultiCore",
		"Page",				"MegaMeshPage",
		"Scroll",			"Scroll",
		"Walk Bridge",		"WalkBridge",
		"Walk Rope",		"WalkRope",
	};
#endif

#if false
	void AddMod(GameObject go, string name, bool modobj)
	{
		if ( go )
		{
			MeshFilter mf = go.GetComponent<MeshFilter>();
			if ( mf != null )
			{
				if ( modobj )
				{
					MegaModifyObject mod = go.GetComponent<MegaModifyObject>();

					if ( mod == null )
					{
						mod = go.AddComponent<MegaModifyObject>();
						mod.NormalMethod = NormalMethod;
					}
				}

				if ( name != null )
				{
					MegaModifier md = (MegaModifier)go.AddComponent(name);
					if ( md )
					{
						md.gizCol1 = col1;
						md.gizCol2 = col2;
					}
				}
			}
		}
	}
#endif

	void AddModType(GameObject go, System.Type name, bool modobj)
	{
		if ( go )
		{
			if ( !name.IsSubclassOf(typeof(MegaModifier)) && !name.IsSubclassOf(typeof(MegaWarp)) )
			{
				go.AddComponent(name);
			}
			else
			{
				MeshFilter mf = go.GetComponent<MeshFilter>();
				if ( mf != null )
				{
					if ( modobj )
					{
						if ( name.IsSubclassOf(typeof(MegaModifier)) )
						{
							MegaModifyObject mod = go.GetComponent<MegaModifyObject>();

							if ( mod == null )
							{
								mod = go.AddComponent<MegaModifyObject>();
								mod.NormalMethod = NormalMethod;
							}
						}
					}

					if ( name != null )
					{
						if ( name.IsSubclassOf(typeof(MegaModifier)) )
						{
							MegaModifier md = (MegaModifier)go.AddComponent(name);
							if ( md )
							{
								md.gizCol1 = col1;
								md.gizCol2 = col2;
							}
						}
						else
						{
							if ( name.IsSubclassOf(typeof(MegaWarp)) )
							{
								MegaWarp md = (MegaWarp)go.AddComponent(name);
								if ( md )
								{
									md.GizCol1 = col1;
									md.GizCol2 = col2;
								}
							}
							else
							{
								go.AddComponent(name);
							}
						}
					}
				}
				else
				{
					if ( name.IsSubclassOf(typeof(MegaWarp)) )
					{
						MegaWarp md = (MegaWarp)go.AddComponent(name);
						if ( md )
						{
							md.GizCol1 = col1;
							md.GizCol2 = col2;
						}
					}
				}
			}
		}
	}

	// Put common params in, and each shape has its sections
	Vector2 scroll = Vector2.zero;

	//bool showmods = false;
	//bool showuvmods = false;
	//bool showselmods = false;
	//bool showmorphmods = false;
	//bool showutilsmods = false;

#if false
	void DoButtons(GameObject obj, string[] buttons, float width, int bstep, bool modobj)
	{
		Color c = GUI.backgroundColor;
		int off = 0;
		Color guicol = GUI.color;
		GUI.color = Color.blue;
		GUI.backgroundColor = Color.red;
		GUI.contentColor = Color.green;
		Color col1 = new Color(1.0f, 0.627f, 0.0f);
		Color col2 = new Color(0.0f, 0.627f, 1.0f);

		for ( int i = 0; i < buttons.Length; i += 2 )
		{
			if ( (i & 2) == 0 )
				GUI.color = col1;	//Color.blue;
			else
				GUI.color = col2;	//Color.yellow;

			if ( off == 0 )
			{
				EditorGUILayout.BeginHorizontal();
			}

			if ( GUILayout.Button(buttons[i], GUILayout.Width(width)) )
				AddMod(obj, buttons[i + 1], modobj);

			off++;
			if ( off == bstep )
			{
				off = 0;
				EditorGUILayout.EndHorizontal();
			}
		}

		if ( off != 0 )
			EditorGUILayout.EndHorizontal();

		GUI.backgroundColor = c;
		GUI.color = guicol;
		GUI.contentColor = Color.white;
	}
#endif

	int DoButtons(GameObject obj, MegaModBut[] buttons, float width, int bstep, bool modobj, int off)
	{
		Color c = GUI.backgroundColor;
		//int off = 0;
		GUI.backgroundColor = Color.blue;
		Color guicol = GUI.color;
		GUI.color = new Color(1, 1, 1, 1);	//Color.white;
		GUI.backgroundColor = new Color(0, 0, 0, 0);	//Color.white;
		GUI.contentColor = Color.white;
		//Color col1 = new Color(1.0f, 0.627f, 0.0f);
		//Color col2 = new Color(0.0f, 0.627f, 1.0f);

		for ( int i = 0; i < buttons.Length; i++ )
		{
			//if ( (i & 2) == 0 )
				//GUI.backgroundColor = buttons[i].color;	//Color.blue;
			GUI.contentColor = buttons[i].color;	//Color.blue;
				//else
			//	GUI.backgroundColor = col2;	//Color.yellow;
			//Color co = buttons[i].color * 0.08f;
			//co.a = 0.18f;
			GUI.backgroundColor = buttons[i].color * 0.08f;

			if ( off == 0 )
			{
				EditorGUILayout.BeginHorizontal();
			}

			//if ( GUILayout.Button(buttons[i].content, GUILayout.Width(width)) )
			if ( GUILayout.Button(buttons[i].content, GUILayout.Width(width)) )
				AddModType(obj, buttons[i].classname, modobj);

			off++;
			if ( off == bstep )
			{
				off = 0;
				EditorGUILayout.EndHorizontal();
			}
		}

		//if ( off != 0 )
		//	EditorGUILayout.EndHorizontal();

		GUI.backgroundColor = c;
		GUI.color = guicol;
		return off;
	}

	public int toolbarInt = 0;
	public string[] toolbarStrings = new string[] {"All", "Mod", "Warp", "UV", "Sel", "Util" };

	void OnGUI()
	{
		//name = EditorGUILayout.TextField("Name", name);
		scroll = EditorGUILayout.BeginScrollView(scroll);

		GameObject obj = Selection.activeGameObject;

		float width = this.position.width;	// / 2.0f;

		//if ( obj )
		{
			//MeshFilter mf = obj.GetComponent<MeshFilter>();
			//if ( mf != null )
			{
				//if ( GUILayout.Button("Modify Object") ) AddMod(obj, null, true);	//.AddComponent<MegaModifyObject>();

				float butwidth = 80.0f;

				int bstep = (int)(width / butwidth);
				if ( bstep == 0 )
					bstep = 1;

				//int off = 0;

				//showmods = EditorGUILayout.Foldout(showmods, "Modifiers");

				toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings, GUILayout.MaxWidth(250.0f));

				//if ( showmods )
				{
					int off = 0;
					//DoScrollIcons(obj, buts, 48.0f, (int)(width / 48.0f), true);
					//DoIcons(obj, buts, 48.0f, (int)(width / 48.0f), true);
					switch ( toolbarInt )
					{
						case 0:
							off = DoButtons(obj, mods, (width / bstep) - 6.0f, bstep, true, off);
							off = DoButtons(obj, warpmods, (width / bstep) - 6.0f, bstep, true, off);
							off = DoButtons(obj, uvmods, (width / bstep) - 6.0f, bstep, true, off);
							off = DoButtons(obj, selmods, (width / bstep) - 6.0f, bstep, true, off);
							off = DoButtons(obj, utilmods, (width / bstep) - 6.0f, bstep, true, off);
							break;

						case 1:	DoButtons(obj, mods, (width / bstep) - 6.0f, bstep, true, off);	break;
						case 2:	DoButtons(obj, warpmods, (width / bstep) - 6.0f, bstep, true, off);	break;
						case 3: DoButtons(obj, uvmods, (width / bstep) - 6.0f, bstep, true, off); break;
						case 4: DoButtons(obj, selmods, (width / bstep) - 6.0f, bstep, true, off); break;
						case 5: DoButtons(obj, utilmods, (width / bstep) - 6.0f, bstep, true, off); break;
					}

					if ( off != 0 )
						EditorGUILayout.EndHorizontal();

					//GUI.backgroundColor = Color.grey;
#if false
					if ( GUILayout.Button("Modify Object") ) AddMod(obj, null, true);	//.AddComponent<MegaModifyObject>();

					showcommon = EditorGUILayout.Foldout(showcommon, "Common");

					if ( showcommon )
					{
						NormalMethod = (MegaNormalMethod)EditorGUILayout.EnumPopup("Normal Method", NormalMethod);

						//axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", axis);
						col1 = EditorGUILayout.ColorField("Color 1", col1);
						col2 = EditorGUILayout.ColorField("Color 2", col2);
					}
#endif
	#if false
					for ( int i = 0; i < mods.Length; i += 2 )
					{
						if ( off == 0 )
						{
							EditorGUILayout.BeginHorizontal();
						}

						if ( GUILayout.Button(mods[i], GUILayout.Width((width / bstep) - 6.0f)) )
							AddMod(obj, mods[i + 1]);

						off++;
						if ( off == bstep )
						{
							off = 0;
							EditorGUILayout.EndHorizontal();
						}
					}
	#endif
				}

				//showuvmods = EditorGUILayout.Foldout(showuvmods, "UV Modifiers");

				//if ( showuvmods )
				//	DoButtons(obj, uvmods, (width / bstep) - 6.0f, bstep, true);

				//showmorphmods = EditorGUILayout.Foldout(showmorphmods, "Morph");

				//if ( showmorphmods )
				//	DoButtons(obj, morphmods, (width / bstep) - 6.0f, bstep, true);

				//showselmods = EditorGUILayout.Foldout(showselmods, "Selection");

				//if ( showselmods )
				//	DoButtons(obj, selmods, (width / bstep) - 6.0f, bstep, true);

				//showutilsmods = EditorGUILayout.Foldout(showutilsmods, "Utils");

				//if ( showutilsmods )
				//	DoButtons(obj, utils, (width / bstep) - 6.0f, bstep, false);
			}
		}

		EditorGUILayout.EndScrollView();
	}

#if false
	static void CreateShape(string type)
	{
		Vector3 pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		MegaShape ms = null;
		GameObject go = new GameObject(type + " Shape");

		switch ( type )
		{
			case "Circle": ms = go.AddComponent<MegaShapeCircle>(); break;
			case "Star": ms = go.AddComponent<MegaShapeStar>(); break;
			case "NGon": ms = go.AddComponent<MegaShapeNGon>(); break;
			case "Arc": ms = go.AddComponent<MegaShapeArc>(); break;
			case "Ellipse": ms = go.AddComponent<MegaShapeEllipse>(); break;
			case "Rectangle": ms = go.AddComponent<MegaShapeRectangle>(); break;
			case "Helix": ms = go.AddComponent<MegaShapeHelix>(); break;
		}

		go.transform.position = pos;
		Selection.activeObject = go;

		if ( ms != null )
		{
			ms.axis = axis;
			ms.drawHandles = drawhandles;
			ms.drawKnots = drawknots;
			ms.col1 = col1;
			ms.col2 = col2;
			ms.KnotSize = knotsize;
			ms.stepdist = stepdist;
		}
	}
#endif
}