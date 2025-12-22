import { MiniLoader } from '@/shared/components';
import { ROUTES } from '@/shared/constants';
import type { Metadata } from 'next';
import { Suspense } from 'react';

export const metadata: Metadata = ROUTES.home.metadata;

export default async function HomePage() {
  return (
    <section className='flex flex-col items-center gap-y-8'>
      <h2 className='text-xl font-medium tracking-wider'>Home page</h2>
      <Suspense fallback={<MiniLoader />}></Suspense>
    </section>
  );
}
