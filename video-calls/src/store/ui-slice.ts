import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

type UIState = Readonly<{
  verificationInfo?: Readonly<{
    name: string;
    email: string;
  }>;
}>;

const initialState: UIState = {
  verificationInfo: undefined,
};

export const uiSlice = createSlice({
  name: 'ui',
  initialState,
  reducers: {
    toggleEmailVerification: (
      state,
      action: PayloadAction<UIState['verificationInfo']>
    ) => {
      state.verificationInfo = action.payload;
    },
  },
});

export const uiActions = uiSlice.actions;
