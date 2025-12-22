import { stagger, useAnimate } from 'framer-motion';
import { useEffect } from 'react';

export function useShakeError(error: unknown) {
  const [scope, animate] = useAnimate<HTMLFormElement>();

  useEffect(() => {
    if (!error) return;
    const elements = scope.current?.querySelectorAll(
      'input.error, textarea.error, label.error, div.error'
    );
    if (!elements || elements.length === 0) return;

    animate(
      elements,
      { x: [0, -10, 0, 10, 0] },
      { type: 'keyframes', duration: 0.5, delay: stagger(0.2) }
    );
  }, [animate, error, scope]);

  return scope;
}
