import {
  Alert,
  Button,
  PasswordInput,
  SegmentedControl,
  Stack,
  TextInput,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { useState } from 'react';
import { useRegisterBusiness, useRegisterUser } from '../../hooks';

type AccountType = 'user' | 'business';

export type RegisterFormValues = {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
};

type RegisterFormProps = {
  /** Wywoływane po udanej rejestracji. */
  onSuccess?: () => void;
};

export const RegisterForm = ({ onSuccess }: RegisterFormProps) => {
  const [accountType, setAccountType] = useState<AccountType>('user');
  const registerUser = useRegisterUser();
  const registerBusiness = useRegisterBusiness();

  const mutation = accountType === 'business' ? registerBusiness : registerUser;

  const form = useForm<RegisterFormValues>({
    initialValues: {
      username: '',
      email: '',
      password: '',
      confirmPassword: '',
    },
    validate: {
      username: (value) =>
        value.trim().length >= 3 ? null : 'Nazwa musi mieć min. 3 znaki',
      email: (value) =>
        /^\S+@\S+\.\S+$/.test(value) ? null : 'Nieprawidłowy adres e-mail',
      password: (value) =>
        value.length >= 6 ? null : 'Hasło musi mieć min. 6 znaków',
      confirmPassword: (value, values) =>
        value === values.password ? null : 'Hasła nie są takie same',
    },
  });

  const handleSubmit = form.onSubmit((values) => {
    mutation.mutate(
      {
        body: {
          username: values.username,
          email: values.email,
          password: values.password,
        },
      },
      {
        onSuccess: () => onSuccess?.(),
      },
    );
  });

  return (
    <form onSubmit={handleSubmit}>
      <Stack gap="md">
        {mutation.isError && (
          <Alert color="red" title="Błąd rejestracji">
            {mutation.error instanceof Error
              ? mutation.error.message
              : 'Nie udało się zarejestrować. Spróbuj ponownie.'}
          </Alert>
        )}

        <SegmentedControl
          fullWidth
          value={accountType}
          onChange={(value) => setAccountType(value as AccountType)}
          data={[
            { label: 'Użytkownik', value: 'user' },
            { label: 'Firma', value: 'business' },
          ]}
        />

        <TextInput
          label="Nazwa użytkownika"
          placeholder="Twoja nazwa"
          withAsterisk
          {...form.getInputProps('username')}
        />

        <TextInput
          label="E-mail"
          placeholder="twoj@email.com"
          withAsterisk
          {...form.getInputProps('email')}
        />

        <PasswordInput
          label="Hasło"
          placeholder="Twoje hasło"
          withAsterisk
          {...form.getInputProps('password')}
        />

        <PasswordInput
          label="Powtórz hasło"
          placeholder="Powtórz hasło"
          withAsterisk
          {...form.getInputProps('confirmPassword')}
        />

        <Button type="submit" fullWidth loading={mutation.isPending}>
          Zarejestruj się
        </Button>
      </Stack>
    </form>
  );
};
