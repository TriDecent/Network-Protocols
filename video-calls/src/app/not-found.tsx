import {
  Button,
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from '@/shared/components/ui';
import { ROUTES } from '@/shared/constants';
import type { Metadata } from 'next';
import Link from 'next/link';
import { FaLinkSlash } from 'react-icons/fa6';

export const metadata: Metadata = ROUTES['not-found'].metadata;

export default function RootNotFound() {
  return (
    <div className='flex min-h-[calc(100vh-8rem)] items-center justify-center px-4'>
      <Empty className='max-w-2xl border-0'>
        <EmptyHeader>
          <EmptyMedia variant='icon'>
            <FaLinkSlash />
          </EmptyMedia>
          <EmptyTitle>Page Not Found</EmptyTitle>
          <EmptyDescription>
            The page you&apos;re looking for doesn&apos;t exist or has been
            moved.
          </EmptyDescription>
        </EmptyHeader>

        <EmptyContent>
          <div className='flex w-full flex-col gap-3 sm:w-auto sm:flex-row'>
            <Button asChild size='lg' className='w-full sm:w-auto'>
              <Link href={ROUTES.home.href}>Go to Home</Link>
            </Button>
          </div>
        </EmptyContent>
      </Empty>
    </div>
  );
}
