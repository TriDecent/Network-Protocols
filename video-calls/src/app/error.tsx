'use client';

import { ErrorBlock } from '@/shared/components';

export default function RootError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <section className='flex justify-center'>
      <ErrorBlock
        message={error.message}
        action={{ type: 'button', onClick: reset, text: 'Try again' }}
      />
    </section>
  );
}
