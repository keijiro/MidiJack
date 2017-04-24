using UnityEngine;

namespace MidiJack
{
    public class MidiDroidCallback: AndroidJavaProxy
    {
        public delegate void RawMidiDelegate(object sender, MidiMessage m);
        public event RawMidiDelegate DroidMidiEvent;

        public delegate void NoteDataDelegate(object sender, NoteDataEventArgs data);
        public event NoteDataDelegate NoteOnEvent;
        public event NoteDataDelegate NoteOffEvent;
        public event NoteDataDelegate ControlChangeEvent;

        public MidiDroidCallback() : base("mmmlabs.com.mididroid.MidiCallback") { }

        public void noteOn(int channel, int noteIndex, int velocity)
        {
            if(NoteOnEvent != null)
            {
                NoteOnEvent(this, new NoteDataEventArgs(channel, noteIndex, velocity));
            }
            //Debug.LogFormat("Callback NoteOn {0} {1} {2}", channel, noteIndex, velocity);
        }

        public void noteOff(int channel, int noteIndex, int velocity)
        {
            if (NoteOffEvent != null)
            {
                NoteOffEvent(this, new NoteDataEventArgs(channel, noteIndex, velocity));
            }
            //Debug.LogFormat("Callback NoteOff {0} {1} {2}", channel, noteIndex, velocity);
        }

        public void controlChange(int channel, int ccIndex, int value)
        {
            if(ControlChangeEvent != null)
            {
                ControlChangeEvent(this, new NoteDataEventArgs(channel, ccIndex, value));
            }
        }

        public void pitchBend(int channel, int bend)
        {
            //
        }

        public void midiJackMessage(int deviceIndex, byte status, byte data1, byte data2)
        {
            if(DroidMidiEvent != null)
            {
                DroidMidiEvent(this, new MidiMessage((uint)deviceIndex, status, data1, data2));
            }
        }
    }

    public class NoteDataEventArgs : System.EventArgs
    {
        public int channel;
        public int noteIndex;
        public int velocity;
        public NoteDataEventArgs(int channel, int noteIndex, int velocity)
        {
            this.channel = channel;
            this.noteIndex = noteIndex;
            this.velocity = velocity;
        }
    }
}