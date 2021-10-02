﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;
using RocketLibLoadMod;

namespace RocketLib0
{
    public partial class RocketLib
    {
        /// <summary>
        /// On screen logger.
        /// </summary>
        public class ScreenLogger : MonoBehaviour
        {

            /// <summary>
            /// The Id of your mod who will display on the log. Call this variable before calling the Log function. Example :
            /// <code>RocketLib.ScreenLogger.ModId = "ID";</code>
            /// </summary>
            public static string ModId;
            /// <summary>
            /// Know if the ScreenLogger is successfully load.
            /// </summary>
            public static bool isSuccessfullyLoad
            {
                get
                {
                    return _isSuccessfullyLoad;
                }
            }

            internal static bool _isSuccessfullyLoad = false;

            internal static float timeRemaining = (float)Main.settings.logTimer;

            internal static GUIStyle LogStyle = new GUIStyle();

            internal static List<string> logList = new List<string>();
            internal static Dictionary<string, RLogType> logDico = new Dictionary<string, RLogType>();
            internal static List<string> logListForTxt = new List<string>();

            internal static int nbrUmmLog = 0;

            private static string LogFilePath = Main.mod.Path + "\\Log.txt";

            private static bool getFirstLaunch;
            internal static List<string> StartLog = new List<string>();
            private static float timeFirstLaunch = 5;

            internal static bool Load()
            {
                try
                {
                    new GameObject(typeof(ScreenLogger).FullName, typeof(ScreenLogger));
                    AddStartLog("RocketLib ScreenLogger successfully Loaded !\n");
                    return true;
                }
                catch (Exception ex)
                {
                    Main.Log(ex);
                }

                return false;
            }

            internal static void AddStartLog(string str)
            {
                str = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + str;
                StartLog.Add(str);
                logListForTxt.Add(str);
            }

            /// <summary>
            /// Clear the log on screen.
            /// </summary>
            public static void Clear()
            {
                logList = new List<string>();
                StartLog.Clear();
                getFirstLaunch = true;
            }
            /*
            /// <summary>
            /// Add log to the screen.
            /// </summary>
            /// <param name="str">Log Message</param>
            public static void Log(object str)
            {
                Log(str, RLogType.Log);
            }*/

            /// <summary>
            /// Add log to the screen.
            /// </summary>
            /// <param name="str">Log Message</param>
            /// <param name="type">RLogType</param>
            public static void Log(object str, RLogType type = RLogType.Log)
            {
                if (!isSuccessfullyLoad) return;
                string dateTimeNow = DateTime.Now.ToString("HH:mm:ss");
                string newString = $"[{dateTimeNow}] [{ModId}] [" + type.ToString() + "] : " + str.ToString();

                if (type == RLogType.Log)
                    newString = $"[{ModId}] " + (string)str;

                logList.Add("\n" + newString);
                logListForTxt.Add(newString);
            }

            void OnDestroy()
            {
                Main.Log("ScreenLogger DESTROY");
            }

            void Awake()
            {
                DontDestroyOnLoad(this);
                ClearTxtLog();
            }

            void Start()
            {
                /*ModId = "RocketLibTest";
                Log("TEST Log", RLogType.Log);
                Log("TEST Warning", RLogType.Warning);
                Log("TEST ERROR", RLogType.Error);
                Log("TEST Exception", RLogType.Exception);
                Log("TEST Information", RLogType.Information);*/
            }

            void Update()
            {
                if (Input.GetKeyDown(KeyCode.F3)) Clear();

                if (logList.Count > 0)
                {
                    if (timeRemaining > 0)
                    {
                        timeRemaining -= Time.deltaTime;
                    }
                    else
                    {
                        logList.Remove(logList.First());
                        timeRemaining = Main.settings.logTimer;
                    }
                }
                if (!getFirstLaunch)
                {
                    if (timeFirstLaunch > 0) timeFirstLaunch -= Time.deltaTime;
                    else
                    {
                        StartLog.Clear();
                        getFirstLaunch = true;
                    }
                }
                if (logList.Count > 30) logList.Remove(logList.First());
                CheckUMMLog();
                WriteLogTXT();
            }


            void OnGUI()
            {
                if (!isSuccessfullyLoad) return;
                if (Main.settings.OnScreenLog && (logList.Count > 0 || StartLog.Count > 0))
                {
                    GUILayout.BeginVertical("box");
                    if (!getFirstLaunch)
                    {
                        foreach (string msg in StartLog)
                        {
                            LogStyle.normal.textColor = Color.green;
                            GUILayout.Label(msg, LogStyle);
                        }
                    }

                    foreach (string log in logList)
                    {
                        WhichColor(log);
                        GUILayout.Label(log, LogStyle);
                    }
                    GUILayout.EndVertical();
                }
            }

            void WhichColor(string LogMsg)
            {
                LogMsg = LogMsg.ToLower();
                if (LogMsg.Contains("error") || LogMsg.Contains("exception"))
                {
                    LogStyle.normal.textColor = Color.red; return;
                }
                else if (LogMsg.Contains("warning"))
                {
                    LogStyle.normal.textColor = Color.yellow; return;
                }
                else if (LogMsg.Contains("[information]"))
                {
                    LogStyle.normal.textColor = Color.blue; return;
                }
                else if(LogMsg.Contains("successful loaded"))
                {
                    LogStyle.normal.textColor = Color.green; return;
                }
                else
                {
                    LogStyle.normal.textColor = Color.white; return;
                }
            }

            void CheckUMMLog()
            {
                List<string> ummLog = Traverse.Create(typeof(UnityModManager.Logger)).Field("history").GetValue<List<string>>();
                if (ummLog.Count > nbrUmmLog)
                {
                    if(Main.settings.ShowManagerLog)
                    {
                        for (var str = nbrUmmLog; str < ummLog.Count; str++)
                        {
                                string dateTimeNow = DateTime.Now.ToString("HH:mm:ss");
                                logList.Add("\n[" + dateTimeNow + "] " + ummLog[str]);
                        }
                    }
                    nbrUmmLog = ummLog.Count;
                }
            }
            void WriteLogTXT()
            {
                try
                {
                    if (!File.Exists(LogFilePath))
                    {
                        using (File.Create(LogFilePath)); // Ignore warning.
                    }
                    if (logListForTxt.Count > 0)
                    {
                        using (StreamWriter writer = File.AppendText(LogFilePath))
                        {
                            foreach (var str in logListForTxt)
                            {
                                writer.WriteLine(str);
                            }
                        }
                        logListForTxt = new List<string>();
                    }
                }
                catch(Exception ex)
                {
                    Main.Log(ex);
                }
            }
            void ClearTxtLog()
            {
                if (File.Exists(LogFilePath))
                {
                    try
                    {
                        File.Delete(LogFilePath);
                    }
                    catch (Exception ex)
                    {
                        Main.Log(ex);
                    }
                }
            }
        }
    }
}

/// <summary>
/// Type of log for the log. They each have a "custom" color.
/// </summary>
public enum RLogType
{
    /// <summary>
    /// White
    /// </summary>
    Log,
    /// <summary>
    /// Yellow
    /// </summary>
    Warning,
    /// <summary>
    /// Red
    /// </summary>
    Error,
    /// <summary>
    /// Red
    /// </summary>
    Exception,
    /// <summary>
    /// Blue
    /// </summary>
    Information
}
