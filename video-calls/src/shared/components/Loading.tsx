import { LoaderPinwheel } from 'lucide-react';

export function Loading() {
  return (
    <div className='flex h-screen justify-center'>
      <LoaderPinwheel className='h-8 w-8 translate-y-32 animate-spin text-center' />
    </div>
  );
}
