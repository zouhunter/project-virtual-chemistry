using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;

namespace HighlightingSystem
{
	[RequireComponent(typeof(Camera))]
	public class HighlightingBase : MonoBehaviour
	{
		#region Static Fields and Constants
		static protected readonly Color colorClear = new Color(0f, 0f, 0f, 0f);
		static protected readonly string renderBufferName = "HighlightingSystem";
		static protected readonly Matrix4x4 identityMatrix = Matrix4x4.identity;
		protected const CameraEvent queue = CameraEvent.BeforeImageEffectsOpaque;

		static protected RenderTargetIdentifier highlightingBufferID;
		static protected RenderTargetIdentifier cameraTargetID;
		
		static protected Mesh quad;

		// Graphics device version identifiers
		protected const int OGL = 0;
		protected const int D3D9 = 1;
		protected const int D3D11 = 2;

		// Current graphics device version: 0 = OpenGL or unknown (default), 1 = Direct3D 9, 2 = Direct3D 11
		static protected int graphicsDeviceVersion = D3D9;
		#endregion
		
		#region Public Fields
		// Depth offset factor for highlighting shaders
		public float offsetFactor = 0f;

		// Depth offset units for highlighting shaders
		public float offsetUnits = 0f;

		// Highlighting buffer size downsample factor
		public int downsampleFactor
		{
			get { return _downsampleFactor; }
			set
			{
				if (_downsampleFactor != value)
				{
					// Is power of two check
					if ((value != 0) && ((value & (value - 1)) == 0))
					{
						_downsampleFactor = value;
						isDirty = true;
					}
					else
					{
						Debug.LogWarning("HighlightingSystem : Prevented attempt to set incorrect downsample factor value.");
					}
				}
			}
		}
		
		// Blur iterations
		public int iterations
		{
			get { return _iterations; }
			set
			{
				if (_iterations != value)
				{
					_iterations = value;
					isDirty = true;
				}
			}
		}
		
		// Blur minimal spread
		public float blurMinSpread
		{
			get { return _blurMinSpread; }
			set
			{
				if (_blurMinSpread != value)
				{
					_blurMinSpread = value;
					isDirty = true;
				}
			}
		}
		
		// Blur spread per iteration
		public float blurSpread
		{
			get { return _blurSpread; }
			set
			{
				if (_blurSpread != value)
				{
					_blurSpread = value;
					isDirty = true;
				}
			}
		}
		
		// Blurring intensity for the blur material
		public float blurIntensity
		{
			get { return _blurIntensity; }
			set
			{
				if (_blurIntensity != value)
				{
					_blurIntensity = value;
					if (Application.isPlaying)
					{
						blurMaterial.SetFloat(ShaderPropertyID._Intensity, _blurIntensity);
					}
				}
			}
		}
		#endregion
		
		#region Protected Fields
		protected CommandBuffer renderBuffer;

		protected bool isDirty = true;

		protected int cachedWidth = -1;
		protected int cachedHeight = -1;
		protected int cachedAA = -1;

		[FormerlySerializedAs("downsampleFactor")]
		[SerializeField]
		protected int _downsampleFactor = 4;

		[FormerlySerializedAs("iterations")]
		[SerializeField]
		protected int _iterations = 2;

		[FormerlySerializedAs("blurMinSpread")]
		[SerializeField]
		protected float _blurMinSpread = 0.65f;

		[FormerlySerializedAs("blurSpread")]
		[SerializeField]
		protected float _blurSpread = 0.25f;

		[SerializeField]
		protected float _blurIntensity = 0.3f;

		// RenderTexture with highlighting buffer
		protected RenderTexture highlightingBuffer = null;

		// Camera reference
		protected Camera cam = null;

		// True if HighlightingSystem is supported on this platform
		protected bool isSupported = false;

		// True if framebuffer depth data is currently available (it is required for the highlighting occlusion feature)
		protected bool isDepthAvailable = true;

		// Material parameters
		protected const int BLUR = 0;
		protected const int CUT = 1;
		protected const int COMP = 2;
		static protected readonly string[] shaderPaths = new string[]
		{
			"Hidden/Highlighted/Blur", 
			"Hidden/Highlighted/Cut", 
			"Hidden/Highlighted/Composite", 
		};
		static protected Shader[] shaders;
		static protected Material[] materials;

		// Static materials
		static protected Material cutMaterial;
		static protected Material compMaterial;

		// Dynamic materials
		protected Material blurMaterial;

		static protected bool initialized = false;
		#endregion

		#region MonoBehaviour
		// 
		protected virtual void OnEnable()
		{
			if (!CheckInstance()) { return; }

			Initialize();

			isSupported = CheckSupported();
			if (!isSupported)
			{
				enabled = false;
				Debug.LogWarning("HighlightingSystem : Highlighting System has been disabled due to unsupported Unity features on the current platform!");
				return;
			}

			blurMaterial = new Material(materials[BLUR]);
			
			// Set initial intensity in blur material
			blurMaterial.SetFloat(ShaderPropertyID._Intensity, _blurIntensity);

			renderBuffer = new CommandBuffer();
			renderBuffer.name = renderBufferName;

			cam = GetComponent<Camera>();
			UpdateHighlightingBuffer();

			// Force-rebuild renderBuffer
			isDirty = true;

			cam.AddCommandBuffer(queue, renderBuffer);
		}
		
		// 
		protected virtual void OnDisable()
		{
			if (renderBuffer != null)
			{
				cam.RemoveCommandBuffer(queue, renderBuffer);
				renderBuffer = null;
			}

			if (highlightingBuffer != null && highlightingBuffer.IsCreated())
			{
				highlightingBuffer.Release();
				highlightingBuffer = null;
			}
		}

		// 
		protected virtual void LateUpdate()
		{
			UpdateHighlightingBuffer();
		}

		// 
		protected virtual void OnPreRender()
		{
			int aa = GetAA();
			
			bool depthAvailable = (aa == 1);
			
			// In case MSAA is enabled in forward/vertex lit rendeirng paths - depth buffer is not available
			if (aa > 1 && (cam.actualRenderingPath == RenderingPath.Forward || cam.actualRenderingPath == RenderingPath.VertexLit))
			{
				depthAvailable = false;
			}
			
			// Check if framebuffer depth data availability has changed
			if (isDepthAvailable != depthAvailable)
			{
				isDepthAvailable = depthAvailable;
				// Update ZWrite value for all highlighting shaders correspondingly (isDepthAvailable ? ZWrite Off : ZWrite On)
				Highlighter.SetZWrite(isDepthAvailable ? 0f : 1f);
				if (isDepthAvailable)
				{
					Debug.LogWarning("HighlightingSystem : Framebuffer depth data is available back again and will be used to occlude highlighting. Highlighting occluders disabled.");
				}
				else
				{
					Debug.LogWarning("HighlightingSystem : Framebuffer depth data is not available and can't be used to occlude highlighting. Highlighting occluders enabled.");
				}
				isDirty = true;
			}
			
			// Set global depth offset properties for highlighting shaders to the values which has this HighlightingBase component
			Highlighter.SetOffsetFactor(offsetFactor);
			Highlighter.SetOffsetUnits(offsetUnits);

			isDirty |= HighlighterManager.isDirty;
			isDirty |= HighlightersChanged();

			if (isDirty)
			{
				RebuildCommandBuffer();
				isDirty = false;
			}
		}

		// 
		protected virtual void OnRenderImage(RenderTexture src, RenderTexture dst)
		{
			Graphics.Blit(src, dst, compMaterial);
		}
		#endregion

		#region Internal
		// 
		static protected void Initialize()
		{
			if (initialized) { return; }

			// Determine graphics device version
			string version = SystemInfo.graphicsDeviceVersion.ToLower();
			if (version.StartsWith("direct3d") || version.StartsWith("directx 11"))
			{
				if (version.StartsWith("direct3d 11") || version.StartsWith("directx 11")) { graphicsDeviceVersion = D3D11; }
				else { graphicsDeviceVersion = D3D9; }
			}
			else { graphicsDeviceVersion = OGL; }

			// Initialize shader property constants
			ShaderPropertyID.Initialize();

			// Initialize shaders and materials
			int l = shaderPaths.Length;
			shaders = new Shader[l];
			materials = new Material[l];
			for (int i = 0; i < l; i++)
			{
				Shader shader = Shader.Find(shaderPaths[i]);
				shaders[i] = shader;
				
				Material material = new Material(shader);
				material.hideFlags = HideFlags.HideAndDontSave;
				materials[i] = material;
			}
			cutMaterial = materials[CUT];
			compMaterial = materials[COMP];

			// Initialize static RenderTargetIdentifiers
			highlightingBufferID = new RenderTargetIdentifier(ShaderPropertyID._HighlightingBuffer);
			cameraTargetID = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);

			// Create static quad mesh
			CreateQuad();

			initialized = true;
		}

		// 
		static protected void CreateQuad()
		{
			if (quad == null)
			{
				quad = new Mesh();
			}
			else
			{
				quad.Clear();
			}
			
			float y1 = 1f;
			float y2 = -1f;
			
			if (graphicsDeviceVersion == OGL)
			{
				y1 = -1f;
				y2 = 1f;
			}
			
			quad.vertices = new Vector3[]
			{
				new Vector3(-1f, y1, 0f), // Bottom-Left
				new Vector3(-1f, y2, 0f), // Upper-Left
				new Vector3( 1f, y2, 0f), // Upper-Right
				new Vector3( 1f, y1, 0f)  // Bottom-Right
			};
			
			quad.uv = new Vector2[]
			{
				new Vector2(0f, 0f), 
				new Vector2(0f, 1f), 
				new Vector2(1f, 1f), 
				new Vector2(1f, 0f)
			};
			
			quad.colors = new Color[]
			{
				colorClear, 
				colorClear, 
				colorClear, 
				colorClear
			};
			
			quad.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
		}

		// 
		protected virtual int GetAA()
		{
			int aa = QualitySettings.antiAliasing;
			if (aa == 0) { aa = 1; }

			// Reset aa value to 1 in case camera is in DeferredLighting or DeferredShading Rendering Path
			if (cam.actualRenderingPath == RenderingPath.DeferredLighting || cam.actualRenderingPath == RenderingPath.DeferredShading) { aa = 1; }

			return aa;
		}

		// 
		protected virtual void UpdateHighlightingBuffer()
		{
			int aa = GetAA();
			
			if (cam.pixelWidth == cachedWidth && cam.pixelHeight == cachedHeight && aa == cachedAA) { return; }

			cachedWidth = cam.pixelWidth;
			cachedHeight = cam.pixelHeight;
			cachedAA = aa;

			if (highlightingBuffer != null && highlightingBuffer.IsCreated())
			{
				highlightingBuffer.Release();
			}

			highlightingBuffer = new RenderTexture(cachedWidth, cachedHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			highlightingBuffer.antiAliasing = cachedAA;
			highlightingBuffer.filterMode = FilterMode.Bilinear;
			highlightingBuffer.useMipMap = false;
			highlightingBuffer.wrapMode = TextureWrapMode.Clamp;
			if (!highlightingBuffer.Create())
			{
				Debug.LogError("HighlightingSystem : UpdateHighlightingBuffer() : Failed to create highlightingBuffer RenderTexture!");
			}

			// Always set as dirty, because camera width/height/aa has changed
			isDirty = true;
		}

		// Allow only single instance of the HighlightingBase component on a GameObject
		public virtual bool CheckInstance()
		{
			HighlightingBase[] highlightingBases = GetComponents<HighlightingBase>();
			if (highlightingBases.Length > 1 && highlightingBases[0] != this)
			{
				enabled = false;
				string className = this.GetType().ToString();
				Debug.LogWarning(string.Format("HighlightingSystem : Only single instance of the HighlightingRenderer component is allowed on a single Gameobject! {0} has been disabled on GameObject with name '{1}'.", className, name));
				return false;
			}
			return true;
		}
		
		// 
		protected virtual bool CheckSupported()
		{
			// Image Effects supported?
			if (!SystemInfo.supportsImageEffects)
			{
				Debug.LogWarning("HighlightingSystem : Image effects is not supported on this platform!");
				return false;
			}
			
			// Render Textures supported?
			if (!SystemInfo.supportsRenderTextures)
			{
				Debug.LogWarning("HighlightingSystem : RenderTextures is not supported on this platform!");
				return false;
			}
			
			// Required Render Texture Format supported?
			if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32))
			{
				Debug.LogWarning("HighlightingSystem : RenderTextureFormat.ARGB32 is not supported on this platform!");
				return false;
			}
			
			// HighlightingOpaque shader supported?
			if (!Highlighter.opaqueShader.isSupported)
			{
				Debug.LogWarning("HighlightingSystem : HighlightingOpaque shader is not supported on this platform!");
				return false;
			}
			
			// HighlightingTransparent shader supported?
			if (!Highlighter.transparentShader.isSupported)
			{
				Debug.LogWarning("HighlightingSystem : HighlightingTransparent shader is not supported on this platform!");
				return false;
			}

			// Required shaders supported?
			for (int i = 0; i < shaders.Length; i++)
			{
				Shader shader = shaders[i];
				if (!shader.isSupported)
				{
					Debug.LogWarning("HighlightingSystem : Shader '" + shader.name + "' is not supported on this platform!");
					return false;
				}
			}

			return true;
		}

		// 
		protected virtual bool HighlightersChanged()
		{
			bool changed = false;

			// Check if list of highlighted objects has changed
			HashSet<Highlighter>.Enumerator e = HighlighterManager.GetEnumerator();
			while (e.MoveNext())
			{
				Highlighter highlighter = e.Current;
				changed |= highlighter.UpdateHighlighting(isDepthAvailable);
			}

			return changed;
		}

		// 
		protected virtual void RebuildCommandBuffer()
		{
			renderBuffer.Clear();

			int aa = GetAA();

			RenderTargetIdentifier depthID = isDepthAvailable ? cameraTargetID : highlightingBufferID;

			// Prepare and clear render target
			renderBuffer.GetTemporaryRT(ShaderPropertyID._HighlightingBuffer, -1, -1, isDepthAvailable ? 0 : 24, FilterMode.Bilinear, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, aa);
			renderBuffer.SetRenderTarget(highlightingBufferID, depthID);
			renderBuffer.ClearRenderTarget(!isDepthAvailable, true, colorClear);

			// Fill buffer with highlighters rendering commands
			HashSet<Highlighter>.Enumerator e;
			e = HighlighterManager.GetEnumerator();
			while (e.MoveNext())
			{
				Highlighter highlighter = e.Current;
				highlighter.FillBuffer(ref renderBuffer, false);
			}
			e = HighlighterManager.GetEnumerator();
			while (e.MoveNext())
			{
				Highlighter highlighter = e.Current;
				highlighter.FillBuffer(ref renderBuffer, true);
			}

			// Create two buffers for blurring the image
			RenderTargetIdentifier blur1ID = new RenderTargetIdentifier(ShaderPropertyID._HighlightingBlur1);
			RenderTargetIdentifier blur2ID = new RenderTargetIdentifier(ShaderPropertyID._HighlightingBlur2);

			int width = cam.pixelWidth / _downsampleFactor;
			int height = cam.pixelHeight / _downsampleFactor;

			renderBuffer.GetTemporaryRT(ShaderPropertyID._HighlightingBlur1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			renderBuffer.GetTemporaryRT(ShaderPropertyID._HighlightingBlur2, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

			Vector4 v;
			v = new Vector4(1f / (float)cam.pixelWidth, 1f / (float)cam.pixelHeight, 0f, 0f);
			renderBuffer.SetGlobalVector(ShaderPropertyID._HighlightingBufferTexelSize, v);
			renderBuffer.SetGlobalVector(ShaderPropertyID._HighlightingBlurredTexelSize, v * _downsampleFactor);

			renderBuffer.Blit(highlightingBufferID, blur1ID);

			// Blur the small texture
			bool oddEven = true;
			for (int i = 0; i < _iterations; i++)
			{
				float off = _blurMinSpread + i * _blurSpread;
				renderBuffer.SetGlobalFloat(ShaderPropertyID._HighlightingBlurOffset, off);
				
				if (oddEven)
				{
					renderBuffer.Blit(blur1ID, blur2ID, blurMaterial);
				}
				else
				{
					renderBuffer.Blit(blur2ID, blur1ID, blurMaterial);
				}
				
				oddEven = !oddEven;
			}

			v = new Vector4(-1f / (float)cam.pixelWidth, 1f / (float)cam.pixelHeight, 0f, 0f);
			renderBuffer.SetGlobalVector(ShaderPropertyID._HighlightingBufferTexelSize, v);

			// Upscale blurred texture and cut stencil from it
			renderBuffer.SetGlobalTexture(ShaderPropertyID._HighlightingBlurred, oddEven ? blur1ID : blur2ID);
			renderBuffer.SetRenderTarget(highlightingBufferID, depthID);
			renderBuffer.DrawMesh(quad, identityMatrix, cutMaterial);

			// Cleanup
			renderBuffer.ReleaseTemporaryRT(ShaderPropertyID._HighlightingBlur1);
			renderBuffer.ReleaseTemporaryRT(ShaderPropertyID._HighlightingBlur2);
		}
		#endregion
	}
}