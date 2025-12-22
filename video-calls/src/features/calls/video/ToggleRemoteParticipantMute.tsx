'use client';

import { Button } from '@/shared/components/ui';
import { useAppSelector } from '@/shared/hooks';
import { useParticipantTracks } from '@livekit/components-react';
import { RemoteTrackPublication, Track } from 'livekit-client';
import { useCallback, useMemo } from 'react';
import { IoVolumeMedium, IoVolumeMute } from 'react-icons/io5';

export function ToggleRemoteParticipantMute() {
  const isGlobalSpeakerEnabled = useAppSelector(
    state => state.call.isSpeakerEnabled
  );
  const participantTracks = useParticipantTracks([
    Track.Source.Microphone,
    Track.Source.ScreenShareAudio,
    Track.Source.Unknown,
  ]);
  const participant = participantTracks.at(0)?.participant;

  const allAudioTracks = useMemo(() => {
    if (!participant) return [];

    return Array.from(participant.audioTrackPublications.values()).filter(
      (track): track is RemoteTrackPublication =>
        track.kind === Track.Kind.Audio &&
        track instanceof RemoteTrackPublication
    );
  }, [participant]);

  const isAnySubscribed = allAudioTracks.some(track => track.isSubscribed);

  const handleToggleMute = useCallback(() => {
    if (!participant || allAudioTracks.length === 0) return;

    const targetStatus = !isAnySubscribed;

    allAudioTracks.forEach(track => track.setSubscribed(targetStatus));
  }, [participant, allAudioTracks, isAnySubscribed]);

  if (participant?.isLocal || allAudioTracks.length === 0) return null;

  return (
    <Button
      size={'icon'}
      onClick={handleToggleMute}
      variant={'ghost'}
      disabled={!isGlobalSpeakerEnabled} // prevent toggles from making participants audible when the global speaker is off
      className='bg-muted rounded-full'
    >
      {!isAnySubscribed ? <IoVolumeMute /> : <IoVolumeMedium />}
    </Button>
  );
}
