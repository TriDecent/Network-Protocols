'use client';

import { cn } from '@/shared/utils';
import {
  ParticipantContext,
  TrackRefContext,
  useTracks,
} from '@livekit/components-react';
import { RoomEvent, Track } from 'livekit-client';
import { type ReactNode } from 'react';

export function VideoGrid({ children }: { children: ReactNode }) {
  const tracks = useTracks(
    [
      { source: Track.Source.Camera, withPlaceholder: true },
      { source: Track.Source.ScreenShare, withPlaceholder: false },
    ],
    {
      updateOnlyOn: [
        RoomEvent.ParticipantConnected,
        RoomEvent.ParticipantDisconnected,
        RoomEvent.TrackPublished,
        RoomEvent.TrackUnpublished,
        RoomEvent.TrackMuted,
        RoomEvent.TrackUnmuted,
      ],
    }
  );

  const gridClass = getGridLayout(tracks.length);

  return (
    <div
      className={cn(
        'grid h-full w-full gap-4 transition-all duration-300 ease-in-out',
        gridClass
      )}
    >
      {tracks.map(track => (
        <TrackRefContext
          value={track}
          key={`${track.participant.identity}-${track.source}`}
        >
          <ParticipantContext value={track.participant}>
            {children}
          </ParticipantContext>
        </TrackRefContext>
      ))}
    </div>
  );
}

function getGridLayout(trackCount: number): string {
  if (trackCount === 0) return 'flex items-center justify-center';
  if (trackCount === 1) return 'grid-cols-1';
  if (trackCount === 2) return 'grid-cols-1 md:grid-cols-2';
  if (trackCount <= 4) return 'grid-cols-1 md:grid-cols-2 lg:grid-cols-2';
  if (trackCount <= 9) return 'grid-cols-2 md:grid-cols-3';
  if (trackCount <= 16) return 'grid-cols-2 md:grid-cols-4';
  return 'grid-cols-2 md:grid-cols-4 lg:grid-cols-5';
}
