'use client';

import { Button } from '@/shared/components/ui';
import { useTracks } from '@livekit/components-react';
import { LocalTrackPublication, RoomEvent, Track } from 'livekit-client';
import { useCallback, useTransition } from 'react';
import { LuHeadphoneOff, LuHeadphones } from 'react-icons/lu';

export function ScreenShareAudioControl() {
  const [isPending, startTransition] = useTransition();
  const screenShareAudioTracks = useTracks([Track.Source.ScreenShareAudio], {
    updateOnlyOn: [
      RoomEvent.LocalTrackPublished,
      RoomEvent.LocalTrackUnpublished,
      RoomEvent.TrackMuted,
      RoomEvent.TrackUnmuted,
    ],
  });
  const localTracks = screenShareAudioTracks.filter(
    track => track.participant.isLocal
  );

  const screenShareAudioPub = localTracks.at(0)?.publication;

  const toggleAudio = useCallback(async () => {
    if (!(screenShareAudioPub instanceof LocalTrackPublication)) return;

    if (screenShareAudioPub.isMuted) {
      startTransition(async () => {
        await screenShareAudioPub.unmute();
      });
      return;
    }

    startTransition(async () => {
      await screenShareAudioPub.mute();
    });
  }, [screenShareAudioPub]);

  // Extract to a primitive variable to force React's reconciliation.
  // screenShareAudioPub is a mutable object. Without capturing the value here,
  // React may skip DOM updates during reconciliation because the object reference
  // remains the same, even if its internal state changes.
  // This behavior becomes critical if useTransition is removed.
  const isScreenShareAudioMuted = screenShareAudioPub?.isMuted;

  return screenShareAudioPub ? (
    <Button
      size={'icon'}
      variant={!isScreenShareAudioMuted ? 'outline' : 'destructive'}
      disabled={isPending}
      onClick={() => toggleAudio()}
    >
      {!isScreenShareAudioMuted ? (
        <LuHeadphones className='size-5' />
      ) : (
        <LuHeadphoneOff className='size-5' />
      )}
    </Button>
  ) : null;
}
