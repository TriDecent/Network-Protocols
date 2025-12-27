'use client';

import { Button } from '@/shared/components/ui';
import { useDisconnectButton } from '@livekit/components-react';
import { FaPhoneSlash } from 'react-icons/fa6';

export function DisconnectControl() {
  const { buttonProps: disconnectProps } = useDisconnectButton({});

  return (
    <Button
      {...disconnectProps}
      variant='destructive'
      size='icon'
      className='rounded-full shadow-lg'
    >
      <FaPhoneSlash className='size-5' />
    </Button>
  );
}
