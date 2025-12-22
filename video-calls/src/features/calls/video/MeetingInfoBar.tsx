'use client';

import { Badge } from '@/shared/components/ui';
import { useConnectionState, useParticipants } from '@livekit/components-react';
import {
  ConnectionQuality,
  ConnectionState,
  RoomEvent,
  type LocalParticipant,
  type RemoteParticipant,
} from 'livekit-client';
import { Users, Volume2 } from 'lucide-react';
import { FaSignal } from 'react-icons/fa';
import { MdOutlineSignalCellularAlt2Bar } from 'react-icons/md';

export function MeetingInfoBar() {
  const participants = useParticipants({
    updateOnlyOn: [
      RoomEvent.ParticipantConnected,
      RoomEvent.ParticipantDisconnected,
      RoomEvent.ActiveSpeakersChanged,
      RoomEvent.ConnectionQualityChanged,
    ],
  });
  const connectionState = useConnectionState();

  const speakingCount = participants.filter(p => p.isSpeaking).length;

  const isLocalPoor = participants.some(
    p => p.isLocal && p.connectionQuality === ConnectionQuality.Poor
  );

  const isReconnecting = connectionState === ConnectionState.Reconnecting;

  const hasNetworkIssues = isLocalPoor || isReconnecting;

  return (
    <div className='flex items-center gap-2'>
      <Badge variant='outline' className='gap-2'>
        <Users />
        <span className='hidden sm:inline'>
          {getParticipantsSummary(participants)}
        </span>
        <span className='sm:hidden'>{participants.length}</span>
      </Badge>

      {hasNetworkIssues ? (
        <Badge variant='destructive' className='animate-pulse gap-1.5'>
          <MdOutlineSignalCellularAlt2Bar />
          {isReconnecting ? 'Reconnecting' : 'Unstable'}
        </Badge>
      ) : (
        <Badge variant='secondary' className='gap-1.5'>
          <FaSignal />
          Stable
        </Badge>
      )}

      {speakingCount > 0 ? (
        <Badge
          variant='success'
          className='animate-in fade-in zoom-in gap-1.5 duration-500'
        >
          <Volume2 className='animate-pulse' />
          {speakingCount} Speaking
        </Badge>
      ) : null}
    </div>
  );
}

function getParticipantsSummary(
  participants: readonly (RemoteParticipant | LocalParticipant)[]
) {
  const total = participants.length;

  if (total === 0) return 'No one here';

  if (total === 1) return 'Just You';

  const remoteParticipants = participants.filter(p => !p.isLocal);
  const remoteNames = remoteParticipants.map(p => p.identity);

  if (remoteParticipants.length === 1) {
    return `You & ${remoteNames[0] || 'Guest'}`;
  }

  return `You, ${remoteNames[0] || 'Guest'} & ${remoteParticipants.length - 1} others`;
}
