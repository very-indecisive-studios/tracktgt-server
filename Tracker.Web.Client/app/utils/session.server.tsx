import { createCookieSessionStorage, redirect } from "@remix-run/node";

let sessionSecret = process.env.SESSION_SECRET;
if (!sessionSecret) {
	throw new Error("SESSION_SECRET must be set");
}

let { getSession, commitSession, destroySession } = createCookieSessionStorage({
	cookie: {
		name: "tracktgt_session",
		secure: true,
		secrets: [sessionSecret],
		sameSite: "lax",
		path: "/",
		maxAge: 60 * 60 * 24 * 30,
		httpOnly: true,
	},
});

export function getUserSession(request: Request) {
	return getSession(request.headers.get("Cookie"));
}

export async function getUserId(request: Request) {
	let session = await getUserSession(request);
	let userId = session.get("userId");
	if (!userId || typeof userId !== "string") return null;
	return userId;
}

export async function requireUserId(request: Request) {
	let session = await getUserSession(request);
	let userId = session.get("userId");
	if (!userId || typeof userId !== "string") throw redirect("/login");
	return userId;
}

export async function endUserSession(request: Request) {
	let session = await getSession(request.headers.get("Cookie"));
	return redirect("/login", {
		headers: { "Set-Cookie": await destroySession(session) },
	});
}

export async function createUserSession(userId: string, redirectTo: string) {
	let session = await getSession();
	session.set("userId", userId);
	return redirect(redirectTo, {
		headers: { "Set-Cookie": await commitSession(session) },
	});
}
