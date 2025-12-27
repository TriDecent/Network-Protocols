import { ButtonGroup, ButtonGroupSeparator } from '@/shared/components/ui';

import { SpeakerControl } from '../../post-join/control';
import { ConnectControl } from './ConnectControl';
import { LocalCameraControl } from './LocalCameraControl';
import { LocalMicrophoneControl } from './LocalMicrophoneControl';

export function PreJoinControl() {
  return (
    <ButtonGroup className='h-fit'>
      <ButtonGroup>
        <SpeakerControl />
        <LocalCameraControl />
        <LocalMicrophoneControl />
      </ButtonGroup>

      <ButtonGroupSeparator />

      <ButtonGroup>
        <ConnectControl />
      </ButtonGroup>
    </ButtonGroup>
  );
}
