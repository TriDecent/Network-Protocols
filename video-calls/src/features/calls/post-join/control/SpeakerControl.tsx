'use client';

import { Button } from '@/shared/components/ui';
import { useAppDispatch, useAppSelector } from '@/shared/hooks';
import { callActions } from '@/store';
import { HiOutlineSpeakerWave, HiOutlineSpeakerXMark } from 'react-icons/hi2';

export function SpeakerControl() {
  const isSpeakerEnabled = useAppSelector(
    state => state.call.shouldSpeakerEnabled
  );
  const dispatch = useAppDispatch();

  return (
    <Button
      variant={isSpeakerEnabled ? 'outline' : 'destructive'}
      size={'icon'}
      onClick={() => {
        dispatch(callActions.setSpeakerEnabled(!isSpeakerEnabled));
      }}
    >
      {isSpeakerEnabled ? (
        <HiOutlineSpeakerWave className='size-5' />
      ) : (
        <HiOutlineSpeakerXMark className='size-5' />
      )}
    </Button>
  );
}
