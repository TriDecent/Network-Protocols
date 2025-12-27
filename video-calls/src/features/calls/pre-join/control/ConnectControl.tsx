'use client';

import { Button } from '@/shared/components/ui';
import { useRoomContext } from '@livekit/components-react';
import { use, useTransition } from 'react';
import { FaPhone } from 'react-icons/fa6';
import { CallContext } from '../../provider';

export function ConnectControl() {
  const [isPending, startTransition] = useTransition();
  const room = useRoomContext();
  const context = use(CallContext);

  if (!context) throw new Error('CallContext is not available');

  return (
    <Button
      variant='default'
      size='icon'
      className='rounded-full bg-green-400 shadow-lg dark:bg-green-500'
      disabled={isPending}
      onClick={() => {
        startTransition(async () => {
          await room.connect(context.serverUrl, context.token);
        });
      }}
    >
      <FaPhone className='size-5' />
    </Button>
  );
}
