import {
  detectBot,
  protectSignup,
  slidingWindow,
  tokenBucket,
  type ArcjetDecision,
  type ArcjetEmailType,
} from '@arcjet/next';
import { NextResponse } from 'next/server';

import { ROUTES } from '@/shared/constants';
import arcjet, {
  shield,
  type BotOptions,
  type EmailOptions,
  type SlidingWindowRateLimitOptions,
} from '@arcjet/next';
import { redirect, RedirectType } from 'next/navigation';

const arcjetMode = process.env.NODE_ENV === 'development' ? 'DRY_RUN' : 'LIVE';

// Authenticated users - track by userId only
const ajAuthenticated = arcjet({
  key: process.env.ARCJET_KEY!,
  characteristics: ['userId', 'action'],
  rules: [shield({ mode: arcjetMode })],
});

// Anonymous users - track by IP only
const ajAnonymous = arcjet({
  key: process.env.ARCJET_KEY!,
  characteristics: ['ip.src', 'action'],
  rules: [shield({ mode: arcjetMode })],
});

export const botSettings: BotOptions = {
  mode: arcjetMode,
  allow: [],
};

export const restrictiveRateLimitSettings: SlidingWindowRateLimitOptions<[]> = {
  mode: arcjetMode,
  max: 10,
  interval: '10m',
};

export const extremeRestrictiveRateLimitSettings: SlidingWindowRateLimitOptions<
  []
> = {
  mode: arcjetMode,
  max: 3,
  interval: '1h',
};

export const laxRateLimitSettings: SlidingWindowRateLimitOptions<[]> = {
  mode: arcjetMode,
  max: 30,
  interval: '1m',
};

export const emailSettings: EmailOptions = {
  mode: arcjetMode,
  deny: ['DISPOSABLE', 'INVALID', 'NO_MX_RECORDS'],
};

export const ajLaxAuthenticated = ajAuthenticated
  .withRule(detectBot(botSettings))
  .withRule(slidingWindow(laxRateLimitSettings));

export const ajRestrictiveAuthenticated = ajAuthenticated
  .withRule(detectBot(botSettings))
  .withRule(slidingWindow(restrictiveRateLimitSettings));

export const ajExtremeRestrictiveAuthenticated = ajAuthenticated
  .withRule(detectBot(botSettings))
  .withRule(slidingWindow(extremeRestrictiveRateLimitSettings));

export const ajLaxAnonymous = ajAnonymous
  .withRule(detectBot(botSettings))
  .withRule(slidingWindow(laxRateLimitSettings));

export const ajRestrictiveAnonymous = ajAnonymous
  .withRule(detectBot(botSettings))
  .withRule(slidingWindow(restrictiveRateLimitSettings));

export const ajSignup = ajAnonymous.withRule(
  protectSignup({
    email: emailSettings,
    bots: botSettings,
    rateLimit: restrictiveRateLimitSettings,
  })
);

export const ajEmailSend = ajAnonymous
  .withRule(detectBot(botSettings))
  .withRule(
    tokenBucket({
      mode: 'LIVE',
      capacity: 3,
      refillRate: 1,
      interval: '1h',
      // Why emailId?
      // It keeps yelling
      /*
       ✦Aj ERROR Failure running rule: RATE_LIMIT due to unable 
       to generate fingerprint: error generating identifier - 
       requested a user-defined `email` characteristic but the 
       `email` value was empty */
      characteristics: ['emailId'],
    })
  );

export function handleDeniedInServerAction(
  decision: ArcjetDecision,
  callbackUrl?: string
) {
  const searchParams = new URLSearchParams();

  const message = decision.reason.isRateLimit()
    ? 'Too many requests. Please try again later.'
    : decision.reason.isBot()
      ? 'Bot detected.'
      : decision.reason.isEmail()
        ? getEmailErrorMessage(decision.reason.emailTypes)
        : 'Forbidden';

  searchParams.set('message', message);
  searchParams.set('callbackUrl', callbackUrl ?? '/auth');

  redirect(
    `${ROUTES.error.href}?${searchParams.toString()}`,
    RedirectType.replace
  );
}

export function respondToDenial(decision: ArcjetDecision): NextResponse {
  if (decision.reason.isRateLimit())
    return NextResponse.json(
      { error: 'Too many requests. Please try again later.' },
      { status: 429 }
    );

  if (decision.reason.isBot())
    return NextResponse.json({ error: 'Bot detected.' }, { status: 403 });

  if (decision.reason.isEmail()) {
    const message = getEmailErrorMessage(decision.reason.emailTypes);
    return NextResponse.json({ error: message }, { status: 400 });
  }

  return NextResponse.json({ error: 'Forbidden' }, { status: 403 });
}

function getEmailErrorMessage(emailTypes: ArcjetEmailType[]): string {
  if (emailTypes.includes('INVALID')) return 'Email address format is invalid';
  if (emailTypes.includes('DISPOSABLE'))
    return 'Disposable email addresses are not allowed';
  if (emailTypes.includes('NO_MX_RECORDS')) return 'Email domain is not valid';
  return 'Invalid email';
}
