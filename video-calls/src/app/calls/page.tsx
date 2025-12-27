import { CallCard } from '@/features/calls';
import { ROUTES } from '@/shared/constants';
import { RoomProvider } from '@/shared/provider';
import { AccessToken } from 'livekit-server-sdk';
import type { Metadata } from 'next';
import z from 'zod';

export const metadata: Metadata = ROUTES.calls.metadata;

const schema = z.object({
  roomName: z.string(),
  userName: z.string(),
});

export default async function CallsPage({
  searchParams,
}: {
  searchParams: unknown;
}) {
  const parsedResult = schema.safeParse(await searchParams);

  const serverUrl = process.env.LIVEKIT_URL;
  if (!serverUrl) throw new Error('LIVEKIT_URL must be set');

  if (!parsedResult.success) throw new Error('Invalid search parameters');

  const token = await createToken(
    parsedResult.data.roomName,
    parsedResult.data.userName
  );

  return (
    <section>
      {parsedResult.success ? (
        <RoomProvider serverUrl={serverUrl} token={token}>
          <CallCard />
        </RoomProvider>
      ) : null}
    </section>
  );
}

function createToken(roomName: string, userName: string): Promise<string> {
  const apiKey = process.env.LIVEKIT_API_KEY;
  const apiSecret = process.env.LIVEKIT_API_SECRET;

  if (!apiKey || !apiSecret)
    throw new Error('LIVEKIT_API_KEY and LIVEKIT_API_SECRET must be set');

  const accessToken = new AccessToken(apiKey, apiSecret, {
    identity: userName,
    name: userName, // display name
  });

  accessToken.addGrant({
    roomJoin: true,
    room: roomName,
    canPublish: true, // allow mic/cam
    canSubscribe: true, // allow receiving tracks from others
  });

  return accessToken.toJwt();
}
