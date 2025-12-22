'use client';

import { Avatar, AvatarFallback } from '@/shared/components/ui';
import { cn } from '@/shared/utils';
import {
  isTrackReference,
  useIsMuted,
  useIsSpeaking,
  useTrackRefContext,
  VideoTrack,
} from '@livekit/components-react';
import { Track } from 'livekit-client';
import { FaRegUser } from 'react-icons/fa';
import { IoMdMic, IoMdMicOff } from 'react-icons/io';
import { ToggleRemoteParticipantMute } from './ToggleRemoteParticipantMute';

export function VideoTile() {
  const cameraTracks = useTrackRefContext();

  if (!cameraTracks)
    throw new Error('VideoTile must be used within a TrackRefContext');

  if (
    cameraTracks.source !== Track.Source.Camera &&
    cameraTracks.source !== Track.Source.ScreenShare
  )
    throw new Error('Invalid track source for VideoTile');

  const { participant } = cameraTracks;

  const isCameraMuted =
    useIsMuted(cameraTracks) || !isTrackReference(cameraTracks);
  const isParticipantSpeaking = useIsSpeaking(participant);

  return (
    <div
      className={cn(
        'relative flex h-full min-h-48 w-full items-center justify-center overflow-hidden rounded-xl border',
        'transition-all duration-200 ease-in-out',
        isParticipantSpeaking
          ? 'border-transparent shadow-[0_0_20px] ring-2 shadow-green-500 ring-green-500 ring-offset-1 ring-offset-green-500'
          : 'ring-0'
      )}
    >
      {isCameraMuted ? (
        <Avatar className='size-16 shadow'>
          <AvatarFallback>
            <FaRegUser className='size-8' />
          </AvatarFallback>
        </Avatar>
      ) : (
        <VideoTrack trackRef={cameraTracks} />
      )}

      <div className='absolute top-3 left-3'>
        <ToggleRemoteParticipantMute />
      </div>

      <div className='absolute bottom-3 left-3 flex items-center gap-2 rounded-md bg-black/60 px-2.5 py-1.5 text-xs font-medium backdrop-blur-lg'>
        <ParticipantLabel
          identity={participant.identity}
          isLocal={participant.isLocal}
        />
      </div>

      <div className='absolute top-3 right-3'>
        <MicrophoneStatus
          isEnabled={
            participant.isMicrophoneEnabled /* assumes the camera track updates on track muted or unmuted */
          }
        />
      </div>
    </div>
  );
}

function ParticipantLabel({
  identity,
  isLocal,
}: {
  identity: string;
  isLocal: boolean;
}) {
  return (
    <>
      <span className='max-w-30 truncate text-white'>{identity}</span>
      {isLocal ? <span className='text-white/60'>(You)</span> : null}
    </>
  );
}

function MicrophoneStatus({ isEnabled }: { isEnabled: boolean }) {
  return isEnabled ? (
    <div className='rounded-full bg-white/20 p-1.5 backdrop-blur-md'>
      <IoMdMic className='size-4 text-white' />
    </div>
  ) : (
    <div className='rounded-full bg-red-500/80 p-1.5 backdrop-blur-md'>
      <IoMdMicOff className='size-4 text-white' />
    </div>
  );
}
