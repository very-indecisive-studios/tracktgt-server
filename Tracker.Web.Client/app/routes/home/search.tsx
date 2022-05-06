import { Group, Title, Text, Container, Stack, Card, Button } from "@mantine/core";
import { json, LoaderFunction } from "@remix-run/node";
import {
    backendAPIClientInstance,
    BackendAPIException,
    RegisterUserCommand,
    SearchGamesResult
} from "../../../backend";
import { createUserSession } from "~/utils/session.server";
import { useLoaderData, useSearchParams } from "@remix-run/react";

export const loader: LoaderFunction = async ({ request }) => {
    try {
        const url = new URL(request.url);
        const title = url.searchParams.get("title");
        
        const backendResult = await backendAPIClientInstance.game_SearchGames(title);

        if (backendResult.status === 200) {
            return json(backendResult.result);
        }
    } catch(err) {
        const backendError = err as BackendAPIException

        return ({ formError: backendError.result ?? "Error occured while registering." });
    }
}

interface SearchResultItemProps {
    title: string
}

function SearchResultItem({ title }: SearchResultItemProps) {
    return (
        <div style={{ width: "100%", margin: 'auto' }}>
            <Card shadow="xs" p="lg">
                <Group>
                    <Title order={4}>{title}</Title>
                </Group>
            </Card>
        </div>
    );
}

export default function Search() {
    const searchResults = useLoaderData<SearchGamesResult>();
    const [searchParams, _] = useSearchParams();
    const title = searchParams.get("title");


    return (
        <Container>
            <Title my={16} order={2}>Search results for "{title}"</Title>
            <Stack>
                {searchResults?.games?.map(g => (
                    <SearchResultItem title={g.title ?? ""} />
                ))}
            </Stack>
        </Container>
    );
}