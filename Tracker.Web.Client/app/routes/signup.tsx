import { Button, Container, Text, TextInput, Title } from "@mantine/core";
import { ActionFunction, json, LoaderFunction, redirect } from "@remix-run/node";
import { Form, useActionData } from "@remix-run/react";
import { createUserSession, getUserId } from "~/utils/session.server";
import { AuthResult, register } from "auth";
import { z } from "zod";
import { backendAPIClientInstance, BackendAPIException, RegisterUserCommand } from "backend";

interface ActionData {
    userName?: string;
    email?: string;
    password?: string;
    confirmPassword?: string;
    formError?: string;
}

const badRequest = (data: any) => json(data, { status: 400 });

export const loader: LoaderFunction = async ({ request }) => {
    // Redirect to home if user is signed in.
    const userId = await getUserId(request);
    if (userId) {
        return redirect("/home")
    }

    return null;
}

export const action: ActionFunction = async ({ request }) => {
    let formData = Object.fromEntries(await request.formData());

    const formDataSchema = z
        .object({
            userName: z.string(),
            email: z.string().email("Invalid email."),
            password: z.string().min(6, "Password must be at least 6 characters."),
            confirmPassword: z.string().min(6, "Password must be at least 6 characters.")
        })
        .refine((data) => data.password === data.confirmPassword, { 
            message: "Passwords don't match.",
            path: ["confirmPassword"]
        });
    
    const parsedFormData = formDataSchema.safeParse(formData);
    
    if (!parsedFormData.success) {
        return badRequest(parsedFormData.error.flatten().fieldErrors);
    }

    const { email, password, userName } = parsedFormData.data;
    const authResult = await register(email, password);
    
    if (authResult.error || !authResult.userId) {
        return ({ formError: authResult.error ?? "Error occured while registering." });
    } 

    try {
        const backendResult = await backendAPIClientInstance.user_RegisterUser(new RegisterUserCommand({
            remoteUserId: authResult.userId,
            email: email,
            userName: userName,
        }));

        if (backendResult.status === 200) {
            return createUserSession(authResult.userId, "/home");    
        }

        return ({ formError: backendResult.result ?? "Error occured while registering." });

    } catch(err) {
        const backendError = err as BackendAPIException

        return ({ formError: backendError.result ?? "Error occured while registering." });
    }
}

export default function SignUp() {
    const actionData = useActionData<ActionData>()

    return (
        <>
            <Container size={"xs"}>
                <Title mb={24} order={1}>Create a tracktgt account</Title>
                <Form method="post">
                    <TextInput name="userName" label="Username" type="text" error={actionData?.userName}/>
                    <TextInput mt={16} name="email" label="Email address" type="email" error={actionData?.email}/>
                    <TextInput mt={16} name="password" label="Password" type="password" error={actionData?.password} />
                    <TextInput mt={16} name="confirmPassword" label="Confirm Password" type="password" error={actionData?.confirmPassword} />
                    <Button type="submit" mt={16}>Sign up</Button>
                </Form>
                <Text hidden={!(actionData?.formError)} color={"red"}>{actionData?.formError}</Text>
            </Container>
        </>
    );
}