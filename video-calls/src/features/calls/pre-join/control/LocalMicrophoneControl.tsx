'use client';

import { Button } from '@/shared/components/ui';
import { useAppDispatch, useAppSelector } from '@/shared/hooks';
import { callActions } from '@/store';
import { Mic, MicOff } from 'lucide-react';

export function LocalMicrophoneControl() {
  const shouldMicEnabled = useAppSelector(state => state.call.shouldMicEnabled);
  const dispatch = useAppDispatch();

  return (
    <Button
      variant={shouldMicEnabled ? 'outline' : 'destructive'}
      size='icon'
      onClick={() => {
        dispatch(callActions.setMicrophoneEnabled(!shouldMicEnabled));
      }}
    >
      {shouldMicEnabled ? (
        <Mic className='size-5' />
      ) : (
        <MicOff className='size-5' />
      )}
    </Button>
  );
}
