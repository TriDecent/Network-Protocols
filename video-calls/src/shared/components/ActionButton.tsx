'use client';

import { Button } from '@/shared/components/ui';
import { isRedirectError } from 'next/dist/client/components/redirect-error';
import {
  useEffect,
  useTransition,
  type ComponentProps,
  type ReactNode,
} from 'react';
import { toast } from 'sonner';
import { MiniLoader } from './MiniLoading';

type ConfirmDialog = {
  message?: string;
  description?: string;
  actionLabel?: ReactNode;
};

type ActionButtonProps = {
  action: () => Promise<void>;
  onSuccess?: () => void;
  onError?: (error: Error | unknown) => void;
  onPending?: () => void;
  confirm?: ConfirmDialog;
};

export function ActionButton({
  action,
  onSuccess,
  onError,
  onPending,
  confirm,
  ...props
}: ActionButtonProps & ComponentProps<typeof Button>) {
  const [isPending, startTransition] = useTransition();

  useEffect(() => {
    if (!isPending) return;
    onPending?.();
  }, [isPending, onPending]);

  const handleAction = () => {
    startTransition(async () => {
      try {
        await action();
        onSuccess?.();
      } catch (error) {
        if (isRedirectError(error)) {
          onError?.(error);
          throw error;
        }
        onError?.(error);
      }
    });
  };

  const handleClick = () => {
    if (!confirm) {
      handleAction();
      return;
    }

    toast.warning(confirm.message ?? 'Are you sure?', {
      description: confirm.description,
      action: {
        label: confirm.actionLabel ?? 'Yes',
        onClick: handleAction,
      },
    });
  };

  return (
    <Button
      {...props}
      disabled={isPending ? isPending : props.disabled}
      onClick={handleClick}
    >
      {isPending ? <MiniLoader /> : props.children}
    </Button>
  );
}
