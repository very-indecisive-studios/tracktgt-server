import { Button, Container, TextInput, Title, Text, PasswordInput, Center, createStyles } from "@mantine/core";
import { ActionFunction, json, LoaderFunction, redirect } from "@remix-run/node";
import { Form, useActionData, useTransition } from "@remix-run/react";
import { z } from "zod";
import { createUserSession, getUserId } from "~/utils/session.server";
import { login } from "auth";

interface ActionData {
    email?: string;
    password?: string;
    formError?: string;
}

const badRequest = (data: any) => json(data, {status: 400});

export const loader: LoaderFunction = async ({request}) => {
    // Redirect to home if user is signed in.
    const userId = await getUserId(request);
    if (userId) {
        return redirect("/home")
    }

    return null;
}

export const action: ActionFunction = async ({request}) => {
    let formData = Object.fromEntries(await request.formData());

    // Validate form.
    const formDataSchema = z
        .object({
            email: z.string().email("Invalid email."),
            password: z.string().min(6, "Password must be at least 6 characters."),
        });

    const parsedFormData = formDataSchema.safeParse(formData);

    if (!parsedFormData.success) {
        return badRequest(parsedFormData.error.flatten().fieldErrors);
    }

    const {email, password} = parsedFormData.data;

    // Login with Firebase.
    const authResult = await login(email, password);

    if (!authResult.userId || authResult.error) {
        return ({formError: authResult.error ?? "Error occured while logging in."});
    }

    return createUserSession(authResult.userId, "/home");
}

const useStyles = createStyles((theme, _params, getRef) => ({
    loginContainer: {
        width: "100vw",
        height: "100vh"
    }
}));

export default function Login() {
    const actionData = useActionData<ActionData>()
    const transition = useTransition()
    const { classes } = useStyles();
    
    return (
        <>
            <Center className={classes.loginContainer}>
                <Container size={"xs"}>
                        <Title mb={24} order={1}>Welcome back to tracktgt</Title>
                        <Form method="post">
                            <TextInput mt={16} name="email" label="Email address" type="email" error={actionData?.email}/>
                            <PasswordInput mt={16} name="password" label="Password" error={actionData?.password}/>
                            <Button mt={16} type="submit" loading={transition.state === "submitting"}>Login</Button>
                        </Form>
                        <Text hidden={!(actionData?.formError)} color={"red"}>{actionData?.formError}</Text>
                </Container>
            </Center>

        </>
    )
}