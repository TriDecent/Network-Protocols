import { createContext } from 'react';

export type Context = { serverUrl: string; token: string };

export const CallContext = createContext<Context | null>(null);
