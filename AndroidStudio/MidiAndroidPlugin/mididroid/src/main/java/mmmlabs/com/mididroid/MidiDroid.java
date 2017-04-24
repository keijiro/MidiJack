package mmmlabs.com.mididroid;

// Features.
import android.app.Fragment;
import android.content.Context;
import android.media.midi.MidiDeviceInfo;
import android.os.Bundle;

// Unity
import com.example.android.common.midi.MidiConstants;
import com.example.android.common.midi.MidiFramer;
import com.unity3d.player.UnityPlayer;

// Debug
import android.util.Log;

// MIDI
import android.media.midi.MidiManager;
import android.media.midi.MidiDevice;
import android.media.midi.*;

import java.io.IOException;

public class MidiDroid extends Fragment {

    // Constants.
    public static final String TAG = "MidiDROID";

    // Singleton instance.
    public static MidiDroid instance;

    boolean foundDevice = false;

    int deviceIndex;

    MidiCallback midiCallback;

    MidiManager manager;

    // Receiver that parses raw data into complete messages.
    MidiFramer connectFramer = new MidiFramer(new MyReceiver());

    public static void start()
    {
        // Instantiate and add to Unity Player Activity.
        Log.i(TAG, "Starting MidiDROID");
        instance = new MidiDroid();
        UnityPlayer.currentActivity.getFragmentManager().beginTransaction().add(instance, MidiDroid.TAG).commit();
    }

    public void findADevice(){
        foundDevice = false;
        String[] devices = getDevices();
        for (int i = 0; i < devices.length; i++){
            openDeviceAtIndex(i);
        }
    }

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setRetainInstance(true); // Retain between configuration changes (like device rotation)

        manager = (MidiManager)UnityPlayer.currentActivity.getApplicationContext().getSystemService(Context.MIDI_SERVICE);
    }

    public String[] getDevices(){
        MidiDeviceInfo[] infos = manager.getDevices();
        String[] deviceResult = new String[infos.length];
        for (int i = 0; i < infos.length; i++) {
            Bundle properties = infos[i].getProperties();
            String name = properties.getString(MidiDeviceInfo.PROPERTY_PRODUCT);
            if(name != null){
                Log.i(TAG, "Device Property Name is " + name);
                deviceResult[i] = name;
            }
        }
        return deviceResult;
    }

    private MidiOutputPort mOutputPort;

    public void openDeviceAtIndex(int index){
        if(foundDevice) return;

        MidiDeviceInfo[] infos = manager.getDevices();
        final MidiDeviceInfo info = infos[index];
        final int thisIndex = index;

        if (info != null) {
            manager.openDevice(info, new MidiManager.OnDeviceOpenedListener() {

                @Override
                public void onDeviceOpened(MidiDevice device) {
                    if(foundDevice) return;

                    if (device == null) {
                        Log.e(MidiConstants.TAG, "could not open " + info.getProperties().getString(MidiDeviceInfo.PROPERTY_NAME));
                        return;
                    } else {
                        mOutputPort = device.openOutputPort(0);
                        if (mOutputPort == null) {
                            Log.e(MidiConstants.TAG,
                                    "could not open output port for " + info.getProperties().getString(MidiDeviceInfo.PROPERTY_NAME));
                            return;
                        }
                  //      mOutputPort.connect(new LogReceiver());
                        mOutputPort.connect(connectFramer);
                        Log.i(TAG, "Opened device " + info.getProperties().getString(MidiDeviceInfo.PROPERTY_NAME));
                        foundDevice = true;
                        deviceIndex = thisIndex;
                    }
                }
            }, null);
            // Don't run the callback on the UI thread because openOutputPort might take a while.
        }
    };

    private class MyReceiver extends MidiReceiver {
        @Override
        public void onSend(byte[] data, int offset, int count, long timestamp)
                throws IOException {
                if(midiCallback != null){
                    midiCallback.midiJackMessage(deviceIndex, data[0], data[1], data[2]);
                }
        }
    }

    public void setMidiCallback(MidiCallback callback){
        midiCallback = callback;
    }

}
