// MidiJack plug-in entry point.
// By Keijiro Takahashi, 2013
// https://github.com/keijiro/MidiJack

#include <CoreMIDI/CoreMIDI.h>
#include <queue>
#include <mutex>

#pragma mark Private classes

namespace
{
    // MIDI message structure.
    union Message
    {
        uint64_t uint64Value;
        
        struct
        {
            MIDIUniqueID source;
            Byte status;
            Byte data[2];
        };
        
        Message(MIDIUniqueID aSource, Byte aStatus)
        :   source(aSource), status(aStatus)
        {
            data[0] = data[1] = 0;
        }
    };
    
    static_assert(sizeof(Message) == sizeof(uint64_t), "Wrong data size.");

    // MIDI source ID array.
    MIDIUniqueID sourceIDs[256];

    // Incoming MIDI message queue.
    std::queue<Message> messageQueue;
    std::mutex messageQueueLock;

    // Core MIDI objects.
    MIDIClientRef client;
    MIDIPortRef inputPort;
    
    // Reset-is-required flag.
    bool resetIsRequired = true;
}

#pragma mark Core MIDI callback function

namespace
{
    extern "C" void MIDIStateChangedHander(const MIDINotification* message, void* refCon)
    {
        // Only process additions and removals.
        if (message->messageID != kMIDIMsgObjectAdded && message->messageID != kMIDIMsgObjectRemoved) return;
        
        // Only process source operations.
        auto addRemoveDetail = reinterpret_cast<const MIDIObjectAddRemoveNotification*>(message);
        if (addRemoveDetail->childType != kMIDIObjectType_Source) return;
        
        // Order to reset the plug-in.
        resetIsRequired = true;
    }
    
    extern "C" void MIDIReadProc(const MIDIPacketList *packetList, void *readProcRefCon, void *srcConnRefCon)
    {
        auto sourceID = *reinterpret_cast<MIDIUniqueID*>(srcConnRefCon);
        
        messageQueueLock.lock();
        
        // Transform the packets into MIDI messages and push it to the message queue.
        const MIDIPacket *packet = &packetList->packet[0];
        for (int packetCount = 0; packetCount < packetList->numPackets; packetCount++) {
            // Extract MIDI messages from the data stream.
            for (int offs = 0; offs < packet->length;) {
                Message message(sourceID, packet->data[offs++]);
                for (int dc = 0; offs < packet->length && (packet->data[offs] < 0x80); dc++, offs++) {
                    if (dc < 2) message.data[dc] = packet->data[offs];
                }
                messageQueue.push(message);
            }
            packet = MIDIPacketNext(packet);
        }
        
        messageQueueLock.unlock();
    }
}

#pragma mark Private functions
    
namespace
{
    void ResetPluginIfRequired()
    {
        if (!resetIsRequired) return;
        
        // Dispose the old MIDI client if exists.
        if (client != 0) MIDIClientDispose(client);
        
        // Clear the message queue.
        std::queue<Message> emptyQueue;
        std::swap(messageQueue, emptyQueue);
        
        // Create a MIDI client.
        MIDIClientCreate(CFSTR("UnityMIDIReceiver Client"), MIDIStateChangedHander, nullptr, &client);
        
        // Create a MIDI port which covers all MIDI sources.
        MIDIInputPortCreate(client, CFSTR("UnityMIDIReceiver Input Port"), MIDIReadProc, nullptr, &inputPort);
        
        // Enumerate the all MIDI sources.
        ItemCount sourceCount = MIDIGetNumberOfSources();
        assert(sourceCount < sizeof(sourceIDs));
        
        for (int i = 0; i < sourceCount; i++) {
            // Connect the MIDI source to the input port.
            MIDIEndpointRef source = MIDIGetSource(i);
            MIDIObjectGetIntegerProperty(source, kMIDIPropertyUniqueID, &sourceIDs[i]);
            MIDIPortConnectSource(inputPort, source, &sourceIDs[i]);
        }
        
        resetIsRequired = false;
    }
}

#pragma mark Exposed functions

// Counts the number of endpoints.
extern "C" int MidiJackCountEndpoints()
{
    ResetPluginIfRequired();
    return static_cast<int>(MIDIGetNumberOfSources());
}

// Get the unique ID of an endpoint.
extern "C" uint32_t MidiJackGetEndpointIDAtIndex(int index)
{
    return sourceIDs[index];
}

// Get the name of an endpoint.
extern "C" const char* MidiJackGetEndpointName(uint32_t id)
{
    ResetPluginIfRequired();

    MIDIObjectRef object;
    MIDIObjectType type;
    MIDIObjectFindByUniqueID(id, &object, &type);
    assert(type == kMIDIObjectType_Source);
    
    CFStringRef name;
    MIDIObjectGetStringProperty(object, kMIDIPropertyDisplayName, &name);
    
    static char buffer[256];
    CFStringGetCString(name, buffer, sizeof(buffer), kCFStringEncodingUTF8);
    
    return buffer;
}

// Retrieve and erase an MIDI message data from the message queue.
extern "C" uint64_t MidiJackDequeueIncomingData()
{
    ResetPluginIfRequired();

    if (messageQueue.empty()) return 0;
    
    messageQueueLock.lock();
    Message m = messageQueue.front();
    messageQueue.pop();
    messageQueueLock.unlock();
    
    return m.uint64Value;
}
