'use client';

import { ActionButton } from '@/shared/components';
import { useEffect, useState, type ComponentProps } from 'react';

const RESEND_INTERVAL = 30;

type SendEmailButtonProps = Readonly<{
  alreadySent: boolean;
}> &
  ComponentProps<typeof ActionButton>;

export function SendEmailButton({
  alreadySent,
  ...props
}: SendEmailButtonProps) {
  const [countDown, setCountDown] = useState(alreadySent ? RESEND_INTERVAL : 0);
  const [hasSent, setHasSent] = useState(alreadySent);

  useEffect(() => {
    if (countDown <= 0) return;

    const timeoutId = setTimeout(() => {
      setCountDown(prev => prev - 1);
    }, 1000);

    return () => clearTimeout(timeoutId);
  }, [countDown]);

  return (
    <ActionButton
      {...props}
      onSuccess={() => {
        setHasSent(true);
        setCountDown(RESEND_INTERVAL);
        props?.onSuccess?.();
      }}
      onError={error => {
        setCountDown(0);
        props.onError?.(error);
      }}
      disabled={countDown > 0}
    >
      {!hasSent
        ? 'Send Email'
        : countDown > 0
          ? `Resend in ${countDown}s`
          : 'Resend Again'}
    </ActionButton>
  );
}
