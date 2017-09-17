
using UnityEngine;

public enum MegaHoseType
{
	Round,
	Rectangle,
	DSection,
}

// Option for mesh to deform along
public enum MegaHoseSmooth
{
	SMOOTHALL,
	SMOOTHNONE,
	SMOOTHSIDES,
	SMOOTHSEGS,
}

[ExecuteInEditMode]
[AddComponentMenu("MegaShapes/Hose")]
[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class MegaHose : MonoBehaviour
{
	public bool				freecreate			= true;
	public bool				updatemesh			= true;
	Matrix4x4				S					= Matrix4x4.identity;
	const float				HalfIntMax			= 16383.5f;
	const float				PIover2				= 1.570796327f;
	const float				EPSILON				= 0.0001f;
	public MegaSpline		hosespline			= new MegaSpline();
	Mesh					mesh;
	public Vector3[]		verts				= new Vector3[1];
	public Vector2[]		uvs					= new Vector2[1];
	public int[]			faces				= new int[1];
	public Vector3[]		normals;

	public bool				optimize			= false;
	public bool				calctangents		= false;
	public bool				recalcCollider		= false;
	public bool				calcnormals			= false;
	public bool				capends				= true;
	public GameObject		custnode2;
	public GameObject		custnode;

	public Vector3			offset = Vector3.zero;
	public Vector3			offset1 = Vector3.zero;
	public Vector3			rotate = Vector3.zero;
	public Vector3			rotate1 = Vector3.zero;
	public Vector3			scale = Vector3.one;
	public Vector3			scale1 = Vector3.one;

	public int				endsmethod			= 0;
	public float			noreflength			= 1.0f;
	public int				segments			= 45;
	public MegaHoseSmooth	smooth				= MegaHoseSmooth.SMOOTHALL;
	public MegaHoseType		wiretype			= MegaHoseType.Round;
	public float			rnddia				= 0.2f;
	public int				rndsides			= 8;
	public float			rndrot				= 0.0f;
	public float			rectwidth			= 0.2f;
	public float			rectdepth			= 0.2f;
	public float			rectfillet			= 0.0f;
	public int				rectfilletsides		= 0;
	public float			rectrotangle		= 0.0f;
	public float			dsecwidth			= 0.2f;
	public float			dsecdepth			= 0.2f;
	public float			dsecfillet			= 0.0f;
	public int				dsecfilletsides		= 0;
	public int				dsecrndsides		= 4;
	public float			dsecrotangle		= 0.0f;
	public bool				mapmemapme			= true;
	public bool				flexon				= false;
	public float			flexstart			= 0.1f;
	public float			flexstop			= 0.9f;
	public int				flexcycles			= 5;
	public float			flexdiameter		= -0.2f;
	public float			tension1			= 10.0f;
	public float			tension2			= 10.0f;

	public bool				usebulgecurve		= false;
	public AnimationCurve	bulge				= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public float			bulgeamount			= 1.0f;
	public float			bulgeoffset			= 0.0f;
	public Vector2			uvscale				= Vector2.one;

	public bool				animatebulge		= false;
	public float			bulgespeed			= 0.0f;
	public float			minbulge			= -1.0f;
	public float			maxbulge			= 2.0f;

	public bool				displayspline		= true;
	//public bool				realtime			= true;

	bool visible = true;
	public bool InvisibleUpdate = false;
	public bool	dolateupdate = false;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=3436");
	}

	void Awake()
	{
		updatemesh = true;
		rebuildcross = true;
		Rebuild();
	}

	void Reset()
	{
		Rebuild();
	}

	void OnBecameVisible()
	{
		visible = true;
	}

	void OnBecameInvisible()
	{
		visible = false;
	}

	public void SetEndTarget(int end, GameObject target)
	{
		if ( end == 0 )
		{
			custnode = target;
		}
		else
		{
			custnode2 = target;
		}

		updatemesh = true;
	}

	public void Rebuild()
	{
		MeshFilter mf = GetComponent<MeshFilter>();

		if ( mf != null )
		{
			Mesh mesh = mf.sharedMesh;	//Utils.GetMesh(gameObject);

			if ( mesh == null )
			{
				mesh = new Mesh();
				mf.sharedMesh = mesh;
			}

			if ( mesh != null )
			{
				BuildMesh();
			}
			//updatemesh = false;
		}
	}

	void MakeSaveVertex(int NvertsPerRing, int nfillets, int nsides, MegaHoseType wtype)
	{
		if ( wtype == MegaHoseType.Round )
		{
			//Debug.Log("Verts " + NvertsPerRing);
			float ang = (Mathf.PI * 2.0f) / (float)(NvertsPerRing - 1);
			float diar = rnddia;
			diar *= 0.5f;
			for ( int i = 0; i < NvertsPerRing; i++ )
			{
				float u = (float)(i + 1) * ang;
				SaveVertex[i] = new Vector3(diar * (float)Mathf.Cos(u), diar * (float)Mathf.Sin(u), 0.0f);
			}
		}
		else
		{
			if ( wtype == MegaHoseType.Rectangle )
			{
				int savevertcnt = 0;
				int qtrverts = 1 + nfillets;
				int hlfverts = 2 * qtrverts;
				int thrverts = qtrverts + hlfverts;
				float Wr = rectwidth * 0.5f;
				float Dr = rectdepth * 0.5f;
				float Zfr = rectfillet;

				if ( Zfr < 0.0f )
					Zfr = 0.0f;

				if ( nfillets > 0 )
				{
					float WmZ = Wr - Zfr, DmZ = Dr - Zfr;
					float ZmW = -WmZ, ZmD = -DmZ;
					SaveVertex[0]					= new Vector3(Wr , DmZ, 0.0f);
					SaveVertex[nfillets]			= new Vector3(WmZ, Dr , 0.0f);
					SaveVertex[qtrverts]			= new Vector3(ZmW, Dr , 0.0f);
					SaveVertex[qtrverts + nfillets]	= new Vector3(-Wr, DmZ, 0.0f);
					SaveVertex[hlfverts]			= new Vector3(-Wr, ZmD, 0.0f);
					SaveVertex[hlfverts + nfillets]	= new Vector3(ZmW, -Dr, 0.0f);
					SaveVertex[thrverts]			= new Vector3(WmZ, -Dr, 0.0f);
					SaveVertex[thrverts + nfillets]	= new Vector3(Wr , ZmD, 0.0f);

					if ( nfillets > 1 )
					{
						float ang = PIover2 / (float)nfillets;
						savevertcnt = 1;
						for ( int i = 0; i < nfillets - 1; i++ )
						{
							float u = (float)(i + 1) * ang;
							float cu = Zfr * Mathf.Cos(u), su = Zfr * Mathf.Sin(u);
							SaveVertex[savevertcnt]				= new Vector3(WmZ + cu, DmZ + su, 0.0f);
							SaveVertex[savevertcnt + qtrverts]	= new Vector3(ZmW - su, DmZ + cu, 0.0f);
							SaveVertex[savevertcnt + hlfverts]	= new Vector3(ZmW - cu, ZmD - su, 0.0f);
							SaveVertex[savevertcnt + thrverts]	= new Vector3(WmZ + su, ZmD - cu, 0.0f);
							savevertcnt++;
						}
					}

					SaveVertex[SaveVertex.Length - 1] = SaveVertex[0];
				}
				else
				{
					if ( smooth == MegaHoseSmooth.SMOOTHNONE )
					{
						SaveVertex[savevertcnt++] = new Vector3(Wr, Dr, 0.0f);
						SaveVertex[savevertcnt++] = new Vector3(-Wr, Dr, 0.0f);
						SaveVertex[savevertcnt++] = new Vector3(-Wr, Dr, 0.0f);
						SaveVertex[savevertcnt++] = new Vector3(-Wr, -Dr, 0.0f);
						SaveVertex[savevertcnt++] = new Vector3(-Wr, -Dr, 0.0f);
						SaveVertex[savevertcnt++] = new Vector3(Wr, -Dr, 0.0f);
						SaveVertex[savevertcnt++] = new Vector3(Wr, -Dr, 0.0f);
						SaveVertex[savevertcnt++] = new Vector3(Wr, Dr, 0.0f);
					}
					else
					{
						SaveVertex[savevertcnt++] = new Vector3(Wr, Dr, 0.0f);
						SaveVertex[savevertcnt++] = new Vector3(-Wr, Dr, 0.0f);
						SaveVertex[savevertcnt++] = new Vector3(-Wr, -Dr, 0.0f);
						SaveVertex[savevertcnt++] = new Vector3(Wr, -Dr, 0.0f);
						SaveVertex[savevertcnt++] = new Vector3(Wr, Dr, 0.0f);
					}
				}
			}
			else
			{
				int savevertcnt = 0;
				float sang = Mathf.PI / (float)nsides;

				float Wr = dsecwidth * 0.5f;
				float Dr = dsecdepth * 0.5f;
				float Zfr = dsecfillet;
				if ( Zfr < 0.0f )
					Zfr = 0.0f;

				float LeftCenter = Dr-Wr;

				if ( nfillets > 0 )
				{
					float DmZ = Dr-Zfr,
					  ZmD = -DmZ,
					  WmZ = Wr-Zfr;
					int oneqtrverts = 1 + nfillets;
					int threeqtrverts = oneqtrverts + 1 + nsides;

					SaveVertex[0]							= new Vector3(Wr , DmZ, 0.0f);
					SaveVertex[nfillets]					= new Vector3(WmZ, Dr , 0.0f);
					SaveVertex[oneqtrverts]					= new Vector3(LeftCenter, Dr, 0.0f);
					SaveVertex[oneqtrverts + nsides]		= new Vector3(LeftCenter, -Dr, 0.0f);
					SaveVertex[threeqtrverts]				= new Vector3(WmZ, -Dr , 0.0f);
					SaveVertex[threeqtrverts + nfillets]	= new Vector3(Wr, ZmD, 0.0f);

					if ( nfillets > 1 )
					{
						float ang = PIover2 / (float)nfillets;
						savevertcnt = 1;
						for ( int i = 0; i < nfillets - 1; i++ )
						{
							float u = (float)(i + 1) * ang;
							float cu = Zfr * Mathf.Cos(u);
							float su = Zfr * Mathf.Sin(u);
							SaveVertex[savevertcnt]					= new Vector3(WmZ + cu, DmZ + su, 0.0f);
							SaveVertex[savevertcnt+threeqtrverts]	= new Vector3(WmZ + su, ZmD - cu, 0.0f);
							savevertcnt++;
						}
					}
					savevertcnt = 1 + oneqtrverts;
					for ( int i = 0; i < nsides - 1; i++ )
					{
						float u = (float)(i + 1) * sang;
						float cu = Dr * Mathf.Cos(u);
						float su = Dr * Mathf.Sin(u);
						SaveVertex[savevertcnt] = new Vector3(LeftCenter - su, cu, 0.0f);
						savevertcnt++;
					}
				}
				else
				{
					SaveVertex[savevertcnt] = new Vector3(Wr, Dr, 0.0f);
					savevertcnt++;
					SaveVertex[savevertcnt] = new Vector3(LeftCenter, Dr, 0.0f);
					savevertcnt++;
					for ( int i = 0; i < nsides - 1; i++ )
					{
						float u = (float)(i + 1) * sang;
						float cu = Dr * Mathf.Cos(u);
						float su = Dr * Mathf.Sin(u);
						SaveVertex[savevertcnt] = new Vector3(LeftCenter - su, cu, 0.0f);
						savevertcnt++;
					}
					SaveVertex[savevertcnt] = new Vector3(LeftCenter, -Dr, 0.0f);
					savevertcnt++;
					SaveVertex[savevertcnt] = new Vector3(Wr, -Dr, 0.0f);
					savevertcnt++;
				}

				SaveVertex[SaveVertex.Length - 1] = SaveVertex[0];
			}
		}

		// UVs
		float dist = 0.0f;
		Vector3 last = Vector3.zero;

		for ( int i = 0; i < NvertsPerRing; i++ )
		{
			if ( i > 0 )
				dist += (SaveVertex[i] - last).magnitude;

			SaveUV[i] = new Vector2(0.0f, dist * uvscale.y);

			last = SaveVertex[i];
		}

		for ( int i = 0; i < NvertsPerRing; i++ )
			SaveUV[i].y /= dist;

		float rotangle = 0.0f;
		switch ( wtype )
		{
			case MegaHoseType.Round:		rotangle = rndrot;			break;
			case MegaHoseType.Rectangle:	rotangle = rectrotangle;	break;
			case MegaHoseType.DSection:		rotangle = dsecrotangle;	break;
		}

		if ( rotangle != 0.0f )
		{
			rotangle *= Mathf.Deg2Rad;
			float cosu = Mathf.Cos(rotangle);
			float sinu = Mathf.Sin(rotangle);
			for ( int m = 0; m < NvertsPerRing; m++ )
			{
				float tempx = SaveVertex[m].x * cosu - SaveVertex[m].y * sinu;
				float tempy = SaveVertex[m].x * sinu + SaveVertex[m].y * cosu;
				SaveVertex[m].x = tempx;
				SaveVertex[m].y = tempy;
			}
		}

		// Normals
		if ( calcnormals )
		{
			for ( int i = 0; i < NvertsPerRing; i++ )
			{
				int ii = (i + 1) % NvertsPerRing;
				Vector3 delta = (SaveVertex[ii] - SaveVertex[i]).normalized;

				//SaveNormals[i] = new Vector3(-delta.y, delta.x, 0.0f);
				//SaveNormals[i] = new Vector3(-delta.x, delta.y, 0.0f);
				//SaveNormals[i] = new Vector3(delta.y, delta.x, 0.0f);
				SaveNormals[i] = new Vector3(delta.y, -delta.x, 0.0f);
			}
		}
	}

	void FixHoseFillet()
	{
		float width = rectwidth;
		float height = rectdepth;
		float fillet = rectfillet;
		float hh = 0.5f * Mathf.Abs(height);
		float ww = 0.5f * Mathf.Abs(width);
		float maxf = (hh > ww ? ww : hh);
		if ( fillet > maxf )
			rectfillet = maxf;
	}

	float RND11()
	{
		float num = Random.Range(0.0f, 32768.0f) - HalfIntMax;
		return (num / HalfIntMax);
	}

	void Mult1X3(Vector3 A, Matrix4x4 B, ref Vector3 C)
	{
		C[0] = A[0] * B[0, 0] + A[1] * B[0, 1] + A[2] * B[0, 2];
		C[1] = A[0] * B[1, 0] + A[1] * B[1, 1] + A[2] * B[1, 2];
		C[2] = A[0] * B[2, 0] + A[1] * B[2, 1] + A[2] * B[2, 2];
	}


	void Mult1X4(Vector4 A, Matrix4x4 B, ref Vector4 C)
	{
		C[0] = A[0] * B[0, 0] + A[1] * B[0, 1] + A[2] * B[0, 2] + A[3] * B[0, 3];
		C[1] = A[0] * B[1, 0] + A[1] * B[1, 1] + A[2] * B[1, 2] + A[3] * B[1, 3];
		C[2] = A[0] * B[2, 0] + A[1] * B[2, 1] + A[2] * B[2, 2] + A[3] * B[2, 3];
		C[3] = A[0] * B[3, 0] + A[1] * B[3, 1] + A[2] * B[3, 2] + A[3] * B[3, 3];
	}

	void SetUpRotation(Vector3 Q, Vector3 W, float Theta, ref Matrix4x4 Rq)
	{
		float ww1,ww2,ww3,w12,w13,w23,CosTheta,SinTheta,MinCosTheta;
		Vector3 temp = Vector3.zero;
		Matrix4x4 R = Matrix4x4.identity;

		ww1 = W[0] * W[0];
		ww2 = W[1] * W[1];
		ww3 = W[2] * W[2];
		w12 = W[0] * W[1];
		w13 = W[0] * W[2];
		w23 = W[1] * W[2];
		CosTheta = Mathf.Cos(Theta);
		MinCosTheta = 1.0f - CosTheta;
		SinTheta = Mathf.Sin(Theta);
		R[0, 0] = ww1 + (1.0f - ww1) * CosTheta;
		R[1, 0] = w12 * MinCosTheta + W[2] * SinTheta;
		R[2, 0] = w13 * MinCosTheta - W[1] * SinTheta;
		R[0, 1] = w12 * MinCosTheta - W[2] * SinTheta;
		R[1, 1] = ww2 + (1.0f - ww2) * CosTheta;
		R[2, 1] = w23 * MinCosTheta + W[0] * SinTheta;
		R[0, 2] = w13 * MinCosTheta + W[1] * SinTheta;
		R[1, 2] = w23 * MinCosTheta - W[0] * SinTheta;
		R[2, 2] = ww3 + (1.0f - ww3) * CosTheta;
		Mult1X3(Q, R, ref temp);

		Rq.SetColumn(0, R.GetColumn(0));
		Rq.SetColumn(1, R.GetColumn(1));
		Rq.SetColumn(2, R.GetColumn(2));
		Rq[0, 3] = Q[0] - temp.x;
		Rq[1, 3] = Q[1] - temp.y;
		Rq[2, 3] = Q[2] - temp.z;
		Rq[3, 0] = Rq[3, 1] = Rq[3, 2] = 0.0f;
		Rq[3, 3] = 1.0f;
	}

	void RotateOnePoint(ref Vector3 Pin, Vector3 Q, Vector3 W, float Theta)
	{
		Matrix4x4 Rq = Matrix4x4.identity;
		Vector4 Pout = Vector3.zero;
		Vector4 Pby4;

		SetUpRotation(Q, W, Theta, ref Rq);
		Pby4 = Pin;
		Pby4[3] = 1.0f;

		Mult1X4(Pby4, Rq, ref Pout);
		Pin = Pout;
	}

	Vector3 endp1 = Vector3.zero;
	Vector3 endp2 = Vector3.zero;
	Vector3 endr1 = Vector3.zero;
	Vector3 endr2 = Vector3.zero;
	public Vector3[] SaveVertex;
	public Vector2[]	SaveUV;
	public Vector3[]	SaveNormals;

	public bool rebuildcross = true;

	public int NvertsPerRing = 0;
	public int Nverts = 0;

	void Update()
	{
		if ( animatebulge )
		{
			bulgeoffset += bulgespeed * Time.deltaTime;
			if ( bulgeoffset > maxbulge )
				bulgeoffset -= maxbulge - minbulge;

			if ( bulgeoffset < minbulge )
				bulgeoffset += maxbulge - minbulge;

			updatemesh = true;
		}

		if ( custnode )
		{
			if ( custnode.transform.position != endp1 )
			{
				endp1 = custnode.transform.position;
				updatemesh = true;
			}

			if ( custnode.transform.eulerAngles != endr1 )
			{
				endr1 = custnode.transform.eulerAngles;
				updatemesh = true;
			}
		}

		if ( custnode2 )
		{
			if ( custnode2.transform.position != endp2 )
			{
				endp1 = custnode2.transform.position;
				updatemesh = true;
			}

			if ( custnode2.transform.eulerAngles != endr2 )
			{
				endr2 = custnode2.transform.eulerAngles;
				updatemesh = true;
			}
		}

		if ( !dolateupdate )
		{
			if ( visible || InvisibleUpdate )
			{
				// Check transforms so we dont update unless we have to
				if ( updatemesh )	//|| Application.isEditor )
				{
					updatemesh = false;
					BuildMesh();
				}
			}
		}
	}

	void LateUpdate()
	{
		if ( dolateupdate )
		{
			if ( visible || InvisibleUpdate )
			{
				// Check transforms so we dont update unless we have to
				if ( updatemesh )	//|| Application.isEditor )
				{
					updatemesh = false;
					BuildMesh();
				}
			}
		}
	}

	public Vector3 up = Vector3.up;

	Vector3	starty = Vector3.zero;
	Vector3	roty = Vector3.zero;
	float yangle = 0.0f;
	public Matrix4x4 Tlocal = Matrix4x4.identity;

	// we only need to build the savevertex and uvs if mesh def changes, else we can keep the uvs
	void BuildMesh()
	{
		if ( !mesh )
		{
			mesh = GetComponent<MeshFilter>().sharedMesh;
			if ( mesh == null )
			{
				updatemesh = true;
				return;
			}
		}

		if ( hosespline.knots.Count == 0 )
		{
			hosespline.AddKnot(Vector3.zero, Vector3.zero, Vector3.zero);
			hosespline.AddKnot(Vector3.zero, Vector3.zero, Vector3.zero);
		}

		FixHoseFillet();

		bool createfree = freecreate;

		if ( (!createfree) && ((!custnode) || (!custnode2)) )
			createfree = true;

		if ( custnode && custnode2 )
			createfree = false;

		Matrix4x4 mat1,mat2;
		float Lf = 0.0f;
		Tlocal = Matrix4x4.identity;

		Vector3 startvec, endvec, startpoint, endpoint, endy;
		starty = Vector3.zero;
		roty = Vector3.zero;
		yangle = 0.0f;

		Vector3 RV = Vector3.zero;

		if ( createfree )
			Lf = noreflength;
		else
		{	
			RV = up;	//new Vector3(xtmp, ytmp, ztmp);

			mat1 = custnode.transform.localToWorldMatrix;
			mat2 = custnode2.transform.localToWorldMatrix;

			Matrix4x4 mato1 = Matrix4x4.identity;
			Matrix4x4 mato2 = Matrix4x4.identity;
			mato1 = Matrix4x4.TRS(offset, Quaternion.Euler(rotate), scale);
			mato1 = mato1.inverse;

			mato2 = Matrix4x4.TRS(offset1, Quaternion.Euler(rotate1), scale1);
			mato2 = mato2.inverse;
			S = transform.localToWorldMatrix;

			Matrix4x4 mat1NT, mat2NT;

			mat1NT = mat1;
			mat2NT = mat2;
			MegaMatrix.NoTrans(ref mat1NT);
			MegaMatrix.NoTrans(ref mat2NT);
			Vector3 P1 = mat1.MultiplyPoint(mato1.GetColumn(3));
			Vector3 P2 = mat2.MultiplyPoint(mato2.GetColumn(3));

			startvec = mat1NT.MultiplyPoint(mato1.GetColumn(2));
			endvec = mat2NT.MultiplyPoint(mato2.GetColumn(2));
			starty = mat1NT.MultiplyPoint(mato1.GetColumn(1));
			endy = mat2NT.MultiplyPoint(mato2.GetColumn(1));

			Matrix4x4 SI = S.inverse;

			Vector3 P0 = SI.MultiplyPoint(P1);
			Matrix4x4 T1 = mat1;
			MegaMatrix.NoTrans(ref T1);

			Vector3 RVw = T1.MultiplyPoint(RV);
			Lf = (P2 - P1).magnitude;
			Vector3 Zw;
			if ( Lf < 0.01f )
				Zw = P1.normalized;
			else
				Zw = (P2 - P1).normalized;
			Vector3 Xw = Vector3.Cross(RVw, Zw).normalized;
			Vector3 Yw = Vector3.Cross(Zw, Xw).normalized;

			MegaMatrix.NoTrans(ref SI);

			Vector3 Xs = SI.MultiplyPoint(Xw);
			Vector3 Ys = SI.MultiplyPoint(Yw);
			Vector3 Zs = SI.MultiplyPoint(Zw);
			Tlocal.SetColumn(0, Xs);
			Tlocal.SetColumn(1, Ys);
			Tlocal.SetColumn(2, Zs);
			MegaMatrix.SetTrans(ref Tlocal, P0);

			// move z-axes of end transforms into local frame
			Matrix4x4 TlocalInvNT = Tlocal;
			MegaMatrix.NoTrans(ref TlocalInvNT);

			TlocalInvNT = TlocalInvNT.inverse;

			float tenstop = tension1;	// * 0.01f;
			float tensbot = tension2;	// * 0.01f;

			startvec = tensbot * (TlocalInvNT.MultiplyPoint(startvec));
			endvec = tenstop * (TlocalInvNT.MultiplyPoint(endvec));

			starty = TlocalInvNT.MultiplyPoint(starty);
			endy = TlocalInvNT.MultiplyPoint(endy);

			yangle = Mathf.Acos(Vector3.Dot(starty, endy));

			if ( yangle > EPSILON )
				roty = Vector3.Cross(starty, endy).normalized;
			else
				roty = Vector3.zero;

			startpoint = Vector3.zero;
			endpoint = new Vector3(0.0f, 0.0f, Lf);

			hosespline.knots[0].p = startpoint;
			hosespline.knots[0].invec = startpoint - startvec;
			hosespline.knots[0].outvec = startpoint + startvec;

			hosespline.knots[1].p = endpoint;
			hosespline.knots[1].invec = endpoint + endvec;
			hosespline.knots[1].outvec = endpoint - endvec;

			hosespline.CalcLength();	//10);
		}

		MegaHoseType wtype = wiretype;

		int Segs = segments;
		if ( Segs < 3 )
			Segs = 3;

		if ( rebuildcross )
		{
			rebuildcross = false;
			int	nfillets = 0;
			int nsides = 0;

			if ( wtype == MegaHoseType.Round )
			{
				NvertsPerRing = rndsides;
				if ( NvertsPerRing < 3 )
					NvertsPerRing = 3;
			}
			else
			{
				if ( wtype == MegaHoseType.Rectangle )
				{
					nfillets = rectfilletsides;
					if ( nfillets < 0 )
						nfillets = 0;

					if ( smooth == MegaHoseSmooth.SMOOTHNONE )
						NvertsPerRing = (nfillets > 0 ? 8 + 4 * (nfillets - 1) : 8);
					else
						NvertsPerRing = (nfillets > 0 ? 8 + 4 * (nfillets - 1) : 4);	//4);
				}
				else
				{
					nfillets = dsecfilletsides;
					if ( nfillets < 0 )
						nfillets = 0;
					nsides = dsecrndsides;
					if ( nsides < 2 )
						nsides = 2;
					int nsm1 = nsides - 1;
					NvertsPerRing = (nfillets > 0 ? 6 + nsm1 + 2 * (nfillets - 1): 4 + nsm1);
				}
			}

			NvertsPerRing++;

			int NfacesPerEnd,NfacesPerRing,Nfaces = 0;
			//MegaHoseSmooth SMOOTH = smooth;

			Nverts = (Segs + 1) * (NvertsPerRing + 1);	// + 2;
			if ( capends )
				Nverts += 2;

			NfacesPerEnd = NvertsPerRing;
			NfacesPerRing = 6 * NvertsPerRing;
			Nfaces =  Segs * NfacesPerRing;	// + 2 * NfacesPerEnd;

			if ( capends )
			{
				Nfaces += 2 * NfacesPerEnd;
			}

			if ( SaveVertex == null || SaveVertex.Length != NvertsPerRing )
			{
				SaveVertex = new Vector3[NvertsPerRing];
				SaveUV = new Vector2[NvertsPerRing];
			}

			if ( calcnormals )
			{
				if ( SaveNormals == null || SaveNormals.Length != NvertsPerRing )
					SaveNormals = new Vector3[NvertsPerRing];
			}

			MakeSaveVertex(NvertsPerRing, nfillets, nsides, wtype);

			if ( verts == null || verts.Length != Nverts )
			{
				verts = new Vector3[Nverts];
				uvs = new Vector2[Nverts];
				faces = new int[Nfaces * 3];
			}

			if ( calcnormals && (normals == null || normals.Length != Nverts) )
				normals = new Vector3[Nverts];
		}

		if ( Nverts == 0 )
			return;

		bool mapmenow = mapmemapme;

		int thisvert = 0;
		int	last = Nverts - 1;
		int last2 = last - 1;
		int lastvpr = NvertsPerRing;	// - 1;
		int maxseg = Segs + 1;

		float flexhere;
		float dadjust;
		float flexlen;
		float flex1 = flexstart;
		float flex2 = flexstop;
		int flexn = flexcycles;
		float flexd = flexdiameter;

		Vector3 ThisPosition;
		Vector3 ThisXAxis, ThisYAxis, ThisZAxis;
		Vector2 uv = Vector2.zero;

		Matrix4x4 RingTM = Matrix4x4.identity;
		Matrix4x4 invRingTM = Matrix4x4.identity;

		for ( int i = 0; i < maxseg; i++ )
		{
			float incr = (float)i / (float)Segs;

			if ( createfree )
			{
				ThisPosition = new Vector3(0.0f, 0.0f, Lf * incr);
				ThisXAxis = new Vector3(1.0f, 0.0f, 0.0f);
				ThisYAxis = new Vector3(0.0f, 1.0f, 0.0f);
				ThisZAxis = new Vector3(0.0f, 0.0f, 1.0f);
			}
			else
			{
				int k = 0;
				ThisPosition = hosespline.InterpCurve3D(incr, true, ref k);
				ThisZAxis = (hosespline.InterpCurve3D(incr + 0.001f, true, ref k) - ThisPosition).normalized;

				ThisYAxis = starty;
				if ( yangle > EPSILON )
					RotateOnePoint(ref ThisYAxis, Vector3.zero, roty, incr * yangle);

				ThisXAxis = Vector3.Cross(ThisYAxis, ThisZAxis).normalized;
			
				ThisYAxis = Vector3.Cross(ThisZAxis, ThisXAxis);
			}

			RingTM.SetColumn(0, ThisXAxis);
			RingTM.SetColumn(1, ThisYAxis);
			RingTM.SetColumn(2, ThisZAxis);
			MegaMatrix.SetTrans(ref RingTM, ThisPosition);

			if ( !createfree ) 
			{
				RingTM = Tlocal * RingTM;
			}

			if ( calcnormals )
			{
				invRingTM = RingTM;
				MegaMatrix.NoTrans(ref invRingTM);
				//invRingTM = invRingTM.inverse.transpose;
			}

			if ( (incr > flex1) && (incr < flex2) && flexon )
			{
				flexlen = flex2 - flex1;
				if ( flexlen < 0.01f )
					flexlen = 0.01f;
				flexhere = (incr - flex1) / flexlen;

				float ang = (float)flexn * flexhere * (Mathf.PI * 2.0f) + PIover2;
				dadjust = 1.0f + flexd * (1.0f - Mathf.Sin(ang));	//(float)flexn * flexhere * (Mathf.PI * 2.0f) + PIover2));
			}
			else
				dadjust = 0.0f;

			if ( usebulgecurve )
			{
				if ( dadjust == 0.0f )
					dadjust = 1.0f + (bulge.Evaluate(incr + bulgeoffset) * bulgeamount);
				else
					dadjust += bulge.Evaluate(incr + bulgeoffset) * bulgeamount;
			}

			uv.x = 0.999999f * incr * uvscale.x;

			for ( int j = 0; j < NvertsPerRing; j++ )
			{
				int jj = j;	// % NvertsPerRing;

				if ( mapmenow )
				{
					uv.y = SaveUV[jj].y;
					uvs[thisvert] = uv;	//new Vector2(0.999999f * incr * uvscale.x, SaveUV[jj].y);
				}

				if ( dadjust != 0.0f )
				{
					verts[thisvert] = RingTM.MultiplyPoint(dadjust * SaveVertex[jj]);
				}
				else
				{
					verts[thisvert] = RingTM.MultiplyPoint(SaveVertex[jj]);
				}

				if ( calcnormals )
					normals[thisvert] = invRingTM.MultiplyPoint(SaveNormals[jj]).normalized;	//.MultiplyPoint(-SaveNormals[jj]);
				//if ( j == 0 )
				//{
				//	Debug.Log("norm " + normals[thisvert].ToString("0.000") + " save " + SaveNormals[jj].ToString("0.000"));
				//}

				thisvert++;
			}

			if ( mapmenow )
			{
				//uvs[Nverts + i] = new Vector2(0.999999f * incr, 0.999f);
			}

			if ( capends )
			{
				if ( i == 0 )
				{
					verts[last2] = (createfree ? ThisPosition : Tlocal.MultiplyPoint(ThisPosition));
					if ( mapmenow )
						uvs[last2] = Vector3.zero;
				}
				else
				{
					if ( i == Segs )
					{
						verts[last] = createfree ? ThisPosition : Tlocal.MultiplyPoint(ThisPosition);
						if ( mapmenow )
						{
							uvs[last] = Vector3.zero;
						}
					}
				}
			}
		}

		//	Now, set up the faces
		int thisface = 0, v1, v2, v3, v4;
		v3 = last2;
		if ( capends )
		{
			for ( int i = 0; i < NvertsPerRing - 1; i++ )
			{
				v1 = i;
				v2 = (i < lastvpr ? v1 + 1 : v1 - lastvpr);
				//v5 = (i < lastvpr ? v2 : Nverts);

				faces[thisface++] = v2;
				faces[thisface++] = v1;
				faces[thisface++] = v3;
			}
		}

		int ringnum = NvertsPerRing;	// + 1;

		for ( int i = 0; i < Segs; i++ )
		{
			for ( int j = 0; j < NvertsPerRing - 1; j++ )
			{
				v1 = i * ringnum + j;
				v2 = v1 + 1;	//(j < lastvpr? v1 + 1 : v1 - lastvpr);
				v4 = v1 + ringnum;
				v3 = v2 + ringnum;
				faces[thisface++] = v1;
				faces[thisface++] = v2;
				faces[thisface++] = v3;
				faces[thisface++] = v1;
				faces[thisface++] = v3;
				faces[thisface++] = v4;
			}
		}

		int basevert = Segs * ringnum;	//NvertsPerRing;

		v3 = Nverts - 1;
		if ( capends )
		{
			for ( int i = 0; i < NvertsPerRing - 1; i++ )
			{
				v1 = i + basevert;
				v2 = (i < lastvpr? v1 + 1 : v1 - lastvpr);
				//v5 = (i < lastvpr? v2 : Nverts + Segs);
				faces[thisface++] = v1;
				faces[thisface++] = v2;
				faces[thisface++] = v3;
			}
		}

		mesh.Clear();

		mesh.subMeshCount = 1;

		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.triangles = faces;

		if ( calcnormals )
			mesh.normals = normals;
		else
			mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		if ( optimize )
		{
			mesh.Optimize();
		}

		if ( calctangents )
		{
			MegaUtils.BuildTangents(mesh);
		}

		if ( recalcCollider )
		{
			if ( meshCol == null )
				meshCol = GetComponent<MeshCollider>();

			if ( meshCol != null )
			{
				meshCol.sharedMesh = null;
				meshCol.sharedMesh = mesh;
				//bool con = meshCol.convex;
				//meshCol.convex = con;
			}
		}
	}

	public void CalcMatrix(ref Matrix4x4 mat, float incr)
	{
#if false
		Matrix4x4 RingTM = Matrix4x4.identity;
		Matrix4x4 invRingTM = Matrix4x4.identity;

		int k = 0;
		Vector3 ThisPosition = hosespline.InterpCurve3D(incr, true, ref k);
		Vector3 ThisZAxis = (hosespline.InterpCurve3D(incr + 0.001f, true, ref k) - ThisPosition).normalized;

		Vector3 ThisYAxis = starty;
		if ( yangle > EPSILON )
			RotateOnePoint(ref ThisYAxis, Vector3.zero, roty, incr * yangle);

		Vector3 ThisXAxis = Vector3.Cross(ThisYAxis, ThisZAxis).normalized;
		
		ThisYAxis = Vector3.Cross(ThisZAxis, ThisXAxis);

		RingTM.SetColumn(0, ThisXAxis);
		RingTM.SetColumn(1, ThisYAxis);
		RingTM.SetColumn(2, ThisZAxis);
		MegaMatrix.SetTrans(ref RingTM, ThisPosition);
#endif
		mat = Tlocal;	// * RingTM;
	}


	MeshCollider meshCol;

	static float findmappos(float curpos)
	{
		float mappos;

		return (mappos = ((mappos = curpos) < 0.0f ? 0.0f : (mappos > 1.0f ? 1.0f : mappos)));
	}

	void DisplayNormals()
	{

	}

	public Vector3 GetPosition(float alpha)
	{
		Matrix4x4 RingTM = transform.localToWorldMatrix * Tlocal;

		int k = 0;

		return RingTM.MultiplyPoint(hosespline.InterpCurve3D(alpha, true, ref k));
	}
}
