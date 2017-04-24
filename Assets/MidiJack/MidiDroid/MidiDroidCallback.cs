using UnityEngine;

namespace MidiJack
{
    public class MidiDroidCallback: AndroidJavaProxy
    {
        public delegate void RawMidiDelegate(object sender, MidiMessage m);
        public event RawMidiDelegate DroidMidiEvent;

        public MidiDroidCallback() : base("mmmlabs.com.mididroid.MidiCallback") { }

        public void midiJackMessage(int deviceIndex, byte status, byte data1, byte data2)
        {
            if(DroidMidiEvent != null)
            {
                DroidMidiEvent(this, new MidiMessage((uint)deviceIndex, status, data1, data2));
            }
        }
    }
}