'use client';

import { Avatar, AvatarFallback } from '@/shared/components/ui';
import { useAppDispatch, useAppSelector } from '@/shared/hooks';

import { callActions } from '@/store';
import { usePreviewTracks } from '@livekit/components-react';
import { Track } from 'livekit-client';
import { useEffect, useRef } from 'react';
import { FaRegUser } from 'react-icons/fa';
import { toast } from 'sonner';
import { VideoWrapper } from '../../_shared';

export function PreJoinVideo() {
  const shouldCamEnabled = useAppSelector(state => state.call.shouldCamEnabled);
  const shouldMicEnabled = useAppSelector(state => state.call.shouldMicEnabled);
  const dispatch = useAppDispatch();

  const tracks = usePreviewTracks(
    { audio: shouldMicEnabled, video: shouldCamEnabled },
    error => {
      dispatch(callActions.setCameraEnabled(false));
      dispatch(callActions.setMicrophoneEnabled(false));
      toast.error(error.message);
    }
  );

  const videoTrack = tracks?.find(track => track.kind === Track.Kind.Video);
  const audioTrack = tracks?.find(track => track.kind === Track.Kind.Audio);

  const isAudioMuted = audioTrack?.isMuted;
  const videoRef = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    if (!videoTrack || !videoRef.current) return;
    videoTrack.attach(videoRef.current);

    return () => {
      videoTrack.detach();
      videoTrack.stop();
    };
  }, [videoTrack, videoRef]);

  return (
    <VideoWrapper
      identity={''}
      isLocal={true}
      isMicrophoneEnabled={shouldMicEnabled}
      className={
        isAudioMuted
          ? 'border-transparent shadow-[0_0_20px] ring-2 shadow-green-500 ring-green-500 ring-offset-1 ring-offset-green-500'
          : 'ring-0'
      }
    >
      {shouldCamEnabled && videoTrack ? (
        <video ref={videoRef} playsInline />
      ) : (
        <Avatar className='size-16 shadow'>
          <AvatarFallback>
            <FaRegUser className='size-8' />
          </AvatarFallback>
        </Avatar>
      )}
    </VideoWrapper>
  );
}
