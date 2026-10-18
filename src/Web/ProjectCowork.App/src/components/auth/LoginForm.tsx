import {
  Alert,
  Button,
  PasswordInput,
  Stack,
  TextInput,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { useLogin } from '../../hooks';

export type LoginFormValues = {
  email: string;
  password: string;
};

type LoginFormProps = {
  /** Wywoływane po udanym zalogowaniu. */
  onSuccess?: () => void;
};

export const LoginForm = ({ onSuccess }: LoginFormProps) => {
  const login = useLogin();

  const form = useForm<LoginFormValues>({
    initialValues: {
      email: '',
      password: '',
    },
    validate: {
      email: (value) =>
        /^\S+@\S+\.\S+$/.test(value) ? null : 'Nieprawidłowy adres e-mail',
      password: (value) =>
        value.length >= 6 ? null : 'Hasło musi mieć min. 6 znaków',
    },
  });

  const handleSubmit = form.onSubmit((values) => {
    login.mutate(
      { body: values },
      {
        onSuccess: () => onSuccess?.(),
      },
    );
  });

  return (
    <form onSubmit={handleSubmit}>
      <Stack gap="md">
        {login.isError && (
          <Alert color="red" title="Błąd logowania">
            {login.error instanceof Error
              ? login.error.message
              : 'Nie udało się zalogować. Sprawdź dane i spróbuj ponownie.'}
          </Alert>
        )}

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

        <Button type="submit" fullWidth loading={login.isPending}>
          Zaloguj się
        </Button>
      </Stack>
    </form>
  );
};
