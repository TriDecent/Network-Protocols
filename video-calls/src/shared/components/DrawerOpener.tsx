import type { ReactNode } from 'react';
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from './ui';

type DrawerOpenerProps = {
  title: string;
  description: string;
  trigger: ReactNode;
  action?: ReactNode;
  children: ReactNode;
};

export function DrawerOpener(props: DrawerOpenerProps) {
  return (
    <Drawer>
      <DrawerTrigger asChild>{props.trigger}</DrawerTrigger>
      <DrawerContent>
        <DrawerHeader>
          <DrawerTitle>{props.title}</DrawerTitle>
          <DrawerDescription>{props.description}</DrawerDescription>
        </DrawerHeader>

        {props.children}

        {props.action ? <DrawerFooter>{props.action}</DrawerFooter> : null}
      </DrawerContent>
    </Drawer>
  );
}
