import { MantineProvider, MantineThemeOverride } from "@mantine/core";
import type { MetaFunction } from "@remix-run/node";
import {
  Links,
  LiveReload,
  Meta,
  Outlet,
  Scripts,
  ScrollRestoration,
} from "@remix-run/react";
import styles from "~/styles/root.css";

export function links() {
	return [{ rel: "stylesheet", href: styles }];
}

const appTheme: MantineThemeOverride = {
  fontFamily: "'Work Sans'",
  headings: {
    fontFamily: "Poppins"
  },
  colorScheme: 'light',
  primaryColor: 'orange',
};

export const meta: MetaFunction = () => ({
  charset: "utf-8",
  title: "TrackTGT",
  viewport: "width=device-width,initial-scale=1",
});

export default function App() {
  return (
    <html lang="en">
      <head>
        <Meta />
        <Links />
      </head>
      <body>
        <MantineProvider withGlobalStyles withNormalizeCSS theme={appTheme}>
          <Outlet />
        </MantineProvider>
        <ScrollRestoration />
        <Scripts />
        <LiveReload />
      </body>
    </html>
  );
}
