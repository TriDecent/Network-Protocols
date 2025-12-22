import { configureStore } from '@reduxjs/toolkit';
import { setupListeners } from '@reduxjs/toolkit/query';
import { callSlice } from './call-slice';
import { uiSlice } from './ui-slice';

export const store = configureStore({
  reducer: {
    [uiSlice.name]: uiSlice.reducer,
    [callSlice.name]: callSlice.reducer,
  },
  middleware: getDefaultMiddleware => getDefaultMiddleware().concat(),
});

setupListeners(store.dispatch);

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
export type AppStore = typeof store;
