'use client';

import { Button } from '@/shared/components/ui';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog';
import { useAppSelector } from '@/shared/hooks';
import { RoomAudioRenderer, useStartAudio } from '@livekit/components-react';
import { Volume2 } from 'lucide-react';
import { shallowEqual } from 'react-redux';

export function CallAudioRenderer() {
  const isSpeakerEnabled = useAppSelector(
    state => state.call.isSpeakerEnabled,
    shallowEqual
  );

  const { mergedProps: startAudioProps, canPlayAudio } = useStartAudio({
    props: {},
  });

  return (
    <>
      <AlertDialog open={!canPlayAudio}>
        <AlertDialogContent>
          <AlertDialogHeader className='items-center'>
            <div className='bg-primary/10 mb-2 w-fit rounded-full p-4'>
              <Volume2 className='text-primary size-8' />
            </div>
            <AlertDialogTitle>Enable Audio</AlertDialogTitle>
            <AlertDialogDescription>
              Click to start hearing others in the call
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogAction asChild>
            <Button size='lg' onClick={startAudioProps.onClick}>
              <Volume2 className='size-4' />
              Start Audio
            </Button>
          </AlertDialogAction>
        </AlertDialogContent>
      </AlertDialog>
      <RoomAudioRenderer muted={!isSpeakerEnabled} />
    </>
  );
}
