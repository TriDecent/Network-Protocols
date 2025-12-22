import { Metadata } from 'next';

type Route = {
  href: string;
  metadata: Metadata;
  nested?: Record<string, Route>;
};

const BASE_URL = process.env.NEXT_PUBLIC_BASE_URL ?? 'http://localhost:3000';
const APP_NAME = 'Video Call';
const APP_DESCRIPTION =
  'Video Call is a cutting-edge web application that enables seamless video communication with high-quality audio and video streaming, user-friendly interface, and robust security features, making it perfect for both personal and professional use.';

export const ROUTES = {
  root: defineRoute({
    href: '/',
    metadata: {
      title: {
        template: `%s | ${APP_NAME}`,
        default: APP_NAME,
      },
      description: APP_DESCRIPTION,
      metadataBase: new URL(BASE_URL),
      openGraph: {
        title: APP_NAME,
        description: APP_DESCRIPTION,
        url: BASE_URL,
        siteName: APP_NAME,
        type: 'website',
        locale: 'en_US',
      },
      twitter: {
        card: 'summary_large_image',
        title: APP_NAME,
        description: APP_DESCRIPTION,
      },
      robots: {
        index: true,
        follow: true,
        googleBot: {
          index: true,
          follow: true,
        },
      },
    },
  }),
  home: defineRoute({
    href: '/',
    metadata: {
      title: 'Home',
      description:
        'Experience seamless authentication with Better Auth Demo. Sign in or register easily using email, password, or social login.',
      openGraph: {
        title: `Home | ${APP_NAME}`,
        description:
          'Experience seamless authentication with Better Auth Demo.',
        url: BASE_URL,
      },
    },
  }),
  error: defineRoute({
    href: '/error',
    metadata: {
      title: 'Error',
      description:
        'An unexpected error occurred. Please try again or contact support if the issue persists.',
      openGraph: {
        title: `Error | ${APP_NAME}`,
        description: 'An unexpected error occurred.',
        url: `${BASE_URL}/error`,
      },
      robots: {
        index: false,
        follow: false,
      },
    },
  }),
  ['not-found']: defineRoute({
    href: '/not-found',
    metadata: {
      title: 'Page Not Found',
      description:
        'Sorry, the page you requested could not be found or may have been moved.',
      openGraph: {
        title: `404 - Page Not Found | ${APP_NAME}`,
        description: 'Sorry, the page you requested could not be found.',
      },
      robots: {
        index: false,
        follow: false,
      },
    },
  }),
  calls: defineRoute({
    href: '/calls',
    metadata: {
      title: 'Calls',
      description:
        'Join or start high-quality video calls with ease. Enjoy secure, real-time communication for personal or professional meetings.',
      openGraph: {
        title: `Calls | ${APP_NAME}`,
        description: 'Join or start high-quality video calls with ease.',
        url: `${BASE_URL}/calls`,
      },
      twitter: {
        card: 'summary_large_image',
        title: `Calls | ${APP_NAME}`,
        description: 'Join or start high-quality video calls with ease.',
      },
      robots: {
        index: true,
        follow: true,
      },
    },
  }),
} as const;

function defineRoute<T extends Route>(route: T): T {
  return route;
}
