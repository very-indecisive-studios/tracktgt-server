import { forwardRef } from "react";
import { Avatar, Grid, Group, Menu, Navbar, UnstyledButton, Text, ThemeIcon, Divider } from "@mantine/core";
import { Book2, ChevronRight, DeviceGamepad, DeviceTv, Logout, Search, Social } from "tabler-icons-react";
import { Link } from "@remix-run/react";

interface NavbarLinkProps {
    icon: React.ReactElement;
    label: string;
    color: string;
    to: string
}

function NavbarLink({ icon, label, color, to }: NavbarLinkProps) {
    return (
        <UnstyledButton
            component={Link}
            to={to}
            sx={(theme) => ({
                display: 'block',
                width: '100%',
                padding: theme.spacing.xs,
                borderRadius: theme.radius.sm,
                color: theme.colorScheme === 'dark' ? theme.colors.dark[0] : theme.black,

                '&:hover': {
                    backgroundColor:
                        theme.colorScheme === 'dark' ? theme.colors.dark[6] : theme.colors.gray[0],
                },
            })}
        >
            <Group>
                <ThemeIcon color={color} variant="light">
                    {icon}
                </ThemeIcon>

                <Text size="sm">{label}</Text>
            </Group>
        </UnstyledButton>
    );
}

interface UserButtonProps extends React.ComponentPropsWithoutRef<'button'> {
    profileImageURL: string;
    userName: string;
}

const UserButton = forwardRef<HTMLButtonElement, UserButtonProps>(
    ({ profileImageURL, userName, ...others }: UserButtonProps, ref) => (
        <UnstyledButton
            ref={ref}
            sx={(theme) => ({
                display: 'block',
                width: '100%',
                padding: theme.spacing.md,
                color: theme.colorScheme === 'dark' ? theme.colors.dark[0] : theme.black,

                '&:hover': {
                    backgroundColor:
                        theme.colorScheme === 'dark' ? theme.colors.dark[8] : theme.colors.gray[0],
                },
            })}
            {...others}
        >
            <Group>
                <Avatar src={profileImageURL} radius="xl" />
                
                <div style={{ flex: 1 }}>
                    <Text px={4} size="sm" weight={500}>
                        {userName}
                    </Text>
                </div>
                
                <ChevronRight size={18} />
            </Group>
        </UnstyledButton>
    )
);

interface NavbarUserProps {
    userName: string;
    profileImageURL: string;
}

function NavbarUser({ userName, profileImageURL }: NavbarUserProps) {
    return (
        <Group>
            <Menu
                sx={{width: "100%"}}
                position="right"
                placement="end"
                control={
                    <UserButton
                        profileImageURL={profileImageURL}
                        userName={userName}
                    />
                }
            >
                <Menu.Label>Account</Menu.Label>
                <Menu.Item icon={<Logout size={14} />}>Logout</Menu.Item>
            </Menu>
        </Group>
    );
}

interface AppNavbarProps {
    opened: boolean;
    userName: string;
    profileImageURL: string;
}

export default function AppNavbar({ opened, userName, profileImageURL }: AppNavbarProps) {
    return (
        <Navbar p="md" hiddenBreakpoint="sm" hidden={!opened} width={{sm: 200, lg: 300}}>
            <Navbar p="xs" width={{ base: 300 }}>
                <Navbar.Section grow mt="xs">
                    <NavbarLink to={"/home/feed"} icon={<Social size={18} />} label={"Feed"} color={"green"}  />
                    <NavbarLink to={"/home/search"} icon={<Search size={18} />} label={"Search"} color={"indigo"}  />
                    <NavbarLink to={"/home/games"} icon={<DeviceGamepad size={18} />} label={"Games"} color={"blue"}  />
                    <NavbarLink to={"/home/shows"} icon={<DeviceTv size={18} />} label={"Shows"} color={"red"}  />
                    <NavbarLink to={"/home/books"} icon={<Book2 size={18} />} label={"Books"} color={"yellow"}  />
                </Navbar.Section>
                <Divider my="sm" />
                <Navbar.Section>
                    <NavbarUser userName={userName} profileImageURL={profileImageURL} />
                </Navbar.Section>
            </Navbar>
        </Navbar>
    );
}