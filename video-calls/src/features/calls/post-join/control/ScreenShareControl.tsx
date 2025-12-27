'use client';

import { MiniLoader } from '@/shared/components';
import { Button } from '@/shared/components/ui';
import { useAppSelector } from '@/shared/hooks';
import { useTrackToggle } from '@livekit/components-react';
import { Track } from 'livekit-client';
import { LuScreenShare, LuScreenShareOff } from 'react-icons/lu';
import { toast } from 'sonner';

export function ScreenShareControl() {
  const isScreenShareEnabled = useAppSelector(
    state => state.call.shouldScreenShareEnabled
  );
  const {
    toggle: toggleScreenShare,
    pending: screenSharePending,
    enabled: screenShareEnabled,
  } = useTrackToggle({
    source: Track.Source.ScreenShare,
    initialState: isScreenShareEnabled,
    captureOptions: { audio: true },
  });

  return (
    <Button
      size={'icon'}
      variant={screenShareEnabled ? 'destructive' : 'outline'}
      disabled={screenSharePending}
      onClick={async () => {
        try {
          await toggleScreenShare();
        } catch (error) {
          toast.error(
            error instanceof Error
              ? error.message
              : 'Failed to toggle screenshare'
          );
        }
      }}
    >
      {screenSharePending ? (
        <MiniLoader />
      ) : screenShareEnabled ? (
        <LuScreenShareOff className='size-5' />
      ) : (
        <LuScreenShare className='size-5' />
      )}
    </Button>
  );
}
