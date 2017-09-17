using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CaronteFX
{

  [System.Serializable]
  public class CRCollisionEvent : UnityEvent<CRCollisionEvInfo>
  {

  }


  public class CRAnimation : MonoBehaviour
  {
    [System.Serializable]
    public enum RepeatMode
    {
      Loop,
      Clamp,
      PingPong,
    };

    [Flags]
    private enum BROADCASTFLAGS : byte
    {
      ACTIVE   = 1 << 0,
      VISIBLE  = 1 << 1,
      SLEEPING = 1 << 2,
      GHOST    = 1 << 3
    }

    public CRAnimationAsset       activeAnimation     = null;
    public List<CRAnimationAsset> listAnimations      = new List<CRAnimationAsset>();
    public float                  speed               = 1.0f;
    public RepeatMode             repeatMode          = RepeatMode.Loop;
    public CRCollisionEvent       collisionEvent      = null;
    public bool                   animate             = true;
    public bool                   interpolate         = false;
    public bool                   doRuntimeNullChecks = false;
        
    [NonSerialized]
    public float time_ = 0.0f;
    [NonSerialized]
    public int frameCount_ = 0;
    [NonSerialized]
    public float frameTime_ = 0.0f;
    [NonSerialized]
    public int fps_ = 0;
    [NonSerialized]
    public float animationLength_ = 0.0f;
    [NonSerialized]
    public int firstFrame_ = 0;
    [NonSerialized]
    public int lastFrame_ = 0;
    [NonSerialized]
    public int nGameObjects_;
    [NonSerialized]
    public string[] arrGOPath_;
    [NonSerialized]
    public int nEmitters_ = 0;
    [NonSerialized]
    public string[] arrEmitterName_;
    [NonSerialized]
    public CRAnimationAsset animationLastLoaded_;
    [NonSerialized]
    public int lastReadFrame_  = -1;
    [NonSerialized]
    public float lastReadFloatFrame_ = -1;
    
    [HideInInspector]
    public GameObject tmpGameObject_;

    [SerializeField, HideInInspector]
    private List<GameObject> listTmpGameObjects_ = new List<GameObject>();
    [SerializeField, HideInInspector]
    private List<Mesh> listTmpMeshes_ = new List<Mesh>();
    
    private byte[] binaryAnim_;

    private int bCursor1_ = 0;
    private int bCursor2_ = 0;

    private double timeInternal_ = 0.0;

    private Tuple3<Transform, int, int>[] arrGOVC_;

    private BitArray    arrSkipObject_;
    private BitArray    arrIsBone_;
    private Vector2[]   arrVisibilityInterval_;
    private Mesh[]      arrMesh_;
    private long[]      arrFrameOffsets_;
    private int[]       arrCacheIndex_;
    private Vector3[][] arrVertex3Cache_;
    private Vector4[][] arrVertex4Cache_;
    private bool        interpolationModeActive_;

    private int  binaryVersion_;
    private bool sbCompression_;
    private bool sbTangents_;
    private bool internalPaused_;


    private Dictionary<int, int> dictVertexCountCacheIdx_ = new Dictionary<int, int>();
    private List<int> listVertexCacheCount_ = new List<int>();

    CRCollisionEvInfo ceInfo = new CRCollisionEvInfo();

    delegate void ReadFrameDel(float frame);   
    ReadFrameDel readFrameDel_ = new ReadFrameDel( (frame) => {} );

    const float vecQuantz = 1.0f / 127.0f;
    const float posQuantz = 1.0f / 65535.0f;

    void Start()
    {
      LoadAnimation(false);
    }

    void OnApplicationQuit()
    {
      CloseAnimation();
    }

    void OnDestroy()
    {
      CloseAnimation();
    }

    public void LoadAnimation(bool fromEditor)
    {
      if (activeAnimation == null)
      {
        return;
      }

      animationLastLoaded_ = null;
      
      byte[] animBytes = activeAnimation.bytes;

      using (MemoryStream ms = new MemoryStream(animBytes, false) )
      {
        if (ms != null)
        {
          using (BinaryReader br = new BinaryReader(ms) )
          {
            if (br != null)
            {
              binaryVersion_ = br.ReadInt32();

              LoadAnimationCommon(br, fromEditor);

              if (binaryVersion_ < 5 )
              {
                LoadAnimationV0(br, fromEditor);
              }
              if (binaryVersion_ == 5)
              {
                LoadAnimationV5(br, fromEditor);
              }
              else if (binaryVersion_ == 6)
              {
                LoadAnimationV6(br, fromEditor);
              }

              binaryAnim_          = animBytes;
              animationLastLoaded_ = activeAnimation;
              lastReadFrame_       = -1;
              lastReadFloatFrame_  = -1;
            }
          } // BinaryReader
        } //MemoryStream
      }
    }

    public void CloseAnimation()
    {
      binaryAnim_ = null;
    }

    private void LoadAnimationCommon(BinaryReader br, bool fromEditor)
    {
      AssignReadFrameDelegate();
      interpolationModeActive_ = interpolate;

      sbCompression_ = br.ReadBoolean();
      sbTangents_    = br.ReadBoolean();
      frameCount_    = br.ReadInt32();
      fps_           = br.ReadInt32();
      nGameObjects_  = br.ReadInt32();

      firstFrame_      = 0;
      lastFrame_       = Mathf.Max(frameCount_ - 1, 0);

      animationLength_ = (float)lastFrame_ / (float)fps_;
      frameTime_       = 1.0f / (float)fps_;

      arrGOPath_             = new string[nGameObjects_];
      arrGOVC_               = new Tuple3<Transform, int, int>[nGameObjects_];
      arrSkipObject_         = new BitArray(nGameObjects_, false);
      arrIsBone_             = new BitArray(nGameObjects_, false);
      arrVisibilityInterval_ = new Vector2[nGameObjects_];
      arrMesh_               = new Mesh[nGameObjects_];
      arrCacheIndex_         = new int[nGameObjects_];

      dictVertexCountCacheIdx_.Clear();
      listVertexCacheCount_   .Clear();

      if (fromEditor)
      {
        CreateTmpObjects();
      }
    }

    private void LoadAnimationV0(BinaryReader br, bool fromEditor)
    {
      for (int i = 0; i < nGameObjects_; i++)
      {
        string relativePath = br.ReadString();
        int vertexCount     = br.ReadInt32();

        if (fromEditor)
        {
          arrGOPath_[i] = "_tmp/" + relativePath;
        }
        else
        {
          arrGOPath_[i] = relativePath;
        }
     
        CreateCacheIdx(i, vertexCount);
        int offsetBytesSize = CalculateStreamOffsetSize( vertexCount );

        Transform tr = transform.Find(relativePath); 
        arrGOVC_[i] = Tuple3.New(tr, vertexCount, offsetBytesSize);
        
        if ( tr == null || 
           ( tr != null && ( vertexCount > 0 && !tr.gameObject.HasMesh() ) ) )
        {
          arrSkipObject_[i] = true;
          continue;
        }
  
        if(fromEditor)
        {
          AssignGameObjectFromEditor(ref arrGOVC_[i], ref arrMesh_[i]);
        }
        else
        {
          AssignGameObject(ref arrGOVC_[i], ref arrMesh_[i]);
        }         
      } //for GameObjects...

      CreateCaches();

      arrFrameOffsets_ = new long[frameCount_];
      for (int i = 0; i < frameCount_; i++)
      {
        arrFrameOffsets_[i] = br.ReadInt64();
      }
    }

    private void LoadAnimationV5(BinaryReader br, bool fromEditor)
    {
      for (int i = 0; i < nGameObjects_; i++)
      {
        string relativePath = br.ReadString();
        int vertexCount     = br.ReadInt32();
        
        if (fromEditor)
        {
          arrGOPath_[i] = "_tmp/" + relativePath;
        }
        else
        {
          arrGOPath_[i] = relativePath;
        }
     
        CreateCacheIdx(i, vertexCount);
        int offsetBytesSize = CalculateStreamOffsetSize( vertexCount );

        Transform tr = transform.Find(relativePath);
        arrGOVC_[i] = Tuple3.New(tr, vertexCount, offsetBytesSize);

        if ( tr == null || 
           ( tr != null && ( vertexCount > 0 && !tr.gameObject.HasMesh() ) ) )
        {
          arrSkipObject_[i] = true;
          continue;
        }

        arrIsBone_[i] = ( vertexCount == 0 && !tr.gameObject.HasMesh() );
        arrVisibilityInterval_[i] = Vector2.zero;

        if(fromEditor)
        {
          AssignGameObjectFromEditor(ref arrGOVC_[i], ref arrMesh_[i]);
        }
        else
        {
          AssignGameObject(ref arrGOVC_[i], ref arrMesh_[i]);
        }         
      } //for GameObjects...


      nEmitters_ = br.ReadInt32();
      arrEmitterName_ = new string[nEmitters_];
      for (int i = 0; i < nEmitters_; i++)
      {
        arrEmitterName_[i] = br.ReadString();
      }

      CreateCaches();

      arrFrameOffsets_ = new long[frameCount_];
      for (int i = 0; i < frameCount_; i++)
      {
        arrFrameOffsets_[i] = br.ReadInt64();
      }
    }

    private void LoadAnimationV6(BinaryReader br, bool fromEditor)
    {
      for (int i = 0; i < nGameObjects_; i++)
      {
        string relativePath = br.ReadString();
        int vertexCount     = br.ReadInt32();
        Vector2 v           = new Vector2( br.ReadSingle(), br.ReadSingle() );

        if (fromEditor)
        {
          arrGOPath_[i] = "_tmp/" + relativePath;
        }
        else
        {
          arrGOPath_[i] = relativePath;
        }
     
        CreateCacheIdx(i, vertexCount);
        int offsetBytesSize = CalculateStreamOffsetSize( vertexCount );

        Transform tr = transform.Find(relativePath);
        arrGOVC_[i] = Tuple3.New(tr, vertexCount, offsetBytesSize);

        if ( tr == null || 
           ( tr != null && ( vertexCount > 0 && !tr.gameObject.HasMesh() ) ) )
        {
          arrSkipObject_[i] = true;
          continue;
        }

        arrIsBone_[i] = ( vertexCount == 0 && !tr.gameObject.HasMesh() );
        arrVisibilityInterval_[i] = v;

        if(fromEditor)
        {
          AssignGameObjectFromEditor(ref arrGOVC_[i], ref arrMesh_[i]);
        }
        else
        {
          AssignGameObject(ref arrGOVC_[i], ref arrMesh_[i]);
        }         
      } //for GameObjects...


      nEmitters_ = br.ReadInt32();
      arrEmitterName_ = new string[nEmitters_];
      for (int i = 0; i < nEmitters_; i++)
      {
        arrEmitterName_[i] = br.ReadString();
      }

      CreateCaches();

      arrFrameOffsets_ = new long[frameCount_];
      for (int i = 0; i < frameCount_; i++)
      {
        arrFrameOffsets_[i] = br.ReadInt64();
      }
    }

    private byte ReadByte(ref int cursor)
    {
      int offset = cursor;
      cursor += sizeof(byte);
      return binaryAnim_[offset];
    }

    private sbyte ReadSByte(ref int cursor)
    {
      int offset = cursor;
      cursor += sizeof(sbyte);
      return (sbyte)binaryAnim_[offset];
    }

    private bool ReadBoolean(ref int cursor)
    {
      int offset = cursor;
      cursor += sizeof(bool);
      return BitConverter.ToBoolean(binaryAnim_, offset);
    }

    private string ReadString(ref int cursor)
    {
      string str = BitConverter.ToString(binaryAnim_, cursor);
      cursor += str.Length * (sizeof(char) + 1);
      return str;
    }

    private UInt16 ReadUInt16(ref int cursor)
    {
      int offset = cursor;
      cursor += sizeof(UInt16);
      return ( BitConverter.ToUInt16(binaryAnim_, offset) );
    }

    private Int32 ReadInt32(ref int cursor)
    {
      int offset = cursor;
      cursor += sizeof(Int32);
      return BitConverter.ToInt32(binaryAnim_, offset);
    }

    private Int64 ReadInt64(ref int cursor)
    {
      int offset = cursor;
      cursor += sizeof(Int64);
      return BitConverter.ToInt64(binaryAnim_, offset);
    }

    private float ReadSingle(ref int cursor)
    {
      int offset = cursor;
      cursor += sizeof(float);
      return BitConverter.ToSingle(binaryAnim_, offset);
    }

    private void SetCursorAt(Int64 bytesOffset, ref int cursor)
    {
      cursor = (int)bytesOffset;
    }

    private void AdvanceCursor(Int64 bytesOffset, ref int cursor)
    {
      cursor += (int)bytesOffset;
    }

    private void AssignGameObject(ref Tuple3<Transform, int, int> goData, ref Mesh mesh)
    {
      Transform tr        = goData.First;
      int vertexCount     = goData.Second;

      if (vertexCount > 0)
      {
        GameObject go = goData.First.gameObject;
        mesh          = go.GetMeshInstance();
        tr.localScale = Vector3.one;
      }
    }

    private void AssignGameObjectFromEditor(ref Tuple3<Transform, int, int> goData, ref Mesh mesh)
    {
      Transform tr        = goData.First;
      int vertexCount     = goData.Second;
      int offsetBytesSize = goData.Third;

      if (tr != null)
      {
        GameObject tmpGO;
        if (vertexCount > 0)
        {
          GameObject originalTmpObject = tr.gameObject;
          tmpGO = CreateTmpMeshAnimatedObject(originalTmpObject);

          mesh  = tmpGO.GetMesh();

          tmpGO.transform.localScale = Vector3.one;

          UnityEngine.Object.DestroyImmediate(originalTmpObject);

          goData = Tuple3.New(tmpGO.transform, vertexCount, offsetBytesSize);
        }       
      }
    }

    private void AssignReadFrameDelegate()
    {
      if ( binaryVersion_ == 4 )
      {
        readFrameDel_ = ReadFrameV4;
      }
      else if ( binaryVersion_ == 5 )
      {
        readFrameDel_ = ReadFrameV5;     
      }
      else if (binaryVersion_ == 6)
      {
        if (interpolate)
        {
          readFrameDel_ = ReadFrameInterpolatedV6;
        }
        else
        {
          readFrameDel_ = ReadFrameV6;
        }
      }
    }

    private int CalculateStreamOffsetSize( int vertexCount )
    {
      const int nLocationComponents = 3;
      const int nRotationComponents = 4;

      const int nBoxComponents = 6;
      const int nPositionComponents = 3;
      const int nNormalComponents   = 3;
      const int nTangentComponents  = 4;

      const int nFloatBytes    = sizeof(float);
      const int nUInt16Bytes   = sizeof(UInt16);
      const int nSByteByes     = sizeof(sbyte);

      int bytesAdvance = nFloatBytes * (nLocationComponents + nRotationComponents);

      if (vertexCount > 0)
      {
        if ( sbCompression_ )
        { 
          bytesAdvance += nFloatBytes * nBoxComponents + (nUInt16Bytes * nPositionComponents + nSByteByes * nNormalComponents ) * vertexCount;
          if (sbTangents_)
          {
            bytesAdvance += nSByteByes * nTangentComponents * vertexCount;
          }    
        } 
        else
        {
          bytesAdvance += nFloatBytes * nBoxComponents + ( nFloatBytes * ( nPositionComponents + nNormalComponents ) ) * vertexCount;
          if (sbTangents_)
          {
            bytesAdvance += nFloatBytes * nTangentComponents * vertexCount;
          }
        }
      }
 
      return bytesAdvance;
    }

    private GameObject CreateTmpMeshAnimatedObject(GameObject originalGO)
    {
      GameObject tmpEditorGO = (GameObject)UnityEngine.Object.Instantiate(originalGO);
      tmpEditorGO.transform.parent = originalGO.transform.parent;

      MeshFilter meshfilter;
      Mesh meshOriginal = tmpEditorGO.GetMesh(out meshfilter);  
      Mesh meshInstance = (Mesh)UnityEngine.Object.Instantiate(meshOriginal);

      listTmpMeshes_.Add( meshInstance );

      meshfilter.sharedMesh = meshInstance;

      tmpEditorGO.hideFlags = HideFlags.DontSave;
      tmpEditorGO.tag       = "EditorOnly";

      return tmpEditorGO;
    }

    private GameObject CreateTmpObject(GameObject originalGO)
    {
      GameObject tmpEditorGO = (GameObject)UnityEngine.Object.Instantiate(originalGO);
      tmpEditorGO.transform.parent = tmpGameObject_.transform;

      tmpEditorGO.hideFlags = HideFlags.DontSave;
      tmpEditorGO.tag       = "EditorOnly";

      return tmpEditorGO;
    }

    private void CreateTmpObjects()
    {
      int nChild = transform.childCount;

      for (int i = 0; i < nChild; i++)
      {
        Transform tr  = transform.GetChild(i);
        GameObject go = tr.gameObject;
        listTmpGameObjects_.Add(go);
        go.SetActive(false);
      }

      if (tmpGameObject_ == null)
      {
        tmpGameObject_ = new GameObject("_tmp");

        tmpGameObject_.transform.parent        = gameObject.transform;
        tmpGameObject_.transform.localPosition = Vector3.zero;
        tmpGameObject_.transform.localRotation = Quaternion.identity;
        tmpGameObject_.transform.localScale    = Vector3.one;

        tmpGameObject_.hideFlags = HideFlags.DontSave;
        tmpGameObject_.tag = "EditorOnly";
      }

      tmpGameObject_.SetActive(true);

      foreach (GameObject go in listTmpGameObjects_)
      {
        GameObject clonedGO = UnityEngine.Object.Instantiate(go);
        clonedGO.transform.parent = tmpGameObject_.transform;
        clonedGO.SetActive(true);
      }

      listTmpGameObjects_.Clear();
    }

    public void DestroyTmpObjects()
    {
      foreach( Mesh mesh in listTmpMeshes_ )
      {
        UnityEngine.Object.DestroyImmediate(mesh);
      }
      listTmpMeshes_.Clear();
      UnityEngine.Object.DestroyImmediate(tmpGameObject_);

      int nChild = transform.childCount;

      for (int i = 0; i < nChild; i++)
      {
        Transform tr = transform.GetChild(i);
        tr.gameObject.SetActive( true );
      }
    }

    private void CreateCacheIdx(int gameObjectIdx, int vertexCount)
    {
      if (vertexCount > 0)
      {
        if (!dictVertexCountCacheIdx_.ContainsKey(vertexCount))
        {
          listVertexCacheCount_.Add(vertexCount);
          dictVertexCountCacheIdx_[vertexCount] = listVertexCacheCount_.Count - 1;
        }
        arrCacheIndex_[gameObjectIdx] = dictVertexCountCacheIdx_[vertexCount];
      }
      else
      {
        arrCacheIndex_[gameObjectIdx] = -1;
      }
    }

    private void CreateCaches()
    {
      int nCaches = listVertexCacheCount_.Count;
      arrVertex3Cache_ = new Vector3[nCaches][];
      for (int i = 0; i < nCaches; i++)
      {
        arrVertex3Cache_[i] = new Vector3[listVertexCacheCount_[i]];
      }

      if (sbTangents_)
      {
        arrVertex4Cache_ = new Vector4[nCaches][];
        for (int i = 0; i < nCaches; i++)
        {
          arrVertex4Cache_[i] = new Vector4[listVertexCacheCount_[i]];
        }
      }
    }

    void Update()
    {
      if ( activeAnimation != animationLastLoaded_)
      {
        LoadAnimation(false);
      }

      if ( animate && !internalPaused_ && activeAnimation != null)
      {
        timeInternal_ += Time.deltaTime  * speed;

        switch (repeatMode)
        {
          case RepeatMode.Loop:
            time_ = Mathf.Repeat((float)timeInternal_, animationLength_);
            break;
          case RepeatMode.PingPong:
            time_ = Mathf.PingPong((float)timeInternal_, animationLength_);
            break;
          case RepeatMode.Clamp:
            time_ = Mathf.Clamp((float)timeInternal_, 0.0f, animationLength_);
            break;
        }

        if (interpolate != interpolationModeActive_)
        {
          AssignReadFrameDelegate();
          interpolationModeActive_ = interpolate;
        }

        float floatFrame = Mathf.Clamp(time_ * fps_, 0f, (float)lastFrame_);
        readFrameDel_(floatFrame);
      }
    }

    void OnApplicationPause(bool pauseStatus) 
    {
        internalPaused_ = pauseStatus;
    }

    public void SetFrame( float frame )
    {
      float time = frame / (float)fps_;
      SetTime( time );
    }
 
    public void SetTime( float time )
    {
      timeInternal_ = time;
      time_         = time;
      
      if (!animate)
      {
        if (interpolate != interpolationModeActive_)
        {
          AssignReadFrameDelegate();
          interpolationModeActive_ = interpolate;
        }           
              
        float floatFrame = Mathf.Clamp(time_ * fps_, 0f, (float)lastFrame_);
        readFrameDel_(floatFrame); 
      }
    }

    public void ChangeToAnimation(uint itemIdx)
    {
      if ( listAnimations.Count < itemIdx )
      {
        activeAnimation = listAnimations[(int) itemIdx];
      }
    }

    private void SetVisibility(Transform tr, bool isBone, bool isVisible)
    {
      GameObject go = tr.gameObject;

      if (isBone)
      {
        Vector3 currentScale = tr.localScale;
        if ( ( isVisible && go.activeInHierarchy ) && (currentScale != Vector3.one) )
        {
          tr.localScale = Vector3.one;
        }
        else if ( ( !isVisible || !go.activeInHierarchy ) && (currentScale != Vector3.zero) )
        {
          tr.localScale = Vector3.zero;
        }
      }
      else
      {
        bool currentActive = go.activeSelf;
        if ( ( isVisible && !currentActive) ||
             (!isVisible && currentActive )  )
        {
          go.SetActive(isVisible);
        }       
      }
    }

    private void ReadFrameV4(float frame)
    {
      int nearFrame = (int)Mathf.RoundToInt(frame);

      if ( lastReadFrame_ == nearFrame )
      {
        return;
      }

      SetCursorAt(arrFrameOffsets_[nearFrame], ref bCursor1_);

      for ( int i = 0; i < nGameObjects_; i++ )
      {
        Tuple3<Transform, int, int> tGOVC = arrGOVC_[i];

        Transform tr        = tGOVC.First;
        int vertexCount     = tGOVC.Second;
        int offsetBytesSize = tGOVC.Third;

        BROADCASTFLAGS flags = (BROADCASTFLAGS)ReadByte(ref bCursor1_);

        bool isVisible = ( flags & BROADCASTFLAGS.VISIBLE ) == BROADCASTFLAGS.VISIBLE;

        if ( tr == null || (vertexCount > 0 && arrMesh_[i] == null) )
        {
          if ( isVisible )
          {
            AdvanceCursor(offsetBytesSize, ref bCursor1_);
          }
          continue;
        }

        tr.gameObject.SetActive( isVisible );
          
        if (isVisible)
        {
          ReadRQ(tr, ref bCursor1_);

          if (vertexCount > 0)
          {  
            Mesh mesh    = arrMesh_[i];
            int cacheIdx = arrCacheIndex_[i];
            ReadMeshVertices(mesh, cacheIdx, vertexCount, ref bCursor1_);
          }

        } //isVisible

      } //forGameobjects

      lastReadFrame_ = nearFrame;
    }

    private void ReadFrameV5(float frame)
    {
      int nearFrame = (int)Mathf.RoundToInt(frame);

      if ( lastReadFrame_ == nearFrame )
      {
        return;
      }

      long frameOffset = arrFrameOffsets_[nearFrame];

      SetCursorAt(frameOffset, ref bCursor1_);
      for ( int i = 0; i < nGameObjects_; i++ )
      {
        Tuple3<Transform, int, int> tGOVC = arrGOVC_[i];

        Transform tr        = tGOVC.First;
        int vertexCount     = tGOVC.Second;
        int offsetBytesSize = tGOVC.Third;

        BROADCASTFLAGS flags = (BROADCASTFLAGS)ReadByte(ref bCursor1_);

        bool isVisible = ( flags & BROADCASTFLAGS.VISIBLE ) == BROADCASTFLAGS.VISIBLE;
        bool skipGameObject = arrSkipObject_[i];

        if ( skipGameObject )
        {
          if (isVisible)
          {
            AdvanceCursor(offsetBytesSize, ref bCursor1_);
          }
          continue;
        }

        if ( doRuntimeNullChecks )
        {
          bool isGONull   = tr == null;
          bool isMeshNull = (vertexCount > 0) && (arrMesh_[i] == null);

          if ( isGONull || isMeshNull ) 
          {
            if (isVisible)
            {
              AdvanceCursor(offsetBytesSize, ref bCursor1_);
            }     
            continue;
          }      
        }

        SetVisibility(tr, arrIsBone_[i], isVisible);

        if (isVisible)
        {
          ReadRQ(tr, ref bCursor1_);

          if (vertexCount > 0)
          {  
            Mesh mesh    = arrMesh_[i];
            int cacheIdx = arrCacheIndex_[i];
            ReadMeshVertices(mesh, cacheIdx, vertexCount, ref bCursor1_);
          }

        } //isVisible

      } //forGameobjects

      ReadEvents(ref bCursor1_);

      lastReadFrame_ = nearFrame;
    }

    private void ReadFrameV6(float frame)
    {
      int nearFrame = (int)Mathf.RoundToInt(frame);

      if ( lastReadFrame_ == nearFrame )
      {
        return;
      }

      long frameOffset = arrFrameOffsets_[nearFrame];
      SetCursorAt(frameOffset, ref bCursor1_);

      for (int i = 0; i < nGameObjects_; i++)
      {
        Tuple3<Transform, int, int> tGOVC = arrGOVC_[i];

        Transform tr        = tGOVC.First;
        int vertexCount     = tGOVC.Second;
        int offsetBytesSize = tGOVC.Third;

        BROADCASTFLAGS flagsnear = (BROADCASTFLAGS)ReadByte(ref bCursor1_);

        bool isVisible = (flagsnear & BROADCASTFLAGS.VISIBLE) == BROADCASTFLAGS.VISIBLE;
        bool isGhost   = (flagsnear & BROADCASTFLAGS.GHOST)   == BROADCASTFLAGS.GHOST;

        bool exists = isVisible || isGhost;
        
        bool skipGameObject = arrSkipObject_[i];
        if (skipGameObject)
        {
          AdvanceCursorIfExists(offsetBytesSize, exists);
          continue;
        }

        if (doRuntimeNullChecks)
        {
          bool isGONull = tr == null;
          bool isMeshNull = (vertexCount > 0) && (arrMesh_[i] == null);

          if (isGONull || isMeshNull)
          {
            AdvanceCursorIfExists(offsetBytesSize, exists);
            continue;
          }
        }
    
        SetVisibility(tr, arrIsBone_[i], (isVisible && !isGhost) );

        if (isVisible)
        { 
          ReadRQ(tr, ref bCursor1_);

          if (vertexCount > 0)
          {
            Mesh mesh = arrMesh_[i];
            int cacheIdx = arrCacheIndex_[i];
            ReadMeshVertices(mesh, cacheIdx, vertexCount, ref bCursor1_); 
          }
        }
        else if (isGhost)
        {
          AdvanceCursor(offsetBytesSize, ref bCursor1_);
        }
      } //forGameobjects

      ReadEvents(ref bCursor1_);
      lastReadFrame_ = nearFrame;
    }

    private void ReadFrameInterpolatedV6(float frame)
    {
      if ( lastReadFloatFrame_ == frame )
      {
        return;
      }

      int prevFrame = (int)frame;
      int nextFrame = Mathf.Min(prevFrame + 1, lastFrame_);

      float t = frame - prevFrame;

      long prevFrameOffset = arrFrameOffsets_[prevFrame];
      long nextFrameOffset = arrFrameOffsets_[nextFrame];

      SetCursorAt(prevFrameOffset, ref bCursor1_);
      SetCursorAt(nextFrameOffset, ref bCursor2_);

      for (int i = 0; i < nGameObjects_; i++)
      {
        Tuple3<Transform, int, int> tGOVC = arrGOVC_[i];

        Transform tr        = tGOVC.First;
        int vertexCount     = tGOVC.Second;
        int offsetBytesSize = tGOVC.Third;

        BROADCASTFLAGS flagsPrev = (BROADCASTFLAGS)ReadByte(ref bCursor1_);
        BROADCASTFLAGS flagsNext = (BROADCASTFLAGS)ReadByte(ref bCursor2_);

        bool visiblePrev = (flagsPrev & BROADCASTFLAGS.VISIBLE) == BROADCASTFLAGS.VISIBLE;
        bool ghostPrev   = (flagsPrev & BROADCASTFLAGS.GHOST)   == BROADCASTFLAGS.GHOST;

        bool existsPrev = visiblePrev || ghostPrev;

        bool visibleNext = (flagsNext & BROADCASTFLAGS.VISIBLE) == BROADCASTFLAGS.VISIBLE;
        bool ghostNext   = (flagsNext & BROADCASTFLAGS.GHOST)   == BROADCASTFLAGS.GHOST;
  
        bool existsNext  = visibleNext || ghostNext;

        bool skipGameObject = arrSkipObject_[i];

        if (skipGameObject)
        {
          AdvanceCursorsIfExists(offsetBytesSize, existsPrev, existsNext);
          continue;
        }

        if (doRuntimeNullChecks)
        {
          bool isGONull = tr == null;
          bool isMeshNull = (vertexCount > 0) && (arrMesh_[i] == null);

          if (isGONull || isMeshNull)
          {
            AdvanceCursorsIfExists(offsetBytesSize, existsPrev, existsNext);
            continue;
          }
        }
   
        Vector2 visibleTimeInterval = arrVisibilityInterval_[i];

        bool isInsideVisibleTimeInterval = (existsPrev && existsNext) && 
                                            ( time_ >= visibleTimeInterval.x && time_ < visibleTimeInterval.y );

        SetVisibility(tr, arrIsBone_[i], isInsideVisibleTimeInterval);
    
        if (!isInsideVisibleTimeInterval)
        {
          AdvanceCursorsIfExists(offsetBytesSize, existsPrev, existsNext);
        }
        else
        {
          float tAux = t;

          if (ghostPrev && visibleNext)
          { 
            float min = visibleTimeInterval.x;
            float max = nextFrame * frameTime_;
            tAux = (time_ - min) / (max - min);
          }
          else if (ghostNext && visiblePrev)
          {
            float min = prevFrame * frameTime_;
            float max = visibleTimeInterval.y;
            tAux = (time_ - min) / (max - min);
          }
          else if (ghostPrev && ghostNext)
          {
            float min = visibleTimeInterval.x;
            float max = visibleTimeInterval.y;
            tAux = (time_ - min) / (max - min);
          }

          ReadRQ(tAux, tr, ref bCursor1_, ref bCursor2_);
          if (vertexCount > 0)
          {
            Mesh mesh = arrMesh_[i];
            int cacheIdx = arrCacheIndex_[i];
            ReadMeshVertices(tAux, mesh, cacheIdx, vertexCount, ref bCursor1_, ref bCursor2_); 
          }
        }

      } //forGameobjects

      if ( t < 0.5f )
      {
        if (lastReadFrame_ != prevFrame)
        {
          ReadEvents(ref bCursor1_);
          lastReadFrame_ = prevFrame;
        }
      }
      else
      {
        if (lastReadFrame_ != nextFrame)
        {
          ReadEvents(ref bCursor2_);
          lastReadFrame_ = nextFrame;
        }   
      }
  
      lastReadFloatFrame_ = frame;
    }


    private void AdvanceCursorIfExists(long offsetBytesSize, bool exists)
    {
      if (exists)
      {
        AdvanceCursor(offsetBytesSize, ref bCursor1_);
      }
    }


    private void AdvanceCursorsIfExists(long offsetBytesSize, bool existPrev, bool existNext )
    {
      if (existPrev)
      {
        AdvanceCursor(offsetBytesSize, ref bCursor1_);
      }
      if (existNext)
      {
        AdvanceCursor(offsetBytesSize, ref bCursor2_);
      }
    }

    private void ReadRQ( Transform tr, ref int cursor )
    {
      Vector3 r1;
      Quaternion q1;

      r1.x = ReadSingle(ref cursor);
      r1.y = ReadSingle(ref cursor);
      r1.z = ReadSingle(ref cursor);

      q1.x = ReadSingle(ref cursor);
      q1.y = ReadSingle(ref cursor);
      q1.z = ReadSingle(ref cursor);
      q1.w = ReadSingle(ref cursor);

      tr.localPosition = r1;
      tr.localRotation = q1;
    }

    private void ReadRQ( float t, Transform tr, ref int cursor1, ref int cursor2 )
    {
        Vector3 r1;
        Quaternion q1;

        r1.x = ReadSingle(ref cursor1);
        r1.y = ReadSingle(ref cursor1);
        r1.z = ReadSingle(ref cursor1);

        q1.x = ReadSingle(ref cursor1);
        q1.y = ReadSingle(ref cursor1);
        q1.z = ReadSingle(ref cursor1);
        q1.w = ReadSingle(ref cursor1);

        Vector3    r2;
        Quaternion q2;

        r2.x = ReadSingle(ref cursor2);
        r2.y = ReadSingle(ref cursor2);
        r2.z = ReadSingle(ref cursor2);

        q2.x = ReadSingle(ref cursor2);
        q2.y = ReadSingle(ref cursor2);
        q2.z = ReadSingle(ref cursor2);
        q2.w = ReadSingle(ref cursor2);

        tr.localPosition = Vector3.LerpUnclamped(r1, r2, t);
        tr.localRotation = Quaternion.SlerpUnclamped(q1, q2, t);
    }

    private void ReadMeshVertices(Mesh mesh, int cacheIdx, int vertexCount, ref int cursor)
    {
      Vector3 boundsMin;
      boundsMin.x = ReadSingle(ref cursor);
      boundsMin.y = ReadSingle(ref cursor);
      boundsMin.z = ReadSingle(ref cursor);

      Vector3 boundsMax;
      boundsMax.x = ReadSingle(ref cursor);
      boundsMax.y = ReadSingle(ref cursor);
      boundsMax.z = ReadSingle(ref cursor);

      Vector3[] vector3cache = arrVertex3Cache_[cacheIdx];
      Vector3 boundsSize = (boundsMax - boundsMin) * (posQuantz);
      
      if (sbCompression_)
      {
        for (int v = 0; v < vertexCount; v++)
        {
          vector3cache[v].x = boundsMin.x + ((float)ReadUInt16(ref cursor) * boundsSize.x);
          vector3cache[v].y = boundsMin.y + ((float)ReadUInt16(ref cursor) * boundsSize.y);
          vector3cache[v].z = boundsMin.z + ((float)ReadUInt16(ref cursor) * boundsSize.z);
        }

        mesh.vertices = vector3cache;

        for (int v = 0; v < vertexCount; v++)
        {
          vector3cache[v].x = (float)(ReadSByte(ref cursor)) * vecQuantz;
          vector3cache[v].y = (float)(ReadSByte(ref cursor)) * vecQuantz;
          vector3cache[v].z = (float)(ReadSByte(ref cursor)) * vecQuantz;
        }

        mesh.normals  = vector3cache;

        if (sbTangents_)
        {
          Vector4[] vector4cache = arrVertex4Cache_[cacheIdx];

          for (int v = 0; v < vertexCount; v++)
          {
            vector4cache[v].x = (float)(ReadSByte(ref cursor)) * vecQuantz;
            vector4cache[v].y = (float)(ReadSByte(ref cursor)) * vecQuantz;
            vector4cache[v].z = (float)(ReadSByte(ref cursor)) * vecQuantz;
            vector4cache[v].w = (float)(ReadSByte(ref cursor)) * vecQuantz;
          }

          mesh.tangents = vector4cache;
        }
      }
      else
      {
        for (int v = 0; v < vertexCount; v++)
        {
          vector3cache[v].x = ReadSingle(ref cursor);
          vector3cache[v].y = ReadSingle(ref cursor);
          vector3cache[v].z = ReadSingle(ref cursor);
        }

        mesh.vertices = vector3cache;

        for (int v = 0; v < vertexCount; v++)
        {
          vector3cache[v].x = ReadSingle(ref cursor);
          vector3cache[v].y = ReadSingle(ref cursor);
          vector3cache[v].z = ReadSingle(ref cursor);
        }

        mesh.normals = vector3cache;

        if (sbTangents_)
        {
          Vector4[] vector4cache = arrVertex4Cache_[cacheIdx];

          for (int v = 0; v < vertexCount; v++)
          {
            vector4cache[v].x = ReadSingle(ref cursor);
            vector4cache[v].y = ReadSingle(ref cursor);
            vector4cache[v].z = ReadSingle(ref cursor);
            vector4cache[v].w = ReadSingle(ref cursor);
          }

          mesh.tangents = vector4cache;
        }
      }

      Bounds bounds = new Bounds();
      bounds.SetMinMax(boundsMin, boundsMax);
      mesh.bounds = bounds;
    }

    private void ReadMeshVertices(float t, Mesh mesh, int cacheIdx, int vertexCount, ref int cursor1, ref int cursor2)
    {
      Vector3[] vector3cache = arrVertex3Cache_[cacheIdx];

      Vector3 boundsMin1;
      boundsMin1.x = ReadSingle(ref cursor1);
      boundsMin1.y = ReadSingle(ref cursor1);
      boundsMin1.z = ReadSingle(ref cursor1);

      Vector3 boundsMax1;
      boundsMax1.x = ReadSingle(ref cursor1);
      boundsMax1.y = ReadSingle(ref cursor1);
      boundsMax1.z = ReadSingle(ref cursor1);

      Vector3 boundsSize1 = (boundsMax1 - boundsMin1) * (posQuantz);

      Vector3 boundsMin2;
      boundsMin2.x = ReadSingle(ref cursor2);
      boundsMin2.y = ReadSingle(ref cursor2);
      boundsMin2.z = ReadSingle(ref cursor2);

      Vector3 boundsMax2;
      boundsMax2.x = ReadSingle(ref cursor2);
      boundsMax2.y = ReadSingle(ref cursor2);
      boundsMax2.z = ReadSingle(ref cursor2);
 
      Vector3 boundsSize2 = (boundsMax2 - boundsMin2) * (posQuantz);

      Vector3 v3_1;
      Vector3 v3_2;

      if (sbCompression_)
      {
        for (int v = 0; v < vertexCount; v++)
        {           
          v3_1.x = boundsMin1.x + ((float)ReadUInt16(ref cursor1) * boundsSize1.x);
          v3_1.y = boundsMin1.y + ((float)ReadUInt16(ref cursor1) * boundsSize1.y);
          v3_1.z = boundsMin1.z + ((float)ReadUInt16(ref cursor1) * boundsSize1.z);
        
          v3_2.x = boundsMin2.x + ((float)ReadUInt16(ref cursor2) * boundsSize2.x);
          v3_2.y = boundsMin2.y + ((float)ReadUInt16(ref cursor2) * boundsSize2.y);
          v3_2.z = boundsMin2.z + ((float)ReadUInt16(ref cursor2) * boundsSize2.z);

          vector3cache[v] = Vector3.LerpUnclamped(v3_1, v3_2 , t);
        }

        mesh.vertices = vector3cache;

        for (int v = 0; v < vertexCount; v++)
        {
          v3_1.x = (float)(ReadSByte(ref cursor1)) * vecQuantz;
          v3_1.y = (float)(ReadSByte(ref cursor1)) * vecQuantz;
          v3_1.z = (float)(ReadSByte(ref cursor1)) * vecQuantz;

          v3_2.x = (float)(ReadSByte(ref cursor2)) * vecQuantz;
          v3_2.y = (float)(ReadSByte(ref cursor2)) * vecQuantz;
          v3_2.z = (float)(ReadSByte(ref cursor2)) * vecQuantz;

          vector3cache[v] = Vector3.LerpUnclamped(v3_1, v3_2, t); 
        }

        mesh.normals = vector3cache;

        if (sbTangents_)
        {
          Vector4 v4_1;
          Vector4 v4_2;
          Vector4[] vector4cache = arrVertex4Cache_[cacheIdx];

          for (int v = 0; v < vertexCount; v++)
          {
            v4_1.x = (float)(ReadSByte(ref cursor1)) * vecQuantz;
            v4_1.y = (float)(ReadSByte(ref cursor1)) * vecQuantz;
            v4_1.z = (float)(ReadSByte(ref cursor1)) * vecQuantz;
            v4_1.w = (float)(ReadSByte(ref cursor1)) * vecQuantz;

            v4_2.x = (float)(ReadSByte(ref cursor2)) * vecQuantz;
            v4_2.y = (float)(ReadSByte(ref cursor2)) * vecQuantz;
            v4_2.z = (float)(ReadSByte(ref cursor2)) * vecQuantz;
            v4_2.w = (float)(ReadSByte(ref cursor2)) * vecQuantz;

            vector4cache[v] = Vector4.LerpUnclamped(v4_1, v4_2, t);
          }

          mesh.tangents = vector4cache;
        }
      }
      else
      {
        for (int v = 0; v < vertexCount; v++)
        {
          v3_1.x = ReadSingle(ref cursor1);
          v3_1.y = ReadSingle(ref cursor1);
          v3_1.z = ReadSingle(ref cursor1);

          v3_2.x = ReadSingle(ref cursor2);
          v3_2.y = ReadSingle(ref cursor2);
          v3_2.z = ReadSingle(ref cursor2);

          vector3cache[v] = Vector3.LerpUnclamped(v3_1, v3_2, t);
        }

        mesh.vertices = vector3cache;

        for (int v = 0; v < vertexCount; v++)
        {
          v3_1.x = ReadSingle(ref cursor1);
          v3_1.y = ReadSingle(ref cursor1);
          v3_1.z = ReadSingle(ref cursor1);

          v3_2.x = ReadSingle(ref cursor2);
          v3_2.y = ReadSingle(ref cursor2);
          v3_2.z = ReadSingle(ref cursor2);

          vector3cache[v] = Vector3.LerpUnclamped(v3_1, v3_2, t);
        }

        mesh.normals = vector3cache;

        if (sbTangents_)
        {
          Vector4 v4_1;
          Vector4 v4_2;
          Vector4[] vector4cache = arrVertex4Cache_[cacheIdx];

          for (int v = 0; v < vertexCount; v++)
          {

            v4_1.x = ReadSingle(ref cursor1);
            v4_1.y = ReadSingle(ref cursor1);
            v4_1.z = ReadSingle(ref cursor1);
            v4_1.w = ReadSingle(ref cursor1);

            v4_2.x = ReadSingle(ref cursor2);
            v4_2.y = ReadSingle(ref cursor2);
            v4_2.z = ReadSingle(ref cursor2);
            v4_2.w = ReadSingle(ref cursor2);

            vector4cache[v] = Vector4.LerpUnclamped(v4_1, v4_2, t);
          }

          mesh.tangents = vector4cache;
        }
      }

      v3_1 = Vector3.LerpUnclamped(boundsMin1, boundsMin2, t);
      v3_2 = Vector3.LerpUnclamped(boundsMax1, boundsMax2, t);
      Bounds bounds = new Bounds();
      bounds.SetMinMax(v3_1, v3_2);
      mesh.bounds = bounds;
    }

    private void ReadEvents(ref int cursor)
    {
      int nEvents = ReadInt32(ref cursor);
      for (int i = 0; i < nEvents; i++)
      {
        int idEmitter = ReadInt32(ref cursor);
        ceInfo.emitterName_ = arrEmitterName_[idEmitter];

        int idBodyA = ReadInt32(ref cursor);
        int idBodyB = ReadInt32(ref cursor);

        Transform trA = arrGOVC_[idBodyA].First;
        Transform trB = arrGOVC_[idBodyB].First;

        if (trA != null)
        {
          ceInfo.GameObjectA = trA.gameObject;
        }
        else
        {
          ceInfo.GameObjectA = null;
        }

        if (trB != null)
        {
          ceInfo.GameObjectB = trB.gameObject;
        }
        else
        {
          ceInfo.GameObjectB = null;
        }

        Matrix4x4 m_LOCAL_to_WORLD = transform.localToWorldMatrix;

        ceInfo.position_.x = ReadSingle(ref cursor);
        ceInfo.position_.y = ReadSingle(ref cursor);
        ceInfo.position_.z = ReadSingle(ref cursor);

        ceInfo.position_ = m_LOCAL_to_WORLD.MultiplyPoint3x4(ceInfo.position_);

        ceInfo.velocityA_.x = ReadSingle(ref cursor);
        ceInfo.velocityA_.y = ReadSingle(ref cursor);
        ceInfo.velocityA_.x = ReadSingle(ref cursor);     

        ceInfo.velocityA_ = m_LOCAL_to_WORLD.MultiplyVector(ceInfo.velocityA_);

        ceInfo.velocityB_.x = ReadSingle(ref cursor);
        ceInfo.velocityB_.y = ReadSingle(ref cursor);
        ceInfo.velocityB_.x = ReadSingle(ref cursor);

        ceInfo.velocityB_ = m_LOCAL_to_WORLD.MultiplyVector(ceInfo.velocityB_);

        ceInfo.relativeSpeed_N_ = ReadSingle(ref cursor);
        ceInfo.relativeSpeed_T_ = ReadSingle(ref cursor);

        ceInfo.relativeP_N_ = ReadSingle(ref cursor);
        ceInfo.relativeP_T_ = ReadSingle(ref cursor);

        collisionEvent.Invoke(ceInfo);
      }
    }

/*
    private void ReadMeshVerticesUnsafe(Mesh mesh, int cacheIdx, int vertexCount)
    {
      unsafe
      {
        fixed (byte* pAnim = binaryAnim_)
        {
          Vector3 boundsMin;
          boundsMin.x = ( *(float*)(&pAnim[bCursor_]) );
          bCursor_ += sizeof(float);

          boundsMin.y = ( *(float*)(&pAnim[bCursor_]) );
          bCursor_ += sizeof(float);

          boundsMin.z = ( *(float*)(&pAnim[bCursor_]) );
          bCursor_ += sizeof(float);

          Vector3 boundsMax;
          boundsMax.x = ( *(float*)(&pAnim[bCursor_]) );
          bCursor_ += sizeof(float);

          boundsMax.y = ( *(float*)(&pAnim[bCursor_]) );
          bCursor_ += sizeof(float);

          boundsMax.z = ( *(float*)(&pAnim[bCursor_]) );
          bCursor_ += sizeof(float);

          Vector3[] vector3cache = arrVertex3Cache_[cacheIdx];

          Vector3 boundsSize = (boundsMax - boundsMin) * (1.0f / 65535.0f);
          float oo127 = 1.0f / 127.0f;

          if (sbCompression_)
          {
            for (int v = 0; v < vertexCount; v++)
            {
              vector3cache[v].x = boundsMin.x + ( (float)( *(UInt16*)(&pAnim[bCursor_]) ) * boundsSize.x);
              bCursor_ += sizeof(UInt16);
              vector3cache[v].y = boundsMin.y + ( (float)( *(UInt16*)(&pAnim[bCursor_]) ) * boundsSize.y);
              bCursor_ += sizeof(UInt16);
              vector3cache[v].z = boundsMin.z + ( (float)( *(UInt16*)(&pAnim[bCursor_]) ) * boundsSize.z);
              bCursor_ += sizeof(UInt16);
            }

            mesh.vertices = vector3cache;

            for (int v = 0; v < vertexCount; v++)
            {
              vector3cache[v].x = (float)( *(SByte*)(&pAnim[bCursor_]) ) * oo127;
              bCursor_ += sizeof(SByte);
              vector3cache[v].y = (float)( *(SByte*)(&pAnim[bCursor_]) ) * oo127;
              bCursor_ += sizeof(SByte);
              vector3cache[v].z = (float)( *(SByte*)(&pAnim[bCursor_]) ) * oo127;
              bCursor_ += sizeof(SByte);
            }

            mesh.normals = vector3cache;

            if (sbTangents_)
            {
              Vector4[] vector4cache = arrVertex4Cache_[cacheIdx];

              for (int v = 0; v < vertexCount; v++)
              {
                vector4cache[v].x = (float)( *(SByte*)(&pAnim[bCursor_]) ) * oo127;
                bCursor_ += sizeof(SByte);
                vector4cache[v].y = (float)( *(SByte*)(&pAnim[bCursor_]) ) * oo127;
                bCursor_ += sizeof(SByte);
                vector4cache[v].z = (float)( *(SByte*)(&pAnim[bCursor_]) ) * oo127;
                bCursor_ += sizeof(SByte);
                vector4cache[v].w = (float)( *(SByte*)(&pAnim[bCursor_]) ) * oo127;
                bCursor_ += sizeof(SByte);
              }

              mesh.tangents = vector4cache;
            }
          }
          else
          {
            for (int v = 0; v < vertexCount; v++)
            {
              vector3cache[v].x = ReadSingle();
              vector3cache[v].y = ReadSingle();
              vector3cache[v].z = ReadSingle();
            }

            mesh.vertices = vector3cache;

            for (int v = 0; v < vertexCount; v++)
            {
              vector3cache[v].x = ReadSingle();
              vector3cache[v].y = ReadSingle();
              vector3cache[v].z = ReadSingle();
            }

            mesh.normals = vector3cache;

            if (sbTangents_)
            {
              Vector4[] vector4cache = arrVertex4Cache_[cacheIdx];

              for (int v = 0; v < vertexCount; v++)
              {
                vector4cache[v].x = ReadSingle();
                vector4cache[v].y = ReadSingle();
                vector4cache[v].z = ReadSingle();
                vector4cache[v].w = ReadSingle();
              }

              mesh.tangents = vector4cache;
            }
          }

          Bounds bounds = new Bounds();
          bounds.SetMinMax(boundsMin, boundsMax);
          mesh.bounds = bounds;
        }
      }
    }
 */
  }
}
