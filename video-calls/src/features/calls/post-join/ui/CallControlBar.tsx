import { ButtonGroup, ButtonGroupSeparator } from '@/shared/components/ui';
import {
  CameraControl,
  DisconnectControl,
  MicrophoneControl,
  ScreenShareAudioControl,
  ScreenShareControl,
  SpeakerControl,
} from '../control';

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
