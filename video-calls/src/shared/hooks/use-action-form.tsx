'use client';

import { useShakeError } from '@/shared/hooks';
import type { ActionResponse } from '@/shared/types';
import { useActionState, useEffect, useRef } from 'react';
import { useForm, type FieldValues } from 'react-hook-form';

export function useActionForm<
  TResponse extends ActionResponse<unknown, unknown>,
  TPayload extends FieldValues,
>(params: {
  formConfiguration: Parameters<typeof useForm<TPayload>>[number];
  action: (state: Awaited<TResponse>, payload: TPayload) => Promise<TResponse>;
  initialState: Awaited<TResponse>;
  onFormSuccess: (s: Extract<TResponse, { type: 'success' }>) => void;
  onFormError: (s: Extract<TResponse, { type: 'error' }>) => void;
  onFormPending: () => void;
}) {
  const {
    formConfiguration,
    action,
    initialState,
    onFormSuccess,
    onFormError,
    onFormPending,
  } = params;

  const methods = useForm(formConfiguration);

  const [state, dispatch, isPending] = useActionState(action, initialState);

  const animationScope = useShakeError(methods.formState.errors);

  const hadPendingRef = useRef(false);

  useEffect(() => {
    if (isPending) {
      hadPendingRef.current = true;
      onFormPending?.();
      return;
    }

    if (!hadPendingRef.current) return;

    if (state.type === 'success')
      onFormSuccess(state as Extract<TResponse, { type: 'success' }>); // Ts cannot infer in this case

    if (state.type === 'error')
      onFormError(state as Extract<TResponse, { type: 'error' }>); // Ts cannot infer in this case

    hadPendingRef.current = false;
  }, [isPending, state, onFormError, onFormPending, onFormSuccess]);

  return {
    methods,
    state,
    isPending,
    dispatch,
    animationScope,
  };
}
