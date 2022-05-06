import { Button, Center, Container, createStyles, PasswordInput, Text, TextInput, Title } from "@mantine/core";
import { ActionFunction, json, LoaderFunction, redirect } from "@remix-run/node";
import { Form, useActionData, useTransition } from "@remix-run/react";
import { createUserSession, getUserId } from "~/utils/session.server";
import { register } from "auth";
import { z } from "zod";
import { backendAPIClientInstance, BackendAPIException, CheckUserExistQuery, RegisterUserCommand } from "backend";

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
    
    // Validate form.
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

    // Validate user name and email with server.
    try {
        const backendResult = await backendAPIClientInstance.user_CheckUserExist(new CheckUserExistQuery({
            email: email,
            userName: userName,
        }));

        if (backendResult.status !== 200) {
            return ({ formError: backendResult.result ?? "Error occured while registering." });
        }
        
        const { isUserNameTaken, isEmailTaken } =  backendResult.result;
        
        if (isUserNameTaken || isEmailTaken) {
            return {
                userName: isUserNameTaken ? "Username taken." : null,
                email: isEmailTaken ? "Email taken." : null
            }
        }
    } catch(err) {
        const backendError = err as BackendAPIException

        return ({ formError: backendError.result ?? "Error occured while registering." });
    }
    
    // Register with Firebase.
    const authResult = await register(email, password);
    
    if (!authResult.userId || authResult.error) {
        return ({ formError: authResult.error ?? "Error occured while registering." });
    } 
    
    // Register with server.
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

const useStyles = createStyles((theme, _params, getRef) => ({
    signUpContainer: {
        width: "100vw",
        height: "100vh"
    }
}));

export default function SignUp() {
    const actionData = useActionData<ActionData>()
    const transition = useTransition()
    const { classes } = useStyles();

    return (
        <>
            <Center className={classes.signUpContainer}>
                <Container size={"xs"}>
                    <Title mb={24} order={1}>Create a tracktgt account</Title>
                    <Form method="post">
                        <TextInput name="userName" label="Username" type="text" error={actionData?.userName}/>
                        <TextInput mt={16} name="email" label="Email address" type="email" error={actionData?.email}/>
                        <PasswordInput mt={16} name="password" label="Password" error={actionData?.password} />
                        <PasswordInput mt={16} name="confirmPassword" label="Confirm Password" error={actionData?.confirmPassword} />
                        <Button mt={16} type="submit" loading={transition.state === "submitting"}>Sign up</Button>
                    </Form>
                    <Text hidden={!(actionData?.formError)} color={"red"}>{actionData?.formError}</Text>
                </Container>
            </Center>
        </>
    );
}