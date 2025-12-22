import type { NextRequest, ProxyConfig } from 'next/server';

export const config: ProxyConfig = {
  matcher: [],
};

export function proxy(request: NextRequest) {}
