import { Separator } from '@/shared/components/ui';
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/shared/components/ui/card';
import { CallAudioRenderer } from '../video/CallAudioRenderer';
import { CallControlBar } from './CallControlBar';
import { MeetingInfoBar } from './MeetingInfoBar';
import { VideoGrid } from '../video/VideoGrid';
import { VideoTile } from '../video/VideoTile';

export function VideoCallCard() {
  return (
    <Card>
      <CardHeader>
        <CardTitle className='flex items-center gap-2 text-lg font-semibold'>
          <div className='flex size-2 h-2 w-2 animate-pulse rounded-full bg-green-500' />
          Live Meeting
        </CardTitle>
        <CardDescription>
          <MeetingInfoBar />
        </CardDescription>
      </CardHeader>

      <Separator />

      <CardContent>
        <CallAudioRenderer />
        <VideoGrid>
          <VideoTile />
        </VideoGrid>
      </CardContent>

      <Separator />

      <CardFooter className='justify-center'>
        <CallControlBar />
      </CardFooter>
    </Card>
  );
}
