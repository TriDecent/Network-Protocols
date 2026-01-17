'use client';

import { Button } from '@/shared/components/ui';
import { useAppSelector } from '@/shared/hooks';
import { useTrackToggle } from '@livekit/components-react';
import { Track } from 'livekit-client';
import { Mic, MicOff } from 'lucide-react';
import { toast } from 'sonner';

export function MicrophoneControl() {
  const isMicrophoneEnabled = useAppSelector(
    state => state.call.shouldMicEnabled
  );
  const {
    toggle: toggleMicrophone,
    enabled: micEnabled,
    pending: micPending,
  } = useTrackToggle({
    source: Track.Source.Microphone,
    initialState: isMicrophoneEnabled,
  });

  return (
    <Button
      variant={micEnabled ? 'outline' : 'destructive'}
      size='icon'
      disabled={micPending}
      onClick={async () => {
        try {
          await toggleMicrophone();
        } catch (error) {
          toast.error(
            error instanceof Error
              ? error.message
              : 'Failed to toggle microphone'
          );
        }
      }}
    >
      {micEnabled ? <Mic className='size-5' /> : <MicOff className='size-5' />}
    </Button>
  );
}
