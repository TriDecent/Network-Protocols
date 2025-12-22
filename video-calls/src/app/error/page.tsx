import { ErrorBlock } from '@/shared/components';
import { ROUTES } from '@/shared/constants';
import type { Metadata } from 'next';
import z from 'zod';

export const metadata: Metadata = ROUTES.error.metadata;

export default async function ErrorPage({
  searchParams,
}: {
  searchParams: Promise<unknown>;
}) {
  const parsedResult = paramsSchema.safeParse(await searchParams);

  return (
    <section className='flex justify-center'>
      {parsedResult.success ? (
        <ErrorBlock
          message={parsedResult.data.message}
          action={{
            type: 'link',
            href: parsedResult.data.callbackUrl,
            text: 'Go back',
          }}
        />
      ) : (
        <ErrorBlock message='Please try again' />
      )}
    </section>
  );
}

const paramsSchema = z.object({ message: z.string(), callbackUrl: z.string() });
