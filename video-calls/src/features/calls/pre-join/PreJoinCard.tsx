import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/shared/components/ui';
import { PreJoinControl } from './control';
import { PreJoinVideo } from './video';

export function PreJoinCard() {
  return (
    <Card className='mx-auto max-w-2xl'>
      <CardHeader>
        <CardTitle>Ready to join?</CardTitle>
        <CardDescription>Check your device preferences below</CardDescription>
      </CardHeader>
      <CardContent>
        <div className='flex flex-col gap-x-16 gap-y-8 md:flex-row'>
          <PreJoinVideo />
          <div className='mx-auto self-end'>
            <PreJoinControl />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
