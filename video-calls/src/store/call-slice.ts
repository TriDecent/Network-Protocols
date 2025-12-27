import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

type CallConfig = Readonly<{
  shouldCamEnabled: boolean;
  shouldMicEnabled: boolean;
  shouldSpeakerEnabled: boolean;
  shouldScreenShareEnabled: boolean;
  shouldScreenShareAudioEnabled: boolean;
}>;

const initialState: CallConfig = {
  shouldCamEnabled: false,
  shouldMicEnabled: false,
  shouldSpeakerEnabled: false,
  shouldScreenShareEnabled: false,
  shouldScreenShareAudioEnabled: false,
};

export const callSlice = createSlice({
  name: 'call',
  initialState,
  reducers: {
    setCameraEnabled(state, action: PayloadAction<boolean>) {
      state.shouldCamEnabled = action.payload;
    },
    setMicrophoneEnabled(state, action: PayloadAction<boolean>) {
      state.shouldMicEnabled = action.payload;
    },
    setSpeakerEnabled(state, action: PayloadAction<boolean>) {
      state.shouldSpeakerEnabled = action.payload;
    },
  },
});

export const callActions = callSlice.actions;
