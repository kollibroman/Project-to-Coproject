import '@mantine/core/styles.css'
import {Button, Container, MantineProvider, Title} from '@mantine/core';

function App() {
  return (
      <MantineProvider>
          <Container p="md">
              <Title order={1}>Cześć, Mantine działa!</Title>
              <Button mt="sm" color="blue">Przycisk</Button>
          </Container>
      </MantineProvider>
  )
}

export default App
