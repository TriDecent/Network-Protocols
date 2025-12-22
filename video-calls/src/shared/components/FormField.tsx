'use client';

import { type InputHTMLAttributes, type TextareaHTMLAttributes } from 'react';
import {
  Controller,
  type FieldError as FieldErrorType,
  useFormContext,
} from 'react-hook-form';
import { cn } from '../utils/utils';
import {
  Checkbox,
  Item,
  ItemActions,
  ItemContent,
  ItemDescription,
  ItemTitle,
  Label,
} from './ui';
import {
  Field,
  FieldContent,
  FieldDescription,
  FieldError,
  FieldLabel,
} from './ui/field';
import {
  InputGroup,
  InputGroupInput,
  InputGroupTextarea,
} from './ui/input-group';
import { Select, SelectContent, SelectTrigger, SelectValue } from './ui/select';

type FieldOrientation = 'vertical' | 'horizontal' | 'responsive';

interface BaseFieldProps {
  id: string;
  name: string;
  labelText: string;
  labelClassName?: string;
  description?: string;
  className?: string;
  orientation?: FieldOrientation;
  addons?: React.ReactNode;
}

type InputFieldProps = BaseFieldProps & InputHTMLAttributes<HTMLInputElement>;
type TextareaFieldProps = BaseFieldProps &
  TextareaHTMLAttributes<HTMLTextAreaElement>;

export type SelectFieldProps<TSelectProps = Record<string, unknown>> =
  BaseFieldProps &
    TSelectProps & {
      disabled?: boolean;
      placeholder?: string;
      children?: React.ReactNode;
    };

type CheckboxFieldProps = BaseFieldProps &
  React.ComponentPropsWithoutRef<typeof Checkbox>;

export type FormFieldProps<TSelectProps = Record<string, unknown>> =
  | ({ kind: 'input' } & InputFieldProps)
  | ({ kind: 'textarea' } & TextareaFieldProps)
  | ({ kind: 'select' } & SelectFieldProps<TSelectProps>)
  | ({ kind: 'checkbox' } & CheckboxFieldProps);

export function FormField(props: FormFieldProps) {
  const { kind, ...rest } = props;

  switch (kind) {
    case 'input':
      return <ControlledInputField {...(rest as InputFieldProps)} />;
    case 'textarea':
      return <ControlledTextareaField {...(rest as TextareaFieldProps)} />;
    case 'select':
      return <ControlledSelectField {...(rest as SelectFieldProps)} />;
    case 'checkbox':
      return <ControlledCheckboxField {...(rest as CheckboxFieldProps)} />;
    default:
      return null;
  }
}

function ControlledInputField({ ...props }: InputFieldProps) {
  const { control } = useFormContext();
  const { labelText, ...rest } = props;

  return (
    <Controller
      name={props.name}
      control={control}
      render={({ field, fieldState, formState }) => (
        <FieldWrapper {...props} labelText={labelText} error={fieldState.error}>
          <InputGroup
            className={cn(
              'read-only:cursor-not-allowed',
              fieldState.invalid ? 'error' : '',
              props.className
            )}
          >
            <InputGroupInput
              {...field}
              {...rest}
              id={props.id}
              name={props.name}
              disabled={props.disabled || formState.isSubmitting}
              aria-invalid={fieldState.invalid}
            />
            {props.addons}
          </InputGroup>
        </FieldWrapper>
      )}
    />
  );
}

function ControlledTextareaField({ ...props }: TextareaFieldProps) {
  const { control } = useFormContext();
  const { labelText, ...rest } = props;

  return (
    <Controller
      name={props.name}
      control={control}
      render={({ field, fieldState, formState }) => (
        <FieldWrapper {...props} labelText={labelText} error={fieldState.error}>
          <InputGroup
            className={cn(
              'read-only:cursor-not-allowed',
              fieldState.invalid ? 'error' : '',
              props.className
            )}
          >
            <InputGroupTextarea
              {...field}
              {...rest}
              id={props.id}
              name={props.name}
              disabled={props.disabled || formState.isSubmitting}
              aria-invalid={fieldState.invalid}
            />
            {props.addons}
          </InputGroup>
        </FieldWrapper>
      )}
    />
  );
}

function ControlledSelectField({ ...props }: SelectFieldProps) {
  const { control } = useFormContext();
  const { labelText, ...rest } = props;

  return (
    <Controller
      name={props.name}
      control={control}
      render={({ field, fieldState, formState }) => (
        <FieldWrapper
          {...props}
          id={props.id}
          name={props.name}
          error={fieldState.error}
          labelText={labelText}
        >
          <Select
            {...field}
            {...rest}
            name={props.name}
            onValueChange={field.onChange}
            defaultValue={field.value}
          >
            <SelectTrigger
              id={props.id}
              name={props.name}
              disabled={props.disabled || formState.isSubmitting}
              aria-invalid={fieldState.invalid}
              className={cn(
                'w-full',
                fieldState.invalid ? 'error' : '',
                props.className
              )}
            >
              <SelectValue placeholder={props.placeholder} />
            </SelectTrigger>
            <SelectContent>{props.children}</SelectContent>
          </Select>
        </FieldWrapper>
      )}
    />
  );
}

function ControlledCheckboxField({
  orientation = 'responsive',
  ...props
}: CheckboxFieldProps) {
  const { control } = useFormContext();
  const { labelText, ...rest } = props;

  return (
    <Controller
      name={props.name}
      control={control}
      render={({ field, fieldState, formState }) => (
        <Field orientation={orientation} data-invalid={fieldState.invalid}>
          <FieldContent>
            <Item asChild>
              <Label
                htmlFor={props.id}
                className={cn(
                  'bg-muted/50 cursor-pointer items-start rounded-md transition-colors',
                  fieldState.invalid ? 'error border-destructive border' : '',
                  props.disabled || formState.isSubmitting ? 'opacity-70' : '',
                  props.className
                )}
              >
                <ItemActions>
                  <Checkbox
                    {...rest}
                    id={props.id}
                    checked={field.value}
                    onCheckedChange={field.onChange}
                    disabled={props.disabled || formState.isSubmitting}
                    aria-invalid={fieldState.invalid}
                  />
                </ItemActions>

                <ItemContent>
                  <ItemTitle className='leading-none'>{labelText}</ItemTitle>
                  <ItemDescription>{props.description}</ItemDescription>

                  {fieldState.error ? (
                    <FieldError errors={[fieldState.error]} />
                  ) : null}
                </ItemContent>
              </Label>
            </Item>
          </FieldContent>
        </Field>
      )}
    />
  );
}

function FieldWrapper({
  id,
  labelText,
  labelClassName,
  description,
  error,
  orientation = 'responsive',
  children,
}: BaseFieldProps & { error?: FieldErrorType; children: React.ReactNode }) {
  return (
    <Field orientation={orientation} data-invalid={!!error}>
      <FieldLabel htmlFor={id} className={cn('max-w-40', labelClassName)}>
        {labelText}
      </FieldLabel>
      <FieldContent>
        {children}

        {description ? (
          <FieldDescription>{description}</FieldDescription>
        ) : null}
        {error ? <FieldError errors={[error]} /> : null}
      </FieldContent>
    </Field>
  );
}
