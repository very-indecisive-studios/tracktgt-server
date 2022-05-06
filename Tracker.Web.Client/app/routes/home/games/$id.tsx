import { Container, Title } from "@mantine/core";
import { json, LoaderFunction } from "@remix-run/node";
import { useLoaderData } from "@remix-run/react";
import { backendAPIClientInstance, BackendAPIException, GetGameResult } from "../../../../backend";

export const loader: LoaderFunction = async ({ params }) => {
    try {
        const id: number = parseInt(params.id ?? "0");

        const backendResult = await backendAPIClientInstance.game_GetGame(id);

        if (backendResult.status === 200) {
            return json(backendResult.result);
        }
    } catch(err) {
        const backendError = err as BackendAPIException

        return ({ formError: backendError.result ?? "Error occured while registering." });
    }    
}

export default function Game() {
    const game = useLoaderData<GetGameResult>();
    
    console.log(game);
    
    return (
        <Container py={16}>
            <Title order={2}>{game.title}</Title>
        </Container>
    );
}