import { passkeyClient } from '@better-auth/passkey/client';
import {
  inferAdditionalFields,
  twoFactorClient,
} from 'better-auth/client/plugins';
import { createAuthClient } from 'better-auth/react';
import { auth } from './auth';

export const authClient = createAuthClient({
  plugins: [
    inferAdditionalFields<typeof auth>(),
    twoFactorClient(),
    passkeyClient(),
  ],
});
export const { signIn, signUp, useSession } = createAuthClient();
