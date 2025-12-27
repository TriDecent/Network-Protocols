'use client';

import { useConnectionState } from '@livekit/components-react';
import { ConnectionState } from 'livekit-client';
import { VideoCallCard } from './post-join/ui';
import { PreJoinCard } from './pre-join';

export function CallCard() {
  const connectionState = useConnectionState();

  return connectionState === ConnectionState.Connected ? (
    <VideoCallCard />
  ) : (
    <PreJoinCard />
  );
}
