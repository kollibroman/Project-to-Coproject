import { Center, Container, Paper, Tabs, Title } from '@mantine/core';
import { useState } from 'react';
import { useNavigate } from 'react-router';
import { LoginForm, RegisterForm } from '../Components';

export const AuthPage = () => {
  const [tab, setTab] = useState<string | null>('login');
  const navigate = useNavigate();

  const handleSuccess = () => {
    navigate('/', { replace: true });
  };

  return (
    <Center mih="100vh">
      <Container size={420} w="100%">
        <Title order={2} ta="center" mb="lg">
          ProjectCowork
        </Title>

        <Paper shadow="md" p="xl" radius="md" withBorder>
          <Tabs value={tab} onChange={setTab}>
            <Tabs.List grow mb="lg">
              <Tabs.Tab value="login">Logowanie</Tabs.Tab>
              <Tabs.Tab value="register">Rejestracja</Tabs.Tab>
            </Tabs.List>

            <Tabs.Panel value="login">
              <LoginForm onSuccess={handleSuccess} />
            </Tabs.Panel>

            <Tabs.Panel value="register">
              <RegisterForm onSuccess={handleSuccess} />
            </Tabs.Panel>
          </Tabs>
        </Paper>
      </Container>
    </Center>
  );
};
