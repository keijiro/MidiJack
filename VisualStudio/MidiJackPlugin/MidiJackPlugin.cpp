#include "stdafx.h"

namespace
{
    // Basic type aliases
    using DeviceHandle = HMIDIIN;
    using DeviceHandleSend = HMIDIOUT;
    using DeviceID = uint32_t;

    // Utility functions for Win32/64 compatibility
#ifdef _WIN64
    DeviceID DeviceHandleToID(DeviceHandle handle)
    {
        return static_cast<DeviceID>(reinterpret_cast<uint64_t>(handle));
    }
    DeviceID DeviceHandleToID(DeviceHandleSend handle)
    {
        return static_cast<DeviceID>(reinterpret_cast<uint64_t>(handle));
    }
    DeviceHandle DeviceIDToHandle(DeviceID id)
    {
        return reinterpret_cast<DeviceHandle>(static_cast<uint64_t>(id));
    }
    DeviceHandleSend DeviceIDToHandleSend(DeviceID id)
    {
        return reinterpret_cast<DeviceHandleSend>(static_cast<uint64_t>(id));
    }
#else
    DeviceID DeviceHandleToID(DeviceHandle handle)
    {
        return reinterpret_cast<DeviceID>(handle);
    }
    DeviceHandle DeviceIDToHandle(DeviceID id)
    {
        return reinterpret_cast<DeviceHandle>(id);
    }
#endif

    // MIDI message storage class
    class MidiMessage
    {
        DeviceID source_;
        uint8_t status_;
        uint8_t data1_;
        uint8_t data2_;

    public:

        MidiMessage(DeviceID source, uint32_t rawData)
            : source_(source), status_(rawData), data1_(rawData >> 8), data2_(rawData >> 16)
        {
        }

        uint64_t Encode64Bit()
        {
            uint64_t ul = source_;
            ul |= (uint64_t)status_ << 32;
            ul |= (uint64_t)data1_ << 40;
            ul |= (uint64_t)data2_ << 48;
            return ul;
        }

        std::string ToString()
        {
            char temp[256];
            std::snprintf(temp, sizeof(temp), "(%X) %02X %02X %02X", source_, status_, data1_, data2_);
            return temp;
        }
    };

    // Incoming MIDI message queue
    std::queue<MidiMessage> message_queue;

    // Device handler lists
    std::list<DeviceHandle> active_handles;
    std::stack<DeviceHandle> handles_to_close;

    std::list<DeviceHandleSend> active_handles_send;
    std::stack<DeviceHandleSend> handles_to_close_send;


    // Mutex for resources
    std::recursive_mutex resource_lock;
    std::recursive_mutex resource_lock_send;

    // MIDI input callback
    static void CALLBACK MidiInProc(HMIDIIN hMidiIn, UINT wMsg, DWORD_PTR dwInstance, DWORD_PTR dwParam1, DWORD_PTR dwParam2)
    {
        if (wMsg == MIM_DATA)
        {
            DeviceID id = DeviceHandleToID(hMidiIn);
            uint32_t raw = static_cast<uint32_t>(dwParam1);
            resource_lock.lock();
            message_queue.push(MidiMessage(id, raw));
            resource_lock.unlock();
        }
        else if (wMsg == MIM_CLOSE)
        {
            resource_lock.lock();
            handles_to_close.push(hMidiIn);
            resource_lock.unlock();
        }
    }

    // MIDI out callback
    static void MidiOutProc(HMIDIOUT hmo, UINT32 wMsg, DWORD dwInstance, DWORD_PTR dwParam1, DWORD dwParam2)
    {
        if (wMsg == MOM_OPEN) {
        }
        else if (wMsg == MOM_CLOSE) {
            resource_lock.lock();
            handles_to_close_send.push(hmo);
            resource_lock.unlock();
        }
        else if (wMsg == MOM_DONE) {
        }
        
    }

    // Retrieve a name of a given device.
    std::string GetDeviceName(DeviceHandle handle)
    {
        auto casted_id = reinterpret_cast<UINT_PTR>(handle);
        MIDIINCAPS caps;
        if (midiInGetDevCaps(casted_id, &caps, sizeof(caps)) == MMSYSERR_NOERROR) {
            std::wstring name(caps.szPname);
            return std::string(name.begin(), name.end());
        }
        return "unknown";
    }

    std::string GetDeviceName(DeviceHandleSend handle)
    {
        auto casted_id = reinterpret_cast<UINT_PTR>(handle);
        MIDIOUTCAPS caps;
        if (midiOutGetDevCaps(casted_id, &caps, sizeof(caps)) == MMSYSERR_NOERROR) {
            std::wstring name(caps.szPname);
            return std::string(name.begin(), name.end());
        }
        return "unknown";
    }

    // Open a MIDI IN device with a given index.
    void OpenMidiInDevice(unsigned int index)
    {
        static const DWORD_PTR callback = reinterpret_cast<DWORD_PTR>(MidiInProc);
        DeviceHandle handle;
        if (midiInOpen(&handle, index, callback, NULL, CALLBACK_FUNCTION) == MMSYSERR_NOERROR)
        {
            if (midiInStart(handle) == MMSYSERR_NOERROR)
            {
                resource_lock.lock();
                active_handles.push_back(handle);
                resource_lock.unlock();
            }
            else
            {
                midiInClose(handle);
            }
        }
    }

    // Open a MIDI OUT device with a given index.
    void OpenMidiOutDevice(unsigned int index) {
        DeviceHandleSend handle;
        static const DWORD_PTR callback = reinterpret_cast<DWORD_PTR>(MidiOutProc);

        if (midiOutOpen(&handle, index, callback, NULL, CALLBACK_FUNCTION) == MMSYSERR_NOERROR)
        //if (midiOutOpen(&handle, index, NULL, NULL, CALLBACK_NULL) == MMSYSERR_NOERROR)
        {
            resource_lock_send.lock();
            active_handles_send.push_back(handle);
            resource_lock_send.unlock();
        }
        else {
            midiOutClose(handle);
        }

    }

    // Close a given MidiIn handler.
    void CloseMidiInDevice(DeviceHandle handle)
    {
        midiInClose(handle);

        resource_lock.lock();
        active_handles.remove(handle);
        resource_lock.unlock();
    }

    // Close a given MidiOut handler.
    void CloseMidiOutDevice(DeviceHandleSend handle)
    {
        midiOutClose(handle);

        resource_lock_send.lock();
        active_handles_send.remove(handle);
        resource_lock_send.unlock();
    }

    // Open the all devices.
    void OpenAllDevices()
    {
        int device_count_recv = midiInGetNumDevs();
        int device_count_send = midiOutGetNumDevs();

        for (int i = 0; i < device_count_recv; i++) OpenMidiInDevice(i);
        for (int i = 0; i < device_count_send; i++) OpenMidiOutDevice(i);
    }

    // Refresh device handlers
    void RefreshDevices()
    {
        resource_lock.lock();

        // Close disconnected handlers.
        while (!handles_to_close.empty()) {
            CloseMidiInDevice(handles_to_close.top());
            handles_to_close.pop();
        }
        while (!handles_to_close_send.empty()) {
            CloseMidiOutDevice(handles_to_close_send.top());
            handles_to_close_send.pop();
        }

        // Try open all devices to detect newly connected ones.
        OpenAllDevices();

        resource_lock.unlock();
    }

    // Close the all devices.
    void CloseAllDevices()
    {
        resource_lock.lock();
        while (!active_handles.empty())
            CloseMidiInDevice(active_handles.front());
        resource_lock.unlock();

        resource_lock_send.lock();
        while (!active_handles_send.empty())
            CloseMidiOutDevice(active_handles_send.front());
        resource_lock_send.unlock();
    }
}

// Exported functions

#define EXPORT_API extern "C" __declspec(dllexport)

// Counts the number of MIDI IN endpoints.
EXPORT_API int MidiJackCountEndpoints()
{
    return static_cast<int>(active_handles.size());
}

// Counts the number of MIDI OUT endpoints.
EXPORT_API int MidiJackCountSendEndpoints()
{
    return static_cast<int>(active_handles_send.size());
}

// Get the unique ID of an MIDI IN endpoint.
EXPORT_API uint32_t MidiJackGetEndpointIDAtIndex(int index)
{
    auto itr = active_handles.begin();
    std::advance(itr, index);
    return DeviceHandleToID(*itr);
}

// Get the unique ID of an MIDI OUT endpoint.
EXPORT_API uint32_t MidiJackGetSendEndpointIDAtIndex(int index)
{
    auto itr = active_handles_send.begin();
    std::advance(itr, index);
    return DeviceHandleToID(*itr);
}


// Get the name of an MIDI IN endpoint.
EXPORT_API const char* MidiJackGetEndpointName(uint32_t id)
{
    auto handle = DeviceIDToHandle(id);
    static std::string buffer;
    buffer = GetDeviceName(handle);
    return buffer.c_str();
}


// Get the name of an MIDI OUT endpoint.
EXPORT_API const char* MidiJackGetSendEndpointName(uint32_t id)
{
    auto handle = DeviceIDToHandleSend(id);
    static std::string buffer;
    buffer = GetDeviceName(handle);
    return buffer.c_str();
}

// Retrieve and erase an MIDI message data from the message queue.
EXPORT_API uint64_t MidiJackDequeueIncomingData()
{
    RefreshDevices();

    if (message_queue.empty()) return 0;

    resource_lock.lock();
    auto msg = message_queue.front();
    message_queue.pop();
    resource_lock.unlock();

    return msg.Encode64Bit();
}

// MIDI OUT
// data = 3 bytes midi note/cc message
EXPORT_API uint32_t MidiJackSendData(DeviceID dist, uint32_t data)
{
    RefreshDevices();

    // formatting data bytes for midiOutShortMsg
    DWORD msg;
    msg	 = ((DWORD)data & 0x00FF0000) >> 16;
    msg |= ((DWORD)data & 0x0000FF00);
    msg |= ((DWORD)data & 0x000000FF) << 16;

    for_each(active_handles_send.begin(), active_handles_send.end(), [&](DeviceHandleSend dh) {
        if (DeviceIDToHandleSend(dist) == dh) {
            midiOutShortMsg(dh, msg);
        }
    });

    return 0;
}

// MIDI SysEx OUT
EXPORT_API void MidiJackSendSysExData()
{

}
