
// All code copyright Chris West 2011.
// If you make any improvements please send them back to me at chris@west-racing.com so I can update the package.
#if !UNITY_FLASH && !UNITY_METRO && !UNITY_WP8
using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;

public enum IMGFormat
{
	Tga,
	Jpg,
}

//[ExecuteInEditMode]
public class MegaGrab : MonoBehaviour
{
	public Camera				SrcCamera			= null;				// camera to use for screenshot
	public KeyCode				GrabKey				=	KeyCode.S;		// Key to grab the screenshot
	public int					ResUpscale			= 1;				// How much to increase the screen shot res by
	public float				Blur				= 1.0f;				// Pixel oversampling
	public int					AASamples			= 8;				// Anti aliasing samples
	public AnisotropicFiltering	FilterMode			= AnisotropicFiltering.ForceEnable;	// Filter mode
	public bool					UseJitter			= false;			// use random jitter for AA sampling
	public string				SaveName			= "Grab";			// Base name for grabs
	public string				Format				= "dddd MMM dd yyyy HH_mm_ss";	// format string for date time info
	public string				Enviro				= "";				// Enviro variable to use ie USERPROFILE
	public string				Path				= "";
	public bool					UseDOF				= false;			// Use Dof grab
	public float				focalDistance		= 8.0f;				// DOF focal distance
	public int					totalSegments		= 8;				// How many DOF samples
	public float				sampleRadius		= 1.0f;				// Amount of DOF effect
	//public float sampleBias = 1.0f;
	public bool					CalcFromSize		= false;			// Let grab calc res from dpi and Width(in inches)
	public int					Dpi					= 300;				// Number of Dots per inch required
	public float				Width				= 24.0f;			// Final physical size of grab using Dpi
	public int					NumberOfGrabs		= 0;				// Read only of how many grabs will happen
	public float				EstimatedTime		= 0.0f;				// Guide to show how long a grab will take in Seconds
	public int					GrabWidthWillBe		= 0;				// Width of final image
	public int					GrabHeightWillBe	= 0;				// Height of final IMage
	public bool					UseCoroutine		= false;			// Use coroutine method, needed for later versions of unity
	float						mleft;
	float						mright;
	float						mtop;
	float						mbottom;
	int							sampcount;
	Vector2[]					poisson;
	Texture2D					grabtex;
	Color[,]					accbuf;
	Color[,]					blendbuf;
	byte[]						output1;
	Color[]						outputjpg;
	AnisotropicFiltering		filtering;
	MGBlendTable				blendtable;
	int							DOFSamples;
	//float						DOFJitter = 5.0f;
	Vector3						camfor;
	Vector3						campos;
	Matrix4x4					camtm;

	// Calc the camera offsets and rots in init

	void CalcDOFInfo(Camera camera)
	{
		camtm = camera.transform.localToWorldMatrix;
		campos = camera.transform.position;
		camfor = camera.transform.forward;
	}

	void ChangeDOFPos(int segment)
	{
		float theta = (float)segment / (float)(totalSegments) * Mathf.PI * 2.0f;
		float radius = sampleRadius;

		float uOffset = radius * Mathf.Cos(theta);
		float vOffset = radius * Mathf.Sin(theta);

		Vector3 newCameraLocation = new Vector3(uOffset, vOffset, 0.0f);
		Vector3 initialTargetLocation = camfor * focalDistance;	//new Vector3(0.0f, 0.0f, -focalDistance);

		Vector3 tpos = initialTargetLocation + campos;	//srcCamera.transform.TransformPoint(initialTargetLocation);
		SrcCamera.transform.position = camtm.MultiplyPoint3x4(newCameraLocation);
		SrcCamera.transform.LookAt(tpos);
	}

	static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
	{
		Matrix4x4 m = Matrix4x4.identity;

		m[0, 0] = (2.0f * near) / (right - left);
		m[1, 1] = (2.0f * near) / (top - bottom);
		m[0, 2] = (right + left) / (right - left);
		m[1, 2] = (top + bottom) / (top - bottom);
		m[2, 2] = -(far + near) / (far - near);
		m[2, 3] = -(2.0f * far * near) / (far - near);
		m[3, 2] = -1.0f;
		m[3, 3] = 0.0f;
		return m;
	}

	Matrix4x4 CalcProjectionMatrix(float left, float right, float bottom, float top, float near, float far, float xoff, float yoff)
	{
		float scalex = (right - left) / (float)Screen.width;
		float scaley = (top - bottom) / (float)Screen.height;

		return PerspectiveOffCenter(left - xoff * scalex, right - xoff * scalex, bottom - yoff * scaley, top - yoff * scaley, near, far);
	}

	void Cleanup()
	{
		QualitySettings.anisotropicFiltering = filtering;
	}

	bool InitGrab(int width, int height, int aasamples)
	{
		blendtable = new MGBlendTable(32, 32, totalSegments, 0.4f, true);

		if ( ResUpscale < 1 )
			ResUpscale = 1;

		if ( AASamples < 1 )
			AASamples = 1;

		if ( SrcCamera == null )
			SrcCamera = Camera.main;

		if ( SrcCamera == null )
		{
			Debug.Log("No Camera set as source and no main camera found in the scene");
			return false;
		}
		CalcDOFInfo(SrcCamera);

		if ( OutputFormat == IMGFormat.Tga )
			output1 = new byte[(width * ResUpscale) * (height * ResUpscale) * 3];
		else
			outputjpg = new Color[(width * ResUpscale) * (height * ResUpscale)];

		if ( output1 != null || outputjpg != null )
		{
			filtering = QualitySettings.anisotropicFiltering;
			QualitySettings.anisotropicFiltering = FilterMode;

			grabtex = new Texture2D(width, height, TextureFormat.RGB24, false);

			if ( grabtex != null )
			{
				accbuf = new Color[width, height];
				blendbuf = new Color[width, height];

				if ( accbuf != null )
				{
					float l = (1.0f - Blur) * 0.5f;
					float h = 1.0f + ((Blur - 1.0f) * 0.5f);

					if ( UseJitter)
					{
						poisson = new Vector2[aasamples];

						sampcount = aasamples;
						for ( int i = 0; i < aasamples; i++ )
						{
							Vector2 pos = new Vector2();
							pos.x = Mathf.Lerp(l, h, UnityEngine.Random.value);
							pos.y = Mathf.Lerp(l, h, UnityEngine.Random.value);
							poisson[i] = pos;
						}
					}
					else
					{
						int samples = (int)Mathf.Sqrt((float)aasamples);
						if ( samples < 1 )
							samples = 1;

						sampcount = samples * samples;

						poisson = new Vector2[samples * samples];

						int i = 0;

						for ( int ya = 0; ya < samples; ya++ )
						{
							for ( int xa = 0; xa < samples; xa++ )
							{
								float xa1 = ((float)xa / (float)samples);
								float ya1 = ((float)ya / (float)samples);

								Vector2 pos = new Vector2();
								pos.x = Mathf.Lerp(l, h, xa1);
								pos.y = Mathf.Lerp(l, h, ya1);
								poisson[i++] = pos;
							}
						}
					}

					return true;
				}
			}
		}

		Debug.Log("Cant create a large enough texture, Try lower ResUpscale value");
		return false;
	}

	Texture2D GrabImage(int samples, float x, float y)
	{
		float ps = 1.0f / (float)ResUpscale;

		for ( int i = 0; i < sampcount; i++ )
		{
			float xa = poisson[i].x * ps;
			float ya = poisson[i].y * ps;

			// Move view and grab
			float xo = x + xa;
			float yo = y + ya;

			SrcCamera.projectionMatrix = CalcProjectionMatrix(mleft, mright, mbottom, mtop, SrcCamera.nearClipPlane, SrcCamera.farClipPlane, xo, yo);

			SrcCamera.Render();

			// Read screen contents into the texture
			grabtex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			grabtex.Apply();

			if ( i == 0 )
			{
				for ( int ty = 0; ty < Screen.height; ty++ )
				{
					for ( int tx = 0; tx < Screen.width; tx++ )
						accbuf[tx, ty] = grabtex.GetPixel(tx, ty);
				}
			}
			else
			{
				for ( int ty = 0; ty < Screen.height; ty++ )
				{
					for ( int tx = 0; tx < Screen.width; tx++ )
						accbuf[tx, ty] += grabtex.GetPixel(tx, ty);
				}
			}
		}

		for ( int ty = 0; ty < Screen.height; ty++ )
		{
			for ( int tx = 0; tx < Screen.width; tx++ )
				grabtex.SetPixel(tx, ty, accbuf[tx, ty] / sampcount);
		}

		grabtex.Apply();

		return grabtex;
	}

	void GrabAA(float x, float y)
	{
		float ps = 1.0f / (float)ResUpscale;

		for ( int ty = 0; ty < Screen.height; ty++ )
		{
			for ( int tx = 0; tx < Screen.width; tx++ )
				accbuf[tx, ty] = Color.black;
		}

		for ( int i = 0; i < sampcount; i++ )
		{
			float xa = poisson[i].x * ps;
			float ya = poisson[i].y * ps;

			// Move view and grab
			float xo = x + xa;
			float yo = y + ya;

			SrcCamera.projectionMatrix = CalcProjectionMatrix(mleft, mright, mbottom, mtop, SrcCamera.nearClipPlane, SrcCamera.farClipPlane, xo, yo);

			SrcCamera.Render();

			// Read screen contents into the texture
			grabtex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			grabtex.Apply();

			for ( int ty = 0; ty < Screen.height; ty++ )
			{
				for ( int tx = 0; tx < Screen.width; tx++ )
					accbuf[tx, ty] += grabtex.GetPixel(tx, ty);
			}
		}

		for ( int ty = 0; ty < Screen.height; ty++ )
		{
			for ( int tx = 0; tx < Screen.width; tx++ )
				accbuf[tx, ty] = accbuf[tx, ty] / sampcount;
		}
	}

	// return accbuf here 
	Texture2D GrabImageDOF(int samples, float x, float y)
	{
		//float ps = 1.0f / (float)ResUpscale;

		for ( int ty = 0; ty < Screen.height; ty++ )
		{
			for ( int tx = 0; tx < Screen.width; tx++ )
				blendbuf[tx, ty] = Color.black;
		}

		for ( int d = 0; d < totalSegments; d++ )
		{
			ChangeDOFPos(d);
			//SrcCamera.transform.localToWorldMatrix = ChangeCameraDOF(d);

			GrabAA(x, y);

			// Blend image
			blendtable.BlendImages(blendbuf, accbuf, Screen.width, Screen.height, d);
		}

		return grabtex;
	}

	void DoGrabTGA()
	{
		if ( InitGrab(Screen.width, Screen.height, AASamples) )
		{
			mtop		= SrcCamera.nearClipPlane * Mathf.Tan(SrcCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
			mbottom	= -mtop;
			mleft		= mbottom * SrcCamera.aspect;
			mright	= mtop * SrcCamera.aspect;

			//float mWidth	= SrcCamera.nearClipPlane / SrcCamera.projectionMatrix.m00;
			//float mHeight	= SrcCamera.nearClipPlane / SrcCamera.projectionMatrix.m11;
			//float spox		= mWidth / (Screen.width * ResUpscale * 1.0f);
			//float spoy		= mHeight / (Screen.height * ResUpscale * 1.0f);

			int width = Screen.width;
			int height = Screen.height;

			if ( AASamples < 1 )
				AASamples = 1;

			//int total = ResUpscale * ResUpscale;
			int count = 0;

			for ( int y = 0; y < ResUpscale; y++ )
			{
				float yo = (float)y / (float)ResUpscale;

				for ( int x = 0; x < ResUpscale; x++ )
				{
					//MyDebug.Log("Doing Grab " + count + " of " + total);
					count++;
					float xo = (float)x / (float)ResUpscale;

					Texture2D tex;

					if ( UseDOF )
					{
						tex = GrabImageDOF(AASamples, xo, yo);
						for ( int h = 0; h < Screen.height; h++ )
						{
							int index = ((ResUpscale - y) + (h * ResUpscale) - 1) * (width * ResUpscale);

							for ( int w = 0; w < Screen.width; w++ )
							{
								Color col = blendbuf[w, h];	//tex.GetPixel(w, h);

								int ix = (index + ((ResUpscale - x) + (w * ResUpscale) - 1)) * 3;
								output1[ix + 0] = (byte)(col.b * 255.0f);
								output1[ix + 1] = (byte)(col.g * 255.0f);
								output1[ix + 2] = (byte)(col.r * 255.0f);
							}
						}
					}
					else
					{
						tex = GrabImage(AASamples, xo, yo);
						for ( int h = 0; h < Screen.height; h++ )
						{
							int index = ((ResUpscale - y) + (h * ResUpscale) - 1) * (width * ResUpscale);

							for ( int w = 0; w < Screen.width; w++ )
							{
								Color col = tex.GetPixel(w, h);

								int ix = (index + ((ResUpscale - x) + (w * ResUpscale) - 1)) * 3;
								output1[ix + 0] = (byte)(col.b * 255.0f);
								output1[ix + 1] = (byte)(col.g * 255.0f);
								output1[ix + 2] = (byte)(col.r * 255.0f);
							}
						}
					}
				}
			}

			string epath = "";
			if ( Enviro != null && Enviro.Length > 0 )
				epath = System.Environment.GetEnvironmentVariable(Enviro);

			//string fname = epath + Path + SaveName + " " + (width * ResUpscale) + "x" + (height * ResUpscale) + " " + System.DateTime.Now.ToString(Format);

			// Save big version

			//if ( uploadGrabs )
			//{
			//	string fname = SaveName + " " + (width * ResUpscale) + "x" + (height * ResUpscale) + " " + System.DateTime.Now.ToString(Format);
			//	UploadTGA(fname + ".tga", (width * ResUpscale), (height * ResUpscale), output1);
			//}
			//else
			//{
				string fname = epath + Path + SaveName + " " + (width * ResUpscale) + "x" + (height * ResUpscale) + " " + System.DateTime.Now.ToString(Format);
				SaveTGA(fname + ".tga", (width * ResUpscale), (height * ResUpscale), output1);
			//}

			//SaveTGA(fname + ".tga", (width * ResUpscale), (height * ResUpscale), output1);

			//SrcCamera.camera.worldToCameraMatrix = cameraMat;
			//SrcCamera.ResetWorldToCameraMatrix();
			SrcCamera.ResetProjectionMatrix();
			Cleanup();
		}
	}

	public IMGFormat OutputFormat = IMGFormat.Jpg;
	public float Quality = 75.0f;

	void DoGrabJPG()
	{
		if ( InitGrab(Screen.width, Screen.height, AASamples) )
		{
			mtop = SrcCamera.nearClipPlane * Mathf.Tan(SrcCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
			mbottom = -mtop;
			mleft = mbottom * SrcCamera.aspect;
			mright = mtop * SrcCamera.aspect;

			//float mWidth	= SrcCamera.nearClipPlane / SrcCamera.projectionMatrix.m00;
			//float mHeight	= SrcCamera.nearClipPlane / SrcCamera.projectionMatrix.m11;
			//float spox		= mWidth / (Screen.width * ResUpscale * 1.0f);
			//float spoy		= mHeight / (Screen.height * ResUpscale * 1.0f);

			int width = Screen.width;
			int height = Screen.height;

			if ( AASamples < 1 )
				AASamples = 1;

			//int total = ResUpscale * ResUpscale;
			int count = 0;

			for ( int y = 0; y < ResUpscale; y++ )
			{
				float yo = (float)y / (float)ResUpscale;

				for ( int x = 0; x < ResUpscale; x++ )
				{
					//MyDebug.Log("Doing Grab " + count + " of " + total);
					count++;
					float xo = (float)x / (float)ResUpscale;

					Texture2D tex;

					if ( UseDOF )
					{
						tex = GrabImageDOF(AASamples, xo, yo);
						for ( int h = 0; h < Screen.height; h++ )
						{
							int index = ((ResUpscale - y) + (h * ResUpscale) - 1) * (width * ResUpscale);

							for ( int w = 0; w < Screen.width; w++ )
							{
								Color col = blendbuf[w, h];	//tex.GetPixel(w, h);

								int ix = (index + ((ResUpscale - x) + (w * ResUpscale) - 1));
								outputjpg[ix] = col;
							}
						}
					}
					else
					{
						tex = GrabImage(AASamples, xo, yo);
						for ( int h = 0; h < Screen.height; h++ )
						{
							int index = ((ResUpscale - y) + (h * ResUpscale) - 1) * (width * ResUpscale);

							for ( int w = 0; w < Screen.width; w++ )
							{
								Color col = tex.GetPixel(w, h);

								int ix = (index + ((ResUpscale - x) + (w * ResUpscale) - 1));
								outputjpg[ix] = col;
							}
						}
					}
				}
			}

			string epath = "";
			if ( Enviro != null && Enviro.Length > 0 )
				epath = System.Environment.GetEnvironmentVariable(Enviro);

			//string fname = epath + Path + SaveName + " " + (width * ResUpscale) + "x" + (height * ResUpscale) + " " + System.DateTime.Now.ToString(Format);

			// Save big version

			if ( uploadGrabs )
			{
				string fname = SaveName + " " + (width * ResUpscale) + "x" + (height * ResUpscale) + " " + System.DateTime.Now.ToString(Format);
				UploadJPG(fname + ".jpg", (width * ResUpscale), (height * ResUpscale), outputjpg);
			}
			else
			{
				string fname = epath + Path + SaveName + " " + (width * ResUpscale) + "x" + (height * ResUpscale) + " " + System.DateTime.Now.ToString(Format);
				SaveJPG(fname + ".jpg", (width * ResUpscale), (height * ResUpscale), outputjpg);
			}
			//SrcCamera.camera.worldToCameraMatrix = cameraMat;
			//SrcCamera.ResetWorldToCameraMatrix();
			SrcCamera.ResetProjectionMatrix();
			Cleanup();
		}
	}

	public bool uploadGrabs = false;

	void SaveJPG(string filename, int width, int height, Color[] pixels)
	{
		FileStream fs = new FileStream(filename, FileMode.Create);
		if ( fs != null )
		{
			BinaryWriter bw = new BinaryWriter(fs);

			if ( bw != null )
			{
				Quality = Mathf.Clamp(Quality, 0.0f, 100.0f);
				JPGEncoder NewEncoder = new JPGEncoder(pixels, width, height, Quality);
				NewEncoder.doEncoding();
				byte[] TexData = NewEncoder.GetBytes();

				bw.Write(TexData);
				bw.Close();
			}
			fs.Close();
		}
	}

	void UploadJPG(string filename, int width, int height, Color[] pixels)
	{
		Quality = Mathf.Clamp(Quality, 0.0f, 100.0f);
		JPGEncoder NewEncoder = new JPGEncoder(pixels, width, height, Quality);
		NewEncoder.doEncoding();
		byte[] TexData = NewEncoder.GetBytes();

		UploadFile(TexData, m_URL, filename);
	}

#if false
	void UploadTGA(string filename, int width, int height, byte[] pixels)
	{
		//byte[] data = new byte[output1.Length * 4];
		//Buffer.BlockCopy(output1, 0, data, 0, data.Length);

		UploadFile(pixels, m_URL, filename);
	}
#endif

	void SaveTGA(string filename, int width, int height, byte[] pixels)
	{
		FileStream fs = new FileStream(filename, FileMode.Create);

		if ( fs != null )
		{
			BinaryWriter bw = new BinaryWriter(fs);

			if ( bw != null )
			{
				bw.Write((short)0);
				bw.Write((byte)2);
				bw.Write((int)0);
				bw.Write((int)0);
				bw.Write((byte)0);
				bw.Write((short)width);
				bw.Write((short)height);
				bw.Write((byte)24);
				bw.Write((byte)0);

				for ( int h = 0; h < pixels.Length; h++ )
					bw.Write(pixels[h]);

				bw.Close();
			}

			fs.Close();
		}
	}

	void CalcUpscale()
	{
		float w = Width / ((float)Screen.width / (float)Dpi);	// * Width;
		ResUpscale = (int)(w);
		GrabWidthWillBe = Screen.width * ResUpscale;
		GrabHeightWillBe = Screen.height * ResUpscale;
	}

	void CalcEstimate()
	{
		NumberOfGrabs = ResUpscale * ResUpscale * AASamples;

		if ( UseDOF )
		{
			NumberOfGrabs *= totalSegments;
		}

		EstimatedTime = NumberOfGrabs * 0.41f;
	}

	IEnumerator GrabCoroutine()
	{
		yield return new WaitForEndOfFrame();

		if ( OutputFormat == IMGFormat.Tga )
			DoGrabTGA();
		else
			DoGrabJPG();

		yield return null;
	}

	void LateUpdate()
	{
		if ( Input.GetKeyDown(GrabKey) )
		{
#if UNITY_IPHONE
			Path = Application.persistentDataPath + "/";
#endif
			//StartCoroutine(GrabCoroutine());

			if ( CalcFromSize )
				CalcUpscale();

			CalcEstimate();

			if ( UseCoroutine )
			{
				StartCoroutine(GrabCoroutine());
			}
			else
			{
				float t = Time.realtimeSinceStartup;
				//DoGrabTGA();
				if ( OutputFormat == IMGFormat.Tga )
					DoGrabTGA();
				else
					DoGrabJPG();

				float time = Time.realtimeSinceStartup - t;
				Debug.Log("Took " + time.ToString("0.00000000") + "s");
			}
		}
	}

	void OnDrawGizmos()
	{
		if ( CalcFromSize )
			CalcUpscale();

		CalcEstimate();
	}


	//using UnityEngine;
	//using System.Collections;
 
//public class FileUpload : MonoBehaviour
//{
	public string m_URL = "http://www.west-racing.com/uploadtest1.php";
 
	IEnumerator UploadFileCo(byte[] data, string uploadURL, string filename)
	{
		WWWForm postForm = new WWWForm();
		// version 1
		//postForm.AddBinaryData("theFile",localFile.bytes);
 
		Debug.Log("uploading " + filename);
		// version 2
		postForm.AddField("action", "Upload Image");
		postForm.AddBinaryData("theFile", data, filename, "images/jpg");	//text/plain");
 
		Debug.Log("url " + uploadURL);
		WWW upload = new WWW(uploadURL, postForm);
		yield return upload;
		//if ( upload.error == null )
			Debug.Log("upload done :" + upload.text);
		//else
			Debug.Log("Error during upload: " + upload.error);
	}
 
	void UploadFile(byte[] data, string uploadURL, string filename)
	{
		Debug.Log("Start upload");
		//StartCoroutine(UploadFileCo(data, uploadURL, filename));
		StartCoroutine(UploadLevel(data, uploadURL, filename));
		Debug.Log("len " + data.Length);
	}

	IEnumerator UploadLevel(byte[] data, string uploadURL, string filename)
	{
		WWWForm form = new WWWForm();

		form.AddField("action", "level upload");
		form.AddField("file", "file");
		form.AddBinaryData("file", data, filename, "images/jpg");
		Debug.Log("url " + uploadURL);

		WWW w = new WWW(uploadURL, form);
		yield return w;

		if ( w.error != null )
		{
			print("error");
			print(w.error);
		}
		else
		{
			if ( w.uploadProgress == 1 && w.isDone )
			{
				yield return new WaitForSeconds(5);
			}
		}
	}

#if false
	    <?
        if ( isset ($_POST['action']) ) {
            if($_POST['action'] == "Upload Image") {
                unset($imagename);
     
                if(!isset($_FILES) && isset($HTTP_POST_FILES)) $_FILES = $HTTP_POST_FILES;
     
                if(!isset($_FILES['fileUpload'])) $error["image_file"] = "An image was not found.";
     
                $imagename = basename($_FILES['fileUpload']['name']);
     
                if(empty($imagename)) $error["imagename"] = "The name of the image was not found.";
     
                if(empty($error)) {
                    $newimage = "images/" . $imagename;
                    //echo $newimage;
                    $result = @move_uploaded_file($_FILES['fileUpload']['tmp_name'], $newimage);
                    if ( empty($result) ) $error["result"] = "There was an error moving the uploaded file.";
                }
            }
        } else {
            echo "no form data found";
        }
    ?>
#endif
}

// Example php code to take uploaded jog images
#if false
// Php Code, upload this to your server
<?php
    //check if something its being sent to this script
    if ($_POST)
    {
        //check if theres a field called action in the sent data
        if ( isset ($_POST['action']) )
        {
            //if it indeed theres an field called action. check if its value its level upload
            if($_POST['action'] === 'level upload')
            {
                if(!isset($_FILES) && isset($HTTP_POST_FILES))
                {
                    $_FILES = $HTTP_POST_FILES;
                }
                   
                if ($_FILES['file']['error'] === UPLOAD_ERR_OK)
                {
                    //check if the file has a name, in this script it has to have a name to be stored, the file name is sent by unity
                    if ($_FILES['file']['name'] !== "")
                    {
                        //this checks the file mime type, to filter the kind of files you want to accept, this script is configured to accept only jpg files
                        if ($_FILES['file']['type'] === 'images/jpg')
                        {
                            $uploadfile =  $_FILES['file']['name'];
                            $newimage = "images/" . $uploadfile;
                            move_uploaded_file($_FILES['file']['tmp_name'], $newimage);              
                        }
                    }
                }
            }
        }   
    }
?>
#endif

#endif