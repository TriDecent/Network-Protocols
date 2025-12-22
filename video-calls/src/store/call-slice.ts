import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

type CallConfig = Readonly<{
  isCameraEnabled: boolean;
  isMicrophoneEnabled: boolean;
  isSpeakerEnabled: boolean;
  isScreenShareEnabled: boolean;
  isScreenShareAudioEnabled: boolean;
}>;

const initialState: CallConfig = {
  isCameraEnabled: false,
  isMicrophoneEnabled: false,
  isSpeakerEnabled: false,
  isScreenShareEnabled: false,
  isScreenShareAudioEnabled: false,
};

export const callSlice = createSlice({
  name: 'call',
  initialState,
  reducers: {
    setCameraEnabled(state, action: PayloadAction<boolean>) {
      state.isCameraEnabled = action.payload;
    },
    setMicrophoneEnabled(state, action: PayloadAction<boolean>) {
      state.isMicrophoneEnabled = action.payload;
    },
    setSpeakerEnabled(state, action: PayloadAction<boolean>) {
      state.isSpeakerEnabled = action.payload;
    },
  },
});

export const callActions = callSlice.actions;
