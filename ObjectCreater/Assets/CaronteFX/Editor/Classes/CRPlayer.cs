using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public class CRPlayer
  {
    CNManager       manager_;
    CREntityManager entityManager_;

    int frame_;
    public int Frame
    {
      get
      {
        return frame_;
      }
    }

    int fillFrame_;
    public int FillFrame
    {
      get
      {
        return fillFrame_;
      }
    }

    int maxFrames_;
    public int MaxFrames
    {
      get
      {
        return maxFrames_;
      }
    }

    Color colorBackground_;
    public Color ColorBackground
    {
      get
      {
        return colorBackground_;
      }
    }

    public string MaxFramesString
    {
      get;
      set;
    }

    public string MaxTimeString
    {
      get;
      set;
    }

    double  frameTime_;
    public float FrameTime
    {
      get
      {
        return (float) frameTime_;
      }
    }

    int fps_;
    public int FPS
    {
      get
      {
        return fps_;
      }
    }

    public float Time
    {
      get
      {
        return frame_ * (float)frameTime_;
      }
    }

    public float MaxTime
    {
      get
      {
        return maxFrames_ * (float)frameTime_; 
      }
    }

        
    bool loop_ = true;
    public bool Loop
    {
      get
      {
        return loop_;
      }
      set
      {
        loop_ = value;
      }
    }

    bool userPlaying_ = false;
    public bool UserPlaying
    {
      get
      {
        return userPlaying_;
      }
      set
      {
        userPlaying_ = value;
      }
    }

    bool stopRequested_ = false;
    public bool StopRequested
    {
      get
      {
        return stopRequested_;
      }
    }

    bool changeToEditModeRequested_ = false;
    public bool ChangeToEditModeRequested
    {
      get
      {
        return changeToEditModeRequested_;
      }
    }
   
    double lastSimulatingTime_;
    double frameSpentTime_;

    const float nIterations_ = 16.0f;

    float frameTimeEstimation_;
    float iterationTimeEstimation_;
    
    int currentIteration_ = 0;
    public int CurrentIteration
    {
      get 
      {
        if ( SimulationManager.IsSimulating() )
        {
          return Mathf.Clamp(currentIteration_, 0, 15);
        }
        else
        {
          return ( (int)(nIterations_ - 1.0) );
        }
      }    
    }  

    UN_TimeSliderState timeSliderState_ = new UN_TimeSliderState();
 
    public bool IsPause      { get { return SimulationManager.IsPause(); } }
    public bool IsEditing    { get { return SimulationManager.IsEditing(); } }
    public bool IsSimulating { get { return SimulationManager.IsSimulating(); } }
    public bool IsReplaying  { get { return SimulationManager.IsReplaying(); } }

    public enum Status
    {
      EDITING    = 0,
      SIMULATING = 1,
      REPLAYING  = 2
    }

    public Status CurrentStatus
    {
      get
      {
        if ( IsEditing )
        {
          return Status.EDITING;
        }
        else if ( IsSimulating )
        {
          return Status.SIMULATING;
        }
        else
        {
          return Status.REPLAYING;
        }
      }
    }

    public static string[] stringStatuses_;

    List<CRAnimationData> listAnimationData_ = new List<CRAnimationData>();
    UnityEngine.Mesh      animationBakingMesh_;
    
    public CRPlayer( CNManager manager, CREntityManager entityManager )
    {
      manager_        = manager;
      entityManager_  = entityManager;
      stringStatuses_ = new string[] { "Editing",
                                       "Simulating",
                                       "Replaying"    };
    }
    //-----------------------------------------------------------------------------------
    public void Deinit()
    {
      EditorApplication.update -= UpdateSimulating;
      EditorApplication.update -= UpdateReplaying;
      foreach( CRAnimationData animationData in listAnimationData_)
      {
        animationData.Reset();
      }
      listAnimationData_.Clear();
      stringStatuses_ = new string[] { "Editing",
                                       "Simulating",
                                       "Replaying"    };

      stopRequested_             = false;
      changeToEditModeRequested_ = false;
    }
    //-----------------------------------------------------------------------------------
    public void CycleStatus()
    {
      if (IsSimulating)
      {
        string statusString = GetStatusString();

        if ( stopRequested_ )
        {
          if ( statusString == "Stopping" )
          {
            stringStatuses_[(int)Status.SIMULATING] = "Stopping.";
          }
          else if ( statusString == "Stopping.")
          {
            stringStatuses_[(int)Status.SIMULATING] = "Stopping..";
          }
          else if ( statusString == "Stopping..")
          {
            stringStatuses_[(int)Status.SIMULATING] = "Stopping...";
          }
          else if ( statusString == "Stopping...")
          {
            stringStatuses_[(int)Status.SIMULATING] = "Stopping";
          }
        }
        else
        {
          if ( statusString == "Simulating" )
          {
            stringStatuses_[(int)Status.SIMULATING] = "Simulating.";
          }
          else if ( statusString == "Simulating.")
          {
            stringStatuses_[(int)Status.SIMULATING] = "Simulating..";
          }
          else if ( statusString == "Simulating..")
          {
            stringStatuses_[(int)Status.SIMULATING] = "Simulating...";
          }
          else if ( statusString == "Simulating...")
          {
            stringStatuses_[(int)Status.SIMULATING] = "Simulating";
          }
        }

      }
    }
    //-----------------------------------------------------------------------------------
    public void UpdateSimulating()
    {
      if ( SimulationManager.IsReplaying() )
      {
        ChangeToReplayingModeDone();
      }

      UpdateTimeSlider();
    }
    //-----------------------------------------------------------------------------------
    public void UpdateReplaying()
    {
      UpdateTimeSlider();

      if (changeToEditModeRequested_)
      {
        ChangeToEditMode();
      }
      
      if (IsPause && userPlaying_ )
      {
        if (loop_)
        {
          play();
        }
        else
        {
          userPlaying_ = false;
        }
      }
    }
    //-----------------------------------------------------------------------------------
    private void SampleAnimationDataSimulating(double eventTime, CRAnimationData animData)
    {
      float startTime = animData.timeStart_;
      float length    = animData.timeLenght_;
      float endTime   = startTime + length;
      

      if ( eventTime >= startTime && 
          ( length == float.MaxValue || ( eventTime <= endTime ) ) )
      {   
        animData.UpdateSimulating( animationBakingMesh_, (float)eventTime, (float)frameTime_ );  
      }
    }
    //-----------------------------------------------------------------------------------
    public void UpdateTimeSlider()
    {
      double simulatingCurrentTime = EditorApplication.timeSinceStartup;

      double elapsedTime = simulatingCurrentTime - lastSimulatingTime_;
      lastSimulatingTime_ = simulatingCurrentTime;

      frameSpentTime_ += elapsedTime;

      currentIteration_ = (int)(frameSpentTime_ / iterationTimeEstimation_);

      if ( CRPlayerWindow.IsOpen && SimulationManager.IsTimeSliderUpdateRequired() )
      {
        SimulationManager.UpdateTimeSliderState(timeSliderState_);
        EstimateFrameTimeAndIterations();
 
        frame_            = timeSliderState_.frame_;
        fillFrame_        = timeSliderState_.fillFrame_;
        maxFrames_        = timeSliderState_.maxFrames_;
        colorBackground_  = timeSliderState_.colorBackground_;

        MaxFramesString = maxFrames_.ToString();
        MaxTimeString   = (maxFrames_ * (float)frameTime_).ToString("F2");
      }
      CRPlayerWindow.RepaintIfOpen();
    }
    //-----------------------------------------------------------------------------------
    private void EstimateFrameTimeAndIterations()
    {
      if ( frame_ < timeSliderState_.frame_ )
      {
        int nFrames = timeSliderState_.frame_ - frame_;

        frameTimeEstimation_     = (float)(frameSpentTime_ / nFrames);
        iterationTimeEstimation_ = frameTimeEstimation_ / nIterations_;

        frameSpentTime_ = 0;
      }  
    }
    //-----------------------------------------------------------------------------------
    public void frw()
    {
      pause();
      SimulationManager.SetReplayingFrame(0, false); 
    }
    //-----------------------------------------------------------------------------------
    public void rw()
    {
      pause();
      if (frame_ > 0)
      {
        SimulationManager.SetReplayingFrame((uint)frame_ - 1, false);
      }
    }
    //-----------------------------------------------------------------------------------
    public void play()
    {
      if (frame_ == maxFrames_)
      {
        SimulationManager.SetReplayingFrame((uint)0, false);
      }
      userPlaying_ = true;
      SimulationManager.PauseOff();
    }
    //-----------------------------------------------------------------------------------
    public void pause()
    { 
      if (!IsPause)
      {
        SimulationManager.PauseOn();
        userPlaying_     = false;
      }
    }
    //-----------------------------------------------------------------------------------
    public void stop()
    { 
      stopRequested_ = true;
      stringStatuses_[(int)Status.SIMULATING] = "Stopping";

      pause();
      ChangeToReplayingModeRequest();
    }
    //-----------------------------------------------------------------------------------
    public void ResetUserPlaying()
    {
      if (!IsPause)
      {
        if (Loop)
        {
           userPlaying_ = true;
        }
        else
        {
          userPlaying_ = false;
        }
      }
    }
    //-----------------------------------------------------------------------------------
    private void ChangeToReplayingModeRequest()
    {
      SimulationManager.ChangeToReplayingRq();
    }
    //-----------------------------------------------------------------------------------
    private void ChangeToReplayingModeDone()
    {
      stopRequested_ = false;
      SimulationManager.ChangeToReplayingDone();

      manager_.BuildBakerData();

      EditorApplication.update -= UpdateSimulating;
      EditorApplication.update += UpdateReplaying;

      SimulationManager.SetReplayingFrame( (uint)frame_, false );

      SceneView.RepaintAll();
      CRManagerEditor.RepaintIfOpen();
    }
    //-----------------------------------------------------------------------------------
    public void fw()
    {
      pause();
      if (frame_ < maxFrames_)
      {
        SimulationManager.SetReplayingFrame( (uint)frame_ + 1, false );
      }
    }
    //-----------------------------------------------------------------------------------
    public void ffw()
    {
      pause();
      SimulationManager.SetReplayingFrame((uint)maxFrames_, false);
    }
    //-----------------------------------------------------------------------------------
    public void SetFrame(int frameIdx, bool waitCommandsDone)
    {
      if (Frame != frameIdx)
      {
        pause();
        SimulationManager.SetReplayingFrame((uint)frameIdx, waitCommandsDone);
      }
    }
    //-----------------------------------------------------------------------------------
    public bool ChangeToEditModeRequest()
    {
      bool resetDialog = EditorUtility.DisplayDialog("CaronteFX - Reset simulation", "Changing to edit mode will reset the current simulution. You won't be able to bake it at later time.", "Ok", "Cancel");
      if (resetDialog)
      {
        if (IsSimulating)
        {
          stop();
        }
        else if (IsReplaying)
        {
          pause();
        }
        changeToEditModeRequested_ = true;
      }
      return resetDialog;
    }
    //-----------------------------------------------------------------------------------
    private void ChangeToEditMode()
    {
      CRPlayerWindow.InstanceWillClose();
      PrepareToRestartSimulation();
    }
    //-----------------------------------------------------------------------------------
    private void PrepareToRestartSimulation()
    {
      SimulationManager.SetBroadcastMode(UN_BROADCAST_MODE.EDITING);

      SimulationManager.EditingBegin();     
      SimulationManager.RequestDisplay();   
      
      CRBakeSimulationMenu.CloseIfOpen();
      CRBakeFrameMenu     .CloseIfOpen();

      Deinit();
      manager_.PrepareToRestartSimulation();
    }
    //-----------------------------------------------------------------------------------
    public UN_SimulationProperties SimulatingBeginFirst( SimulationParams simParams )
    {     
      SceneView.RepaintAll();

      fps_          = (int)simParams.fps_;
      frameTime_    = 1.0 / simParams.fps_;

      BufferAnimations(simParams.totalTime_, simParams.fps_);

      SimulationManager.ResetTimeSlider();
      SimulationManager.EditingEnd();
   
      SimulationManager.SetBroadcastMode(UN_BROADCAST_MODE.SIMULATING);
      SimulationManager.SimulatingBeginFirst( simParams );

      UN_SimulationProperties un_simProperties = new UN_SimulationProperties();;
      SimulationManager.GetSimulationProperties( un_simProperties );

      entityManager_.DisableEditingObjects();
   
      lastSimulatingTime_  = EditorApplication.timeSinceStartup;
      
      frameSpentTime_      = 0f;
      frameTimeEstimation_ = float.MaxValue;

      iterationTimeEstimation_ = frameTimeEstimation_ / nIterations_;

      EditorApplication.update -= UpdateSimulating;
      EditorApplication.update += UpdateSimulating;

      SimulationManager.PauseOff();

      return un_simProperties;
    }
    //-----------------------------------------------------------------------------------
    public string GetStatusTitle()
    {
      if (IsEditing)
      {
        return "Editing";
      }
      else if (IsSimulating)
      {
        if (stopRequested_)
        {
          return "Stopping";
        }
        else
        {
          return "Simulating";
        }  
      }
      else
      {
        return "Replaying";
      }
    }
    //-----------------------------------------------------------------------------------
    public string GetStatusString()
    {
      if (IsEditing)
      {
        return stringStatuses_[(int)Status.EDITING];
      }
      else if (IsSimulating)
      {
        return stringStatuses_[(int)Status.SIMULATING];
      }
      else
      {
        return stringStatuses_[(int)Status.REPLAYING];
      }
    }
    //----------------------------------------------------------------------------------
    public void AddAnimation(CNAnimatedbodyEditor animNodeEditor)
    {
      CRAnimationData animData = new CRAnimationData(animNodeEditor);    
      listAnimationData_.Add( animData );  
    }
    //-----------------------------------------------------------------------------------
    private void BufferAnimations(double totalTime, uint fps)
    {    
      bool anyAnimator = entityManager_.CreateAnimatorTmpGameObjects(listAnimationData_);

      if (!anyAnimator)
        return;

      animationBakingMesh_ = new UnityEngine.Mesh();

      double frameTime = 1.0 / fps;
      double currentTime = 0;


      while( currentTime < totalTime )
      {
        float progress        = (float)(currentTime / totalTime);
        float percentage      = progress * 100.0f;
        string progressString = "Progress: " +  percentage.ToString("F2") + "%.";
        EditorUtility.DisplayProgressBar("CaronteFX - Buffering animations", progressString, progress);

        foreach( CRAnimationData animationData in listAnimationData_ )
        {
          SampleAnimationDataSimulating( currentTime, animationData );
        }
        currentTime += frameTime;   
      }
      EditorUtility.ClearProgressBar();

      UnityEngine.Object.DestroyImmediate(animationBakingMesh_);

      entityManager_.DestroyAnimatorTmpGameObjects();
    }
    //----------------------------------------------------------------------------------
  }
}
