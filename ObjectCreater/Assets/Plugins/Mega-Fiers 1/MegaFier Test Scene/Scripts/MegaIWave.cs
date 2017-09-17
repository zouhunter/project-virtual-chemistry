#if false
using UnityEngine;

#if true
[AddComponentMenu("Modifiers/Wave Ripple")]
public class MegaIWave : MegaModifier
{
	public MegaAxis	axis = MegaAxis.Y;
	//public int		cols = 8;
	//public int		rows = 8;

	public override string ModName()	{ return "Wave Ripple"; }
	public override string GetHelpURL() { return "?page_id=2"; }

	// IWave code
	float[,] kernel = new float[13, 13];

	float[] display_map;
	float[] obstruction;
	float[] source;

	float[] height;
	float[] previous_height;
	float[] vertical_derivative;

	void Initialize(float[] data, int size, float value)
	{
		for ( int i = 0; i < size; i++ )
		{
			data[i] = value;
		}
	}

	public static float j0(float x)
	{
		double ax;

		if ( (ax = Mathf.Abs(x)) < 8.0 )
		{
			double y = x * x;
			double ans1 = 57568490574.0 + y * (-13362590354.0 + y * (651619640.7
				+ y * (-11214424.18 + y * (77392.33017 + y * (-184.9052456)))));
			double ans2 = 57568490411.0 + y * (1029532985.0 + y * (9494680.718
				+ y * (59272.64853 + y * (267.8532712 + y * 1.0))));

			return (float)(ans1 / ans2);
		}
		else
		{
			double z = 8.0 / ax;
			double y = z * z;
			double xx = ax - 0.785398164;
			double ans1 = 1.0 + y * (-0.1098628627e-2 + y * (0.2734510407e-4
				+ y * (-0.2073370639e-5 + y * 0.2093887211e-6)));
			double ans2 = -0.1562499995e-1 + y * (0.1430488765e-3
				+ y * (-0.6911147651e-5 + y * (0.7621095161e-6
				- y * 0.934935152e-7)));

			return Mathf.Sqrt((float)(0.636619772 / ax)) * (Mathf.Cos((float)xx) * (float)ans1 - (float)z * Mathf.Sin((float)xx) * (float)ans2);
		}
	}

	// Compute the elements of the convolution kernel
	void InitializeKernel()
	{
		float dk = 0.01f;
		float sigma = 1.0f;
		float norm = 0.0f;

		for ( float k = 0.0f; k < 10.0f; k += dk )
		{
			norm += k * k * Mathf.Exp(-sigma * k * k);
		}

		for ( int i = -6; i <= 6; i++ )
		{
			for ( int j = -6; j <= 6; j++ )
			{
				float r = Mathf.Sqrt((float)(i * i + j * j));
				float kern = 0.0f;
				for ( float k = 0.0f; k < 10.0f; k += dk )
				{
					kern += k * k * Mathf.Exp(-sigma * k * k) * j0(r * k);
				}
				kernel[i + 6, j + 6] = kern / norm;
			}
		}
	}

	public int iwidth = 32;
	public int iheight = 32;
	//int size;

	void ComputeVerticalDerivative()
	{
		// first step:  the interior
		for ( int ix = 6; ix < iwidth - 6; ix++ )
		{
			for ( int iy = 6; iy < iheight - 6; iy++ )
			{
				int index = ix + iwidth * iy;
				float vd = 0.0f;
				for ( int iix = -6; iix <= 6; iix++ )
				{
					for ( int iiy = -6; iiy <= 6; iiy++ )
					{
						int iindex = ix + iix + iwidth * (iy + iiy);
						vd += kernel[iix + 6, iiy + 6] * height[iindex];
					}
				}
				vertical_derivative[index] = vd;
			}
		}
	}

	public float dt = 0.03f;
	public float alpha = 0.3f;
	float gravity = 9.81f;

	void Propagate()
	{
		gravity = 9.8f * dt * dt;

		// apply obstruction
		for ( int i = 0; i < height.Length; i++ )
		{
			height[i] *= obstruction[i];
		}

		// compute vertical derivative
		ComputeVerticalDerivative();

		// advance surface
		float adt = alpha * dt;	//Time.deltaTime;
		float adt2 = 1.0f / (1.0f + adt);
		for ( int i = 0; i < height.Length; i++ )
		{
			float temp = height[i];
			height[i] = height[i] * (2.0f - adt) - previous_height[i] - gravity * vertical_derivative[i];
			height[i] *= adt2;
			height[i] += source[i];
			height[i] *= obstruction[i];
			previous_height[i] = temp;
			// reset source each step
			source[i] = 0.0f;
		}
	}

	void ClearObstruction()
	{
		for ( int i = 0; i < obstruction.Length; i++ )
		{
			obstruction[i] = 1.0f;
		}
	}

	void ClearWaves()
	{
		for ( int i = 0; i < height.Length; i++ )
		{
			height[i] = 0.0f;
			previous_height[i] = 0.0f;
			vertical_derivative[i] = 0.0f;
		}
	}

	void InitIWave()
	{
		// initialize a few variables
		//iwidth = iheight = 100;
		int size = iwidth * iheight;

		//dt = 0.03f;
		//alpha = 0.3f;
		gravity = 9.8f * dt * dt;

		//scaling_factor = 1.0;
		//toggle_animation_on_off = true;

		// allocate space for fields and initialize them
		height              = new float[size];
		previous_height     = new float[size];
		vertical_derivative = new float[size];
		obstruction         = new float[size];
		source              = new float[size];
		display_map         = new float[size];

		ClearWaves();
		ClearObstruction();
		//ConvertToDisplay();
		Initialize(source, size, 0.0f);

		//InitializeBrushes();

		// build the convolution kernel
		InitializeKernel();

		vertexIndices = new int[verts.Length];

		Vector3 bsize = bbox.Size();
		for ( int i = 0; i < verts.Length; i++ )
		{
			float column = ((verts[i].x - bbox.min.x) / bsize.x);// + 0.5;
			float row = ((verts[i].z - bbox.min.z) / bsize.z);// + 0.5;

			if ( column >= 1.0f )
				column = 0.999f;

			if ( row >= 1.0f )
				row = 0.999f;

			int ci = (int)(column * (float)(iwidth));
			int ri = (int)(row * (float)(iheight));
			float position = (ri * (iwidth)) + ci;	// + 0.5f;

			vertexIndices[i] = (int)position;	//] = i;
		}
	}

	[ContextMenu("Reset Sim")]
	public void ResetGrid()
	{
		InitIWave();
	}

	// Float version
	public override void Modify(MegaModifiers mc)
	{
		for ( int i = 0; i < verts.Length; i++ )
		{
			Vector3 p = tm.MultiplyPoint3x4(verts[i]);

			int vertIndex = vertexIndices[i];

			p.y += height[vertIndex];	// * 1.0f / Force) * WaveHeight;

			sverts[i] = invtm.MultiplyPoint3x4(p);
		}
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		if ( i >= 0 )
		{
			int vertIndex = vertexIndices[i];

			p.y += height[vertIndex];	// * 1.0f / Force) * WaveHeight;
		}

		return invtm.MultiplyPoint3x4(p);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		int ax = (int)axis;
		float width = bbox.max[ax] - bbox.min[ax];

		// init
		if ( height == null )
			InitIWave();

		// Update ripples
		Propagate();

		return Prepare(mc);
	}

	//float[] currentBuffer;

	public override bool Prepare(MegaModContext mc)
	{
		return true;
	}

	// Ripple code
	//[HideInInspector]
	//public float[]	buffer1;
	//[HideInInspector]
	//public float[]	buffer2;
	//[HideInInspector]
	public int[]	vertexIndices;
	//public float	damping = 0.999f;
	//public float	WaveHeight = 2.0f;
	//public float	Force = 1000.0f;

#if false
	private bool swapMe = true;

	// Use this for initialization
	void Setup()
	{
		int len = rows * cols;
		buffer1 = new float[len];
		buffer2 = new float[len];

		vertexIndices = new int[verts.Length];

		for ( int i = 0; i < len; i++ )
		{
			buffer1[i] = 0;
			buffer2[i] = 0;
		}

		// this will produce a list of indices that are sorted the way I need them to 
		// be for the algo to work right
		Vector3 size = bbox.Size();
		for ( int i = 0; i < verts.Length; i++ )
		{
			float column = ((verts[i].x - bbox.min.x) / size.x);// + 0.5;
			float row = ((verts[i].z - bbox.min.z) / size.z);// + 0.5;

			if ( column >= 1.0f )
				column = 0.999f;

			if ( row >= 1.0f )
				row = 0.999f;

			int ci = (int)(column * (float)(cols));
			int ri = (int)(row * (float)(rows));
			float position = (ri * (cols)) + ci;	// + 0.5f;

			vertexIndices[i] = (int)position;	//] = i;
		}
	}

	// Normalized position, or even a world and local
	void splashAtPoint(int x, int y, float force)
	{
		int p = ((y * (cols)) + x);
		buffer1[p] = force;
		buffer1[p - 1] = force;
		buffer1[p + 1] = force;
		buffer1[p + (cols + 0)] = force;
		buffer1[p + (cols + 0) + 1] = force;
		buffer1[p + (cols + 0) - 1] = force;
		buffer1[p - (cols + 0)] = force;
		buffer1[p - (cols + 0) + 1] = force;
		buffer1[p - (cols + 0) - 1] = force;
	}

	// TODO: float version as well
	void processRipples(float[] source, float[] dest)
	{
		for ( int y = 1; y < rows - 1; y++ )
		{
			int yoff = (y * (cols));
			for ( int x = 1; x < cols - 1; x++ )
			{
				int p = yoff + x;
				dest[p] = (((source[p - 1] + source[p + 1] + source[p - (cols)] + source[p + (cols)]) * 0.5f) - dest[p]);
				dest[p] = dest[p] * damping;
			}
		}
	}
#endif

	public float Force = 1.0f;
	public float DropTime = 1.0f;
	float time = 0.0f;

	void Update()
	{
		time += Time.deltaTime;

		if ( time > DropTime )
		{
			time = 0.0f;

			int x = Random.Range(6, iwidth - 6);
			int y = Random.Range(6, iheight - 6);
			splashAtPoint(x, y, Force);
		}
	}

	void splashAtPoint(int x, int y, float force)
	{
		int p = ((y * (iwidth)) + x);
		source[p] = force;
		source[p - 1] = force * 0.5f;
		source[p + 1] = force * 0.5f;
		source[p + (iwidth + 0)] = force * 0.5f;
		source[p + (iwidth + 0) + 1] = force * 0.25f;
		source[p + (iwidth + 0) - 1] = force * 0.25f;
		source[p - (iwidth + 0)] = force * 0.5f;
		source[p - (iwidth + 0) + 1] = force * 0.25f;
		source[p - (iwidth + 0) - 1] = force * 0.25f;
	}
}
#endif
#endif