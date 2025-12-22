import { cn } from '@/shared/utils';
import Link from 'next/link';
import type { ReactNode } from 'react';
import { FaFacebook, FaGithub, FaHeart } from 'react-icons/fa6';

interface PageLayoutProps {
  readonly children?: ReactNode;
  readonly className?: string;
}

export const PageLayout = ({ children, className }: PageLayoutProps) => (
  <div className={cn('bg-background relative flex flex-col', className)}>
    <div className='absolute inset-0 bg-[linear-gradient(to_right,#80808012_1px,transparent_1px),linear-gradient(to_bottom,#80808012_1px,transparent_1px)] mask-[radial-gradient(ellipse_60%_50%_at_50%_0%,#000_70%,transparent_180%)] bg-size-[24px_24px]' />
    <div className='relative flex min-h-screen flex-col'>{children}</div>
  </div>
);

const Header = ({ children, className }: PageLayoutProps) => (
  <header
    className={cn(
      'bg-background/80 supports-backdrop-filter:bg-background/60 sticky top-0 z-50 w-full border-b backdrop-blur-xl',
      className
    )}
  >
    <div className='container mx-auto px-4 py-4'>
      <div className='flex items-center justify-between'>{children}</div>
    </div>
  </header>
);

const Main = ({ children, className }: PageLayoutProps) => (
  <main
    className={cn(
      'container mx-auto flex flex-1 flex-col px-4 py-12 lg:py-16',
      className
    )}
  >
    <div className='mx-auto w-full max-w-7xl'>{children}</div>
  </main>
);

const Footer = ({ children, className }: PageLayoutProps) => (
  <footer
    className={cn(
      'bg-background/40 w-full border-t backdrop-blur-xl',
      className
    )}
  >
    {children ?? (
      <div className='container mx-auto px-4 py-12'>
        <div className='grid gap-8 lg:grid-cols-3'>
          <BrandSection />
          <QuickLinks />
          <ConnectionSection />
        </div>

        <div className='mt-8 flex flex-col items-center gap-4 border-t pt-8 sm:flex-row sm:justify-between'>
          <p className='text-muted-foreground flex items-center gap-2 text-sm'>
            <span>© {new Date().getFullYear()} Trí Decent.</span>
            <span className='hidden sm:inline'>All rights reserved.</span>
          </p>
          <p className='text-muted-foreground flex items-center gap-1.5 text-sm'>
            <span>Made with</span>
            <FaHeart className='h-3.5 w-3.5 animate-pulse text-red-500' />
          </p>
        </div>
      </div>
    )}
  </footer>
);

PageLayout.Header = Header;
PageLayout.Main = Main;
PageLayout.Footer = Footer;

function BrandSection() {
  return (
    <div className='flex flex-col gap-4 lg:col-span-1'>
      <h3 className='from-foreground to-muted-foreground bg-linear-to-r bg-clip-text text-lg font-semibold text-transparent'>
        Trí Decent
      </h3>
      <p className='text-muted-foreground text-sm'>
        Pray like it all depends on God, but work like it all depends on you.
      </p>
    </div>
  );
}

function QuickLinks() {
  return (
    <div className='flex flex-col gap-4 lg:col-span-1'>
      <h4 className='text-foreground text-sm font-semibold'>Quick Links</h4>
      <nav className='flex flex-col gap-2'>
        <Link
          target='_blank'
          href='https://www.tridecent.dev'
          className='text-muted-foreground hover:text-foreground text-sm transition-colors'
        >
          Portfolio
        </Link>
      </nav>
    </div>
  );
}

function ConnectionSection() {
  return (
    <div className='flex flex-col gap-4 lg:col-span-1'>
      <h4 className='text-foreground text-sm font-semibold'>Connect</h4>
      <div className='flex items-center gap-3'>
        <Link
          href='https://www.facebook.com/Tridecent/'
          target='_blank'
          rel='noopener noreferrer'
          className='group relative flex h-10 w-10 items-center justify-center overflow-hidden rounded-lg bg-blue-600 text-white shadow-lg shadow-blue-600/20 transition-all duration-300 hover:scale-110 hover:shadow-xl hover:shadow-blue-600/30 dark:bg-blue-500'
          aria-label='Facebook'
        >
          <FaFacebook className='h-5 w-5 transition-transform duration-300 group-hover:scale-110' />
        </Link>
        <Link
          href='https://github.com/TriDecent'
          target='_blank'
          rel='noopener noreferrer'
          className='group bg-foreground text-background shadow-foreground/20 hover:shadow-foreground/30 relative flex h-10 w-10 items-center justify-center overflow-hidden rounded-lg shadow-lg transition-all duration-300 hover:scale-110 hover:shadow-xl'
          aria-label='GitHub'
        >
          <FaGithub className='h-5 w-5 transition-transform duration-300 group-hover:scale-110' />
        </Link>
      </div>
    </div>
  );
}
