using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Concept2api;
using CSAFE_Fitness;

public class RowerCore : MonoBehaviour
{
    private int RowerID = 0;

    private PerformanceMonitor _PM;
    private CSAFE_Device _Rower;
    private CSAFE_ConfigurationData _Data;

    public MachineState RowerState
    {
        get
        {
            if (_PM == null) return MachineState.Error;
            if (_PM.Session == null) return MachineState.Error;
            if (_PM.Session.Devices[RowerID] == null) return MachineState.Error;
            if (_PM.Session.Devices[RowerID].ConfigData == null) return MachineState.Error;

            return _PM.Session.Devices[RowerID].ConfigData.SlaveState;
        }
    }

    public int WorkoutDistanceMeters
    {
        get
        {
            if (_PM == null) return -1;
            if (_PM.Session == null) return -1;
            if (_PM.Session.Devices[RowerID] == null) return -1;
            if (_PM.Session.Devices[RowerID].ConfigData == null) return -1;

            return _PM.Session.Devices[RowerID].ConfigData.WorkoutDistanceMeters;
        }
    }
    public int WorkoutDurationSeconds { get { return _PM.Session.Devices[RowerID].ConfigData.WorkoutTotalSeconds; } }
    public int CadenceSPM { get { return _PM.Session.Devices[RowerID].ConfigData.Cadence; } }
    public int CaloriesCal { get { return _PM.Session.Devices[RowerID].ConfigData.Calories; } }
    public int HeartRateBPM { get { return _PM.Session.Devices[RowerID].ConfigData.HeartRateBPM; } }
    public int PowerWatts { get { return _PM.Session.Devices[RowerID].ConfigData.PowerWatts; } }

    private int _PollCount = 0;
    public int PollCount { get { return _PollCount; } }

    ~RowerCore()
    {
        Debug.Log("Destroying Rower Core...");

        _PM.CloseStream(-1);

        Debug.Log("Rower Core Destroyed!");
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializePMSession();
        //LoadWorkout();
        //StartWorkout();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializePMSession()
    {
        Debug.Log("Initializing PerformanceMonitor Session");

        _PM = new PerformanceMonitor(true);

        int cntStreams = _PM.OpenStream(0);
        Debug.Log("Opened " + cntStreams.ToString() + " Performance Monitor Streams.");

        // RESET EVERYTHING
        _PM.C2_EnvironmentReset(0);
        Debug.Log(_PM.Session.Devices[0].ConfigData.ToString());

        // GET MACHINE STATUS
        if (_PM.C2_GetMachineStatus(0)) Debug.Log("Executed Command C2_GetMachineStatus");
        Debug.Log(_PM.Session.Devices[0].ConfigData.ToString());
    }

    public void LoadWorkout()
    {
        Debug.Log("Loading a Workout");

        // CREATE A WORKOUT
        if (_PM.C2_SetHorizontalDistanceGoal(100)) Debug.Log("Added Command C2_SetHorizontalDistanceGoal");
        if (_PM.C2_SetSplitDuration(C2TimeDistance.DistanceMeters, 100)) Debug.Log("Added Command C2_SetSplitDuration");
        if (_PM.C2_SetPowerGoal(100)) Debug.Log("Added Command C2_SetPowerGoal");
        if (_PM.C2_SetProgram(0)) Debug.Log("Added Command C2_SetProgram");
        if (_PM.CSAFE_CtrlCmds.cmdGoIdle()) Debug.Log("Added Command cmdGoIdle");
        _PM.ExecuteCommands(0);
        Debug.Log(_PM.Session.Devices[0].ConfigData.ToString());
    }

    public void StartWorkout()
    {
        // START A WORKOUT
        if (_PM.CSAFE_CtrlCmds.cmdGoInUse()) Debug.Log("Added Command cmdGoInUse");
        _PM.ExecuteCommands(0);
        Debug.Log(_PM.Session.Devices[0].ConfigData.ToString());

        _PollCount = 0;
        InvokeRepeating("PollMonitorData", 0f, 0.100f);
    }

    public void EndWorkoutPolling()
    {
        CancelInvoke("PollMonitorData");
    }

    void PollMonitorData()
    {
        _PM.C2_GetMonitorData(0);
        _PollCount++;
    }

}
