import { LoaderFunction } from "@remix-run/node";
import { requireUserId } from "~/utils/session.server";
import React, { forwardRef, useState } from 'react';
import {
    AppShell,
    Header,
    Image,
    Text,
    MediaQuery,
    Burger,
    useMantineTheme,
    TextInput,
    Grid,
} from '@mantine/core';
import { Search } from "tabler-icons-react";
import AppNavbar from "~/components/AppNavbar";
import { Outlet } from "@remix-run/react";
import SearchBar from "~/components/SearchBar";

export const loader: LoaderFunction = async ({request}) => {
    // Redirect to login if user is signed in.
    const userId = await requireUserId(request);
    
    // Get user profile.
    return null;
}

export default function Home() {
    const theme = useMantineTheme();
    const [opened, setOpened] = useState(false);

    return (
        <>
            <AppShell
                styles={{
                    main: {
                        background: theme.colorScheme === 'dark' ? theme.colors.dark[8] : theme.colors.gray[0],
                    },
                }}
                navbarOffsetBreakpoint="sm"
                asideOffsetBreakpoint="sm"
                fixed
                navbar={
                    <AppNavbar opened={opened} userName={"User name"} profileImageURL={""} />
                }
                header={
                    <Header height={70} p="md">
                        <MediaQuery smallerThan="sm" styles={{display: 'none'}}>
                            <Grid columns={12} align={"center"}>
                                <Grid.Col span={3}>
                                    <Image fit={"contain"} height={32} width={64} src="/logo_icon.svg">tracktgt</Image>
                                </Grid.Col>
                                <Grid.Col span={6}>
                                    <SearchBar />
                                </Grid.Col>
                                <Grid.Col span={3}>
                                </Grid.Col>
                            </Grid>
                        </MediaQuery>
                        <MediaQuery largerThan="sm" styles={{display: 'none'}}>
                            <Grid columns={12} align={"center"}>
                                <Grid.Col span={1}>
                                    <Burger
                                        opened={opened}
                                        onClick={() => setOpened((o) => !o)}
                                        size="sm"
                                        color={theme.colors.gray[6]}
                                        mr="xl"
                                    />
                                </Grid.Col>
                                <Grid.Col span={2}>
                                    <Image fit={"contain"} height={32} width={64} src="/logo_icon.svg">tracktgt</Image>
                                </Grid.Col>
                                <Grid.Col span={4}>
                                </Grid.Col>
                            </Grid>
                        </MediaQuery>
                    </Header>
                }
            >
                <Outlet />
            </AppShell>
        </>
    );
}