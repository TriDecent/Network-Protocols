import type { ReactNode } from 'react';
import { CallContext, type Context } from './call-context';

export function CallProvider({
  value,
  children,
}: {
  value: Context;
  children: ReactNode;
}) {
  return <CallContext value={value}>{children}</CallContext>;
}
