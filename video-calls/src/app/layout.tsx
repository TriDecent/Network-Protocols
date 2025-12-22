'use cache';

import { PageLayout, ThemeProvider, ThemeToggle } from '@/shared/components';
import { Toaster } from '@/shared/components/ui';
import { ROUTES } from '@/shared/constants';
import { StoreProvider } from '@/shared/provider';
import type { Metadata } from 'next';
import { cacheLife } from 'next/cache';
import { Geist } from 'next/font/google';
import Link from 'next/link';
import './globals.css';

const geistSans = Geist({
  variable: '--font-geist-sans',
  subsets: ['latin'],
});

export const metadata: Metadata = ROUTES.root.metadata;

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  cacheLife('weeks');

  return (
    <html lang='en' suppressHydrationWarning>
      <body className={`${geistSans.variable} antialiased`}>
        <StoreProvider>
          <ThemeProvider attribute='class' defaultTheme='system' enableSystem>
            <PageLayout>
              <PageLayout.Header>
                <h1 className='text-2xl font-medium tracking-wider'>
                  <Link href='/'>Calls</Link>
                </h1>
                <ThemeToggle />
              </PageLayout.Header>
              <PageLayout.Main>{children}</PageLayout.Main>
              <PageLayout.Footer />
            </PageLayout>
            <Toaster position='top-right' richColors />
          </ThemeProvider>
        </StoreProvider>
      </body>
    </html>
  );
}
