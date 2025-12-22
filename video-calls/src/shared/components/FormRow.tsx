import {
  type ComponentProps,
  type InputHTMLAttributes,
  type ReactNode,
  type TextareaHTMLAttributes,
} from 'react';
import { cn } from '../utils/utils';
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

type FormRowProps =
  | ({
      kind: 'input';
    } & ComponentProps<typeof InputField>)
  | ({
      kind: 'textarea';
    } & ComponentProps<typeof TextareaField>)
  | ({ kind: 'select' } & ComponentProps<typeof SelectField>);

export function FormRow(props: FormRowProps) {
  const { kind, ...rest } = props;

  switch (kind) {
    case 'input':
      return <InputField {...(rest as ComponentProps<typeof InputField>)} />;
    case 'textarea':
      return (
        <TextareaField {...(rest as ComponentProps<typeof TextareaField>)} />
      );
    case 'select':
      return <SelectField {...(rest as ComponentProps<typeof SelectField>)} />;
    default:
      return null;
  }
}

function InputField({
  id,
  name,
  labelText,
  labelClassname,
  description,
  error,
  className,
  orientation = 'responsive',
  addons,
  ...props
}: {
  id: string;
  name: string;
  labelText: string;
  labelClassname?: string;
  description: string;
  className?: string;
  error?: string;
  orientation?: 'vertical' | 'horizontal' | 'responsive';
  addons: ReactNode;
} & InputHTMLAttributes<HTMLInputElement>) {
  return (
    <Field orientation={orientation}>
      <FieldLabel htmlFor={id} className={cn('max-w-40', labelClassname)}>
        {labelText}
      </FieldLabel>
      <FieldContent>
        <InputGroup
          className={cn(
            'read-only:cursor-not-allowed',
            error ? 'border-destructive error' : '',
            className
          )}
        >
          <InputGroupInput id={id} name={name} {...props} />
          {addons}
        </InputGroup>

        {description ? (
          <FieldDescription>{description}</FieldDescription>
        ) : null}
        {error ? <FieldError>{error}</FieldError> : null}
      </FieldContent>
    </Field>
  );
}

function TextareaField({
  id,
  name,
  labelText,
  description,
  error,
  labelClassname,
  className,
  orientation = 'responsive',
  addons,
  ...props
}: {
  id: string;
  name: string;
  labelText: string;
  labelClassname?: string;
  description: string;
  className?: string;
  error?: string;
  orientation?: 'vertical' | 'horizontal' | 'responsive';
  addons: ReactNode;
} & TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return (
    <Field orientation={orientation}>
      <FieldLabel htmlFor={id} className={cn('max-w-40', labelClassname)}>
        {labelText}
      </FieldLabel>
      <FieldContent>
        <InputGroup
          className={cn(
            'read-only:cursor-not-allowed',
            error ? 'border-destructive error' : '',
            className
          )}
        >
          <InputGroupTextarea id={id} name={name} {...props} />
          {addons}
        </InputGroup>

        {description ? (
          <FieldDescription>{description}</FieldDescription>
        ) : null}
        {error ? <FieldError>{error}</FieldError> : null}
      </FieldContent>
    </Field>
  );
}

function SelectField({
  id,
  name,
  labelText,
  placeholder,
  renderSelectItems,
  description,
  className,
  labelClassName,
  error,
  orientation = 'responsive',
  ...props
}: {
  id: string;
  name: string;
  labelText: string;
  placeholder: string;
  renderSelectItems: ReactNode;
  description?: string;
  className?: string;
  labelClassName?: string;
  error?: string;
  orientation?: 'vertical' | 'horizontal' | 'responsive';
} & ComponentProps<typeof Select>) {
  return (
    <Field orientation={orientation}>
      <FieldLabel htmlFor={id} className={cn('max-w-40', labelClassName)}>
        {labelText}
      </FieldLabel>
      <FieldContent>
        <Select name={name} {...props}>
          <SelectTrigger
            id={id}
            className={cn(
              'w-full',
              error ? 'border-destructive error' : '',
              className
            )}
          >
            <SelectValue placeholder={placeholder} />
          </SelectTrigger>
          <SelectContent>{renderSelectItems}</SelectContent>
        </Select>
        {description ? (
          <FieldDescription>{description}</FieldDescription>
        ) : null}
        {error ? <FieldError>{error}</FieldError> : null}
      </FieldContent>
    </Field>
  );
}
