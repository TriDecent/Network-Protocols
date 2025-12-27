'use client';

import { Avatar, AvatarFallback } from '@/shared/components/ui';
import {
  isTrackReference,
  useIsMuted,
  useIsSpeaking,
  useTrackRefContext,
  VideoTrack,
} from '@livekit/components-react';
import { Track } from 'livekit-client';
import { FaRegUser } from 'react-icons/fa';
import { VideoWrapper } from '../../_shared';
import { ToggleRemoteParticipantMute } from './ToggleRemoteParticipantMute';

export function VideoTile() {
  const videoTrack = useTrackRefContext();

  if (!videoTrack)
    throw new Error('VideoTile must be used within a TrackRefContext');

  if (
    videoTrack.source !== Track.Source.Camera &&
    videoTrack.source !== Track.Source.ScreenShare
  )
    throw new Error('Invalid track source for VideoTile');

  const { participant } = videoTrack;

  console.log(videoTrack);

  const isCameraMuted = useIsMuted(videoTrack) || !isTrackReference(videoTrack);
  const isParticipantSpeaking = useIsSpeaking(participant);

  return (
    <VideoWrapper
      identity={participant.identity}
      isLocal={participant.isLocal}
      isMicrophoneEnabled={
        participant.isMicrophoneEnabled /* assumes the camera track updates on track muted or unmuted */
      }
      className={
        isParticipantSpeaking
          ? 'border-transparent shadow-[0_0_20px] ring-2 shadow-green-500 ring-green-500 ring-offset-1 ring-offset-green-500'
          : 'ring-0'
      }
      actions={<ToggleRemoteParticipantMute />}
    >
      {isCameraMuted ? (
        <Avatar className='size-16 shadow'>
          <AvatarFallback>
            <FaRegUser className='size-8' />
          </AvatarFallback>
        </Avatar>
      ) : (
        <VideoTrack trackRef={videoTrack} />
      )}
    </VideoWrapper>
  );
}
