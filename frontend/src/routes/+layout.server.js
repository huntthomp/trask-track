import { Agent } from 'undici';
import { redirect } from '@sveltejs/kit';
import { setContext } from 'svelte';
import { PUBLIC_BACKEND_HOST } from '$env/static/public';

const agent = new Agent({ keepAliveTimeout: 1000, keepAliveMaxTimeout: 1000, connect: { rejectUnauthorized: false } });

const unprotectedRoutes = [
  "/",
  "/login",
];

export async function load({ fetch, url }) {
  const res = await fetch(`${PUBLIC_BACKEND_HOST}/account/me`, {
    dispatcher: agent,
    credentials: 'include'
  });

  const user = await res.json();

  if (user == null && !unprotectedRoutes.includes(url.pathname)) {
    console.log("Accessing a protected route with no credentials: " + url);
    throw redirect(307, '/');
  }

  return { user };

}
