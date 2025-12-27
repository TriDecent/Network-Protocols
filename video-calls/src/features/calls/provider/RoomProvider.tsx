'use client';

import { Loading } from '@/shared/components';
import { RoomContext, useLiveKitRoom } from '@livekit/components-react';
import { useEffect, type ReactNode } from 'react';
import { CallProvider } from './CallProvider';

declare global {
  interface Window {
    chrome?: {
      webview?: {
        postMessage: (message: string) => void;
      };
    };
  }
}

export function RoomProvider({
  serverUrl,
  token,
  children,
}: {
  serverUrl: string;
  token: string;
  children: ReactNode;
}) {
  const { room, htmlProps } = useLiveKitRoom({
    serverUrl,
    token,
    connect: false,
  });

  useEffect(() => {
    if (!room) return;

    const handleDisconnected = () => {
      if (!window.chrome?.webview) return;

      const message = JSON.stringify({
        type: 'END_CALL',
        reason: 'USER_DISCONNECTED',
      });

      window.chrome.webview.postMessage(message);
    };

    room.on('disconnected', handleDisconnected);

    return () => {
      room.off('disconnected', handleDisconnected);
    };
  }, [room]);

  if (!room) return <Loading />;

  return (
    <RoomContext value={room}>
      <CallProvider value={{ serverUrl, token }}>
        <div {...htmlProps}>{children}</div>
      </CallProvider>
    </RoomContext>
  );
}
