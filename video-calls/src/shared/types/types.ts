export type OldActionResponse<TInput, TError> =
  | Result<
      { userEntered: null; errors: null },
      {
        userEntered: TInput;
        errors: TError;
      }
    >
  | Readonly<{
      type: 'initial';
      payload: Readonly<{
        userEntered: null;
        errors: null;
      }>;
    }>;

export type Result<TSuccess, TError> = Readonly<
  { type: 'success'; payload: TSuccess } | { type: 'error'; payload: TError }
>;

export type ActionResponse<TSuccess, TError> = Readonly<
  | { type: 'success'; payload: TSuccess }
  | { type: 'error'; payload: TError }
  | { type: 'initial' }
>;
