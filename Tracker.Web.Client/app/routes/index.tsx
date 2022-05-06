import { Button, Header, Image, Text, Title, useMantineTheme, createStyles, Autocomplete } from "@mantine/core";
import { LoaderFunction, redirect } from "@remix-run/node";
import { Link } from "@remix-run/react";
import { ArrowRight } from "tabler-icons-react";
import { getUserId } from "~/utils/session.server";

const useStyles = createStyles((theme, _params, getRef) => ({
	header: {
		position: "fixed",
		width: "100%"
	},
	headerContainer: {
		display: "flex",
		width: "100%",
		flexDirection: "row",
		alignItems: "center"
	},
	headerButtons: {
		display: "grid",
		gridAutoFlow: "column",
		gridColumnGap: "12px",
		marginLeft: "auto"
	},
	hero: {
		position: "relative",
		width: "100vw",
		height: "100vh",
		bottom: "0px",
		zIndex: -100
	},
	heroContent: {
		position: "absolute",
		width: "100vw",
		bottom: "96px",
		display: "flex",
		flexDirection: "column",
		alignItems: "center"
	}
}));

export const loader: LoaderFunction = async ({ request }) => {
    // Redirect to home if user is signed in.
    const userId = await getUserId(request);
    if (userId) {
        return redirect("/home")
    }

    return null;
}

function Hero() {
	const theme = useMantineTheme();
	const { classes } = useStyles();

	return (
		<div className={classes.hero}>
			<div className={classes.heroContent}>
				<Title order={1} align="center">Never miss what you want to watch</Title>
				<Button mt={16} rightIcon={<ArrowRight size={18} />}>Get Started</Button>
			</div>
		</div>
	);
}

export default function Index() {
	const { classes } = useStyles();

	return (
		<>
			<div className={classes.header}>
				<Header height={72} py="md" px="xl">
					<div className={classes.headerContainer}>
						<Image height={32} src="/logo.svg">tracktgt</Image>
					
						<div className={classes.headerButtons}>
							<Button component={Link} to="/login" variant="outline">Login</Button>
							<Button component={Link} to="/signup">Sign Up</Button>
						</div>
					</div>
				</Header>
			</div>

			<Hero/>
		</>
	);
}
