'use client';

import { MiniLoader } from '@/shared/components';
import { Button } from '@/shared/components/ui';
import { useAppSelector } from '@/shared/hooks';
import { useTrackToggle } from '@livekit/components-react';
import { Track } from 'livekit-client';
import { Video, VideoOff } from 'lucide-react';
import { toast } from 'sonner';

export function CameraControl() {
  const isCameraEnabled = useAppSelector(state => state.call.shouldCamEnabled);

  const {
    toggle: toggleMicrophone,
    enabled: cameraEnabled,
    pending: cameraPending,
  } = useTrackToggle({
    source: Track.Source.Camera,
    initialState: isCameraEnabled,
  });

  return (
    <Button
      variant={cameraEnabled ? 'outline' : 'destructive'}
      size='icon'
      disabled={cameraPending}
      onClick={async () => {
        try {
          await toggleMicrophone();
        } catch (error) {
          toast.error(
            error instanceof Error ? error.message : 'Failed to toggle camera'
          );
        }
      }}
    >
      {cameraPending ? (
        <MiniLoader />
      ) : cameraEnabled ? (
        <Video className='size-5' />
      ) : (
        <VideoOff className='size-5' />
      )}
    </Button>
  );
}
