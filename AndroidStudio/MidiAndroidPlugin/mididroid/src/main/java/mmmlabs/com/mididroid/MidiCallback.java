package mmmlabs.com.mididroid;

public interface MidiCallback {
    public void midiJackMessage(int device, byte status, byte data1, byte data2);
}