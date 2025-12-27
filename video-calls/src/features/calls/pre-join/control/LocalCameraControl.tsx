'use client';

import { Button } from '@/shared/components/ui';
import { useAppDispatch, useAppSelector } from '@/shared/hooks';
import { callActions } from '@/store';
import { Video, VideoOff } from 'lucide-react';
import { useCallback } from 'react';

export function LocalCameraControl() {
  const shouldCameraEnabled = useAppSelector(
    state => state.call.shouldCamEnabled
  );
  const dispatch = useAppDispatch();

  const toggleCamera = useCallback(() => {
    dispatch(callActions.setCameraEnabled(!shouldCameraEnabled));
  }, [dispatch, shouldCameraEnabled]);

  return (
    <Button
      variant={shouldCameraEnabled ? 'outline' : 'destructive'}
      size='icon'
      onClick={toggleCamera}
    >
      {shouldCameraEnabled ? (
        <Video className='size-5' />
      ) : (
        <VideoOff className='size-5' />
      )}
    </Button>
  );
}
