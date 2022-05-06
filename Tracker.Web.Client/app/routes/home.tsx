import { LoaderFunction } from "@remix-run/node";
import { requireUserId } from "~/utils/session.server";

export const loader: LoaderFunction = async ({ request }) => {
    // Redirect to login if user is signed in.
    await requireUserId(request);

    return null;
}

export default function Home() {
    return (
        <>
            Home
        </>
    );
}