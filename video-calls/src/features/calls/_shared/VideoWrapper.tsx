'use client';

import { cn } from '@/shared/utils';
import type { ReactNode } from 'react';
import { IoMdMic, IoMdMicOff } from 'react-icons/io';

export function VideoWrapper({
  identity,
  isLocal,
  isMicrophoneEnabled,
  children,
  className,
  actions,
}: {
  identity: string;
  isLocal: boolean;
  isMicrophoneEnabled: boolean;
  children: ReactNode;
  className?: string;
  actions?: ReactNode;
}) {
  return (
    <div
      className={cn(
        'relative flex h-full min-h-48 w-full items-center justify-center overflow-hidden rounded-xl border',
        'transition-all duration-200 ease-in-out',
        className
      )}
    >
      {children}

      {actions ? <div className='absolute top-3 left-3'>{actions}</div> : null}

      <div className='absolute bottom-3 left-3 flex items-center gap-2 rounded-md bg-black/60 px-2.5 py-1.5 text-xs font-medium backdrop-blur-lg'>
        <ParticipantLabel identity={identity} isLocal={isLocal} />
      </div>

      <div className='absolute top-3 right-3'>
        <MicrophoneStatus isEnabled={isMicrophoneEnabled} />
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
      {identity ? (
        <span className='max-w-30 truncate text-white'>{identity}</span>
      ) : null}
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
