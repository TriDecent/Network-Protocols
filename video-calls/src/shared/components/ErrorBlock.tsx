import {
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from '@/shared/components/ui';
import { Button } from '@/shared/components/ui/button';
import { AlertCircle, LinkIcon } from 'lucide-react';
import Link from 'next/link';

interface ErrorBlockProps {
  message: string;
  title?: string;
  action?:
    | {
        type: 'link';
        href: string;
        text: string;
      }
    | {
        type: 'button';
        onClick: () => void;
        text: string;
      };
}

export function ErrorBlock({
  message: error,
  title = 'Oops! Something went wrong',
  action,
}: ErrorBlockProps) {
  const isGenericError = error.includes(
    'An error occurred in the Server Components render'
  );

  const displayError = isGenericError
    ? 'Something went wrong. Please try again later.'
    : error;

  return (
    <Empty className='max-w-md border'>
      <EmptyHeader>
        <EmptyMedia
          variant='icon'
          className='bg-destructive/10 text-destructive'
        >
          <AlertCircle className='size-6' />
        </EmptyMedia>
        <EmptyTitle>{title}</EmptyTitle>
        <EmptyDescription>{displayError}</EmptyDescription>
      </EmptyHeader>

      {action ? (
        <EmptyContent>
          {action.type === 'link' ? (
            <Button asChild className='w-full'>
              <Link href={action.href} replace>
                <LinkIcon /> {action.text}
              </Link>
            </Button>
          ) : (
            <Button onClick={action.onClick} className='w-full'>
              {action.text}
            </Button>
          )}
        </EmptyContent>
      ) : null}
    </Empty>
  );
}
