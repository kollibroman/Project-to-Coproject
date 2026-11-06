import { ActionIcon, Indicator, Tooltip } from '@mantine/core';
import { BellIcon } from '@phosphor-icons/react';
import { useNavigate } from 'react-router';
import { useMyInvitations } from '../hooks';
import { INVITATIONS_ANCHOR } from '../Features/Account/Components/InvitationsSection.tsx';

/** Dzwonek w nagłówku. Widoczny tylko wtedy, gdy użytkownik ma oczekujące zaproszenia. */
export const InvitationsBell = () => {
    const { data } = useMyInvitations();
    const navigate = useNavigate();
    const count = data?.length ?? 0;

    if (count === 0) {
        return null;
    }

    const label = count === 1 ? 'Masz zaproszenie do projektu' : `Masz zaproszenia do projektów: ${count}`;

    return (
        <Tooltip label={label}>
            <Indicator label={count} size={16} color="red" offset={4}>
                <ActionIcon
                    variant="subtle"
                    size="lg"
                    aria-label={label}
                    onClick={() => navigate(`/account#${INVITATIONS_ANCHOR}`)}
                >
                    <BellIcon style={{ width: '70%', height: '70%' }} />
                </ActionIcon>
            </Indicator>
        </Tooltip>
    );
};
