import { passkey } from '@better-auth/passkey';
import { betterAuth } from 'better-auth';
import { prismaAdapter } from 'better-auth/adapters/prisma';
import { nextCookies } from 'better-auth/next-js';
import { twoFactor } from 'better-auth/plugins/two-factor';
import { prisma } from './prisma';

export const auth = betterAuth({
  appName: 'Trí Decent Auth',
  advanced: { cookiePrefix: 'tri-decent' },
  session: { cookieCache: { enabled: true, maxAge: 60 * 5 } },
  plugins: [twoFactor(), passkey(), nextCookies()],
  database: prismaAdapter(prisma, {
    provider: 'postgresql',
  }),
  socialProviders: {
    github: {
      clientId: process.env.GITHUB_CLIENT_ID!,
      clientSecret: process.env.GITHUB_CLIENT_SECRET!,
      mapProfileToUser: () => ({ welcomeSent: false, favoriteNumber: 0 }),
    },
    discord: {
      clientId: process.env.DISCORD_CLIENT_ID!,
      clientSecret: process.env.DISCORD_CLIENT_SECRET!,
      mapProfileToUser: () => ({ welcomeSent: false, favoriteNumber: 0 }),
    },
  },
  account: { accountLinking: { allowDifferentEmails: true } },
  emailAndPassword: {
    enabled: true,
    requireEmailVerification: true,
    sendResetPassword: async data => {
      console.log(data.url);
    },
  },
  emailVerification: {
    autoSignInAfterVerification: true,
    sendOnSignUp: true,
    sendVerificationEmail: async data => {
      console.log(data.url);
    },
  },
  user: {
    additionalFields: {
      welcomeSent: { type: 'boolean', required: true },
      favoriteNumber: { type: 'number', required: true },
    },
    changeEmail: {
      enabled: true,
      sendChangeEmailVerification: async data => {
        console.log(data.url);
      },
    },
    deleteUser: {
      enabled: true,
      sendDeleteAccountVerification: async data => {
        console.log(data.url);
      },
    },
  },
});

export type AuthSession = NonNullable<
  Awaited<ReturnType<typeof auth.api.getSession>>
>;

// export async function ensureSession(headers: Headers, callbackUrl: string) {
//   const session = await auth.api.getSession({ headers });

//   if (session) return session;

//   const searchParams = new URLSearchParams({ callbackUrl });

//   redirect(`${ROUTES.auth.href}?${searchParams.toString()}`);
// }

// export async function ensureSessionFromRequest(
//   request: NextRequest
// ): Promise<NextResponse> {
//   const session = await auth.api.getSession({ headers: request.headers });

//   if (session) return NextResponse.next();

//   const callbackUrl = `${request.nextUrl.pathname}?${request.nextUrl.searchParams.toString()}`;
//   const redirectTo = new URL(ROUTES.auth.href, request.nextUrl);
//   redirectTo.searchParams.set('callbackUrl', callbackUrl.toString());

//   return NextResponse.redirect(redirectTo);
// }
