using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MidiJack
{
    public class MidiDroid
    {

        List<string> deviceNames;

        // Android Glue
        AndroidJavaClass _class;
        AndroidJavaObject mdPlugin { get { return _class.GetStatic<AndroidJavaObject>("instance"); } }
        public MidiDroidCallback callback;

        int currentDevice = -1;

        public void Start()

        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _class = new AndroidJavaClass("mmmlabs.com.mididroid.MidiDroid");
            _class.CallStatic("start");
#endif
            callback = new MidiDroidCallback();
            mdPlugin.Call("setMidiCallback", callback);
            mdPlugin.Call("findADevice");
            /*
            string currentDeviceName = UnityEngine.PlayerPrefs.GetString("MidiDroidDevice", "");
            if(currentDeviceName.Length > 0)
            {
                int index = IndexOfDeviceNamed(currentDeviceName);
                if(index > -1)
                {
                    TryOpenDeviceAt(index);
                }
            }
            else
            {
                TryOpenNextDevice();
            }
            */
        //    TryOpenDeviceAt(2);
        }

        public void SetCallback(MidiDroidCallback callback)
        {
            mdPlugin.Call("setMidiCallback", callback);
        }

        #region MidiJack Methods
        public MidiMessage GetNextMessage()
        {
            MidiMessage m = new MidiMessage(0);
            AndroidJavaObject obj = mdPlugin.Call<AndroidJavaObject>("getIncoming");
            if (obj.GetRawObject().ToInt32() != 0)
            {
                // byte[] returned with some data!
                byte[][] result = AndroidJNIHelper.ConvertFromJNIArray<byte[][]>(obj.GetRawObject());
                if(result.Length == 0)
                {
                    /*
                    // return empty message
                    m.source = 999;
                    m.status = 0;
                    m.data1 = 0;
                    m.data2 = 0;
                    */
                }
                else
                {
                    Debug.LogFormat("Unity Got {0} messages", result.Length);
                    for (int i = 0; i < result.Length; i++)
                    {
                        Debug.LogFormat("Messages {0} is {1} {2} {3}", i, result[i][0], result[i][1], result[i][2]);
                    }
                    /*
                    m.source = (uint)currentDevice;
                    m.status = result[0];
                    m.data1 = result[1];
                    m.data2 = result[2];
                    */
                }
            }
            else
            {
                m.source = 999;
                Debug.LogError("Couldn't parse returned Java Object");
            }
            return m;
        }

        public ulong DequeueIncomingData()
        {
            
            if(mdPlugin != null)
            {
                AndroidJavaObject obj = mdPlugin.Call<AndroidJavaObject>("getIncoming");
                if (obj.GetRawObject().ToInt64() != 0)
                {
                    return (ulong)obj.GetRawObject().ToInt64();
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
            
            return 0;
        }

        public int MidiJackCountEndpoints()
        {
            return 0;
        }

        public uint GetEndpointIdAtIndex(int index)
        {
            return 0;
        }

        public string GetEndpointName(uint id)
        {
            return "";
        }
        #endregion

        private int IndexOfDeviceNamed(string midiDroidDevice)
        {
            int result = -1;
            getDeviceList();
            for (int i = 0; i < deviceNames.Count; i++)
            {
                if(deviceNames[i] == midiDroidDevice)
                {
                    result = i;
                }
            }
            return result;
        }

        public void TryOpenDeviceAt(int deviceIndex)
        {
            mdPlugin.Call("openDeviceAtIndex", deviceIndex);
            currentDevice = deviceIndex;
        //    UnityEngine.PlayerPrefs.SetString("MidiDroidDevice", deviceNames[deviceIndex]);
        }

        public void TryOpenNextDevice()
        {
            getDeviceList();

            if (deviceNames.Count == 0)
            {
                throw new Exception("No Devices available to open");
            }

            int nextDevice = currentDevice + 1;
            if(nextDevice >= deviceNames.Count)
            {
                nextDevice = 0;
            }

            TryOpenDeviceAt(nextDevice);
        }

        public void getDeviceList()
        {
            deviceNames = new List<string>();

            //some methods to set the object that you want to call the method on
            AndroidJavaObject obj = mdPlugin.Call<AndroidJavaObject>("getDevices");
            if (obj.GetRawObject().ToInt32() != 0)
            {
                // String[] returned with some data!
                System.String[] result = AndroidJNIHelper.ConvertFromJNIArray<System.String[]>
                                      (obj.GetRawObject());
                foreach (System.String str in result)
                {
                    // Do something with the strings
                    deviceNames.Add(str);
                }
            }
            else
            {
                Debug.LogErrorFormat("Got null strings back from getDevices");
                // null String[] returned
            }
            obj.Dispose();
        }
    }

}