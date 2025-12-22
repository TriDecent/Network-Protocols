import {
  ajLaxAnonymous,
  ajLaxAuthenticated,
  ajRestrictiveAnonymous,
  ajSignup,
  respondToDenial,
} from '@/lib';
import { auth } from '@/lib/auth';
import { type ArcjetDecision } from '@arcjet/next';
import { toNextJsHandler } from 'better-auth/next-js';
import { NextRequest } from 'next/server';

const betterAuthHandler = toNextJsHandler(auth);
export const { GET } = betterAuthHandler;

export async function POST(request: NextRequest) {
  const decision = await checkFromRequest(request);
  if (decision.isDenied()) return respondToDenial(decision);

  return betterAuthHandler.POST(request);
}

export async function checkFromRequest(
  request: NextRequest
): Promise<ArcjetDecision> {
  const session = await auth.api.getSession({ headers: request.headers });

  if (session)
    return ajLaxAuthenticated.protect(request, {
      action: 'api-authenticated-auth-interaction',
      userId: session.user.id,
    });

  if (request.url.endsWith('/auth/sign-up'))
    return checkSignupRouteForAnonymousUser(request);

  return ajLaxAnonymous.protect(request, {
    action: 'api-anonymous-auth-interaction',
  });
}

async function checkSignupRouteForAnonymousUser(
  request: NextRequest
): Promise<ArcjetDecision> {
  const body: unknown = await request
    .clone()
    .json()
    .catch(() => ({}));

  if (
    body &&
    typeof body === 'object' &&
    'email' in body &&
    typeof body.email === 'string'
  )
    return ajSignup.protect(request, {
      action: 'api-signup',
      email: body.email,
    });

  return ajRestrictiveAnonymous.protect(request, {
    action: 'api-signup-no-email-in-body',
  });
}
