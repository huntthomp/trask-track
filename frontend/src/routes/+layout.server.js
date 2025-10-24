import { redirect } from '@sveltejs/kit';
import { setContext } from 'svelte';

const protectedRoutes = [
    "/profile",
    "/calendars",
    "/settings",
];

export async function load({ fetch, url, cookies }) {
    const path = url.pathname;
    const isProtected = protectedRoutes.some(route =>
        path === route || path.startsWith(`${route}/`)
    );

    const cookieHeader = cookies
        .getAll()
        .map(c => `${c.name}=${c.value}`)
        .join('; ');


    const res = await fetch(`${process.env.BACKEND_HOST}/account/me`, {
        credentials: 'include',
        headers: { Cookie: cookieHeader }
    });

    const user = await res.json();

    if (path === "/") {
        if (user) throw redirect(303, "/profile");
        else throw redirect(303, "/login");
    }

    if (isProtected && user == null) {
        console.log("Accessing a protected route with no credentials: " + url);
        throw redirect(303, "/login");
    }

    return { user };
}