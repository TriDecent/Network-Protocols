import { ButtonGroup, ButtonGroupSeparator } from '@/shared/components/ui';
import { CameraControl } from './CameraControl';
import { DisconnectControl } from './DisconnectControl';
import { MicrophoneControl } from './MicrophoneControl';
import { ScreenShareAudioControl } from './ScreenShareAudioControl';
import { ScreenShareControl } from './ScreenShareControl';
import { SpeakerControl } from './SpeakerControl';

export function CallControlBar() {
  return (
    <ButtonGroup className='space-x-8'>
      <ButtonGroup>
        <SpeakerControl />

        <ButtonGroupSeparator />

        <MicrophoneControl />

        <ButtonGroupSeparator />

        <CameraControl />
      </ButtonGroup>

      <ButtonGroupSeparator />

      <ButtonGroup>
        <ScreenShareControl />

        <ScreenShareAudioControl />
      </ButtonGroup>

      <ButtonGroupSeparator />

      <ButtonGroup>
        <DisconnectControl />
      </ButtonGroup>
    </ButtonGroup>
  );
}
